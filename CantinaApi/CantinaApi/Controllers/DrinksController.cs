using Cantina.Application.Interfaces;
using Cantina.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CantinaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DrinksController : ControllerBase
    {
        private readonly IDrinkRepository _drinkRepository;

        public DrinksController(IDrinkRepository drinkRepository)
        {
            _drinkRepository = drinkRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var drinkes = await _drinkRepository.GetAllAsync();
            return Ok(drinkes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var drink = await _drinkRepository.GetByIdAsync(id);
            if (drink == null) return NotFound(new { Message = "drink not found" });

            return Ok(drink);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Drink drink)
        {
            if (string.IsNullOrWhiteSpace(drink.Name)) return BadRequest(new { Message = "drink name is required" });

            await _drinkRepository.AddAsync(drink);
            return CreatedAtAction(nameof(GetById), new { id = drink.Id }, drink);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Drink drink)
        {
            if (id != drink.Id) return BadRequest(new { Message = "ID mismatch" });

            var existingdrink = await _drinkRepository.GetByIdAsync(id);
            if (existingdrink == null) return NotFound(new { Message = "drink not found" });

            await _drinkRepository.UpdateAsync(drink);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingdrink = await _drinkRepository.GetByIdAsync(id);
            if (existingdrink == null) return NotFound(new { Message = "drink not found" });

            await _drinkRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
