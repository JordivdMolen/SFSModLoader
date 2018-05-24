using System;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class FairingModule : Module
{
	public override bool IsSorted()
	{
		return true;
	}

	private void Start()
	{
		UnityEngine.Object.Destroy(this.back);
		this.front.gameObject.SetActive(true);
	}

	public override void OnPartUsed()
	{
		List<Part> parts = this.part.vessel.partsManager.parts;
		List<FairingModule> list = new List<FairingModule>();
		list.Add(this);
		Ref.connectionCheckId++;
		this.part.connectionCheckId = Ref.connectionCheckId;
		List<Part> list2 = new List<Part>(1)
		{
			this.part
		};
		while (list2.Count > 0)
		{
			for (int i = 0; i < list2[0].joints.Count; i++)
			{
				Part part = (!(list2[0].joints[i].fromPart == list2[0])) ? list2[0].joints[i].fromPart : list2[0].joints[i].toPart;
				if (part.connectionCheckId != Ref.connectionCheckId)
				{
					FairingModule component = part.GetComponent<FairingModule>();
					if (!(component == null))
					{
						part.connectionCheckId = Ref.connectionCheckId;
						list2.Add(part);
						list.Add(component);
					}
				}
			}
			list2.RemoveAt(0);
		}
		for (int j = 0; j < list.Count; j++)
		{
			for (int k = 0; k < list[j].part.joints.Count; k++)
			{
				Part part2 = (!(list[j].part.joints[k].fromPart != list[j].part.joints[k].fromPart)) ? list[j].part.joints[k].fromPart : list[j].part.joints[k].toPart;
				if (!part2.HasFairingModule())
				{
					Part.DestroyJoint(list[j].part.joints[k], false);
					k--;
				}
			}
		}
		List<Part> list3 = new List<Part>();
		for (int l = 0; l < list.Count; l++)
		{
			list[l].SplitFairing(ref list3);
		}
		for (int m = 0; m < list3.Count; m++)
		{
			Vector2 relativePoint = list3[m].vessel.partsManager.rb2d.GetRelativePoint(list3[m].transform.localPosition + list3[m].centerOfMass * list3[m].orientation);
			Vector2 a = list3[m].transform.TransformVector(new Vector3(-1f, 0.25f));
			list3[m].vessel.partsManager.rb2d.AddForceAtPosition(a * list[m / 2].pushForce, relativePoint);
		}
		list3[0].UsePart();
		for (int n = 0; n < parts.Count; n++)
		{
			parts[n].UpdateDragSurfaces(parts[n].GetSurfacesCoveredAmount());
		}
	}

	public void SplitFairing(ref List<Part> partsToPush)
	{
		if (this.fairingHalfPrefab != null)
		{
			Part part = CreateRocket.CreatePart(this.fairingHalfPrefab, this.part.orientation, this.part.transform.parent, this.part.transform.localPosition);
			Part part2 = CreateRocket.CreatePart(this.fairingHalfPrefab, new Orientation(-this.part.orientation.x, this.part.orientation.y, this.part.orientation.z), this.part.transform.parent, this.part.transform.localPosition);
			part2.transform.GetChild(1).gameObject.SetActive(false);
			part2.transform.GetChild(2).gameObject.SetActive(true);
			for (int i = 0; i < this.leftJoints.Length; i++)
			{
				SeparatorModule.TransferJoints(this.part, this.leftJoints[i], part, 0);
			}
			for (int j = 0; j < this.rightJoints.Length; j++)
			{
				SeparatorModule.TransferJoints(this.part, this.rightJoints[j], part2, 0);
			}
			this.part.vessel.partsManager.parts.Add(part);
			this.part.vessel.partsManager.parts.Add(part2);
			partsToPush.Add(part);
			partsToPush.Add(part2);
		}
		this.part.DestroyPart(false, true);
	}

	[BoxGroup("Build", false, false, 0)]
	public GameObject front;

	[BoxGroup("Build", false, false, 0)]
	public GameObject back;

	[BoxGroup("Deploy", false, false, 0)]
	public Transform fairingHalfPrefab;

	[BoxGroup("Deploy", false, false, 0)]
	public float pushForce;

	[BoxGroup("Deploy", false, false, 0)]
	public int[] leftJoints;

	[BoxGroup("Deploy", false, false, 0)]
	public int[] rightJoints;
}
