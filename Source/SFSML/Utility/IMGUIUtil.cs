using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.Utility
{
    static class IMGUIUtil
    {
        private static int ID = int.MaxValue;

        public static int GetWindowID()
        {
            return ID--;
        }
    }
}
