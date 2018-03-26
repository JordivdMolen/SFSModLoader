using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.VesselHooks
{
    public class MyVesselLandedHook : MyHook
    {
        public Vessel vessel;
        public CelestialBodyData body;
        public MyVesselLandedHook(Vessel v, CelestialBodyData b)
        {
            this.vessel = v;
            this.body = b;
        }
    }
}