using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork
{
    public class MyHook : ICloneable
    {
        private bool cancel;
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool isCanceled()
        {
            return this.cancel;
        }

        public void setCanceled(bool can)
        {
            this.cancel = can;
        }
        public T execute<T>()
        {
            return MyHookSystem.executeHook<T>((T)(object)this);
        }
        public object executeDefault()
        {
            return MyHookSystem.executeHook(this, this.GetType());
        }
    }
}