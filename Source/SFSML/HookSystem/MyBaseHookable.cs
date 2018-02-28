/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/14/2018
 * Time: 9:39 PM
 * 
 * Using this file for commercial purposes can result
 * in violating the license!
 */
using SFSML.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SFSML.HookSystem
{
	/// <summary>
	/// Description of MyBaseHookable.
	/// </summary>
	public class MyBaseHookable
	{
		private List<MyInitialHook> hooks = new List<MyInitialHook>();
		public MyBaseHookable()
		{	
		}
		
		public void registerListener<T>(MyBaseHook<T> e)
        {
            if (!e.isListener())
            {
                throw new MyCoreException("This hook is not a listener! @ RegisterListener","registerListener<?>()");
            }
            this.hooks.Add(e);
        }
        public void removeListener(MyInitialHook e)
        {
            this.hooks.Remove(e);
        }
        public T castHook<T>(T e)
        {
            T usedCaller = (T) ((MyInitialHook)(object)e).Clone();
            MyBaseHook<T> convertedBase = (Object) e as MyBaseHook<T>;
            Dictionary<String, FieldInfo> initialFields = convertedBase.getEventArgumets();
            List<MyInitialHook> nhooks = new List<MyInitialHook>(this.hooks);
            foreach (MyInitialHook initHook in nhooks)
            {
                if (!(initHook is T)) continue;
                T ih = (T) (object) initHook;
                MyBaseHook<T> convertedHook = (MyBaseHook<T>) initHook;
                T afterInvoke = default(T);
                bool exception = false;
                try
                {
                    afterInvoke = convertedHook.invoke(usedCaller);
                } catch (Exception ex)
                {
                    ModLoader.mainConsole.tryLogCustom(ex.Message+ex.StackTrace, "ModLoader_EventSystem", LogType.Error);
                    exception = true;
                }
                if (exception) continue;
                convertedHook = (object) afterInvoke as MyBaseHook<T>;
                Dictionary<String, FieldInfo> initHookFields = convertedHook.getEventArgumets();
                if (convertedHook.isCanceled())
                {
                    convertedBase.forceCanceled(true);
                }
                try
                {
                    foreach (String fieldName in initialFields.Keys)
                    {
                        FieldInfo orginField = initialFields[fieldName];
                        FieldInfo newField = initHookFields[fieldName];
                        object orginValue = orginField.GetValue(e);
                        object newValue = newField.GetValue(afterInvoke);
                        if (orginValue != newValue)
                        {
                            orginField.SetValue(e, newValue);
                        }
                    }
                } catch (Exception ex)
                {
                    ModLoader.mainConsole.log("FieldLoop");
                    ModLoader.mainConsole.logError(ex);
                }
            }
            return (T) (object) convertedBase;
        }
	}
}
