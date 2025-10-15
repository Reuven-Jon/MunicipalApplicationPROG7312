using System.Collections.Generic;
using MunicipalApplicationPROG7312.Domain;

namespace MunicipalApplicationPROG7312.Persistance
{
    public interface IEventStore
    {
        IEnumerable<LocalEvent> All();
        LocalEvent? GetById(int id);
        void Add(LocalEvent e);
    }
}
