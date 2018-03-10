using NewBuildSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
    public class MyPartStartDragHook : MyHook
    {
        public PlacedPart target;
        public Vector3 pos;
        public MyPartStartDragHook(PlacedPart part, Vector3 newPos)
        {
            this.target = part;
            this.pos = newPos;
        }
    }
}