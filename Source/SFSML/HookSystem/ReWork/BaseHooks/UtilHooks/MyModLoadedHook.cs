using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
	public class MyModLoadedHook : MyHook
	{
		public MyModLoadedHook(MyMod mod)
		{
			this.targetHook = mod;
		}

		public MyMod targetHook;
	}
}
