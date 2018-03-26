using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.VesselHooks
{
    public class MyVesselOnDestroyHook : MyHook
    {
        public Vessel destroyed;
        public MyVesselOnDestroyHook(Vessel vessel)
        {
            this.destroyed = vessel;
        }
    }
}