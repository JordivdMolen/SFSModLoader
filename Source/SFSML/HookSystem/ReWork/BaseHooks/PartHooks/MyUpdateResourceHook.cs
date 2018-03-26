using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
	public class MyUpdateResourceHook : MyHook
    {
        /// <summary>
        /// Can be changed to edit what amount should be removed from the tank.
        /// </summary>
        public float oldAmount;
        public float newAmount;
        public ResourceModule.Grup part;
	public MyUpdateResourceHook(float oldA, float newA, ResourceModule.Grup targetPart)
        {
	    this.oldAmount = oldA;
            this.newAmount = newA;
            this.part = targetPart;
        }
    }
}