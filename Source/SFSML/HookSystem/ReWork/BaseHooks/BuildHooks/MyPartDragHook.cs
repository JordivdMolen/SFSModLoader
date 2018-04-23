using System;
using NewBuildSystem;
using UnityEngine;

namespace SFSML.HookSystem.ReWork.BaseHooks.BuildHooks
{
	public class MyPartDragHook : MyHook
	{
		public MyPartDragHook(PlacedPart part, Vector3 newPos)
		{
			this.target = part;
			this.pos = newPos;
		}

		public PlacedPart target;

		public Vector3 pos;
	}
}
