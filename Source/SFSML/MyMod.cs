using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML
{
    public abstract class MyMod : MyBaseHookable
    {
        public readonly String myName;
        public readonly String myDescription;
        public readonly String myVersion;
        protected Sprite myIcon;
        private string dataPath;
        private string configPath;
        private MyConfig configurationObject;
        private Type cfgType = null;
        private MyAssetHolder assetHolder;
        public MyMod(String name, String description, string version)
        {
            this.myName = name;
            this.myDescription = description;
            this.myVersion = version;
        }
        public MyMod(String name, String description, string version, Type configurationType)
        {
            this.myName = name;
            this.myDescription = description;
            this.myVersion = version;
            this.cfgType = configurationType;
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
            if (this.dataPath == null)
            {
                this.dataPath = path;
                if (cfgType != null)
                this.configurationObject = new MyConfig(path + ".cfg",this.cfgType);
            }
        }
        public string getDataPath()
        {
            return this.dataPath;
        }
        public MyConfig config()
        {
            return configurationObject;
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
            if (this.assetHolder != null)
                return false;
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    if (Path.GetExtension(file) == ".mlasset")
                    {
                        AssetBundle ab = AssetBundle.LoadFromFile(file);
                        if (ab == null)
                        {
                            ModLoader.mainConsole.tryLogCustom("Tried to load ModLoader-Assets from " + file + ", but failed.", this.myName, LogType.Generic);
                            continue;
                        }
                        this.assetHolder = new MyAssetHolder(ab);
                        return true;
                    }
                }
                ModLoader.mainConsole.tryLogCustom("Loaded mod assets.", this.myName, LogType.Generic);
            }
            else
            {
                ModLoader.mainConsole.tryLogCustom("Mod doesn't have asset folder. Skippig asset load proccess", this.myName, LogType.Generic);
            }
            return false;
        }

        public bool fetchIcon()
        {
            Sprite texture = this.myAssets().getInstanciated<Sprite>("MyModIcon");
            if (texture != null)
            {
                this.myIcon = texture;
                return true;
            }
            return false;
        }
    }
}
