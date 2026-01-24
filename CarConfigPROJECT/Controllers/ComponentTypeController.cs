using CarConfigPROJECT.DTOs;
using CarConfigDATA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarConfigPROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentTypeController : ControllerBase
    {
        private readonly AutoConfigDbContext _context;

        public ComponentTypeController(AutoConfigDbContext context)
        {
            _context = context;
        }
        // GET: api/ComponentType bez PAGEING-a
        //[HttpGet] ovo je pez pageinga
        //public ActionResult<ComponentTypeDto> Get() {

        //    var allCompTypes = _context.ComponentTypes
        //        .Select(x => new ComponentTypeDto
        //        {
        //            Id = x.Id,
        //            Name = x.Name,
        //        })
        //        .ToList();


        //    return Ok(allCompTypes);
        //}

        // GET: api/ComponentType PAGEING
        [HttpGet("Search")]
        public ActionResult<ComponentTypeGetDto> GetComponentTypes(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = _context.ComponentTypes.Count();



            var allCompTypes = _context.ComponentTypes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(c => c.Id)
                .Select(x => new ComponentTypeGetDto
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .ToList();

            var response = new
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = allCompTypes
            };

            return Ok(allCompTypes);
        }

        // GET: api/ComponentsType/3
        [HttpGet("Search/{id}")]

        public ActionResult<ComponentTypeGetDto> GetComponentTypeById(int id)
        {
            var componentType = _context.ComponentTypes
                .Where(x => x.Id == id)
                .Select(x => new ComponentTypeGetDto
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .FirstOrDefault();

            if (componentType == null)
                return NotFound($"Component with ID {id} not found");
            else
                return Ok(componentType);
        }



        // POST: api/ComponentsType
        [HttpPost("Create")]
        public ActionResult<ComponentTypePostDto> CreateComponentType([FromBody] ComponentTypePostDto dto)
        {

            var newComponentType = new ComponentType
            {
                Name = dto.Name,
            };

            _context.ComponentTypes.Add(newComponentType);
            _context.SaveChanges();

            var result = new ComponentTypeGetDto
            {
                Id = newComponentType.Id,
                Name = newComponentType.Name
            };

            return CreatedAtAction
                (nameof(GetComponentTypeById), new { id = newComponentType.Id }, result);
        }


        // PUT: api/ComponentsType
        [HttpPut("Update/{id}")]
        public ActionResult<ComponentTypePostDto> UpdateComponentType(int id, [FromBody] ComponentTypePostDto dto)
        {
            var componentType = _context.ComponentTypes.Find(id);

            if (componentType == null)
                return NotFound();

            componentType.Name = dto.Name;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComponentTypeExists(id))
                    return NotFound(new { message = "ComponentType not found." });
                else
                    throw;
            }

            var result = new ComponentTypeGetDto
            {
                Id = componentType.Id,
                Name = componentType.Name
            };

            return Ok(new
            {
                message = "ComponentType is updated",
                updatedComponentType = result
            });
        }

        // DELETE: api/ComponentsType/6
        [HttpDelete("Delete/{id}")]
        public ActionResult<ComponentTypeGetDto> DeleteComponentType(int id)
        {
            var componentType = _context.ComponentTypes.Find(id);
            if (componentType == null)
                return NotFound();

            _context.ComponentTypes.Remove(componentType);
            _context.SaveChanges();

            return Ok($"Component Type with ID: {id} and name: {componentType.Name} was DELETED");
        }

        private bool ComponentTypeExists(int id)
        {
            return _context.ComponentTypes.Any(x => x.Id == id);

        }
    }
}
