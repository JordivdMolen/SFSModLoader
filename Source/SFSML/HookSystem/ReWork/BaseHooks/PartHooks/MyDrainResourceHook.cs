using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
    public class MyDrainResourceHook : MyHook
    {
        /// <summary>
        /// Can be changed to edit what amount should be removed from the tank.
        /// </summary>
        public float amountToTake;
        public float newAmount;
        public ResourceModule.Grup part;
        public MyDrainResourceHook(float take, float newA, ResourceModule.Grup targetPart)
        {
            this.amountToTake = take;
            this.newAmount = newA;
            this.part = targetPart;
        }
    }
}