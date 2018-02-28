using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.FrameRelated
{
    class MyGeneralOnGuiHook : MyBaseHook<MyGeneralOnGuiHook>
    {
        Ref.SceneType scene;
        public MyGeneralOnGuiHook(Ref.SceneType targetScene)
        {
            this.scene = targetScene;
        }
        public MyGeneralOnGuiHook() { }
    }
}
