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
using SFSML.GameManager;
using System.Collections.Generic;

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
        private Dictionary<string,MyMod> mods = new Dictionary<string, MyMod>();
        private Canvas overlayObject = null;
        public MyConsole myConsole;
        /// <summary>
        /// The main game manager to receive events and inputs.
        /// This will receive all the NON-STATIC OBJECT hooks.
        /// Things like the main entry point of the game will have their own hook manager!
        /// </summary>
        public readonly static MyGameManager manager = new MyGameManager();

        public static readonly string version = "pre-1.0.0.a-2";
        private static readonly string logTag = "ModLoader "+version;
        private GameObject overlay;
		public ModLoader()
		{
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                this.myConsole = new MyConsole();
                mainConsole = this.myConsole;
            }
            myConsole.log("Loading ModLoader Project "+version,"");
            try
            {
                AssetBundle b = AssetBundle.LoadFromFile(Application.dataPath + "/overlay.overlay1");
                GameObject o = Ref.Instantiate<GameObject>(b.LoadAsset<GameObject>("SFSML_Overlay"));
                o.SetActive(false);
                this.overlay = o;
                mainConsole.log(o.name);
            } catch (Exception e)
            {
                mainConsole.logError(e);
            }
        }

       
		
		public void startLoadProcedure()
        {
            mainConsole.log("Initiating load procedure", logTag);
            this.performDirCheck();

            this.loadPriorityMods();
            this.loadMods();
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
                mainConsole.log("Loading mod: " + modFileName, logTag);
                Assembly modAssembly = Assembly.LoadFrom(modFile);
                MyMod entryObject = null;
                bool hasTextureHolder = false;
                MyAssetHolder textureHolder = null;
                foreach (Type modType in modAssembly.GetTypes())
                {
                    object[] attributeList = modType.GetCustomAttributes(typeof(MyModEntryPoint), true);
                    if (attributeList.Length == 1)
                    {
                        entryObject = Activator.CreateInstance(modType) as MyMod;
                        foreach (FieldInfo fi in modType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (fi.FieldType.Equals(typeof(MyAssetHolder)))
                            {
                                mainConsole.log("Assigning assetHolder", logTag);
                                hasTextureHolder = true;
                                textureHolder = fi.GetValue(entryObject) as MyAssetHolder;
                            }
                        }
                    }
                }
                if (entryObject == null)
                {
                    mainConsole.log("Mod does not contain a ModLoader entry point.", "ModLoader");
                    mainConsole.log("It is possible this mod is not compatible with ModLoader.", "ModLoader");
                    return;
                }
                string infoPath = Path.Combine(Path.GetDirectoryName(modFile), Path.GetFileNameWithoutExtension(modFile));
                if (Directory.Exists(infoPath))
                {
                    string assetPath = Path.Combine(infoPath, "Assets");
                    if (Directory.Exists(assetPath) && textureHolder != null)
                    {
                        foreach (string file in Directory.GetFiles(assetPath))
                        {
                            if (Path.GetExtension(file) == ".mlasset")
                            {
                                AssetBundle ab = AssetBundle.LoadFromFile(file);
                                if (ab==null)
                                {
                                    mainConsole.log("Tried to load ModLoader-Assets from " + file + ", but failed.", logTag);
                                    continue;
                                }
                                textureHolder.ab = ab;
                            }
                        }
                        mainConsole.log("Loaded mod assets.", logTag);
                    } else
                    {
                        if (textureHolder == null)
                        {
                            mainConsole.log("Mod doesn't have textureHolder. Skipping asset load proccess", logTag);
                        } else
                        {
                            mainConsole.log("Mod doesn't have asset folder. Skippig asset load proccess", logTag);
                        }
                    }
                }
                if (mods.ContainsKey(entryObject.myName))
                {
                    mainConsole.log("Mod by the name " + entryObject.myName + " already exists!", logTag);
                    return;
                }
                mods[entryObject.myName] = entryObject;
                string dataPath = this.getMyDataDirectory() + modFileName;
                entryObject.assignDataPath(dataPath);
                entryObject.Load();
                mainConsole.log("Loaded " + entryObject.myName+".\n"+entryObject.myDescription+"\nVersion "+entryObject.myVersion, logTag);
                this.loadedMods++;
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

        public void RunUpdate()
        {

        }

        public bool isModLoaded(String name)
        {
            return this.mods.ContainsKey(name);
        }
    }
}
