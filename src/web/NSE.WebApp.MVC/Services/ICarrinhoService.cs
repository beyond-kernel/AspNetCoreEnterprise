using System;
using System.Threading.Tasks;
using NSE.WebApp.MVC.Models;
using static NSE.WebAPI.Core.Controllers.MainController;

namespace NSE.WebApp.MVC.Services
{
    public interface ICarrinhoService
    {
        Task<CarrinhoViewModel> ObterCarrinho();
        Task<WebAPI.Core.Controllers.MainController.ResponseResult> AdicionarItemCarrinho(ItemProdutoViewModel produto);
        Task<WebAPI.Core.Controllers.MainController.ResponseResult> AtualizarItemCarrinho(Guid produtoId, ItemProdutoViewModel produto);
        Task<WebAPI.Core.Controllers.MainController.ResponseResult> RemoverItemCarrinho(Guid produtoId);
    }
}