using System;
using NewBuildSystem;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
	public class MyVesselLoadedHook : MyHook
	{
		public MyVesselLoadedHook(Build.BuildSave save)
		{
			this.save = save;
		}

		public Build.BuildSave save;
	}
}
