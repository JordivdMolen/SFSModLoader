
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.MainHooks
{
    class MyGameLoadedHook : MyBaseHook<MyGameLoadedHook>
    {
        public ModLoader core;
        public MyGameLoadedHook(ModLoader coreLoader, String test)
        {
            this.core = coreLoader;
        }
    }
}
