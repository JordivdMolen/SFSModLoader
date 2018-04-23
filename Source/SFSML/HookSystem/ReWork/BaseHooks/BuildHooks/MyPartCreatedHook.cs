using System;
using NewBuildSystem;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
	public class MyPartCreatedHook : MyHook
	{
		public MyPartCreatedHook(PlacedPart targetPart)
		{
			this.target = targetPart;
		}

		public PlacedPart target;
	}
}
