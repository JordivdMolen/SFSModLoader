using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.PartHooks
{
	public class MyUpdateResourceHook : MyHook
	{
		public MyUpdateResourceHook(float oldA, float newA)//, ResourceModule.Grup targetPart)
		{
			this.oldAmount = oldA;
			this.newAmount = newA;
			//this.part = targetPart;
		}

		public float oldAmount;

		public float newAmount;

		//public ResourceModule.Grup part;
	}
}
