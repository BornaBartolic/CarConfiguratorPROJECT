using CarConfigDATA.Models;
using CarConfigPROJECTmvc.ViewModels.ComponentCompatibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]

public class ComponentCompatibilityController : Controller
{
    private readonly AutoConfigDbContext _context;

    public ComponentCompatibilityController(AutoConfigDbContext context)
    {
        _context = context;
    }

    // ------------------------- helpers -------------------------

    // Tries to find the ComponentTypeId for "Car Type" (supports a few possible names).
    private async Task<int?> GetCarTypeComponentTypeIdAsync()
    {
        var names = new[] { "Car Type", "Tip auta", "Vrsta auta", "CarType" };

        var id = await _context.ComponentTypes
            .AsNoTracking()
            .Where(t => names.Contains(t.Name))
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync();

        return id;
    }

    private async Task<(int carTypeId, int otherId)?> NormalizeCarTypePairAsync(int idA, int idB)
    {
        var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();
        if (carTypeTypeId == null) return null;

        var a = await _context.CarComponents.AsNoTracking().FirstOrDefaultAsync(c => c.Id == idA);
        var b = await _context.CarComponents.AsNoTracking().FirstOrDefaultAsync(c => c.Id == idB);

        if (a == null || b == null) return null;

        bool aIsCarType = a.ComponentTypeId == carTypeTypeId.Value;
        bool bIsCarType = b.ComponentTypeId == carTypeTypeId.Value;

        // Must be exactly one Car Type
        if (aIsCarType == bIsCarType) return null;

        return aIsCarType ? (a.Id, b.Id) : (b.Id, a.Id);
    }

    // ========================= INDEX =========================
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;
        if (page < 1) page = 1;

        var query = _context.CarComponentCompatibilities
            .AsNoTracking()
            .OrderBy(x => x.Id);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var compatPage = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var componentIds = compatPage
            .SelectMany(x => new[] { x.CarComponentId1, x.CarComponentId2 })
            .Distinct()
            .ToList();

        var componentNames = await _context.CarComponents
            .AsNoTracking()
            .Where(c => componentIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var model = compatPage.Select(x => new CarConfigPROJECTmvc.ViewModels.ComponentCompatibility.ComponentCompatibilityRowVm
        {
            Id = x.Id,
            Component1Name = componentNames.TryGetValue(x.CarComponentId1, out var n1) ? n1 : $"#{x.CarComponentId1}",
            Component2Name = componentNames.TryGetValue(x.CarComponentId2, out var n2) ? n2 : $"#{x.CarComponentId2}"
        }).ToList();

        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;

        return View(model);
    }

    // ========================= CREATE GET =========================
    public async Task<IActionResult> Create()
    {
        var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();

        // Ako iz nekog razloga ne nađe Car Type type, fallback: sve u oba (da ne pukne view)
        if (carTypeTypeId == null)
        {
            var all = await _context.CarComponents
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            var fallbackVm = new ComponentCompatibilityVM
            {
                CarTypes = new List<SelectListItem>(),
                Components = all
            };

            ModelState.AddModelError("", "Car Type ComponentType not found. Please create a ComponentType named 'Car Type'.");
            return View(fallbackVm);
        }

        var vm = new ComponentCompatibilityVM
        {
            // 1st dropdown: ONLY Car Type components
            CarTypes = await _context.CarComponents
                .AsNoTracking()
                .Where(c => c.ComponentTypeId == carTypeTypeId.Value)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync(),

            // 2nd dropdown: everything EXCEPT Car Type components
            Components = await _context.CarComponents
                .AsNoTracking()
                .Where(c => c.ComponentTypeId != carTypeTypeId.Value)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync()
        };

        return View(vm);
    }

    // ========================= CREATE POST =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ComponentCompatibilityVM vm)
    {
        if (vm.CarComponentId1 == vm.CarComponentId2)
            ModelState.AddModelError("", "A component cannot be compatible with itself.");

        var normalized = await NormalizeCarTypePairAsync(vm.CarComponentId1, vm.CarComponentId2);
        if (normalized == null)
        {
            ModelState.AddModelError("", "You must select exactly one Car Type component and one non-Car-Type component.");
        }
        else
        {
            var (carTypeId, otherId) = normalized.Value;

            // (This check is basically redundant now, but kept)
            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();
            var otherComponent = await _context.CarComponents.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == otherId);

            if (carTypeTypeId != null && otherComponent != null && otherComponent.ComponentTypeId == carTypeTypeId.Value)
            {
                ModelState.AddModelError("", "Two Car Type components cannot be linked.");
            }

            bool exists = await _context.CarComponentCompatibilities
                .AsNoTracking()
                .AnyAsync(c => c.CarComponentId1 == carTypeId && c.CarComponentId2 == otherId);

            if (exists)
                ModelState.AddModelError("", "This compatibility link already exists.");
        }

