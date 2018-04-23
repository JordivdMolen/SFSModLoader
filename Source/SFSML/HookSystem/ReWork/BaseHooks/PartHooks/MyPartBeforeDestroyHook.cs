using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
	public class MyPartBeforeDestroyHook : MyHook
	{
		public MyPartBeforeDestroyHook(Part targetPart)
		{
			this.part = targetPart;
		}

		public Part part;
	}
}
