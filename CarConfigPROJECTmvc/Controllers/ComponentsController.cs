using CarConfigDATA.Models;
using CarConfigPROJECTmvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarConfigPROJECTmvc.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ComponentsController : Controller
    {
        private readonly AutoConfigDbContext _context;

        public ComponentsController(AutoConfigDbContext context)
        {
            _context = context;
        }

        // GET: ComponentsController
        public IActionResult Index(string searchString, string typeFilter, int page = 1)
        {
            int pageSize = 10; // 10 stavki po stranici

            // Dohvati query
            var query = _context.CarComponents
                .Include(c => c.ComponentType)
                .AsQueryable();

            // Filter po nazivu
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Name.Contains(searchString));
            }

            // Filter po tipu
            if (!string.IsNullOrEmpty(typeFilter))
            {
                query = query.Where(c => c.ComponentType.Name == typeFilter);
            }

            // Sort po nazivu
            query = query.OrderBy(c => c.Name);

            int totalItems = query.Count();

            // Dohvati stavke za trenutnu stranicu
            var components = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ComponentVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    ComponentTypeName = c.ComponentType.Name
                })
                .ToList();

            // SelectList za dropdown
            ViewBag.ComponentTypes = new SelectList(
                _context.ComponentTypes.Select(t => t.Name).ToList(),
                typeFilter
            );

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(components);
        }





        // GET: ComponentsController/Details/5
        public IActionResult Details(int id)
        {
            var component = _context.CarComponents
                .Include(c => c.ComponentType)
                .FirstOrDefault(c => c.Id == id);

            if (component == null)
                return NotFound();

            var model = new ComponentVM
            {
                Id = component.Id,
                Name = component.Name,
                Description = component.Description,
                Price = component.Price,
                ComponentTypeName = component.ComponentType.Name // string za prikaz
            };

            return View(model);
        }

        // GET: ComponentsController/Create
        public IActionResult Create()
        {
            var model = new ComponentVM
            {
                ComponentTypeList = _context.ComponentTypes
                    .Select(t => new SelectListItem
                    {
                        Value = t.Name, // VALUE je naziv
                        Text = t.Name
                    })
                    .ToList()
            };

            return View(model);
        }


        // POST: ComponentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ComponentVM component)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Ponovno popuni dropdown
                    component.ComponentTypeList = _context.ComponentTypes
                        .Select(t => new SelectListItem
                        {
                            Value = t.Name,
                            Text = t.Name
                        })
                        .ToList();

                    return View(component);
                }

                // Pronađi ID po nazivu
                var typeEntity = _context.ComponentTypes
                    .FirstOrDefault(t => t.Name == component.ComponentTypeName);

                if (typeEntity == null)
                {
                    ModelState.AddModelError("ComponentTypeId", "Tip komponente ne postoji.");

                    component.ComponentTypeList = _context.ComponentTypes
                        .Select(t => new SelectListItem
                        {
                            Value = t.Name,
                            Text = t.Name
                        })
                        .ToList();

                    return View(component);
                }

                var newComponent = new CarComponent
                {
                    Name = component.Name,
                    Description = component.Description,
                    Price = component.Price,
                    ComponentTypeId = typeEntity.Id // ID iz baze
                };

                _context.CarComponents.Add(newComponent);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                component.ComponentTypeList = _context.ComponentTypes
                    .Select(t => new SelectListItem
                    {
                        Value = t.Name,
                        Text = t.Name
                    })
                    .ToList();

                return View(component);
            }
        }


        // GET: ComponentsController/Edit/5
        public IActionResult Edit(int id)
        {
            var component = _context.CarComponents
                .Include(c => c.ComponentType)
                .FirstOrDefault(c => c.Id == id);

            if (component == null)
                return NotFound();

            var model = new ComponentVM
            {
                Id = component.Id,
                Name = component.Name,
                Description = component.Description,
                Price = component.Price,
                ComponentTypeName = component.ComponentType.Name, // string za view
                ComponentTypeList = _context.ComponentTypes
                    .Select(t => new SelectListItem
                    {
                        Value = t.Name,
                        Text = t.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: ComponentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ComponentVM component)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    
                    component.ComponentTypeList = _context.ComponentTypes
                        .Select(t => new SelectListItem
                        {
                            Value = t.Name,
                            Text = t.Name
                        })
                        .ToList();

                    return View(component);
                }

                var existing = _context.CarComponents
                    .FirstOrDefault(c => c.Id == component.Id);

                if (existing == null)
                    return NotFound();

                
                var typeEntity = _context.ComponentTypes
                    .FirstOrDefault(t => t.Name == component.ComponentTypeName);

                if (typeEntity == null)
                {
                    ModelState.AddModelError("ComponentTypeName", "Tip komponente ne postoji.");
                    component.ComponentTypeList = _context.ComponentTypes
                        .Select(t => new SelectListItem
                        {
                            Value = t.Name,
                            Text = t.Name
                        })
                        .ToList();
                    return View(component);
                }

              
                existing.Name = component.Name;
                existing.Description = component.Description;
                existing.Price = component.Price;
                existing.ComponentTypeId = typeEntity.Id;

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
               
                component.ComponentTypeList = _context.ComponentTypes
                    .Select(t => new SelectListItem
                    {
                        Value = t.Name,
                        Text = t.Name
                    })
                    .ToList();
                return View(component);
            }
        }


        public IActionResult Delete(int id)
        {
            var component = _context.CarComponents
                .Include(c => c.ComponentType)
                .FirstOrDefault(c => c.Id == id);

            if (component == null)
                return NotFound();

            // Učitaj sve kompatibilnosti gdje sudjeluje komponenta i uključi navigacijske property
            var compatibilities = _context.CarComponentCompatibilities
                .Include(c => c.CarComponent1)
                    .ThenInclude(c => c.ComponentType)
                .Include(c => c.CarComponent2)
                    .ThenInclude(c => c.ComponentType)
                .Where(c => c.CarComponentId1 == id || c.CarComponentId2 == id)
                .ToList();

            var model = new ComponentDeleteVM
            {
                Component = component,
                Compatibilities = compatibilities
            };

            return View(model);
        }





        // POST: Components/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1) pobriši sve veze konfiguracija -> komponenta
            var links = _context.CarConfigurationComponents
                .Where(x => x.CarComponentId == id);

            _context.CarConfigurationComponents.RemoveRange(links);

            // 2) pobriši i kompatibilnosti gdje se pojavljuje komponenta (inače će i to puknut)
            var compat = _context.CarComponentCompatibilities
                .Where(x => x.CarComponentId1 == id || x.CarComponentId2 == id);

            _context.CarComponentCompatibilities.RemoveRange(compat);

            // 3) tek onda komponentu
            var component = await _context.CarComponents.FindAsync(id);
            if (component != null)
                _context.CarComponents.Remove(component);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
