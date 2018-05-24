using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using UnityEngine;

namespace Assets.Scripts.Addons.JDIS.JDISWeb.Responses
{
	public class JDISRocketResponse : MonoBehaviour
	{
		public void Awake()
		{
			CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider();
			CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromSource(new CompilerParameters
			{
				ReferencedAssemblies = 
				{
					"System.dll",
					Assembly.GetExecutingAssembly().Location
				},
				GenerateInMemory = true,
				GenerateExecutable = false
			}, new string[]
			{
				string.Empty
			});
		}
	}
}
