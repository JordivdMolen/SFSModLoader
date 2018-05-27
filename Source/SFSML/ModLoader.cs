using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Assets.SFSML.Utility;
using SFSML.Attributes;
using SFSML.Enum;
using SFSML.Exceptions;
using SFSML.HookSystem.ReWork;
using SFSML.HookSystem.ReWork.BaseHooks.UtilHooks;
using SFSML.IO.Storable;
using SFSML.Translation;
using SFSML.Translation.Languages;
using UnityEngine;

namespace SFSML
{
	[MyModEntryPoint]
	public class ModLoader
	{
		public ModLoader()
		{
			bool flag = Application.platform == RuntimePlatform.WindowsPlayer;
			if (flag)
			{
				this.myConsole = new MyConsole();
				ModLoader.mainConsole = this.myConsole;
			}
			this.performDirCheck();
			ModLoader.translation = new MyTranslationController<ModLoaderLang>(this.getMyDataDirectory(), "ModLoaderTranslations");
			ModLoader.logTag = ModLoader.translation.autoFormat("@LogTag", new object[0]);
			this.myConsole.tryLogCustom(ModLoader.translation.autoFormat("@EntryMessage", new object[0]), ModLoader.logTag, LogType.Generic);
			SerializableList<string> serializableList = new SerializableList<string>();
			new MyHookListener(delegate(MyHook hook)
			{
				MyKeyDownHook myKeyDownHook = (MyKeyDownHook)hook;
				bool flag2 = myKeyDownHook.keyDown == KeyCode.F12;
				if (flag2)
				{
					ModLoader.mainConsole.toggleConsole();
				}
				return hook;
			}, typeof(MyKeyDownHook));
		}

		public void startLoadProcedure()
		{
			ModLoader.mainConsole.tryLogCustom("Initiating load procedure", ModLoader.logTag, LogType.Generic);
			try
			{
				this.loadPriorityMods();
			}
			catch (Exception e)
			{
				this.myConsole.logError(e);
			}
			try
			{
				this.loadMods();
			}
			catch (Exception e2)
			{
				this.myConsole.logError(e2);
			}
		}

		public string getMyBaseDirectory()
		{
			return Path.Combine(Application.dataPath, "SFSML");
		}

		public string getMyModDirectory()
		{
			return Path.Combine(Path.Combine(Application.dataPath, "SFSML"), "Mods");
		}

		public string getMyDataDirectory()
		{
			return Path.Combine(Path.Combine(Application.dataPath, "SFSML"), "Data");
		}

		public string getMyDirectory(PathFinder path)
		{
			string result;
			switch (path)
			{
			case PathFinder.MyDataDir:
				result = this.getMyDataDirectory();
				break;
			case PathFinder.MyModDir:
				result = this.getMyModDirectory();
				break;
			case PathFinder.MyPriorityModDir:
				result = Path.Combine(this.getMyModDirectory(), "priority");
				break;
			case PathFinder.MyNormalModDir:
				result = Path.Combine(this.getMyModDirectory(), "normal");
				break;
			default:
				result = null;
				break;
			}
			return result;
		}

		private void performDirCheck()
		{
			bool flag = !Directory.Exists(this.getMyBaseDirectory());
			if (flag)
			{
				Directory.CreateDirectory(this.getMyBaseDirectory());
				ModLoader.mainConsole.tryLogCustom("Created SFSML directory.", "DirChecker", LogType.Generic);
			}
			bool flag2 = !Directory.Exists(this.getMyModDirectory());
			if (flag2)
			{
				Directory.CreateDirectory(this.getMyModDirectory());
				Directory.CreateDirectory(this.getMyDirectory(PathFinder.MyNormalModDir));
				Directory.CreateDirectory(this.getMyDirectory(PathFinder.MyPriorityModDir));
				ModLoader.mainConsole.tryLogCustom("Created Mods directory.", "DirChecker", LogType.Generic);
			}
			bool flag3 = !Directory.Exists(this.getMyDataDirectory());
			if (flag3)
			{
				Directory.CreateDirectory(this.getMyDataDirectory());
				ModLoader.mainConsole.tryLogCustom("Created Data directory.", "DirChecker", LogType.Generic);
			}
		}

