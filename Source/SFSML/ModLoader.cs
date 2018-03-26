/*
 * Created by SharpDevelop.
 * User: JordivdMolen
 * Date: 2/14/2018
 * Time: 10:06 PM
 * 
 * Using this file for commercial purposes can result
 * in violating the license!
 */
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Reflection;
using SFSML.HookSystem;
using SFSML.Exceptions;
using SFSML.Attributes;
using System.Collections.Generic;
using System.Threading;
using Assets.SFSML.Utility;
using UnityEngine.Purchasing;
using SFSML.HookSystem.ReWork;
using SFSML.HookSystem.ReWork.BaseHooks.UtilHooks;
using SFSML.Enum;
using SFSML.Translation;
using SFSML.Translation.Languages;

namespace SFSML
{
    /// <summary>
    /// The coreclass of SFSML.
    /// </summary>
    [MyModEntryPoint]
    public class ModLoader
	{
        public static MyConsole mainConsole;

		public int loadedMods = 0;
        private Dictionary<string,MyMod> mods = new Dictionary<string, MyMod>();
        private Canvas overlayObject = null;
        public MyConsole myConsole;
        /// <summary>
        /// The main game manager to receive events and inputs.
        /// This will receive all the NON-STATIC OBJECT hooks.
        /// Things like the main entry point of the game will have their own hook manager!
        /// </summary>

        public static readonly string version = "1.0.0.R2";
        private static string logTag = "ModLoader "+version;
        public static MyTranslationController<ModLoaderLang> translation;
        private GameObject overlay;
		public ModLoader()
        {
            this.performDirCheck();
            //Setup local/static fields
            translation = new MyTranslationController<ModLoaderLang>(this.getMyDataDirectory(), "ModLoaderTranslations");
            logTag = translation.autoFormat("@LogTag");
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                this.myConsole = new MyConsole();
                mainConsole = this.myConsole;
            }
            myConsole.tryLogCustom(String.Format(translation.autoFormat("@EntryMessage"),translation.autoFormat("@LoaderVersion")),logTag, LogType.Generic);

            new MyHookListener((hook) =>
            {
                MyKeyDownHook keyHook = (MyKeyDownHook)hook;
                if (keyHook.keyDown == KeyCode.F12)
                {
                    mainConsole.toggleConsole();
                }
                return hook;
            }, typeof(MyKeyDownHook));
        }

       
		
		public void startLoadProcedure()
        {
            mainConsole.tryLogCustom("Initiating load procedure", logTag, LogType.Generic);

            try
            {
                this.loadPriorityMods();
            } catch (Exception e)
            {
                this.myConsole.logError(e);
            }
            try { 
                this.loadMods();
            }
            catch (Exception e)
            {
                this.myConsole.logError(e);
            }
        }
        
        public string getMyBaseDirectory()
        {
		    return Path.Combine(Application.dataPath,"SFSML");
        }

        public string getMyModDirectory()
        {
		    return Path.Combine(Path.Combine(Application.dataPath,"SFSML"),"Mods");
        }

        public string getMyDataDirectory()
        {
		    return Path.Combine(Path.Combine(Application.dataPath,"SFSML"),"Data");
        }

        public string getMyDirectory(PathFinder path)
        {
            switch (path)
            {
                case PathFinder.MyDataDir:
                    return this.getMyDataDirectory();
                case PathFinder.MyModDir:
                    return this.getMyModDirectory();
                case PathFinder.MyNormalModDir:
					return Path.Combine(this.getMyModDirectory(), "normal");
                case PathFinder.MyPriorityModDir:
                    return Path.Combine(this.getMyModDirectory(), "priority"); 
            }
            return null;
        }

