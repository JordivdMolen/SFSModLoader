using System;
using UnityEngine;

namespace SFSML.Utility
{
	public class UnityUtil
	{
		public static GameObject FindInObject(GameObject parent, string name)
		{
			Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>(true);
			foreach (Transform transform in componentsInChildren)
			{
				bool flag = transform.name.Equals(name);
				if (flag)
				{
					return transform.gameObject;
				}
			}
			return null;
		}

		public UnityUtil()
		{
		}
	}
}
