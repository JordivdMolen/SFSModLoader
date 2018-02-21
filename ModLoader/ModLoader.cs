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
using SFSML.HookSystem.MainHooks;
using SFSML.Exceptions;
using SFSML.Attributes;

namespace SFSML
{
    /// <summary>
    /// The coreclass of SFSML.
    /// </summary>
    [MyModEntryPoint]
    public class ModLoader : MyBaseHookable
	{
        public static MyConsole mainConsole;

		public int loadedMods = 0;
        private Canvas overlayObject = null;
        public MyConsole myConsole;
        
		public ModLoader()
		{
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            this.myConsole = new MyConsole();
            mainConsole = this.myConsole;
            
        }
		
		public void startLoadProcedure()
        {
            mainConsole.log("Initiating load procedure", "Core");
            this.performDirCheck();

            this.loadPriorityMods();
            this.loadMods();
        }

        public void toggleOverlay()
        {
            overlayObject.enabled = !overlayObject.enabled;
        }

        public string getMyBaseDirectory()
        {
            return Application.dataPath + "/SFSML/";
        }

        public string getMyModDirectory()
        {
            return Application.dataPath + "/SFSML/Mods/";
        }

        public string getMyDataDirectory()
        {
            return Application.dataPath + "/SFSML/Data/";
        }

        private void performDirCheck()
        {
            if (!Directory.Exists(this.getMyBaseDirectory()))
            {
                Directory.CreateDirectory(this.getMyBaseDirectory());
                mainConsole.log("Created SFSML directory.", "DirChecker");
            }
            if (!Directory.Exists(this.getMyModDirectory()))
            {
                Directory.CreateDirectory(this.getMyModDirectory());
                Directory.CreateDirectory(this.getMyModDirectory()+ "priority/");
                Directory.CreateDirectory(this.getMyModDirectory() + "normal/");
                mainConsole.log("Created Mods directory.", "DirChecker");
            }
            if (!Directory.Exists(this.getMyDataDirectory()))
            {
                Directory.CreateDirectory(this.getMyDataDirectory());
                mainConsole.log("Created Data directory.", "DirChecker");
            }

        }
        private void loadPriorityMods()
        {
            try
            {
                string[] priorityMods = Directory.GetFiles(this.getMyModDirectory() + "priority/");
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
                string[] normalMods = Directory.GetFiles(this.getMyModDirectory() + "normal/");
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
                mainConsole.log("Loading mod: " + modFileName,"SFSML");
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
                string dataPath = this.getMyDataDirectory() + modFileName;
                entryObject.assignDataPath(dataPath);
                entryObject.Load();
                mainConsole.log("Loaded " + entryObject.myName+".\n"+entryObject.myDescription+"\nVersion "+entryObject.myVersion, entryObject.myName);
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
    }
}
