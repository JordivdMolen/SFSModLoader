using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
	public class MyPartUsedHook : MyHook
	{
		public MyPartUsedHook(Part targetPart)
		{
			this.used = targetPart;
		}

		public Part used;
	}
}