		private void loadPriorityMods()
		{
			try
			{
				string[] files = Directory.GetFiles(this.getMyDirectory(PathFinder.MyPriorityModDir));
				foreach (string text in files)
				{
					bool flag = Path.GetExtension(text) != ".dll";
					if (!flag)
					{
						this.loadModFromFile(text);
					}
				}
			}
			catch (MyCoreException ex)
			{
				ex.caller = new MyCoreException.MyCaller("loadPriorityMods", "ModLoader.cs");
			}
			catch (Exception e)
			{
				ModLoader.mainConsole.logError(e);
			}
		}

		private void loadMods()
		{
			try
			{
				string[] files = Directory.GetFiles(this.getMyDirectory(PathFinder.MyNormalModDir));
				foreach (string modFile in files)
				{
					this.loadModFromFile(modFile);
				}
			}
			catch (MyCoreException ex)
			{
				ex.caller = new MyCoreException.MyCaller("loadMods", "ModLoader.cs");
			}
			catch (Exception e)
			{
				ModLoader.mainConsole.logError(e);
			}
		}

		private void loadModFromFile(string modFile)
		{
			try
			{
				bool flag = Path.GetExtension(modFile) != ".dll";
				if (!flag)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(modFile);
					ModLoader.mainConsole.log("Loading mod: " + fileNameWithoutExtension, ModLoader.logTag);
					Assembly assembly = Assembly.LoadFrom(modFile);
					MyMod myMod = null;
					foreach (Type type in assembly.GetTypes())
					{
						object[] customAttributes = type.GetCustomAttributes(typeof(MyModEntryPoint), true);
						bool flag2 = customAttributes.Length == 1;
						if (flag2)
						{
							myMod = (Activator.CreateInstance(type) as MyMod);
						}
					}
					bool flag3 = myMod == null;
					if (flag3)
					{
						ModLoader.mainConsole.tryLogCustom("Mod does not contain a ModLoader entry point.", ModLoader.logTag, LogType.Generic);
						ModLoader.mainConsole.tryLogCustom("It is possible this mod is not compatible with ModLoader.", ModLoader.logTag, LogType.Generic);
					}
					else
					{
						List<string> list = new List<string>();
						foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
						{
							list.Add(assemblyName.FullName);
						}
						ModLoader.mainConsole.tryLogCustom("Assemblies: [" + string.Join(",", list.ToArray()) + "]", ModLoader.logTag, LogType.Generic);
						string path = Path.Combine(Path.GetDirectoryName(modFile), Path.GetFileNameWithoutExtension(modFile));
						bool flag4 = myMod.LoadAssets(Path.Combine(path, "Assets"));
						if (flag4)
						{
							ModLoader.mainConsole.tryLogCustom("Loaded assets.", ModLoader.logTag, LogType.Generic);
							bool flag5 = myMod.fetchIcon();
							if (flag5)
							{
								ModLoader.mainConsole.tryLogCustom("Loaded custom icon (Use UIE to display).", ModLoader.logTag, LogType.Generic);
							}
						}
						bool flag6 = this.mods.ContainsKey(myMod.myName);
						if (flag6)
						{
							ModLoader.mainConsole.tryLogCustom("Mod by the name " + myMod.myName + " already exists!", ModLoader.logTag, LogType.Generic);
						}
						else
						{
							string path2 = Path.Combine(this.getMyDataDirectory(), fileNameWithoutExtension);
							myMod.assignDataPath(path2);
							MethodInfo[] methods = myMod.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							foreach (MethodInfo methodInfo in methods)
							{
								MyListenerAttribute[] array2 = (MyListenerAttribute[])methodInfo.GetCustomAttributes(typeof(MyListenerAttribute), true);
								bool flag7 = array2.Length == 1;
								if (flag7)
								{
									new MyHookListener(methodInfo, myMod);
								}
							}
							FieldInfo[] fields = myMod.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							foreach (FieldInfo fieldInfo in fields)
							{
								MyHookListenerContainer[] array4 = (MyHookListenerContainer[])fieldInfo.GetCustomAttributes(typeof(MyHookListenerContainer), true);
								bool flag8 = array4.Length == 1;
								if (flag8)
								{
									MethodInfo[] methods2 = fieldInfo.GetValue(myMod).GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
									foreach (MethodInfo listenMethod in methods2)
									{
										new MyHookListener(listenMethod, myMod);
									}
								}
							}
							myMod.Load();
							ModLoader.mainConsole.tryLogCustom(ModLoader.translation.autoFormat("@ModEntry", new object[]
							{
								myMod.myName,
								myMod.myVersion,
								myMod.myDescription
							}), ModLoader.logTag, LogType.Generic);
							this.loadedMods++;
							this.mods[myMod.myName] = myMod;
							MyHookSystem.executeHook<MyModLoadedHook>(new MyModLoadedHook(myMod));
						}
					}
				}
			}
			catch (MyCoreException ex)
			{
				ex.caller = new MyCoreException.MyCaller("loadModFromFile", "ModLoader.cs");
			}
			catch (Exception e)
			{
				ModLoader.mainConsole.logError(e);
			}
		}

