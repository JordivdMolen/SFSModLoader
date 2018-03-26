using SFSML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SFSML.Translation
{
    public class MyTranslationController<T>
    {
        public readonly string translationPath;
        public readonly T translatable;
        public MyTranslationController(MyMod host) : this(host.getDataPath(),"translations")
        { 
        }
        public MyTranslationController(String purePath) : this(purePath,"translations")
        {
        }
        public MyTranslationController(MyMod host, String name) : this(host.getDataPath(), name)
        {
        }
        public MyTranslationController(String purePath, String name)
        {
            this.translationPath = Path.Combine(purePath, name+".translation");
            if (!File.Exists(this.translationPath))
            {
                this.translatable = Activator.CreateInstance<T>();
                File.WriteAllText(this.translationPath, JsonUtility.ToJson(this.translatable,true));
            }
            this.translatable = JsonUtility.FromJson<T>(File.ReadAllText(this.translationPath));
        }

        public String autoFormat(string translation, params object[] formatations)
        {
            if (translation.StartsWith("@"))
            {
                translation = translation.Substring(1);
                foreach (FieldInfo info in this.translatable.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (info.Name.Equals(translation))
                    {
                        translation = info.GetValue(this.translatable).ToString();
                    }
                }
            }
            foreach (FieldInfo info in this.translatable.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (translation.Contains("%"+info.Name+"%"))
                {
                    translation = translation.Replace("%" + info.Name + "%", info.GetValue(this.translatable).ToString());
                }
            }
            for (int x = 0; x < formatations.Length; x++)
            {
                translation = translation.Replace("{" + x.ToString() + "}", formatations[x].ToString());
            }
            return translation;
        }
    }
}