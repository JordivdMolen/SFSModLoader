using NewBuildSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
    /// <summary>
    /// Called when user drags a part from the part menu and it gets created for the first time.
    /// </summary>
    public class MyPartCreatedHook : MyHook
    {
        public PlacedPart target;
        public MyPartCreatedHook(PlacedPart targetPart)
        {
            this.target = targetPart;
        }
    }
}