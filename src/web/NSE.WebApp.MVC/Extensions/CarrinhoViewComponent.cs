using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSE.WebApp.MVC.Models;
using NSE.WebApp.MVC.Services;

namespace NSE.WebApp.MVC.Extensions
{
    public class CarrinhoViewComponent : ViewComponent
    {
        private readonly IComprasBffService _ComprasBffService;

        public CarrinhoViewComponent(IComprasBffService ComprasBffService)
        {
            _ComprasBffService = ComprasBffService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _ComprasBffService.ObterCarrinho() ?? new CarrinhoViewModel());
        }
    }
}