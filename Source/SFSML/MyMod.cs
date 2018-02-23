using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML
{
    public abstract class MyMod : MyBaseHookable
    {
        public readonly String myName;
        public readonly String myDescription;
        public readonly String myVersion;
        private string dataPath;
        private string configPath;
        private MyConfig configurationObject;
        private Type cfgType = null;
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
    }
}
