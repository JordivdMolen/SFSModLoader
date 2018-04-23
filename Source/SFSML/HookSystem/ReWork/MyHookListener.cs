using System;
using System.Reflection;

namespace SFSML.HookSystem.ReWork
{
	public class MyHookListener
	{
		public MyHookListener(Func<MyHook, MyHook> method, Type target)
		{
			this.targetHook = target;
			this.listenMethod = method;
			MyHookSystem.listeners.Add(this);
		}

		public MyHookListener(MethodInfo listenMethod, object context)
		{
			this.targetHook = listenMethod.GetParameters()[0].ParameterType;
			Type returnType = listenMethod.ReturnType;
			bool flag = !MyHookListener.IsSubclassOfRawGeneric(typeof(MyHook), returnType);
			if (flag)
			{
				throw new Exception("Does not return a subtype of MyHook.");
			}
			this.listenMethod = ((MyHook e) => (MyHook)listenMethod.Invoke(context, new object[]
			{
				e
			}));
			MyHookSystem.listeners.Add(this);
		}

		public MyHook invokeHook(MyHook e)
		{
			return this.listenMethod(e);
		}

		private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				Type type = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				bool flag = generic == type;
				if (flag)
				{
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public readonly Type targetHook;

		public readonly Func<MyHook, MyHook> listenMethod;
	}
}
