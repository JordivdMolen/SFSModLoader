using System;
using System.Collections.Generic;
using NewBuildSystem;
using UnityEngine;

public class CreateRocket
{
    public static List<Part> CreateBuildParts(Vector3 createPos, Build.BuildSave toLaunch, PartDatabase partDatabase)
    {
        List<Part> list = new List<Part>();
        List<PlacedPart> list2 = Build.BuildSave.PlacedPartSave.FromSave(toLaunch.parts, partDatabase);
        for (int i = 0; i < list2.Count; i++)
        {
            if (list2[i].partData == null || list2[i].partData.prefab == null || (!Ref.hasPartsExpansion && list2[i].partData.inFull))
            {
                list2.RemoveAt(i);
                i--;
            }
            else
            {
                for (int j = i + 1; j < list2.Count; j++)
                {
                    if (PlacedPart.IsOverplaing(list2[i], list2[j]))
                    {
                        list2.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        foreach (PlacedPart placedPart in list2)
        {
            Part component = UnityEngine.Object.Instantiate<Transform>(placedPart.partData.prefab, createPos + (Vector3)placedPart.position, Quaternion.identity).GetComponent<Part>();
            component.orientation = placedPart.orientation.DeepCopy();
            Orientation.ApplyOrientation(component.transform, placedPart.orientation);
            list.Add(component);
        }
        for (int k = 0; k < list2.Count; k++)
        {
            for (int l = 0; l < list2[k].partData.attachmentSurfaces.Length; l++)
            {
                for (int m = k + 1; m < list2.Count; m++)
                {
                    for (int n = 0; n < list2[m].partData.attachmentSurfaces.Length; n++)
                    {
                        if (PlacedPart.SurfacesConnect(list2[k], list2[k].partData.attachmentSurfaces[l], list2[m], list2[m].partData.attachmentSurfaces[n]) > 0f)
                        {
                            bool fuelFlow = list2[k].partData.attachmentSurfaces[l].fuelFlow && list2[m].partData.attachmentSurfaces[n].fuelFlow;
                            new Part.Joint(list2[m].position - list2[k].position, list[k], list[m], l, n, fuelFlow);
                        }
                    }
                }
            }
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
