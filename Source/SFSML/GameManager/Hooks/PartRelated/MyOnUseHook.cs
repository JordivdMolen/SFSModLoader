using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.PartRelated
{
    public class MyOnUseHook : MyBaseHook<MyOnUseHook>
    {
        public readonly Part eventTarget;
        public MyOnUseHook(Part target)
        {
            this.eventTarget = target;
        }

        public MyOnUseHook() { }
    }
}
