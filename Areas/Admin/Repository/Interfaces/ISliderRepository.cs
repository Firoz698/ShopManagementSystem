using ShopManagementSystem.Generic.Repository.Interfaces;
using ShopManagementSystem.Models;

namespace ShopManagementSystem.Areas.Admin.Repository.Interfaces
{
    public interface ISliderRepository : IRepository<Slider>
    {
        Task<List<Slider>> GetAllOrderedAsync();
        Task<Slider?> GetByIdAsync(int id);
    }
}
