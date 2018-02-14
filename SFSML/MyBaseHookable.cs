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

namespace SFSML
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
		
		protected void invokeHook(String hookName, Object[] arguments)
		{
			foreach (MyBaseHook hook in this.hooks)
			{
				hook.invokeAfterCheck(hookName,arguments);
			}
		}
	}
}
