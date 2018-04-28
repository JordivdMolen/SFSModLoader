using System;
using NewBuildSystem;
using System.Collections.Generic;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
	public class MyVesselLoadedHook : MyHook
	{
		public MyVesselLoadedHook(List<PlacedPart> parts)
		{
            this.parts = parts;
		}

		public List<PlacedPart> parts;
	}
}
