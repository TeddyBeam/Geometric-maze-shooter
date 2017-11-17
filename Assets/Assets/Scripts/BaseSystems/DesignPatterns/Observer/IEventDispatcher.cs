using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseSystems.DesignPatterns.Observer
{
    public interface IEventDispatcher
    {
        void RegisterListener(EventsID eventID, Action<object> callback);
        void PostEvent(EventsID eventID, object param = null);
        void RemoveListener(EventsID eventID, Action<object> callback);
        void ClearAllListener();
    }
}
