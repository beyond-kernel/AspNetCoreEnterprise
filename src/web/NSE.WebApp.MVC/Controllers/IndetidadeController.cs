using Microsoft.AspNetCore.Mvc;
using NSE.WebApp.MVC.Models;
using NSE.WebApp.MVC.Services;

namespace NSE.WebApp.MVC.Controllers
{
    [ApiController]
    [Route("Identidade")]
    public class IndetidadeController : Controller
    {
        private readonly IAutenticacaoService _autenticacaoService;

        public IndetidadeController(IAutenticacaoService autenticacaoService)
        {
            _autenticacaoService = autenticacaoService;
        }

        [HttpGet]
        [Route("Registro")]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [Route("Registro")]
        public async Task<IActionResult> Registro([FromForm] UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid) return View(usuarioRegistro);

            //API - Registro

            var resposta = await _autenticacaoService.Registro(usuarioRegistro);


            if (false) return View(usuarioRegistro);

            //Realizar Login

            return RedirectToAction("Index", "Home");

        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromForm] UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return View(usuarioLogin);

            //API - Login

            var resposta = await _autenticacaoService.Login(usuarioLogin);

            if (false) return View(usuarioLogin);

            //Realizar Login

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("sair")]
        public async Task<IActionResult> Logout()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
