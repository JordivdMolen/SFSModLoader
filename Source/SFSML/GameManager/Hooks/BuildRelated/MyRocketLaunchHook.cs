using NewBuildSystem;
using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.BuildRelated
{
    class MyRocketLaunchHook : MyBaseHook<MyRocketLaunchHook>
    {
        public List<PlacedPart> parts = new List<PlacedPart>();
        public MyRocketLaunchHook(List<PlacedPart> placed)
        {
            this.parts = placed;
        }
    }
}
