using NewBuildSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
    /// <summary>
    /// Called when a user drags a part in the buildmenu.
    /// </summary>
    public class MyPartDragHook : MyHook
    {
        public PlacedPart target;
        public Vector3 pos;
        public MyPartDragHook(PlacedPart part, Vector3 newPos)
        {
            this.target = part;
            this.pos = newPos;
        }
    }
}