using CarConfigDATA.Models;
using CarConfigPROJECTmvc.Models;
using CarConfigPROJECTmvc.ViewModels.Component;
using CarConfigPROJECTmvc.ViewModels.ComponentType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]

public class ComponentTypeController : Controller
{
    private readonly AutoConfigDbContext _context;

    public ComponentTypeController(AutoConfigDbContext context)
    {
        _context = context;
    }

    // GET: Index sa search + paginacijom
    public IActionResult Index(string searchString, int page = 1)
    {
        int pageSize = 10;

        var query = _context.ComponentTypes.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(t => t.Name.Contains(searchString));
        }

        query = query.OrderBy(t => t.Name);

        int totalItems = query.Count();

        var componentTypes = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new ComponentTypeVM
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToList();

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        ViewBag.SearchString = searchString;

        return View(componentTypes);
    }

    // GET: Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ComponentTypeVM vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var entity = new ComponentType
        {
            Name = vm.Name
        };

        _context.ComponentTypes.Add(entity);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // GET: Edit
    public IActionResult Edit(int id)
    {
        var entity = _context.ComponentTypes.Find(id);
        if (entity == null) return NotFound();

        var vm = new ComponentTypeVM
        {
            Id = entity.Id,
            Name = entity.Name
        };

        return View(vm);
    }

    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ComponentTypeVM vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var entity = _context.ComponentTypes.Find(vm.Id);
        if (entity == null) return NotFound();

        entity.Name = vm.Name;

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // GET: Details
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _context.ComponentTypes
            .Where(t => t.Id == id)
            .Select(t => new ComponentTypeDetailsVm
            {
                Id = t.Id,
                Name = t.Name,
                Components = _context.CarComponents
                    .Where(c => c.ComponentTypeId == t.Id)
                    .OrderBy(c => c.Name)
                    .Select(c => new ComponentRowVm
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Price = c.Price
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (vm == null) return NotFound();

        return View(vm);
    }

    // GET: Delete
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _context.ComponentTypes
            .Where(t => t.Id == id)
            .Select(t => new ComponentTypeDeleteVm
            {
                Id = t.Id,
                Name = t.Name,
                ComponentNames = _context.CarComponents
                    .Where(c => c.ComponentTypeId == t.Id)
                    .Select(c => c.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (type == null)
            return NotFound();

        return View(type);
    }


    // POST: DeleteConfirmed
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // 1) dohvati sve komponente tog tipa
        var componentIds = await _context.CarComponents
            .Where(c => c.ComponentTypeId == id)
            .Select(c => c.Id)
            .ToListAsync();

        if (componentIds.Count > 0)
        {
            // 2) pobriši veze konfiguracija -> komponente
            var configLinks = _context.CarConfigurationComponents
                .Where(x => componentIds.Contains(x.CarComponentId));
            _context.CarConfigurationComponents.RemoveRange(configLinks);

            // 3) pobriši kompatibilnosti gdje se komponente pojavljuju
            var compatLinks = _context.CarComponentCompatibilities
                .Where(x => componentIds.Contains(x.CarComponentId1) || componentIds.Contains(x.CarComponentId2));
            _context.CarComponentCompatibilities.RemoveRange(compatLinks);

            // 4) obriši komponente
            var comps = _context.CarComponents.Where(c => c.ComponentTypeId == id);
            _context.CarComponents.RemoveRange(comps);
        }

        // 5) tek sad obriši component type
        var type = await _context.ComponentTypes.FindAsync(id);
        if (type != null)
            _context.ComponentTypes.Remove(type);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}
