using System;
using System.Collections.Generic;
using SFSML.HookSystem.ReWork.BaseHooks.PartHooks;
using Sirenix.OdinInspector;
using UnityEngine;

public class ResourceModule : Module
{
	public override List<object> SaveVariables
	{
		get
		{
			return new List<object>
			{
				this.resourceAmount
			};
		}
	}

	public override List<string> DescriptionVariables
	{
		get
		{
			return (!this.showParametersDescription) ? new List<string>() : new List<string>
			{
				string.Concat(new object[]
				{
					this.resourceType.resourceName,
					": ",
					this.resourceSpace,
					this.resourceType.resourceUnit
				})
			};
		}
	}

	public ResourceModule()
	{
	}

	[BoxGroup]
	public FloatValueHolder resourceAmount;

	[BoxGroup]
	public float resourceSpace;

	[BoxGroup]
	public Resource resourceType;

	[BoxGroup("B", false, false, 0)]
	public ResourceModule.Grup resourceGrup;

	public bool showParametersDescription = true;

	[Serializable]
	public class Grup
	{
		public Grup(ResourceModule start)
		{
			this.isValid = true;
			start.part.vessel.partsManager.resourceGrups.Add(this);
			this.resourceType = start.resourceType;
			List<ResourceModule> list = new List<ResourceModule>();
			Ref.connectionCheckId++;
			start.part.connectionCheckId = Ref.connectionCheckId;
			list.Add(start);
			List<Part> list2 = new List<Part>(1)
			{
				start.part
			};
			while (list2.Count > 0)
			{
				for (int i = 0; i < list2[0].joints.Count; i++)
				{
					bool fuelFlow = list2[0].joints[i].fuelFlow;
					if (fuelFlow)
					{
						Part part = (!(list2[0].joints[i].fromPart == list2[0])) ? list2[0].joints[i].fromPart : list2[0].joints[i].toPart;
						bool flag = part.connectionCheckId != Ref.connectionCheckId;
						if (flag)
						{
							ResourceModule resourceModule = part.GetResourceModule();
							bool flag2 = !(resourceModule == null) && !(start.resourceType != resourceModule.resourceType);
							if (flag2)
							{
								part.connectionCheckId = Ref.connectionCheckId;
								list2.Add(part);
								resourceModule.resourceGrup = this;
								list.Add(resourceModule);
							}
						}
					}
				}
				list2.RemoveAt(0);
			}
			this.resourceModules = list.ToArray();
			this.resourceSpace = 0f;
			this.resourceAmount = 0f;
			for (int j = 0; j < this.resourceModules.Length; j++)
			{
				this.resourceModules[j].resourceGrup = this;
				this.resourceSpace += this.resourceModules[j].resourceSpace;
				this.resourceAmount += this.resourceModules[j].resourceAmount.floatValue;
			}
			this.SetTanks();
			bool flag3 = start.part.vessel == Ref.mainVessel;
			if (flag3)
			{
				Ref.controller.RepositionFuelIcons();
			}
		}

		public bool canTakeResource
		{
			get
			{
				return this.resourceAmount > 0f && this.isValid;
			}
		}

		public bool canAddResource
		{
			get
			{
				return this.resourcePercent < 1f && this.isValid;
			}
		}

		public void TakeResource(float removeAmount)
		{
			this.resourceAmount = Mathf.Max(this.resourceAmount - removeAmount, 0f);
			this.SetTanks();
		}

		public void AddResource(float addAmount)
		{
			this.resourceAmount = Mathf.Min(this.resourceAmount + addAmount, this.resourceSpace);
			this.SetTanks();
		}

		public void ChangeResource(float add)
		{
			float newA = Mathf.Min(this.resourceAmount + add, this.resourceSpace);
			newA = Mathf.Max(this.resourceAmount - add, 0f);
			MyUpdateResourceHook myUpdateResourceHook = new MyUpdateResourceHook(this.resourceAmount, newA, this).execute<MyUpdateResourceHook>();
			bool flag = myUpdateResourceHook.isCanceled();
			if (!flag)
			{
				myUpdateResourceHook.part.resourceAmount = myUpdateResourceHook.newAmount;
			}
		}

		private void SetTanks()
		{
			this.resourcePercent = this.resourceAmount / this.resourceSpace;
			for (int i = 0; i < this.resourceModules.Length; i++)
			{
				this.resourceModules[i].resourceAmount.floatValue = this.resourceModules[i].resourceSpace * this.resourcePercent;
			}
			bool flag = this.fuelIcon != null;
			if (flag)
			{
				this.fuelIcon.GetChild(0).localScale = new Vector3(this.resourcePercent, this.fuelIcon.GetChild(0).localScale.y, 1f);
			}
		}

		public void DestroyGrup()
		{
			this.isValid = false;
		}

		public bool isValid;

		[BoxGroup]
		public float resourceAmount;

		[BoxGroup]
		public float resourceSpace;

		[BoxGroup]
		[Range(0f, 1f)]
		public float resourcePercent;

		[BoxGroup]
		public Resource resourceType;

		[BoxGroup]
		public Transform fuelIcon;

		public ResourceModule[] resourceModules;
	}
}
