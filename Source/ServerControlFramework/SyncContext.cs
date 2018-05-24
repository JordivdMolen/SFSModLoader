using System;
using System.Threading;

namespace ServerControlFramework
{
	public class SyncContext
	{
		private static SynchronizationContext getContext()
		{
			if (SyncContext.context == null)
			{
				SyncContext.context = new SynchronizationContext();
			}
			return SyncContext.context;
		}

		public static void RunOnUI(Action toDo)
		{
			SyncContext.getContext().Post(delegate(object a)
			{
				toDo();
			}, null);
		}

		public static void RunOnUI<T>(Action<T> toDo, T arg)
		{
			SyncContext.getContext().Post(delegate(object a)
			{
				toDo(arg);
			}, null);
		}

		public static void RunOnUI<T, T1>(Action<T, T1> toDo, T arg, T1 arg2)
		{
			SyncContext.getContext().Post(delegate(object a)
			{
				toDo(arg, arg2);
			}, null);
		}

		public static void RunOnUI<T, T1, T2>(Action<T, T1, T2> toDo, T arg, T1 arg2, T2 arg3)
		{
			SyncContext.getContext().Post(delegate(object a)
			{
				toDo(arg, arg2, arg3);
			}, null);
		}

		public static void Sepperate(Action toDo)
		{
			ThreadPool.QueueUserWorkItem(delegate(object s)
			{
				toDo();
			}, null);
		}

		public static SynchronizationContext context;
	}
}
