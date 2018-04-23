using System;

namespace SFSML.HookSystem.ReWork
{
	public class MyHook : ICloneable
	{
		public object Clone()
		{
			return base.MemberwiseClone();
		}

		public bool isCanceled()
		{
			return this.cancel;
		}

		public void setCanceled(bool can)
		{
			this.cancel = can;
		}

		public T execute<T>()
		{
			return MyHookSystem.executeHook<T>((T)((object)this));
		}

		public object executeDefault()
		{
			return MyHookSystem.executeHook(this, base.GetType());
		}

		public MyHook()
		{
		}

		private bool cancel;
	}
}
