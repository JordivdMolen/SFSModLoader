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
			if (this.parts[i].name == partName)
			{
				return this.parts[i];
			}
		}
		MonoBehaviour.print("Could not find: " + partName);
		return null;
	}

	public List<PartData> parts;
}
