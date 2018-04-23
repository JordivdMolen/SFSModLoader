using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
	public class MyPartUseHook : MyHook
	{
		public MyPartUseHook(Part targetPart)
		{
			this.used = targetPart;
		}

		public Part used;
	}
}
