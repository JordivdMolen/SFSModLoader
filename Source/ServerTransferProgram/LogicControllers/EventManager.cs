using System;
using System.Collections.Generic;
using ServerTransferProgram.LogicControllers.EventTypes;

namespace ServerTransferProgram.LogicControllers
{
	public class EventManager
	{
		public void RegisterListener(EventListenerCollectable listener)
		{
			this.eventListeners.Add(listener);
			listener.holder = this;
		}

		public void RemoveListener(EventListenerCollectable listener)
		{
			this.eventListeners.Remove(listener);
		}

		public T Invoke<T>(T myEvent)
		{
			if (!myEvent.GetType().IsSubclassOf(typeof(Event)))
			{
				throw new Exception("This class isn't a subclass of Event");
			}
			bool cancelled = false;
			EventListenerCollectable[] array = this.eventListeners.ToArray();
			foreach (EventListenerCollectable eventListenerCollectable in array)
			{
				if (eventListenerCollectable.targetType == myEvent.GetType())
				{
					if (!eventListenerCollectable.lowInvoke(myEvent))
					{
						cancelled = true;
						break;
					}
				}
			}
			if (myEvent.GetType().IsSubclassOf(typeof(CancellableEvent)))
			{
				((CancellableEvent)((object)myEvent)).SetCancelled(cancelled);
			}
			return myEvent;
		}

		private List<EventListenerCollectable> eventListeners = new List<EventListenerCollectable>();
	}
}
