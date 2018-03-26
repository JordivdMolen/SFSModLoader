using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
    public class MyPartUseHook : MyHook
    {
        public Part used;
        public MyPartUseHook(Part targetPart)
        {
            this.used = targetPart;
        }
    }
}