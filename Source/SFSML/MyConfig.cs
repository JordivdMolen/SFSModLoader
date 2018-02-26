using SFSML.Exceptions;
using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML
{
    public class MyConfig
    {
        Type t;
        private object configuration = null;
        public readonly string configurationPath;
        public MyConfig(string path, Type baseType)
        {
            this.t = baseType;
            this.configurationPath = path;
            this.loadConfiguration(baseType);
        }

        public T getConfiguration<T>()
        {
            return (T) this.configuration;
        }
        public void loadConfiguration(Type configType)
        {
            if (!File.Exists(this.configurationPath))
            {
                object obj = Activator.CreateInstance(configType);
                if (!(obj is IMyConfig))
                {
                    throw new MyCoreException("configType is not part of a IMyConfig!", "MyConfig.cs");
                }
                IMyConfig cfg = obj as IMyConfig;
                cfg.SetupDefaults();
                this.configuration = cfg;
                ((IMyConfig)this.configuration).setParent(this);
                this.save();
            }
            String json = File.ReadAllText(this.configurationPath);
            this.configuration = JsonUtility.FromJson(json, configType);
            ((IMyConfig)this.configuration).setParent(this);
        }

        public void save()
        {
            string json = JsonUtility.ToJson(this.configuration,true);
            string path = this.configurationPath;
            File.WriteAllText(path, json);
        }
    }

    public abstract class IMyConfig
    {
        [NonSerialized]
        private MyConfig parent = null;
        abstract public void SetupDefaults();
        public void save()
        {
            this.parent.save();
        }
        public void setParent(MyConfig par)
        {
            if (this.parent == null)
            {
                this.parent = par;
            }
        }
    }
    
}
