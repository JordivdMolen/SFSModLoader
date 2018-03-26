using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewBuildSystem;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
    public class MyRocketToLaunchpadHook : MyHook
    {
		public List<PlacedPart> rocket;
		public MyRocketToLaunchpadHook(List<PlacedPart> parts)
		{
			this.rocket = parts;
		}
    }
}