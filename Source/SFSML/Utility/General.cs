using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SFSML.Utility
{
    public class General
    {
        public static long getMillis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
