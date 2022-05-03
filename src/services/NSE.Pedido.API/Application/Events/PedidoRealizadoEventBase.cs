using NSE.MessageBus;

namespace NSE.Pedidos.API.Application.Events
{
    public class PedidoRealizadoEventBase
    {
        private readonly IMessageBus _bus;

        public Task Handle(PedidoRealizadoEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}