using Microsoft.AspNetCore.Mvc;
using ShopManagementSystem.Repository.Interfaces;
using ShopManagementSystem.ViewModels;

namespace ShopManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeRepository _homeRepo;

        public HomeController(IHomeRepository homeRepo)
        {
            _homeRepo = homeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                Sliders = await _homeRepo.GetActiveSlidersAsync(),
                Categories = await _homeRepo.GetActiveCategoriesAsync(8),
                Products = await _homeRepo.GetLatestProductsAsync(8)
            };
            return View(vm);
        }

        public IActionResult Error() => View();
    }
}