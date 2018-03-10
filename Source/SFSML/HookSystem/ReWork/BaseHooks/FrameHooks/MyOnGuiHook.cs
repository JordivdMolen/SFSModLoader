using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.FrameHooks
{
    public class MyOnGuiHook : MyHook
    {
        public Ref.SceneType currentScene;
        public MyOnGuiHook(Ref.SceneType scene)
        {
            this.currentScene = scene;
        }
    }
}