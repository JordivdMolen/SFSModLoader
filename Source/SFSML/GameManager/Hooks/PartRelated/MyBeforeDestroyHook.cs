using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.PartRelated
{
    public class MyBeforeDestroyHook : MyBaseHook<MyBeforeDestroyHook>
    {
        public readonly Part eventTarget;
        public MyBeforeDestroyHook(Part target)
        {
            this.eventTarget = target;
        }

        public MyBeforeDestroyHook() { }
    }
}
