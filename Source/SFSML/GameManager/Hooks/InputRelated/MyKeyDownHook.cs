using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML.GameManager.Hooks.InputRelated
{
    public class MyKeyDownHook : MyBaseHook<MyKeyDownHook>
    {
        public KeyCode keyDown;
        public bool register;
        public MyKeyDownHook(KeyCode key, bool registerKey)
        {
            this.keyDown = key;
            this.register = registerKey;
        }

        /// <summary>
        /// For event-listeners.
        /// </summary>
        public MyKeyDownHook() { }
    }
}
