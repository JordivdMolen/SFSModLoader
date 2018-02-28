using SFSML;
using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SFSML.GameManager.Hooks.ModLoader
{
    public class MyModLoadedHook : MyBaseHook<MyModLoadedHook>
    {
        public MyMod targetMod;
        public MyModLoadedHook(MyMod mod)
        {
            this.targetMod = mod;
        }

        public MyModLoadedHook() { }
    }
}
