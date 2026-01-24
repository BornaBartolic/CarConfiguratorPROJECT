using CarConfigDATA;
using CarConfigDATA.Models;
using CarConfigDATA.Services;
using CarConfigPROJECTmvc.Infrastructure;
using CarConfigPROJECTmvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarConfigPROJECTmvc.Controllers;

[Authorize]
public class CarConfiguratorController : Controller
{
    private const string SessionKey = "CONFIG_SELECTED_COMPONENT_IDS";

    private readonly AutoConfigDbContext _db;
    private readonly IComponentTypeOrderStore _orderStore;

    public CarConfiguratorController(AutoConfigDbContext db, IComponentTypeOrderStore orderStore)
    {
        _db = db;
        _orderStore = orderStore;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;
        if (page < 1) page = 1;

        var baseQuery = _db.CarConfigurations
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var pageConfigs = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new { c.Id, c.Name, c.CreatedAt })
            .ToListAsync();

        var ids = pageConfigs.Select(x => x.Id).ToList();

        var totals = await _db.CarConfigurationComponents
            .AsNoTracking()
            .Where(x => ids.Contains(x.CarConfigurationId))
            .GroupBy(x => x.CarConfigurationId)
            .Select(g => new
            {
                Id = g.Key,
                Total = g.Sum(x => x.CarComponent.Price)
            })
            .ToDictionaryAsync(x => x.Id, x => x.Total);

        var model = pageConfigs.Select(c => new CarConfigurationListItemVm
        {
            Id = c.Id,
            Name = c.Name,
            CreatedAt = c.CreatedAt,
            TotalPrice = totals.TryGetValue(c.Id, out var t) ? t : 0m
        }).ToList();

        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;

