using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSE.Clientes.API.Models;

namespace NSE.Clientes.API.Controllers
{
    public class ClientesController : Controller
    {

        private readonly IClienteRepository _clienteRepository;

        public ClientesController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        [HttpGet("clientes")]
        public async Task<IEnumerable<NSE.Clientes.API.Models.Cliente>> Index()
        {
            return await _clienteRepository.ObterTodos();
        }
    }
}