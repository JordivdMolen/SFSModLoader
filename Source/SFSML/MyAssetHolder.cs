using System;
using UnityEngine;

namespace SFSML
{
	public class MyAssetHolder
	{
		public MyAssetHolder(AssetBundle tgt)
		{
			this.ab = tgt;
		}

		public T getAsset<T>(string name)
		{
			bool flag = this.ab == null;
			T result;
			if (flag)
			{
				result = default(T);
			}
			else
			{
				result = (T)((object)this.ab.LoadAsset(name, typeof(T)));
			}
			return result;
		}

		public T getInstanciated<T>(string name)
		{
			bool flag = this.ab == null;
			T result;
			if (flag)
			{
				result = default(T);
			}
			else
			{
				UnityEngine.Object @object = this.ab.LoadAsset(name, typeof(T));
				bool flag2 = @object == null;
				if (flag2)
				{
					result = default(T);
				}
				else
				{
					result = (T)((object)UnityEngine.Object.Instantiate(@object));
				}
			}
			return result;
		}

		public AssetBundle ab;
	}
}
