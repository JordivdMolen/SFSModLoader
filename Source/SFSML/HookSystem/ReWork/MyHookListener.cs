using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SFSML.HookSystem.ReWork
{
    public class MyHookListener
    {
        public readonly Type targetHook;
        public readonly Func<MyHook, MyHook> listenMethod;

        /// <summary>
        /// Runtime method.
        /// </summary>
        /// <param name="method"></param>
        public MyHookListener(Func<MyHook, MyHook> method,Type target)
        {
            this.targetHook = target;
            this.listenMethod = method;
            MyHookSystem.listeners.Add(this);
        }
        
        /// <summary>
        /// Init method.
        /// </summary>
        /// <param name="listenMethod"></param>
        /// <param name="context"></param>
        public MyHookListener(MethodInfo listenMethod, object context)
        {
            this.targetHook = listenMethod.GetParameters()[0].ParameterType;
            Type retType = listenMethod.ReturnType;
            if (!IsSubclassOfRawGeneric(typeof(MyHook), retType))
            {
                throw new Exception("Does not return a subtype of MyHook.");
            }
            this.listenMethod = (e) =>
            {
                return (MyHook) listenMethod.Invoke(context, new object[] { e });
            };
            MyHookSystem.listeners.Add(this);
        }


        public MyHook invokeHook(MyHook e)
        {
            return this.listenMethod(e);
        }


        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        
    }
}