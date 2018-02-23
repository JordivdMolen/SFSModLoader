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

namespace SFSML.HookSystem.HookExceptions
{
	/// <summary>
	/// Description of NotHookedException.
	/// </summary>
	public class NotHookedException : Exception
	{
		public MyInitialHook target;
		public NotHookedException(MyInitialHook tgt) : base("This hook is not registered in a MyBaseHookable")
		{
			this.target = tgt;
		}
	}
}
