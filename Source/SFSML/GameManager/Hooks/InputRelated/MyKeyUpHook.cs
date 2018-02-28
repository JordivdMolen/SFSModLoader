using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML.GameManager.Hooks.InputRelated
{
    public class MyKeyUpHook : MyBaseHook<MyKeyUpHook>
    {
        public KeyCode keyUp;
        public bool register;
        public MyKeyUpHook(KeyCode key, bool registerKey)
        {
            this.keyUp = key;
            this.register = registerKey;
        }

        /// <summary>
        /// For event-listeners.
        /// </summary>
        public MyKeyUpHook() { }
    }
}
