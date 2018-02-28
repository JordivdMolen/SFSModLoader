using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML.Utility
{
    public class UnityUtil
    {
        public static GameObject FindInObject(GameObject parent, string name)
        {
            Transform[] trans = parent.GetComponentsInChildren<Transform>(true);
            foreach(Transform t in trans)
            {
                if (t.name.Equals(name))
                {
                    return t.gameObject;
                }
            }
            return null;
        }
    }
}
