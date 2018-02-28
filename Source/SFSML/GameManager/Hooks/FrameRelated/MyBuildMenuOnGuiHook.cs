using NewBuildSystem;
using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.FrameRelated
{
    public class MyBuildMenuOnGuiHook : MyBaseHook<MyBuildMenuOnGuiHook>
    {
        Build buildTarget;
        public MyBuildMenuOnGuiHook(Build target)
        {
            this.buildTarget = target;
        }
        public MyBuildMenuOnGuiHook() { }
    }
}
