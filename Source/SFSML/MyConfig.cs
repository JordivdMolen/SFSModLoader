using System;
using System.IO;
using SFSML.Exceptions;
using UnityEngine;

namespace SFSML
{
	public class MyConfig
	{
		public MyConfig(string path, Type baseType)
		{
			this.t = baseType;
			this.configurationPath = path;
			this.loadConfiguration(baseType);
		}

		public T getConfiguration<T>()
		{
			return (T)((object)this.configuration);
		}

		public void loadConfiguration(Type configType)
		{
			bool flag = !Directory.Exists(Path.GetDirectoryName(this.configurationPath));
			if (flag)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(this.configurationPath));
			}
			bool flag2 = !File.Exists(this.configurationPath);
			if (flag2)
			{
				object obj = Activator.CreateInstance(configType);
				bool flag3 = !(obj is IMyConfig);
				if (flag3)
				{
					throw new MyCoreException("configType is not part of a IMyConfig!", "MyConfig.cs");
				}
				IMyConfig myConfig = obj as IMyConfig;
				myConfig.SetupDefaults();
				this.configuration = myConfig;
				((IMyConfig)this.configuration).setParent(this);
				this.save();
			}
			string json = File.ReadAllText(this.configurationPath);
			this.configuration = JsonUtility.FromJson(json, configType);
			((IMyConfig)this.configuration).setParent(this);
		}

		public void save()
		{
			string contents = JsonUtility.ToJson(this.configuration, true);
			string path = this.configurationPath;
			File.WriteAllText(path, contents);
		}

		private Type t;

		private object configuration = null;

		public readonly string configurationPath;
	}
}
