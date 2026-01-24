using CarConfigDATA.Models;
using CarConfigDATA.Services;
using CarConfigPROJECTmvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarConfigPROJECTmvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConfiguratorOptionsController : Controller
    {
        private readonly AutoConfigDbContext _db;
        private readonly IComponentTypeOrderStore _orderStore;

        public ConfiguratorOptionsController(AutoConfigDbContext db, IComponentTypeOrderStore orderStore)
        {
            _db = db;
            _orderStore = orderStore;
        }

        // -------- helpers --------

        private async Task<int?> GetCarTypeComponentTypeIdAsync()
        {
            var names = new[] { "Car Type", "Tip auta", "Vrsta auta", "CarType" };

            return await _db.ComponentTypes
                .AsNoTracking()
                .Where(t => names.Contains(t.Name))
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();
        }

        private static List<int> ForceCarTypeFirst(List<int> order, int carTypeTypeId)
        {
            // remove duplicates
            order = order.Distinct().ToList();

            // if CarType is present, move to front
            if (order.Remove(carTypeTypeId))
                order.Insert(0, carTypeTypeId);

            return order;
        }

        private async Task<List<int>> EnsureOrderHasAllTypes()
        {
            var allIds = await _db.ComponentTypes
                .OrderBy(t => t.Id)
                .Select(t => t.Id)
                .ToListAsync();

            var order = await _orderStore.GetOrderAsync();

            // keep only existing ids + distinct
            order = order.Where(id => allIds.Contains(id)).Distinct().ToList();

            // append missing
            foreach (var id in allIds)
                if (!order.Contains(id))
                    order.Add(id);

            // force Car Type first (if exists)
            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();
            if (carTypeTypeId != null)
                order = ForceCarTypeFirst(order, carTypeTypeId.Value);

            await _orderStore.SaveOrderAsync(order);
            return order;
        }

        // -------- actions --------

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var types = await _db.ComponentTypes
                .OrderBy(t => t.Id)
                .Select(t => new { t.Id, t.Name })
                .ToListAsync();

            var savedOrder = await EnsureOrderHasAllTypes();

            var index = savedOrder
                .Select((id, i) => new { id, i })
                .ToDictionary(x => x.id, x => x.i);

            var ordered = types
                .OrderBy(t => index.ContainsKey(t.Id) ? index[t.Id] : int.MaxValue)
                .ThenBy(t => t.Id)
                .ToList();

            var vm = new ComponentTypeOrderVm
            {
                Items = ordered.Select(t => new ComponentTypeOrderItemVm
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var order = await EnsureOrderHasAllTypes();
            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();

            // if Car Type exists, block moving it or moving something into position 0
            if (carTypeTypeId != null)
            {
                if (id == carTypeTypeId.Value) return RedirectToAction(nameof(Index));

                var idx = order.IndexOf(id);
                if (idx <= 0) return RedirectToAction(nameof(Index));
                if (idx - 1 == 0 && order[0] == carTypeTypeId.Value) return RedirectToAction(nameof(Index)); // can't pass Car Type
            }

            var i = order.IndexOf(id);
            if (i > 0)
            {
                (order[i - 1], order[i]) = (order[i], order[i - 1]);
                await _orderStore.SaveOrderAsync(order);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var order = await EnsureOrderHasAllTypes();
            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();

            if (carTypeTypeId != null && id == carTypeTypeId.Value)
                return RedirectToAction(nameof(Index)); // Car Type cannot move down

            var idx = order.IndexOf(id);
            if (idx >= 0 && idx < order.Count - 1)
            {
                (order[idx], order[idx + 1]) = (order[idx + 1], order[idx]);
                await _orderStore.SaveOrderAsync(order);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetOrder()
        {
            // "Default" = by Id, but with Car Type forced to first
            var order = await _db.ComponentTypes
                .OrderBy(t => t.Id)
                .Select(t => t.Id)
                .ToListAsync();

            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();
            if (carTypeTypeId != null)
                order = ForceCarTypeFirst(order, carTypeTypeId.Value);

            await _orderStore.SaveOrderAsync(order);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrder([FromForm] List<int> orderedIds)
        {
            if (orderedIds == null || orderedIds.Count == 0)
                return RedirectToAction(nameof(Index));

            // keep only valid ids + fill missing
            var allIds = await _db.ComponentTypes.Select(t => t.Id).ToListAsync();
            var cleaned = orderedIds.Where(id => allIds.Contains(id)).Distinct().ToList();

            foreach (var id in allIds.OrderBy(x => x))
                if (!cleaned.Contains(id))
                    cleaned.Add(id);

            // force Car Type first
            var carTypeTypeId = await GetCarTypeComponentTypeIdAsync();
            if (carTypeTypeId != null)
                cleaned = ForceCarTypeFirst(cleaned, carTypeTypeId.Value);

            await _orderStore.SaveOrderAsync(cleaned);
            return RedirectToAction(nameof(Index));
        }
    }
}
