using System;

namespace SFSML.HookSystem.ReWork.BaseHooks.FrameHooks
{
	public class MyOnGuiHook : MyHook
	{
		public MyOnGuiHook(Ref.SceneType scene)
		{
			this.currentScene = scene;
		}

		public Ref.SceneType currentScene;
	}
}
