using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
    public class MyModLoadedHook : MyHook
    {
        public MyMod targetHook;
        public MyModLoadedHook(MyMod mod)
        {
            this.targetHook = mod;
        }
    }
}