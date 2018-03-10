using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
    public class MyPartBeforeDestroyHook : MyHook
    {
        public Part part;
        public MyPartBeforeDestroyHook(Part targetPart)
        {
            this.part = targetPart;
        }
    }
}