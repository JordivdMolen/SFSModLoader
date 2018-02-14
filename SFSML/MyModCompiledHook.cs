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

namespace SFSML
{
	/// <summary>
	/// Description of MyModCompiledHook.
	/// </summary>
	public class MyModCompiledHook : MyBaseHook
	{
		public MyModCompiledHook() : base("ModCompiled")
		{
		}
		
		public override void invoke(object[] args)
		{
			Console.WriteLine(args[0].ToString() + " Has been loaded!");
		}
	}
}