        if (!ModelState.IsValid)
        {
            // IMPORTANT: repopulate BOTH dropdowns
            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();

            if (carTypeTypeId != null)
            {
                vm.CarTypes = await _context.CarComponents
                    .AsNoTracking()
                    .Where(c => c.ComponentTypeId == carTypeTypeId.Value)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();

                vm.Components = await _context.CarComponents
                    .AsNoTracking()
                    .Where(c => c.ComponentTypeId != carTypeTypeId.Value)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();
            }
            else
            {
                // fallback
                vm.CarTypes = new List<SelectListItem>();
                vm.Components = await _context.CarComponents
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();
            }

            return View(vm);
        }

        var (ctId, oId) = normalized!.Value;

        var compatibility = new CarComponentCompatibility
        {
            CarComponentId1 = ctId,  // ALWAYS Car Type
            CarComponentId2 = oId    // ALWAYS other component
        };

        _context.CarComponentCompatibilities.Add(compatibility);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    // ========================= EDIT GET =========================
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _context.CarComponentCompatibilities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound();

        var normalized = await NormalizeCarTypePairAsync(
            entity.CarComponentId1,
            entity.CarComponentId2
        );

        if (normalized == null)
            return BadRequest();

        var (carTypeId, otherId) = normalized.Value;

        var vm = new ComponentCompatibilityVM
        {
            Id = entity.Id,
            CarComponentId1 = carTypeId, // ALWAYS Car Type
            CarComponentId2 = otherId    // ALWAYS other component
        };

        var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();

        if (carTypeTypeId != null)
        {
            vm.CarTypes = await _context.CarComponents
                .AsNoTracking()
                .Where(c => c.ComponentTypeId == carTypeTypeId.Value)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            vm.Components = await _context.CarComponents
                .AsNoTracking()
                .Where(c => c.ComponentTypeId != carTypeTypeId.Value)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {   
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
        else
        {
            vm.CarTypes = new List<SelectListItem>();
            vm.Components = await _context.CarComponents
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        return View(vm);
    }

    // ========================= EDIT POST =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ComponentCompatibilityVM vm)
    {
        if (vm.CarComponentId1 == vm.CarComponentId2)
            ModelState.AddModelError("", "A component cannot be compatible with itself.");

        var normalized = await NormalizeCarTypePairAsync(
            vm.CarComponentId1,
            vm.CarComponentId2
        );

        if (normalized == null)
        {
            ModelState.AddModelError(
                "",
                "You must select exactly one Car Type component and one non-Car-Type component."
            );
        }

        if (!ModelState.IsValid)
        {
            // REPOLULATE DROPDOWNS (SAME AS CREATE)
            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();

            if (carTypeTypeId != null)
            {
                vm.CarTypes = await _context.CarComponents
                    .AsNoTracking()
                    .Where(c => c.ComponentTypeId == carTypeTypeId.Value)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                vm.Components = await _context.CarComponents
                    .AsNoTracking()
                    .Where(c => c.ComponentTypeId != carTypeTypeId.Value)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();
            }
            else
            {
                vm.CarTypes = new List<SelectListItem>();
                vm.Components = await _context.CarComponents
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();
            }

            return View(vm);
        }

        var (ctId, oId) = normalized!.Value;

        var entity = await _context.CarComponentCompatibilities
            .FirstOrDefaultAsync(x => x.Id == vm.Id);

        if (entity == null)
            return NotFound();

        entity.CarComponentId1 = ctId;
        entity.CarComponentId2 = oId;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    // ========================= DELETE =========================
    public async Task<IActionResult> Delete(int id)
    {
        var comp = await _context.CarComponentCompatibilities.FindAsync(id);
        if (comp == null) return NotFound();

        _context.CarComponentCompatibilities.Remove(comp);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
