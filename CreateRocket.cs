using NewBuildSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CreateRocket
{
	public static List<Part> CreateBuildParts(Vector3 createPos, Build.BuildSave toLaunch, PartDatabase partDatabase)
	{
		List<Part> list = new List<Part>();
		List<PlacedPart> list2 = Build.BuildSave.PlacedPartSave.FromSave(toLaunch.parts, partDatabase);
		foreach (PlacedPart current in list2)
		{
			Part component = UnityEngine.Object.Instantiate<Transform>(current.partData.prefab, createPos + (Vector3)current.position, Quaternion.identity).GetComponent<Part>();
			component.orientation = current.orientation.DeepCopy();
			Orientation.ApplyOrientation(component.transform, current.orientation);
			list.Add(component);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			for (int j = 0; j < list2[i].partData.attachmentSurfaces.Length; j++)
			{
				for (int k = i + 1; k < list2.Count; k++)
				{
					for (int l = 0; l < list2[k].partData.attachmentSurfaces.Length; l++)
					{
						if (PlacedPart.SurfacesConnect(list2[i], list2[i].partData.attachmentSurfaces[j], list2[k], list2[k].partData.attachmentSurfaces[l]) > 0f)
						{
							bool fuelFlow = list2[i].partData.attachmentSurfaces[j].fuelFlow && list2[k].partData.attachmentSurfaces[l].fuelFlow;
							new Part.Joint(list2[k].position - list2[i].position, list[i], list[k], j, l, fuelFlow);
						}
					}
				}
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			list[m].UpdateConnected();
		}
		return list;
	}

	public static Part CreatePart(Transform prefab, Orientation orientation, Transform parent, Vector3 localPosition)
	{
		Part component = UnityEngine.Object.Instantiate<Transform>(prefab).GetComponent<Part>();
		component.transform.parent = parent;
		component.transform.localPosition = localPosition;
		component.orientation = orientation.DeepCopy();
		Orientation.ApplyOrientation(component.transform, orientation);
		return component;
	}
}
