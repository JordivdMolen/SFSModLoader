/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/14/2018
 * Time: 8:59 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.CSharp;
using SFSML.HookSystem;

namespace SFSML
{
	/// <summary>
	/// Description of RuntimeCompiler.
	/// </summary>
	public class RuntimeCompiler : MyBaseHookable
	{
		public int compiledObjects = 0;
		private List<CompilerError> errors = new List<CompilerError>();
		private CSharpCodeProvider compiler = new CSharpCodeProvider();
		private CompilerParameters parameters = new CompilerParameters();
		public RuntimeCompiler(String[] RefAssemblies)
		{
			
			this.parameters.ReferencedAssemblies.AddRange(RefAssemblies);
			this.parameters.GenerateExecutable = false;
			this.parameters.GenerateInMemory = true;
			
		}
		
		public Assembly importCS(String targetFile)
		{
			CompilerResults res = this.compiler.CompileAssemblyFromSource(this.parameters,File.ReadAllText(targetFile));
			if (res.Errors.Count > 0)
			{
				foreach (CompilerError e in res.Errors)
				{
					this.errors.Add(e);
				}
				return null;
			}
			return res.CompiledAssembly;
		}
		
		public Assembly importDll(String targetDll)
		{
			Assembly a = null;
			try{
				a = Assembly.LoadFrom(targetDll);
			}catch(Exception e)
			{
				return null;
			}
			this.compiledObjects++;
			return a;
		}
		
	}
}
