using System;
using NewBuildSystem;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
	public class MyVesselSavedHook : MyHook
	{
		public MyVesselSavedHook(Build.BuildSave save)
		{
			this.save = save;
		}

		public Build.BuildSave save;
	}
}
