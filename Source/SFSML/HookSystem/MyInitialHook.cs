using SFSML.HookSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem
{
    public class MyInitialHook : ICloneable
    {
        private MyBaseHookable infested = null;
        protected Type baseType;
        protected bool cancel = false;
        public MyInitialHook()
        {
        }

        public Type getInitialType()
        {
            return this.baseType;
        }

        public bool isCanceled()
        {
            return this.cancel;
        }

        public void forceCanceled(bool state)
        {
            this.cancel = state;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