        return View(model);
    }

    // Start / reset
    [HttpGet]
    public IActionResult Create()
    {
        HttpContext.Session.SetJson(SessionKey, new List<int>());
        return RedirectToAction(nameof(Step), new { stepIndex = 0 });
    }

    [HttpGet]
    public async Task<IActionResult> Step(int stepIndex)
    {
        var orderedTypes = await GetOrderedComponentTypes();
        if (orderedTypes.Count == 0)
            return Content("No ComponentType found in the database.");

        if (stepIndex < 0) stepIndex = 0;

        if (stepIndex >= orderedTypes.Count)
            return RedirectToAction(nameof(Summary));

        var chosenIds = HttpContext.Session.GetJson<List<int>>(SessionKey) ?? new List<int>();

        // If user goes back, cut selections after this step
        if (chosenIds.Count > stepIndex)
        {
            chosenIds = chosenIds.Take(stepIndex).ToList();
            HttpContext.Session.SetJson(SessionKey, chosenIds);
        }

        var currentType = orderedTypes[stepIndex];

        // Options with Description
        List<CarConfigurationOptionsVM> options;

        if (stepIndex == 0)
        {
            // Car Type options: show all
            options = await _db.CarComponents
                .AsNoTracking()
                .Where(c => c.ComponentTypeId == currentType.Id)
                .OrderBy(c => c.Name)
                .Select(c => new CarConfigurationOptionsVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price
                })
                .ToListAsync();
        }
        else
        {
            var carTypeId = await GetSelectedCarTypeId(chosenIds, orderedTypes);
            if (carTypeId == null)
                return RedirectToAction(nameof(Create));

            IQueryable<CarComponent> query = _db.CarComponents
                .AsNoTracking()
                .Where(c => c.ComponentTypeId == currentType.Id);

            // FAST path: CarType is always stored in CarComponentId1
            query = query.Where(c =>
                _db.CarComponentCompatibilities.Any(x =>
                    x.CarComponentId1 == carTypeId.Value && x.CarComponentId2 == c.Id
                )
            );

            options = await query
                .OrderBy(c => c.Name)
                .Select(c => new CarConfigurationOptionsVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price
                })
                .ToListAsync();
        }

        var chosenSoFar = await BuildChosenSoFar(chosenIds);

        var vm = new ConfigStepVm
        {
            StepIndex = stepIndex,
            ComponentTypeId = currentType.Id,
            ComponentTypeName = currentType.Name,
            Options = options,
            ChosenSoFar = chosenSoFar,
            SelectedComponentId = null
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Step(ConfigStepVm vm)
    {
        if (vm.SelectedComponentId == null)
        {
            ModelState.AddModelError(nameof(vm.SelectedComponentId), "You must select an option.");
            return await Step(vm.StepIndex);
        }

        var chosenIds = HttpContext.Session.GetJson<List<int>>(SessionKey) ?? new List<int>();

        // session is source of truth
        if (chosenIds.Count > vm.StepIndex)
            chosenIds = chosenIds.Take(vm.StepIndex).ToList();

        // Only check compatibility against selected Car Type (step 0)
        if (vm.StepIndex > 0)
        {
            var orderedTypes = await GetOrderedComponentTypes();
            var carTypeId = await GetSelectedCarTypeId(chosenIds, orderedTypes);
            if (carTypeId == null)
                return RedirectToAction(nameof(Create));

            // FAST: CarType always in Id1
            var ok = await _db.CarComponentCompatibilities.AnyAsync(x =>
                x.CarComponentId1 == carTypeId.Value && x.CarComponentId2 == vm.SelectedComponentId.Value
            );

            if (!ok)
            {
                ModelState.AddModelError(nameof(vm.SelectedComponentId), "Selected option is not compatible with the chosen Car Type.");
                return await Step(vm.StepIndex);
            }
        }

        chosenIds.Add(vm.SelectedComponentId.Value);
        HttpContext.Session.SetJson(SessionKey, chosenIds);

        return RedirectToAction(nameof(Step), new { stepIndex = vm.StepIndex + 1 });
    }

    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        var chosenIds = HttpContext.Session.GetJson<List<int>>(SessionKey) ?? new List<int>();
        if (!chosenIds.Any())
            return RedirectToAction(nameof(Create));

        // ✅ must complete all steps before summary
        if (!await IsConfigurationComplete(chosenIds))
        {
            TempData["Error"] = "Please complete all steps before reviewing and saving your configuration.";
            return RedirectToAction(nameof(Step), new { stepIndex = chosenIds.Count });
        }

        var components = await _db.CarComponents
            .Where(c => chosenIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Price,
                TypeName = c.ComponentType.Name
            })
            .ToListAsync();

        var ordered = chosenIds
            .Select(id => components.First(c => c.Id == id))
            .ToList();

        ViewBag.Total = ordered.Sum(x => x.Price);

        return View(ordered);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string? name)
    {
        var chosenIds = HttpContext.Session.GetJson<List<int>>(SessionKey) ?? new List<int>();
        if (chosenIds.Count == 0)
            return RedirectToAction(nameof(Create));

        // ✅ must complete all steps before saving
        if (!await IsConfigurationComplete(chosenIds))
        {
            TempData["Error"] = "Please complete all steps before saving your configuration.";
            return RedirectToAction(nameof(Step), new { stepIndex = chosenIds.Count });
        }

        var config = new CarConfiguration
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Unnamed Configuration" : name.Trim(),
            CreatedAt = DateTime.Now
        };

        _db.CarConfigurations.Add(config);
        await _db.SaveChangesAsync();

        foreach (var id in chosenIds.Distinct())
        {
            _db.CarConfigurationComponents.Add(new CarConfigurationComponent
            {
                CarConfigurationId = config.Id,
                CarComponentId = id
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = config.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var config = await _db.CarConfigurations
            .Where(c => c.Id == id)
            .Select(c => new { c.Id, c.Name, c.CreatedAt })
            .FirstOrDefaultAsync();

        if (config == null) return NotFound();

        var typeOrder = await _orderStore.GetOrderAsync();
        var typeIndex = typeOrder
            .Select((typeId, i) => new { typeId, i })
            .ToDictionary(x => x.typeId, x => x.i);

        var components = await _db.CarConfigurationComponents
            .Where(x => x.CarConfigurationId == id)
            .Select(x => new CarConfigurationComponentVm
            {
                TypeId = x.CarComponent.ComponentTypeId,
                TypeName = x.CarComponent.ComponentType.Name,
                ComponentName = x.CarComponent.Name,
                Price = x.CarComponent.Price
            })
            .ToListAsync();

        components = components
            .OrderBy(x => typeIndex.TryGetValue(x.TypeId, out var idx) ? idx : int.MaxValue)
            .ThenBy(x => x.TypeId)
            .ThenBy(x => x.ComponentName)
            .ToList();

        var vm = new CarConfigurationDetailsVm
        {
            Id = config.Id,
            Name = config.Name,
            CreatedAt = config.CreatedAt,
            Components = components
        };

        return View(vm);
    }

    // -------------------- helpers --------------------

    private async Task<bool> IsConfigurationComplete(List<int> chosenIds)
    {
        var orderedTypes = await GetOrderedComponentTypes();
        return chosenIds != null && chosenIds.Count == orderedTypes.Count;
    }

    private async Task<List<ComponentType>> GetOrderedComponentTypes()
    {
        var types = await _db.ComponentTypes.OrderBy(t => t.Id).ToListAsync();
        var order = await _orderStore.GetOrderAsync();

        var index = order.Select((id, i) => new { id, i }).ToDictionary(x => x.id, x => x.i);

        return types
            .OrderBy(t => index.ContainsKey(t.Id) ? index[t.Id] : int.MaxValue)
            .ThenBy(t => t.Id)
            .ToList();
    }

    /// <summary>
    /// Gets the selected Car Type component id from session.
    /// Assumption: step 0 (first ordered type) is Car Type.
    /// </summary>
    private async Task<int?> GetSelectedCarTypeId(List<int> chosenIds, List<ComponentType> orderedTypes)
    {
        if (chosenIds.Count == 0) return null;
        if (orderedTypes.Count == 0) return null;

        var carTypeTypeId = orderedTypes[0].Id;

        return await _db.CarComponents
            .AsNoTracking()
            .Where(c => c.ComponentTypeId == carTypeTypeId && chosenIds.Contains(c.Id))
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<List<ChosenItemVm>> BuildChosenSoFar(List<int> chosenIds)
    {
        if (chosenIds == null || chosenIds.Count == 0)
            return new List<ChosenItemVm>();

        var items = await _db.CarComponents
            .AsNoTracking()
            .Where(c => chosenIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.Name,
                TypeName = c.ComponentType.Name
            })
            .ToListAsync();

        var map = items.ToDictionary(x => x.Id);

        return chosenIds
            .Where(id => map.ContainsKey(id))
            .Select(id => new ChosenItemVm
            {
                TypeName = map[id].TypeName,
                ComponentName = map[id].Name
            })
            .ToList();
    }

    [HttpGet]
    public IActionResult Back(int stepIndex)
    {
        var prev = stepIndex - 1;
        if (prev < 0) return RedirectToAction(nameof(Create));

        return RedirectToAction(nameof(Step), new { stepIndex = prev });
    }
}
