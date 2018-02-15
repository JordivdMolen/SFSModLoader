/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/14/2018
 * Time: 9:43 PM
 * 
 * Using this file for commercial purposes can result
 * in violating the license!
 */
using System;
using System.Collections.Generic;

namespace SFSML.HookSystem
{
	/// <summary>
	/// Description of MyModCompiledHook.
	/// </summary>
	public abstract class MyModCompiledHook : MyBaseHook
	{
		public MyModCompiledHook() : base("ModCompiled")
		{
		}
		
		public override void invoke(Dictionary<String,Object> args)
		{
			Console.WriteLine(args["myModName"].ToString() + " Has been loaded!");
			this.onInvoke(args);
		}
		
		protected abstract void onInvoke(Dictionary<String,Object> args);
	}
}
