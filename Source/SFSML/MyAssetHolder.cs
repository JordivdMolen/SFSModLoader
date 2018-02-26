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
        public T getAsset<T>(string name)
        {
            return (T) (object) ab.LoadAsset(name);
        }

        public T getInstanciated<T>(string name)
        {
            return (T)(object)Ref.Instantiate(ab.LoadAsset(name,typeof(T)));
        }
    }

    
}
