using SFSML;
using SFSML.HookSystem.ReWork.BaseHooks.FrameHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML
{
    public class SFSMLBase : MonoBehaviour
    {
        public static ModLoader mainLoader;
        public static GameObject root;
        public static void initialize()
        {
            if (mainLoader == null)
            {
                try
                {
                    mainLoader = new ModLoader();
                } catch (Exception e)
                {
                    ModLoader.mainConsole.logError(e);
                }
                mainLoader.myConsole.tryLogCustom(ModLoader.translation.autoFormat("@ModloaderInitated","SFSMLBase"), "Base", LogType.Generic);
                mainLoader.startLoadProcedure();
                GameObject r = new GameObject();
                r.AddComponent<SFSMLBase>();
                UnityEngine.Object.DontDestroyOnLoad(r);
                r.SetActive(true);
                root = r;
            }
        }
        public static ModLoader myModLoader()
        {
            return mainLoader;
        }


        public SFSMLBase()
        {
            this.enabled = true;
        }
        public void Update()
        {
            mainLoader.RunUpdate();
        }
        public void Awake()
        {
        }

        public void OnGUI()
        {
            new MyOnGuiHook(Ref.currentScene).executeDefault();
        }
    }


}