		public void RunUpdate()
		{
			long num = General.getMillis() - this.lastUpdate;
			bool flag = num < 100L;
			if (!flag)
			{
				this.KeyUpdate();
				this.lastUpdate = General.getMillis();
			}
		}

		private void KeyUpdate()
		{
			foreach (object obj in this.codes)
			{
				KeyCode keyCode = (KeyCode)obj;
				bool key = Input.GetKey(keyCode);
				if (key)
				{
					bool flag = !this.keyDown.Contains(keyCode);
					if (flag)
					{
						MyKeyDownHook myKeyDownHook = MyHookSystem.executeHook<MyKeyDownHook>(new MyKeyDownHook(keyCode));
						bool flag2 = myKeyDownHook.isCanceled();
						if (flag2)
						{
							bool register = myKeyDownHook.register;
							if (register)
							{
								this.keyDown.Add(keyCode);
							}
							return;
						}
						this.keyDown.Add(keyCode);
					}
				}
			}
			List<KeyCode> list = new List<KeyCode>(this.keyDown);
			foreach (KeyCode keyCode2 in list)
			{
				bool flag3 = !Input.GetKey(keyCode2);
				if (flag3)
				{
					MyKeyUpHook baseHook = new MyKeyUpHook(keyCode2);
					MyKeyUpHook myKeyUpHook = MyHookSystem.executeHook<MyKeyUpHook>(baseHook);
					bool flag4 = myKeyUpHook.isCanceled();
					if (flag4)
					{
						bool register2 = myKeyUpHook.register;
						if (register2)
						{
							this.keyDown.Remove(keyCode2);
						}
						break;
					}
					this.keyDown.Remove(keyCode2);
				}
			}
		}

		public bool isModLoaded(string name)
		{
			return this.mods.ContainsKey(name);
		}

		public MyMod getMod(string name)
		{
			return this.mods[name];
		}

		public Dictionary<string, MyMod> getMods()
		{
			return new Dictionary<string, MyMod>(this.mods);
		}

		static ModLoader()
		{
			// Note: this type is marked as 'beforefieldinit'.
		}

		public static MyConsole mainConsole;

		public int loadedMods = 0;

		private Dictionary<string, MyMod> mods = new Dictionary<string, MyMod>();

		private Canvas overlayObject = null;

		public MyConsole myConsole;

		public static readonly string version = "1.3.5";

		private static string logTag = "ModLoader " + ModLoader.version;

		public static MyTranslationController<ModLoaderLang> translation;

		private GameObject overlay;

		private Array codes = System.Enum.GetValues(typeof(KeyCode));

		private List<KeyCode> keyDown = new List<KeyCode>();

		private long lastUpdate = General.getMillis();
	}
}
