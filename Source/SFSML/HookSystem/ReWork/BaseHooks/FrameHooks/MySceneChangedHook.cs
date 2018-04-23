using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.FrameHooks
{
	public class MySceneChangedHook : MyHook
	{
		public MySceneChangedHook(Ref.SceneType current, Ref.SceneType target)
		{
			this.oldScene = current;
			this.newScene = target;
		}

		public Ref.SceneType oldScene;

		public Ref.SceneType newScene;
	}
}
