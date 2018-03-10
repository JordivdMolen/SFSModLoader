using SFSML.HookSystem.ReWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
    public class MySceneChangedHook : MyHook
    {
        public Ref.SceneType oldScene;
        public Ref.SceneType newScene;
        public MySceneChangedHook(Ref.SceneType old, Ref.SceneType nw)
        {
            this.oldScene = old;
            this.newScene = nw;
        }
    }
}