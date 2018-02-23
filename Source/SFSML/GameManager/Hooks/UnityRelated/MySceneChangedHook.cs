using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.GameManager.Hooks.UnityRelated
{
    public class MySceneChangedHook : MyBaseHook<MySceneChangedHook>
    {
        public Ref.SceneType oldScene;
        public Ref.SceneType targetScene;
        /// <summary>
        /// This hook is being executed on MyGameManager use ModLoader.manager as listener
        /// </summary>
        /// <param name="old"></param>
        /// <param name="tgt"></param>
        public MySceneChangedHook(Ref.SceneType old, Ref.SceneType tgt)
        {
            this.oldScene = old;
            this.targetScene = tgt;
        }

        /// <summary>
        /// Made for event-listeners only!
        /// </summary>
        public MySceneChangedHook()
        { }
    }
}
