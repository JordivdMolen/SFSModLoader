using NewBuildSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PartDatabase : MonoBehaviour
{
	public List<PartData> parts;

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
}
