using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
	public class MySceneChangedHook : MyHook
	{
		public MySceneChangedHook(Ref.SceneType old, Ref.SceneType nw)
		{
			this.oldScene = old;
			this.newScene = nw;
		}

		public Ref.SceneType oldScene;

		public Ref.SceneType newScene;
	}
}
