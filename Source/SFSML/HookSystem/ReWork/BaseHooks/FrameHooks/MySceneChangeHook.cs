using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.FrameHooks
{
	public class MySceneChangeHook : MyHook
	{
		public MySceneChangeHook(Ref.SceneType current, Ref.SceneType target)
		{
			this.oldScene = current;
			this.newScene = target;
		}

		public Ref.SceneType oldScene;

		public Ref.SceneType newScene;
	}
}
