using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.VesselHooks
{
	public class MyVesselLandedHook : MyHook
	{
		public MyVesselLandedHook(Vessel v, CelestialBodyData b)
		{
			this.vessel = v;
			this.body = b;
		}

		public Vessel vessel;

		public CelestialBodyData body;
	}
}
