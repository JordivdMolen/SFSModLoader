using System;

namespace ServerTransferProgram.LogicControllers
{
	public class EventListenerCollectable
	{
		public EventListenerCollectable(Type t)
		{
			this.targetType = t;
		}

		public bool lowInvoke(object o)
		{
			return this.invoked(o);
		}

		public readonly Type targetType;

		protected Func<object, bool> invoked;

		public EventManager holder;
	}
}
