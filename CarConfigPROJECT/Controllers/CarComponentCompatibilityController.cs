using CarConfigDATA.Models;
using CarConfigPROJECT.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarConfigPROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarComponentCompatibilityController : ControllerBase
    {
        private readonly AutoConfigDbContext _context;

        public CarComponentCompatibilityController(AutoConfigDbContext context)
        {
            _context = context;
        }

        // GET: api/CarComponentCompatibility
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.CarComponentCompatibilities
                .Include(c => c.CarComponent1)
                .Include(c => c.CarComponent2)
                .Select(c => new CarComponentCompatibilityGetDto
                {
                    Id = c.Id,
                    CarComponentId1 = c.CarComponentId1,
                    CarComponent1Name = c.CarComponent1.Name,
                    CarComponentId2 = c.CarComponentId2,
                    CarComponent2Name = c.CarComponent2.Name
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CarComponentCompatibilityCreateDto dto)
        {
            // 1. Provjera da nisu iste komponente
            if (dto.CarComponentId1 == dto.CarComponentId2)
                return BadRequest("Komponenta ne može biti kompatibilna sama sa sobom.");

            // 2. Provjera da obje komponente postoje
            var component1 = await _context.CarComponents.FindAsync(dto.CarComponentId1);
            var component2 = await _context.CarComponents.FindAsync(dto.CarComponentId2);

            if (component1 == null || component2 == null)
                return NotFound("Jedna od komponenti ne postoji.");

            // 3. Provjera da nisu istog tipa
            if (component1.ComponentTypeId == component2.ComponentTypeId)
                return BadRequest("Dva komponenta istog tipa ne mogu biti kompatibilna.");

            // 4. Normalizacija: manji ID uvijek prvi
            int id1 = Math.Min(dto.CarComponentId1, dto.CarComponentId2);
            int id2 = Math.Max(dto.CarComponentId1, dto.CarComponentId2);

            // 5. Provjera da kombinacija već ne postoji
            bool exists = await _context.CarComponentCompatibilities
                .AnyAsync(c => c.CarComponentId1 == id1 && c.CarComponentId2 == id2);

            if (exists)
                return Conflict("Kompatibilnost već postoji.");

            // 6. Kreiranje nove kompatibilnosti (NE dodaj Id, jer je identity)
            var compatibility = new CarComponentCompatibility
            {
                CarComponentId1 = id1,
                CarComponentId2 = id2
            };

            _context.CarComponentCompatibilities.Add(compatibility);
            await _context.SaveChangesAsync();

            // 7. Vraćanje rezultata
            return CreatedAtAction(nameof(GetById), new { id = compatibility.Id }, compatibility);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var compatibility = await _context.CarComponentCompatibilities
                .Include(c => c.CarComponent1)
                .Include(c => c.CarComponent2)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compatibility == null)
                return NotFound();

            return Ok(compatibility);
        }



        // PUT: api/CarComponentCompatibility/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, CarComponentCompatibilityCreateDto dto)
        {
            if (dto.CarComponentId1 == dto.CarComponentId2)
                return BadRequest("Komponenta ne može biti kompatibilna sama sa sobom.");

            var compatibility = await _context.CarComponentCompatibilities.FindAsync(id);
            if (compatibility == null)
                return NotFound();

            int id1 = Math.Min(dto.CarComponentId1, dto.CarComponentId2);
            int id2 = Math.Max(dto.CarComponentId1, dto.CarComponentId2);

            bool exists = await _context.CarComponentCompatibilities
                .AnyAsync(c => c.CarComponentId1 == id1 && c.CarComponentId2 == id2 && c.Id != id);

            if (exists)
                return Conflict("Kompatibilnost već postoji.");

            compatibility.CarComponentId1 = id1;
            compatibility.CarComponentId2 = id2;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CarComponentCompatibility/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var compatibility = await _context.CarComponentCompatibilities.FindAsync(id);
            if (compatibility == null)
                return NotFound();

            _context.CarComponentCompatibilities.Remove(compatibility);
            await _context.SaveChangesAsync();

            return Ok($"Compatibility with ID {id} deleted.");
        }
    }
}
