using System;
using System.Collections.Generic;
using NewBuildSystem;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
	public class MyRocketToLaunchpadHook : MyHook
	{
		public MyRocketToLaunchpadHook(List<PlacedPart> parts)
		{
			this.rocket = parts;
		}

		public List<PlacedPart> rocket;
	}
}
