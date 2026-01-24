using CarConfigPROJECT.DTOs;
using CarConfigDATA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarConfigPROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentsController : ControllerBase
    {
        private readonly AutoConfigDbContext _context;

        public ComponentsController(AutoConfigDbContext context)
        {
            _context = context;
        }

        // GET: api/CarComponents OBRAĐENO ovo je pez pageinga
        //[HttpGet]
        //public ActionResult<CarComponentDto> GetCarComponents()
        //{
        //    var components =  _context.CarComponents             
        //        .Select(c => new CarComponentDto.CarComponentGetDto
        //        {                  
        //            Name = c.Name,
        //            Id = c.Id,
        //            Description = c.Description,
        //            Price = c.Price,
        //            ComponentTypeId = c.ComponentTypeId,
        //            ComponentTypeName  = c.ComponentType.Name

        //        })
        //        .ToList();

        //    return Ok(components);
        //}

        // GET: api/CarComponents PAGEING

        [HttpGet("Search")]
        public ActionResult GetCarComponents(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = _context.CarComponents.Count();

            var components = _context.CarComponents
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .OrderBy(c => c.Id)
               .Select(c => new CarComponentGetDto
               {
                   Name = c.Name,
                   Id = c.Id,
                   Description = c.Description,
                   Price = c.Price,
                   ComponentTypeId = c.ComponentTypeId,
                   ComponentTypeName = c.ComponentType.Name

               })
                .ToList();

            var response = new
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = components
            };

            return Ok(response);
        }


        // GET: api/Components/5 OBRAĐENO
        [HttpGet("Search/{id}")]
        public ActionResult<CarComponentGetDto> GetById(int id)
        {
            var component = _context.CarComponents
                  .Where(c => c.Id == id)
                  .Select(c => new CarComponentGetDto
                  {
                      Name = c.Name,
                      Id = c.Id,
                      Description = c.Description,
                      Price = c.Price,
                      ComponentTypeId = c.ComponentTypeId,
                      ComponentTypeName = c.ComponentType.Name
                  }
                  ).FirstOrDefault();

            //if na fensi nacin
            return component is null
                ? NotFound($"Component with ID {id} not found")
                : Ok(component);
        }


        // POST: api/Components Kreiranje novog Componenta //OBRAĐENO
        [HttpPost("Create")]
        public ActionResult<CarComponentPostDto> CreateCarComponent([FromBody] CarComponentPostDto dto)
        {
            var newComponent = new CarComponent
            {
                Name = dto.Name,
                ComponentTypeId = dto.ComponentTypeId,
                Price = dto.Price,
                Description = dto.Description,
            };

            _context.CarComponents.Add(newComponent);
            _context.SaveChanges();

            return CreatedAtAction
                (nameof(GetById), new { id = newComponent.Id }, dto);
        }

        // Update: api/Components/ Odrađeno
        [HttpPut("Update/{id}")]
        public ActionResult<CarComponentPostDto> UpdateCarComponent(int id, [FromBody] CarComponentPostDto dto)
        {
            // Dohvati postojeći entitet iz baze
            var component = _context.CarComponents.Find(id);
            if (component == null)
                return NotFound();

            // Mapiraj polja iz DTO-a
            component.Name = dto.Name;
            component.Description = dto.Description;
            component.Price = dto.Price;
            component.ComponentTypeId = dto.ComponentTypeId;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarComponentExists(id))
                    return NotFound(new { message = "Component not found." });
                else
                    throw;
            }

            return Ok(new
            {
                message = "Component is updated",
                updatedComponent = component
            });
        }

        // DELETE: api/Components/5 Odrađeno
        [HttpDelete("Delete/{id}")]
        public ActionResult DeleteCarComponent(int id)
        {
            var component = _context.CarComponents.Find(id);
            if (component == null)
            {
                return NotFound();
            }

            _context.CarComponents.Remove(component);
            _context.SaveChanges();

            return Ok($"Component with ID: {id} and name: {component.Name} was DELETED");
        }

        private bool CarComponentExists(int id)
        {
            return _context.CarComponents.Any(e => e.Id == id);
        }
        
    }
}
