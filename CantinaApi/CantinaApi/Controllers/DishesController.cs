using Cantina.Application.Interfaces;
using Cantina.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CantinaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : ControllerBase
    {
        private readonly IDishRepository _dishRepository;

        public DishesController(IDishRepository dishRepository)
        {
            _dishRepository = dishRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAll()
        {
            var dishes = await _dishRepository.GetAllAsync();
            return Ok(dishes);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetById(int id)
        {
            var dish = await _dishRepository.GetByIdAsync(id);
            if (dish == null) return NotFound(new { Message = "Dish not found" });

            return Ok(dish);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Dish dish)
        {
            if (string.IsNullOrWhiteSpace(dish.Name)) return BadRequest(new { Message = "Dish name is required" });

            await _dishRepository.AddAsync(dish);
            return CreatedAtAction(nameof(GetById), new { id = dish.Id }, dish);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Update(int id, [FromBody] Dish dish)
        {
            if (id != dish.Id) return BadRequest(new { Message = "ID mismatch" });

            var existingDish = await _dishRepository.GetByIdAsync(id);
            if (existingDish == null) return NotFound(new { Message = "Dish not found" });

            await _dishRepository.UpdateAsync(dish);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingDish = await _dishRepository.GetByIdAsync(id);
            if (existingDish == null) return NotFound(new { Message = "Dish not found" });

            await _dishRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
