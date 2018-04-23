using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.VesselHooks
{
	public class MyVesselOnDestroyHook : MyHook
	{
		public MyVesselOnDestroyHook(Vessel vessel)
		{
			this.destroyed = vessel;
		}

		public Vessel destroyed;
	}
}
