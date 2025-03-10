using Cantina.Application.Interfaces;
using Cantina.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Cantina.Infrastructure.Persistence.Repository
{
    public class DishRepository : IDishRepository
    {
        private readonly ApplicationDbContext _context;

        public DishRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dish>> GetAllAsync() => await _context.Dishes.ToListAsync();

        public async Task<Dish?> GetByIdAsync(int id) => await _context.Dishes.FindAsync(id);

        public async Task AddAsync(Dish dish)
        {
            _context.Dishes.Add(dish);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Dish dish)
        {
            _context.Dishes.Update(dish);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
            }
        }
    }
}
