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
using SFSML.MyHookSystem.HookExceptions;

namespace SFSML.HookSystem
{
	/// <summary>
	/// Event-like system, baseclass.
	/// </summary>
	public abstract class MyBaseHook
	{
		readonly private String myHookName;
		private MyBaseHookable infested = null;
		public MyBaseHook(String hookName)
		{
			myHookName = hookName;
		}
		
		public abstract void invoke(Dictionary<String,Object> args);
		
		
		public string getMyHookName()
		{
			return this.myHookName;
		}
		
		public void invokeAfterCheck(String hookName, Dictionary<String,Object> args)
		{
			if (hookName == this.myHookName)
			{
				this.invoke(args);
			}
		}
		
		protected void unHook()
		{
			if (this.infested == null)
			{
				throw new NotHookedException(this);
			}
			
		}
	}
}
