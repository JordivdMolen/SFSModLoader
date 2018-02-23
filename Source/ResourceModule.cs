using SFSML;
using SFSML.GameManager.Hooks.ModuleRelated;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceModule : Module
{
	[Serializable]
	public class Grup
	{
		public bool isValid;

		[BoxGroup]
		public float resourceAmount;

		[BoxGroup]
		public float resourceSpace;

		[BoxGroup, Range(0f, 1f)]
		public float resourcePercent;

		[BoxGroup]
		public Resource resourceType;

		public ResourceModule[] resourceModules;

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
					if (list2[0].joints[i].fuelFlow)
					{
						Part part = (!(list2[0].joints[i].fromPart == list2[0])) ? list2[0].joints[i].fromPart : list2[0].joints[i].toPart;
						if (part.connectionCheckId != Ref.connectionCheckId)
						{
							ResourceModule resourceModule = part.GetResourceModule();
							if (!(resourceModule == null) && !(start.resourceType != resourceModule.resourceType))
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
		}

		public void TakeResource(float removeAmount)
		{
            MyResourceOnTakeHook hook = new MyResourceOnTakeHook(this, removeAmount);
            try
            {
                hook = ModLoader.manager.castHook<MyResourceOnTakeHook>(hook);
                if (hook.isCanceled()) return;
            } catch (Exception e)
            {
                ModLoader.mainConsole.logError(e);
            }
			this.resourceAmount = Mathf.Max(this.resourceAmount - hook.amount, 0f);
			this.SetTanks();
		}

		public void AddResource(float addAmount)
		{
			this.resourceAmount = Mathf.Min(this.resourceAmount + addAmount, this.resourceSpace);
			this.SetTanks();
		}

		private void SetTanks()
		{
			this.resourcePercent = this.resourceAmount / this.resourceSpace;
			for (int i = 0; i < this.resourceModules.Length; i++)
			{
				this.resourceModules[i].resourceAmount.floatValue = this.resourceModules[i].resourceSpace * this.resourcePercent;
			}
		}

		public void DestroyGrup()
		{
			this.isValid = false;
		}
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
}
