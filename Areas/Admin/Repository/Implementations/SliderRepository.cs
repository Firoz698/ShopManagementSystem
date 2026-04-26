using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Areas.Admin.Repository.Interfaces;
using ShopManagementSystem.Data;
using ShopManagementSystem.Generic.Repository.Implementations;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Implementations
{
    public class SliderRepository : Repository<Slider>, ISliderRepository
    {
        public SliderRepository(ApplicationDbContext db) : base(db) { }

        // SortOrder অনুযায়ী সাজানো সব slider
        public async Task<List<Slider>> GetAllOrderedAsync()
        {
            return await _db.Sliders
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        // Id দিয়ে slider খোঁজে
        public async Task<Slider?> GetByIdAsync(int id)
        {
            return await _db.Sliders.FindAsync(id);
        }
    }
}
