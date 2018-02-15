/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/15/2018
 * Time: 10:41 AM
 * 
 * Using this file for commercial purposes can result
 * in violating the license!
 */
using System;
using SFSML.HookSystem;

namespace SFSML.MyHookSystem
{
	/// <summary>
	/// Description of MyCalledHook.
	/// </summary>
	public class MyCalledHook
	{
		MyBaseHook orgin;
		MyBaseHookable caller;
		public MyCalledHook(MyBaseHook org, MyBaseHookable call)
		{
			this.orgin = org;
			this.caller = call;
		}
		public void invoke(Object[] args)
		{
			
		}
	}
}
