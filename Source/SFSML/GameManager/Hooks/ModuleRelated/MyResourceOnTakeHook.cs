using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.ModuleRelated
{
    public class MyResourceOnTakeHook : MyBaseHook<MyResourceOnTakeHook>
    {
        public readonly ResourceModule.Grup eventTarget;
        public float amount;
        public MyResourceOnTakeHook(ResourceModule.Grup target, float toRemove)
        {
            this.eventTarget = target;
            this.amount = toRemove;
        }

        public MyResourceOnTakeHook() { }
    }
}
