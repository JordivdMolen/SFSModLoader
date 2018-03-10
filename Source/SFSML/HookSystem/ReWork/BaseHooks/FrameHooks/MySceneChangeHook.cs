using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.FrameHooks
{
    public class MySceneChangeHook : MyHook
    {
        public Ref.SceneType oldScene;
        public Ref.SceneType newScene;
        public MySceneChangeHook(Ref.SceneType current, Ref.SceneType target)
        {
            this.oldScene = current;
            this.newScene = target;
        }
    }
}