/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/15/2018
 * Time: 10:35 AM
 * 
 * Using this file for commercial purposes can result
 * in violating the license!
 */
using System;
using SFSML.HookSystem;

namespace SFSML.MyHookSystem.HookExceptions
{
	/// <summary>
	/// Description of NotHookedException.
	/// </summary>
	public class NotHookedException : Exception
	{
		public MyBaseHook target;
		public NotHookedException(MyBaseHook tgt) : base("This hook is not registered in a MyBaseHookable")
		{
			this.target = tgt;
		}
	}
}
