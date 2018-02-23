/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/14/2018
 * Time: 9:26 PM
 * 
 * Using this file for commercial purposes can result
 * in violating the license!
 */
using System;
using System.Collections.Generic;
using SFSML.HookSystem.HookExceptions;
using System.Reflection;

namespace SFSML.HookSystem
{
	/// <summary>
	/// Event-like system, baseclass.
	/// </summary>
	public abstract class MyBaseHook<T> : MyInitialHook
	{
		private MyBaseHookable infested = null;
        protected Func<T, T> onInvoke = null;
        public MyBaseHook()
		{
            this.baseType = typeof(T);
		}

        /// <summary>
        /// setOnInvoke AKA Register hook as hookListener
        /// </summary>
        /// <param name="hook">This function will be ran when the hook is casted</param>
        /// <param name="root">This object should be the object you are registering the hook on.</param>
        public void setOnInvoke(Func<T, T> hook, MyBaseHookable root)
        {
            if (this.onInvoke == null)
            {
                this.onInvoke = hook;
                root.registerListener<T>(this);
            }
            else
            {
                throw new Exception("OnInvoke is already set @ setOnInvoke");
            }
        }

        public T invoke(T e)
        {
            if (!(e is MyBaseHook<T>))
            {
                throw new Exception("event has to be an instace of MyBaseHook<T> @ Invoke");
            }
            if (!this.isListener())
            {
                throw new Exception("This hook is not a listener! @ Invoke");
            }
            MyBaseHook<T> hookT = ((object) e) as MyBaseHook<T>;
            T invokeResult = this.onInvoke(e);
            return invokeResult;
        }


        public Dictionary<String,FieldInfo> getEventArgumets()
        {
            Dictionary<String, FieldInfo> args = new Dictionary<String, FieldInfo>();
            foreach (FieldInfo fi in this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                if (fi.DeclaringType == typeof(T))
                {
                    args[fi.Name] = fi;
                }
            }
            return args;
        }

        public bool isListener()
        {
            return this.onInvoke != null;
        }
        
	}
    
}
