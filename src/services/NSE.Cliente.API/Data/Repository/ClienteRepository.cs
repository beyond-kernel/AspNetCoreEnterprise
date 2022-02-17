using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSE.Clientes.API.Models;
using NSE.Core.Data;

namespace NSE.Clientes.API.Data.Repository
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ClienteContext _context;

        public ClienteRepository(ClienteContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<IEnumerable<NSE.Clientes.API.Models.Cliente>> ObterTodos()
        {
            return await _context.Clientes.AsNoTracking().ToListAsync();
        }

        public async Task<NSE.Clientes.API.Models.Cliente> ObterPorCpf(string cpf)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.Cpf.Numero == cpf);
        }

        public void Adicionar(NSE.Clientes.API.Models.Cliente cliente)
        {
            _context.Clientes.Add(cliente);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}