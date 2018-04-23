using System;
using System.Collections.Generic;
using System.Reflection;

namespace SFSML.HookSystem.ReWork
{
	public class MyHookSystem
	{
		public static MyHook executeHook(MyHook hook, Type returnType)
		{
			MyHook myHook = (MyHook)hook.Clone();
			bool canceled = false;
			FieldInfo[] fields = myHook.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			List<MyHookListener> list = new List<MyHookListener>(MyHookSystem.listeners);
			foreach (MyHookListener myHookListener in list)
			{
				bool flag = !myHookListener.targetHook.Equals(returnType);
				if (!flag)
				{
					MyHook myHook2 = myHookListener.invokeHook(myHook);
					bool flag2 = myHook2.isCanceled();
					if (flag2)
					{
						canceled = true;
					}
				}
			}
			myHook.setCanceled(canceled);
			return myHook;
		}

		public static T executeHook<T>(T baseHook)
		{
			MyHook myHook = (MyHook)((MyHook)((object)baseHook)).Clone();
			bool canceled = false;
			FieldInfo[] fields = myHook.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			List<MyHookListener> list = new List<MyHookListener>(MyHookSystem.listeners);
			foreach (MyHookListener myHookListener in list)
			{
				bool flag = !myHookListener.targetHook.Equals(typeof(T));
				if (!flag)
				{
					MyHook myHook2 = myHookListener.invokeHook(myHook);
					bool flag2 = myHook2.isCanceled();
					if (flag2)
					{
						canceled = true;
					}
				}
			}
			myHook.setCanceled(canceled);
			return (T)((object)myHook);
		}

		public MyHookSystem()
		{
		}

		static MyHookSystem()
		{
			// Note: this type is marked as 'beforefieldinit'.
		}

		public static List<MyHookListener> listeners = new List<MyHookListener>();
	}
}
