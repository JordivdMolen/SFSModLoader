/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/14/2018
 * Time: 9:39 PM
 * 
 * Using this file for commercial purposes can result
 * in violating the license!
 */
using System;
using System.Collections.Generic;

namespace SFSML.HookSystem
{
	/// <summary>
	/// Description of MyBaseHookable.
	/// </summary>
	public class MyBaseHookable
	{
		protected List<MyBaseHook> hooks = new List<MyBaseHook>();
		public MyBaseHookable()
		{	
		}
		
		protected void invokeHook(String hookName, Dictionary<String,Object> arguments)
		{
			foreach (MyBaseHook hook in this.hooks)
			{
				hook.invokeAfterCheck(hookName,arguments);
			}
		}
		
		/// <summary>
		/// This is a half-protected function it is possible to call this.
		/// However you should be careful when using this function.
		/// </summary>
		/// <param name="hn"></param>
		/// <param name="args"></param>
		public void forceInvokeHook(String hn, Dictionary<String,Object> args)
		{
			this.invokeHook(hn,args);
		}
		
		public void injectHook(MyBaseHook hook)
		{
			hooks.Add(hook);
		}
	}
}
