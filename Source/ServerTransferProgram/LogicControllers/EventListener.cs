using System;

namespace ServerTransferProgram.LogicControllers
{
	public class EventListener<T> : EventListenerCollectable
	{
		public EventListener(Func<T, bool> listener) : base(typeof(T))
		{
			this.onInvoke = listener;
			this.invoked = delegate(object o)
			{
				T arg = (T)((object)o);
				return this.onInvoke(arg);
			};
		}

		public EventListener(Action<T> listener) : base(typeof(T))
		{
			Func<T, bool> func = delegate(T ev)
			{
				listener(ev);
				return true;
			};
			this.onInvoke = func;
			this.invoked = delegate(object o)
			{
				T arg = (T)((object)o);
				return this.onInvoke(arg);
			};
		}

		public EventListener(Action<T> listener, object nullEventSelector) : base(typeof(T))
		{
			Func<T, bool> func = delegate(T ev)
			{
				listener(ev);
				return true;
			};
			this.onInvoke = func;
			this.invoked = delegate(object o)
			{
				T arg = (T)((object)o);
				return this.onInvoke(arg);
			};
		}

		public bool Invoke(T e)
		{
			return this.onInvoke(e);
		}

		public Func<T, bool> onInvoke;
	}
}
