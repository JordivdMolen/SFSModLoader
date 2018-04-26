using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.VesselHooks
{
	public class MyVesselRecovering : MyHook
	{
		public MyVesselRecovering(Vessel vessel)
		{
			this.vessel = vessel;
		}

		public Vessel vessel;
	}
}
