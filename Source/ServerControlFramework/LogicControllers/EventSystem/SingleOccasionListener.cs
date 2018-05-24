using System;
using ServerTransferProgram.LogicControllers;

namespace ServerControlFramework.LogicControllers.EventSystem
{
	public class SingleOccasionListener<T> : EventListener<T>
	{
		public SingleOccasionListener(Func<T, bool> onRun) : base(onRun)
		{
			this.onInvoke = delegate(T a)
			{
				this.holder.RemoveListener(this);
				return onRun(a);
			};
		}

		public SingleOccasionListener(Action<T> onRun) : base(onRun)
		{
			this.onInvoke = delegate(T a)
			{
				this.holder.RemoveListener(this);
				return true;
			};
		}

		public SingleOccasionListener(Action<T> onRun, object nullable) : base(onRun, null)
		{
			this.onInvoke = delegate(T a)
			{
				this.holder.RemoveListener(this);
				return true;
			};
		}
	}
}