        private void performDirCheck()
        {
            if (!Directory.Exists(this.getMyBaseDirectory()))
            {
                Directory.CreateDirectory(this.getMyBaseDirectory());
                mainConsole.tryLogCustom("Created SFSML directory.", "DirChecker", LogType.Generic);
            }
            if (!Directory.Exists(this.getMyModDirectory()))
            {
                Directory.CreateDirectory(this.getMyModDirectory());
                Directory.CreateDirectory(this.getMyDirectory(PathFinder.MyNormalModDir));
                Directory.CreateDirectory(this.getMyDirectory(PathFinder.MyPriorityModDir));
                mainConsole.tryLogCustom("Created Mods directory.", "DirChecker", LogType.Generic);
            }
            if (!Directory.Exists(this.getMyDataDirectory()))
            {
                Directory.CreateDirectory(this.getMyDataDirectory());
                mainConsole.tryLogCustom("Created Data directory.", "DirChecker", LogType.Generic);
            }

        }
        private void loadPriorityMods()
        {
            try
            {
                string[] priorityMods = Directory.GetFiles(this.getMyDirectory(PathFinder.MyPriorityModDir));
                foreach (string mod in priorityMods)
                {
                    if (Path.GetExtension(mod) != ".dll") continue;
                    this.loadModFromFile(mod);
                }
            }
            catch (MyCoreException e)
            {
                e.caller = new MyCoreException.MyCaller("loadPriorityMods", "ModLoader.cs");
            }
            catch (Exception e)
            {
                mainConsole.logError(e);
            }

        }
        private void loadMods()
        {
            try
            { 
                string[] normalMods = Directory.GetFiles(this.getMyDirectory(PathFinder.MyNormalModDir));
                foreach (string mod in normalMods)
                {
                    this.loadModFromFile(mod);
                }
            }
            catch (MyCoreException e)
            {
                e.caller = new MyCoreException.MyCaller("loadMods", "ModLoader.cs");
            }
            catch (Exception e)
            {
                mainConsole.logError(e);
            }
        }
        private void loadModFromFile(String modFile)
        {
            try
            {
                if (Path.GetExtension(modFile) != ".dll") return;
                string modFileName = Path.GetFileNameWithoutExtension(modFile);
                mainConsole.log("Loading mod: " + modFileName, logTag);
                Assembly modAssembly = Assembly.LoadFrom(modFile);
                MyMod entryObject = null;
				foreach (Type modType in modAssembly.GetTypes())
                {
                    object[] attributeList = modType.GetCustomAttributes(typeof(MyModEntryPoint), true);
                    if (attributeList.Length == 1)
                    {
                        entryObject = Activator.CreateInstance(modType) as MyMod;
                    }
                }
                if (entryObject == null)
                {
                    mainConsole.tryLogCustom("Mod does not contain a ModLoader entry point.", logTag, LogType.Generic);
                    mainConsole.tryLogCustom("It is possible this mod is not compatible with ModLoader.", logTag, LogType.Generic);
                    return;
                }
                string infoPath = Path.Combine(Path.GetDirectoryName(modFile), Path.GetFileNameWithoutExtension(modFile));
                if (entryObject.LoadAssets(Path.Combine(infoPath,"Assets")))
                {
                    mainConsole.tryLogCustom("Loaded assets.", logTag, LogType.Generic);
                    if (entryObject.fetchIcon())
                    {
                        mainConsole.tryLogCustom("Loaded custom icon (Use UIE to display).", logTag, LogType.Generic);
                    }
                }
                if (mods.ContainsKey(entryObject.myName))
                {
                    mainConsole.tryLogCustom("Mod by the name " + entryObject.myName + " already exists!", logTag, LogType.Generic);
                    return;
                }
                string dataPath = Path.Combine(this.getMyDataDirectory(), modFileName);
                entryObject.assignDataPath(dataPath);
				MethodInfo[] methods = entryObject.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (MethodInfo method in methods)
                {
                    MyListenerAttribute[] att = (MyListenerAttribute[]) method.GetCustomAttributes(typeof(MyListenerAttribute), true);
                    if (att.Length == 1)
                    {
                        new HookSystem.ReWork.MyHookListener(method,entryObject);
                    }
                }
				FieldInfo[] fields = entryObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				foreach (FieldInfo field in fields)
				{
					MyHookListenerContainer[] att = (MyHookListenerContainer[]) field.GetCustomAttributes(typeof(MyHookListenerContainer),true);
					if (att.Length == 1)
					{
						MethodInfo[] fieldMethods = field.GetValue(entryObject).GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
						foreach (MethodInfo fieldMethod in fieldMethods)
						{
							new MyHookListener(fieldMethod,entryObject);
						}
					}
				}
                entryObject.Load();
                mainConsole.tryLogCustom(translation.autoFormat("@ModEntry",entryObject.myName,entryObject.myVersion,entryObject.myDescription), logTag, LogType.Generic);
                this.loadedMods++;
                mods[entryObject.myName] = entryObject;
                MyHookSystem.executeHook<MyModLoadedHook>(new MyModLoadedHook(entryObject));
            }
            catch (MyCoreException e)
            {
                e.caller = new MyCoreException.MyCaller("loadModFromFile", "ModLoader.cs");
            }
            catch (Exception e)
            {
                mainConsole.logError(e);
            }
        }
        Array codes = System.Enum.GetValues(typeof(KeyCode));
        List<KeyCode> keyDown = new List<KeyCode>();

        long lastUpdate = General.getMillis();
        public void RunUpdate()
        {
            long delta = General.getMillis() - lastUpdate;
            if (delta < 100)
                return;
            this.KeyUpdate();
            this.lastUpdate = General.getMillis();
        }
        
        private void KeyUpdate()
        {
            foreach (KeyCode code in codes)
            {
                if (Input.GetKey(code))
                {
                    if (!keyDown.Contains(code))
                    {
                        MyKeyDownHook result = MyHookSystem.executeHook<MyKeyDownHook>(new MyKeyDownHook(code));
                        if (result.isCanceled())
                        {
                            if (result.register)
                            {
                                keyDown.Add(code);
                            }
                            return;
                        }
                        keyDown.Add(code);
                    }
                }
            }
            List<KeyCode> nList = new List<KeyCode>(keyDown);
            foreach (KeyCode down in nList)
            {
                if (!Input.GetKey(down))
                {
                    MyKeyUpHook hook = new MyKeyUpHook(down);
                    MyKeyUpHook result = MyHookSystem.executeHook<MyKeyUpHook>(hook);
                    if (result.isCanceled())
                    {
                        if (result.register)
                        {
                            keyDown.Remove(down);
                        }
                        return;
                    }
                    keyDown.Remove(down);
                }
            }
        }

        public bool isModLoaded(String name)
        {
            return this.mods.ContainsKey(name);
        }

        public MyMod getMod(String name)
        {
            return this.mods[name];
        }

        public Dictionary<string, MyMod> getMods()
        {
            return new Dictionary<string, MyMod>(mods);
        }
    }
}
