using System;
using System.Collections.Generic;
using NewBuildSystem;
using UnityEngine;

public class PartDatabase : MonoBehaviour
{
	public PartData GetPartByName(string partName)
	{
		for (int i = 0; i < this.parts.Count; i++)
		{
			bool flag = this.parts[i].name == partName;
			if (flag)
			{
				return this.parts[i];
			}
		}
		MonoBehaviour.print("Could not find: " + partName);
		return null;
	}

	public PartDatabase()
	{
	}

	public List<PartData> parts;
}
