using System;
using System.IO;
using SFSML.Attributes;
using UnityEngine;

namespace SFSML
{
	public abstract class MyMod
	{
		public MyMod(string name, string description, string version)
		{
			this.myName = name;
			this.myDescription = description;
			this.myVersion = version;
		}

		public MyMod(string name, string description, string version, Type cfg)
		{
			this.myName = name;
			this.myDescription = description;
			this.myVersion = version;
			this.cfgType = cfg;
		}

		protected abstract void onLoad();

		protected abstract void onUnload();

		public void Load()
		{
			this.onLoad();
		}

		public void Unload()
		{
			this.onUnload();
		}

		public void assignDataPath(string path)
		{
			bool flag = this.dataPath == null;
			if (flag)
			{
				string path2 = Path.Combine(path, "Config.cfg");
				this.dataPath = path;
				MyConfigIdentifier[] array = (MyConfigIdentifier[])base.GetType().GetCustomAttributes(typeof(MyConfigIdentifier), true);
				bool flag2 = array.Length == 1;
				if (flag2)
				{
					this.configurationObject = new MyConfig(path2, array[0].cfgType);
					this.configurationObject.loadConfiguration(array[0].cfgType);
				}
				else
				{
					bool flag3 = this.cfgType != null;
					if (flag3)
					{
						this.configurationObject = new MyConfig(path2, this.cfgType);
						this.configurationObject.loadConfiguration(this.cfgType);
					}
				}
			}
		}

		public string getDataPath()
		{
			return this.dataPath;
		}

		public MyConfig config()
		{
			return this.configurationObject;
		}

		public Sprite getMyIcon()
		{
			return this.myIcon;
		}

		protected MyAssetHolder myAssets()
		{
			return this.assetHolder;
		}

		public bool LoadAssets(string path)
		{
			bool flag = this.assetHolder != null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = Directory.Exists(path);
				if (flag2)
				{
					foreach (string text in Directory.GetFiles(path))
					{
						bool flag3 = Path.GetExtension(text) == ".mlasset";
						if (flag3)
						{
							AssetBundle assetBundle = AssetBundle.LoadFromFile(text);
							bool flag4 = assetBundle == null;
							if (!flag4)
							{
								this.assetHolder = new MyAssetHolder(assetBundle);
								return true;
							}
							ModLoader.mainConsole.tryLogCustom("Tried to load ModLoader-Assets from " + text + ", but failed.", this.myName, LogType.Error);
						}
					}
					ModLoader.mainConsole.tryLogCustom("Loaded mod assets.", this.myName, LogType.Generic);
				}
				else
				{
					ModLoader.mainConsole.tryLogCustom(ModLoader.translation.autoFormat("@ModNoAssetFolder", new object[]
					{
						this.myName
					}), this.myName, LogType.Generic);
				}
				result = false;
			}
			return result;
		}

		public bool fetchIcon()
		{
			Sprite instanciated = this.myAssets().getInstanciated<Sprite>("MyModIcon");
			bool flag = instanciated != null;
			bool result;
			if (flag)
			{
				this.myIcon = instanciated;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public readonly string myName;

		public readonly string myDescription;

		public readonly string myVersion;

		protected Sprite myIcon;

		private string dataPath;

		private string configPath;

		private MyConfig configurationObject;

		private Type cfgType = null;

		private MyAssetHolder assetHolder;
	}
}
