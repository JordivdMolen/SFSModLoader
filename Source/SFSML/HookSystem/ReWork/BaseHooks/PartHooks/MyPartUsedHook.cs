using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
    public class MyPartUsedHook : MyHook
    {
        public Part used;
        public MyPartUsedHook(Part targetPart)
        {
            this.used = targetPart;
        }
    }
}