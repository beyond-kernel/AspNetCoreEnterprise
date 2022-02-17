using NSE.Core.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSE.Core.DomainObjects
{
    public abstract class Entity
    {
        public Guid Id { get; set; }

        private List<Event> _notificacoesDeEventos;

        public IReadOnlyCollection<Event> Notificacoes => _notificacoesDeEventos?.AsReadOnly();

        public void AdicionarEvento(Event evento)
        {
            _notificacoesDeEventos = _notificacoesDeEventos ?? new List<Event>();
            _notificacoesDeEventos.Add(evento);
        }

        public void RemoverEvento(Event evento)
        {
            _notificacoesDeEventos?.Remove(evento);
        }

        public void LimparEventos()
        {
            _notificacoesDeEventos?.Clear();
        }

        #region comparacoes
        public override bool Equals(object obj)
        {
            var compareTo = obj as Entity;

            if (ReferenceEquals(this, compareTo)) return true;
            if (ReferenceEquals(null, compareTo)) return false;

            return Id.Equals(compareTo.Id);
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 907) + Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}]";
        }
        #endregion
    }
}
