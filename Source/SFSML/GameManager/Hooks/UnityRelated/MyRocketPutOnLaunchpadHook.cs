using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.UnityRelated
{
    public class MyRocketPutOnLaunchpadHook : MyBaseHook<MyRocketPutOnLaunchpadHook>
    {
        public float thrust;
        public bool throttleOn;
        public MyRocketPutOnLaunchpadHook(float myThrust, bool myThrustOn)
        {
            this.thrust = myThrust;
            this.throttleOn = myThrustOn;
        }

        /// <summary>
        /// Made for event-listeners only!
        /// </summary>
        public MyRocketPutOnLaunchpadHook() { }
    }
}
