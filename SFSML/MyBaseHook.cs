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

namespace SFSML
{
	/// <summary>
	/// Event-like system, baseclass.
	/// </summary>
	public abstract class MyBaseHook
	{
		readonly private String MyHookName;
		public MyBaseHook(String hookName)
		{
			MyHookName = hookName;
		}
		
		public abstract void invoke(Object[] args);
		
		public void invokeAfterCheck(String hookName, Object[] args)
		{
			if (hookName == this.MyHookName)
			{
				this.invoke(args);
			}
		}
	}
}
