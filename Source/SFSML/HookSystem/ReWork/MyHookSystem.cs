using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SFSML.HookSystem.ReWork
{
    public class MyHookSystem
    {
        public static List<MyHookListener> listeners = new List<MyHookListener>();
        public static MyHook executeHook(MyHook hook, Type returnType)
        {
            MyHook hookToEdit = (MyHook)hook.Clone();
            bool cancel = false;
            FieldInfo[] mainFields = hookToEdit.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            List<MyHookListener> safeCopy = new List<MyHookListener>(MyHookSystem.listeners);
            foreach (MyHookListener listener in safeCopy)
            {
                if (!listener.targetHook.Equals(returnType)) continue;
                MyHook invokeResult = listener.invokeHook(hookToEdit);
                if (invokeResult.isCanceled())
                {
                    cancel = true;
                }
            }
            hookToEdit.setCanceled(cancel);
            return hookToEdit;
        }
        public static T executeHook<T>(T baseHook)
        {
            MyHook hookToEdit = (MyHook)((MyHook)(object)baseHook).Clone();
            bool cancel = false;
            FieldInfo[] mainFields = hookToEdit.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            List<MyHookListener> safeCopy = new List<MyHookListener>(MyHookSystem.listeners);
            foreach (MyHookListener listener in safeCopy)
            {
                if (!listener.targetHook.Equals(typeof(T))) continue;
                MyHook invokeResult = listener.invokeHook(hookToEdit);
                if (invokeResult.isCanceled())
                {
                    cancel = true;
                }
            }
            hookToEdit.setCanceled(cancel);
            return (T)(object)hookToEdit;
        }
    }
}