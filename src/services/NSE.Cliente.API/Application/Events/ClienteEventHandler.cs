using MediatR;

namespace NSE.Cliente.API.Application.Events
{
    public class ClienteEventHandler : INotificationHandler<ClienteRegistradoEvent>
    {
        public Task Handle(ClienteRegistradoEvent notification, CancellationToken cancellationToken)
        {
            //TODO: Enviar evento de confirmação
            return Task.CompletedTask;
        }
    }
}
