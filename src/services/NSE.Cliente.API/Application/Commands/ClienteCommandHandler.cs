using FluentValidation.Results;
using MediatR;
using NSE.Cliente.API.Application.Events;
using NSE.Clientes.API.Models;
using NSE.Core.Messages;
using c = NSE.Clientes.API.Models;


namespace NSE.Cliente.API.Application.Commands
{
    public class ClienteCommandHandler : CommandHandler, IRequestHandler<RegistrarClienteCommand, ValidationResult>
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteCommandHandler(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<ValidationResult> Handle(RegistrarClienteCommand message, CancellationToken cancellationToken)
        {
            if (!message.EhValido())
                return message.ValidationResult;

            var cliente = new c.Cliente(message.Id, message.Nome, message.Email, message.Cpf);

            var clienteExiste = await _clienteRepository.ObterPorCpf(cliente.Cpf.Numero);

            if (clienteExiste != null)
            {
                AdicionarErros("CPF já está em uso");
                return ValidationResult;
            }

            _clienteRepository.Adicionar(cliente);

            cliente.AdicionarEvento(new ClienteRegistradoEvent(message.Id, message.Nome, message.Email, message.Cpf));

            return await PersistirDados(_clienteRepository.UnitOfWork);
        }

   
    }
}
