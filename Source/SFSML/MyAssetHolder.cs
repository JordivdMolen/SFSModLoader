using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML
{
    public class MyAssetHolder
    {
        public AssetBundle ab;

        public MyAssetHolder(AssetBundle tgt)
        {
            this.ab = tgt;
        }

        public T getAsset<T>(string name)
        {
            if (this.ab == null)
                return default(T);
            return (T) (object) ab.LoadAsset(name,typeof(T));
        }

        public T getInstanciated<T>(string name)
        {
            if (this.ab == null)
                return default(T);
            UnityEngine.Object o = ab.LoadAsset(name, typeof(T));
            if (o == default(UnityEngine.Object)) return default(T);
            return (T)(object)Ref.Instantiate(o);
        }
    }

    
}
