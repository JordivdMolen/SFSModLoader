using System;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class PartsManager : MonoBehaviour
{
	public void ApplyThrustForce(float engineGimbal, Vessel.Throttle throttle, Vessel vessel)
	{
		bool flag = Ref.mainVessel == vessel && Ref.mainVesselTerrainHeight < 20.0 && (!throttle.throttleOn || throttle.throttleRaw == 0f);
		if (flag)
		{
			engineGimbal = 0f;
		}
		float num = 0f;
		for (int i = 0; i < this.engineModules.Count; i++)
		{
			this.engineModules[i].ApplyThrust(ref num, this.rb2d, (!this.engineModules[i].engineOn.boolValue) ? 0f : engineGimbal, throttle, Ref.mainVessel == vessel);
		}
		bool flag2 = num == 0f;
		if (!flag2)
		{
			bool flag3 = vessel == Ref.mainVessel;
			if (flag3)
			{
				Ref.controller.cameraOffset = num * 1.5E-05f * UnityEngine.Random.insideUnitCircle;
			}
		}
	}

	public void ApplyDragForce(CelestialBodyData planet, Double3 posToPlanet)
	{
		bool flag = !planet.atmosphereData.hasAtmosphere;
		if (!flag)
		{
			Double3 @double = Ref.velocityOffset + this.rb2d.velocity;
			bool flag2 = 1.0 > @double.sqrMagnitude;
			if (!flag2)
			{
				double num = posToPlanet.magnitude2d - planet.radius;
				bool flag3 = num > planet.atmosphereData.atmosphereHeightM;
				if (!flag3)
				{
					double d = Math.Exp(-(num / planet.atmosphereData.atmosphereHeightM) * planet.atmosphereData.atmoCurvePow) * planet.atmosphereData.atmoDensity;
					float d2 = 0f;
					Vector2 vector = Vector2.zero;
					float velocityAngle = Mathf.Atan2((float)@double.y, (float)@double.x);
					float rot = (this.rb2d.rotation + 90f) * 0.0174532924f;
					for (int i = 0; i < this.parts.Count; i++)
					{
						this.parts[i].GetDrag(velocityAngle, rot, ref d2, ref vector);
					}
					vector /= d2;
					bool flag4 = float.IsNaN(vector.x);
					if (!flag4)
					{
						this.rb2d.AddForceAtPosition(-(@double.normalized2d * @double.sqrMagnitude * d).toVector2 * d2, this.rb2d.GetRelativePoint(vector));
					}
				}
			}
		}
	}

	public void ApplyTorqueForce(bool controlAuthority, ref float horizontalAxis, Vessel vessel)
	{
		bool flag = this.torque <= 0f;
		if (!flag)
		{
			bool flag2 = !controlAuthority;
			if (flag2)
			{
				bool flag3 = Ref.mainVessel == vessel && Ref.inputController.horizontalAxis != 0f && Ref.controller.msgUI.color.a < 0.6f;
				if (flag3)
				{
					Ref.controller.ShowMsg("No control");
				}
				horizontalAxis = 0f;
			}
			else
			{
				float num = this.torque / this.rb2d.mass / Time.fixedDeltaTime;
				bool flag4 = Ref.mainVessel == vessel && Ref.inputController.horizontalAxis != 0f;
				if (flag4)
				{
					horizontalAxis = Ref.inputController.horizontalAxis;
					this.rb2d.angularVelocity -= num * Ref.inputController.horizontalAxis * Time.fixedDeltaTime;
				}
				else
				{
					horizontalAxis = Mathf.Clamp(this.rb2d.angularVelocity / (num * Time.fixedDeltaTime), -1f, 1f);
					this.rb2d.angularVelocity -= num * horizontalAxis * Time.fixedDeltaTime;
				}
			}
		}
	}

	public void ApplyRcs(Vessel vessel, float horizontalAxis, float rcsInputDirection, bool directionInput)
	{
		bool flag = this.rcsModules.Count == 0;
		if (flag)
		{
			vessel.RCS = false;
		}
		else
		{
			bool flag2 = !this.HasFuelSourceForRcs(vessel, false);
			if (flag2)
			{
				for (int i = 0; i < this.rcsModules.Count; i++)
				{
					this.rcsModules[i].effect.SetTargetTime(0f);
				}
				vessel.RCS = false;
			}
			else
			{
				float num = 0f;
				for (int j = 0; j < this.rcsModules.Count; j++)
				{
					this.rcsModules[j].UseRcs(this.rb2d, vessel, horizontalAxis, rcsInputDirection, directionInput, ref num);
				}
				bool flag3 = num > 0f && !Ref.infiniteFuel;
				if (flag3)
				{
					this.TakeFromAllFuelGrups(num);
				}
			}
		}
	}

	public bool HasFuelSourceForRcs(Vessel vessel, bool forEnabling)
	{
		bool flag = this.HasFuelSource();
		bool result;
		if (flag)
		{
			result = true;
		}
		else
		{
			this.CreateFuelGrup();
			bool flag2 = this.HasFuelSource();
			if (flag2)
			{
				result = true;
			}
			else
			{
				bool flag3 = vessel == Ref.mainVessel;
				if (flag3)
				{
					bool flag4 = this.resourceGrups.Count == 0;
					if (flag4)
					{
						Ref.controller.ShowMsg((!forEnabling) ? "No fuel source, disabled RCS" : "No fuel source", 2f);
					}
					else
					{
						Ref.controller.ShowMsg("Out of fuel", 2f);
					}
				}
				result = false;
			}
		}
		return result;
	}

	public void ToggleRCS(Vessel vessel, bool showMsg)
	{
		bool flag = !vessel.RCS && !this.HasFuelSourceForRcs(vessel, true);
		if (!flag)
		{
			vessel.RCS = !vessel.RCS;
			bool flag2 = !vessel.RCS;
			if (flag2)
			{
				for (int i = 0; i < this.rcsModules.Count; i++)
				{
					this.rcsModules[i].effect.SetTargetTime(0f);
				}
			}
			if (showMsg)
			{
				Ref.controller.ShowMsg("RCS " + ((!vessel.RCS) ? "Off" : "On"));
			}
		}
	}

	private bool HasFuelSource()
	{
		for (int i = 0; i < this.resourceGrups.Count; i++)
		{
			bool canTakeResource = this.resourceGrups[i].canTakeResource;
			if (canTakeResource)
			{
				return true;
			}
		}
		return false;
	}

	private void TakeFromAllFuelGrups(float removeAmount)
	{
		int num = 0;
		for (int i = 0; i < this.resourceGrups.Count; i++)
		{
			bool canTakeResource = this.resourceGrups[i].canTakeResource;
			if (canTakeResource)
			{
				num++;
			}
		}
		for (int j = 0; j < this.resourceGrups.Count; j++)
		{
			bool canTakeResource2 = this.resourceGrups[j].canTakeResource;
			if (canTakeResource2)
			{
				this.resourceGrups[j].TakeResource(removeAmount / (float)num);
			}
		}
	}

	private void CreateFuelGrup()
	{
		ResourceModule fuelTank = this.GetFuelTank();
		bool flag = fuelTank == null;
		if (!flag)
		{
			this.resourceGrups.Add(new ResourceModule.Grup(fuelTank));
		}
	}

	private ResourceModule GetFuelTank()
	{
		for (int i = 0; i < this.parts.Count; i++)
		{
			ResourceModule resourceModule = this.parts[i].GetResourceModule();
			bool flag = resourceModule != null && (this.resourceGrups.Count == 0 || resourceModule.resourceAmount.floatValue > 0f);
			if (flag)
			{
				return resourceModule;
			}
		}
		return null;
	}

	public void ReCenter()
	{
		this.rb2d.transform.parent = null;
		base.transform.position = this.rb2d.worldCenterOfMass;
		this.rb2d.transform.parent = base.transform;
	}

	public void UpdatePartsGruping(bool disablePartToPartDamage, Vessel vessel)
	{
		bool flag = this.parts.Count > 0 && this.parts[0] != null && this.parts[0].gameObject.activeSelf;
		bool flag2 = flag;
		if (flag2)
		{
			this.ResetsParts();
			List<Part> list = new List<Part>(this.parts);
			this.GetConnectedParts(this.parts[0], vessel);
			List<Part> list2 = new List<Part>();
			for (int i = 0; i < list.Count; i++)
			{
				bool flag3 = list[i].vessel == null;
				if (flag3)
				{
					list2.Add(list[i]);
				}
			}
			List<Vessel> list3 = Ref.controller.CreateVesselsFromParts(list2, this.rb2d.velocity, this.rb2d.angularVelocity, vessel.throttle, vessel.vesselAchievements);
			bool flag4 = Ref.mainVessel == vessel;
			if (flag4)
			{
				Ref.controller.RepositionFuelIcons();
			}
			if (disablePartToPartDamage)
			{
				this.DisablePartToPartDamage(0.1f);
				for (int j = 0; j < list3.Count; j++)
				{
					list3[j].partsManager.DisablePartToPartDamage(0.1f);
				}
			}
		}
		else
		{
			vessel.DisasembleVessel();
		}
	}

	private void DisablePartToPartDamage(float disableTime)
	{
		this.partToPartDamage = false;
		base.StopCoroutine("EnablePartToPartDamage");
		base.Invoke("EnablePartToPartDamage", disableTime);
	}

	private void EnablePartToPartDamage()
	{
		this.partToPartDamage = true;
	}

	public bool InsideFairing(Vector2 localPosition)
	{
		for (int i = 0; i < this.fairingModules.Count; i++)
		{
			for (int j = 0; j < this.fairingModules[i].part.partData.areas.Length; j++)
			{
				Vector2 vector = (Vector2)this.fairingModules[i].part.transform.localPosition + this.fairingModules[i].part.partData.areas[j].start * this.fairingModules[i].part.orientation;
				Vector2 vector2 = vector + this.fairingModules[i].part.partData.areas[j].size * this.fairingModules[i].part.orientation;
				bool flag = Utility.IsInsideRange(localPosition.x, vector.x, vector2.x, true) && Utility.IsInsideRange(localPosition.y, vector.y, vector2.y, true);
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetConnectedParts(Part startPart, Vessel vessel)
	{
		for (int i = 0; i < this.parts.Count; i++)
		{
			this.parts[i].vessel = null;
		}
		this.parts.Clear();
		this.rb2d.transform.SetPositionAndRotation(startPart.transform.position, Quaternion.Euler(new Vector3(0f, 0f, startPart.transform.eulerAngles.z - (float)startPart.orientation.z)));
		startPart.transform.parent = this.rb2d.transform;
		startPart.transform.localPosition = Vector3.zero;
		startPart.vessel = vessel;
		this.parts.Add(startPart);
		List<Part> list = new List<Part>(1)
		{
			startPart
		};
		while (list.Count > 0)
		{
			Orientation.ApplyOrientation(list[0].transform, list[0].orientation);
			for (int j = 0; j < list[0].joints.Count; j++)
			{
				Part part = (!(list[0].joints[j].fromPart == list[0])) ? list[0].joints[j].fromPart : list[0].joints[j].toPart;
				bool flag = part.vessel == null;
				if (flag)
				{
					list.Add(part);
					part.vessel = vessel;
					this.parts.Add(part);
					part.transform.parent = this.rb2d.transform;
					part.transform.localPosition = list[0].transform.localPosition + (Vector3)list[0].joints[j].RelativeAnchor(list[0]);
				}
			}
			list.RemoveAt(0);
		}
		this.UpdateCenterOfMass();
		this.SortModules(this.GetModules(), vessel);
	}

	public Part GetCenterPart()
	{
		for (int i = 0; i < this.parts.Count; i++)
		{
			bool flag = this.parts[i].GetComponent<ControlModule>() != null;
			if (flag)
			{
				return this.parts[i];
			}
		}
		return this.parts[0];
	}

	private List<Module> GetModules()
	{
		List<Module> list = new List<Module>();
		for (int i = 0; i < this.parts.Count; i++)
		{
			list.AddRange(this.parts[i].modules);
		}
		return list;
	}

	public void SortModules(List<Module> modules, Vessel vessel)
	{
		this.controlModules.Clear();
		this.torqueModules.Clear();
		this.engineModules.Clear();
		this.rcsModules.Clear();
		this.fairingModules.Clear();
		for (int i = 0; i < modules.Count; i++)
		{
			bool flag = modules[i] is TorqueModule;
			if (flag)
			{
				this.torqueModules.Add(modules[i] as TorqueModule);
			}
			else
			{
				bool flag2 = modules[i] is EngineModule;
				if (flag2)
				{
					this.engineModules.Add(modules[i] as EngineModule);
				}
				else
				{
					bool flag3 = modules[i] is ControlModule;
					if (flag3)
					{
						this.controlModules.Add(modules[i] as ControlModule);
					}
					else
					{
						bool flag4 = modules[i] is RcsModule;
						if (flag4)
						{
							this.rcsModules.Add(modules[i] as RcsModule);
						}
						else
						{
							bool flag5 = modules[i] is FairingModule;
							if (flag5)
							{
								this.fairingModules.Add(modules[i] as FairingModule);
							}
						}
					}
				}
			}
		}
		this.UpdateTorque();
		vessel.controlAuthority = (this.controlModules.Count > 0);
	}

	public void ResetsParts()
	{
		for (int i = 0; i < this.rcsModules.Count; i++)
		{
			this.rcsModules[i].effect.SetTargetTime(0f);
		}
		foreach (ResourceModule.Grup grup in this.resourceGrups)
		{
			grup.DestroyGrup();
		}
		this.resourceGrups = new List<ResourceModule.Grup>();
		foreach (Part part in this.parts)
		{
			bool activeSelf = part.gameObject.activeSelf;
			if (activeSelf)
			{
				part.vessel = null;
			}
		}
	}

	public void UpdateCenterOfMass()
	{
		float num = 0f;
		Vector3 a = Vector3.zero;
		for (int i = 0; i < this.parts.Count; i++)
		{
			num += this.parts[i].mass;
			a += (this.parts[i].transform.localPosition + this.parts[i].centerOfMass * this.parts[i].orientation) * this.parts[i].mass;
		}
		bool flag = float.IsNaN(num);
		if (flag)
		{
			Debug.Log(base.name + "  " + this.parts.ToString());
		}
		this.rb2d.centerOfMass = a / num;
		this.rb2d.mass = num;
	}

	public void UpdateTorque()
	{
		this.torque = 0f;
		for (int i = 0; i < this.torqueModules.Count; i++)
		{
			this.torque += this.torqueModules[i].torque.floatValue;
		}
	}

	public PartsManager()
	{
	}

	public List<Part> parts;

	public bool partToPartDamage = true;

	public Rigidbody2D rb2d;

	[BoxGroup]
	public float torque;

	[FoldoutGroup("Modules", 0)]
	public List<ControlModule> controlModules;

	[FoldoutGroup("Modules", 0)]
	public List<EngineModule> engineModules;

	[FoldoutGroup("Modules", 0)]
	public List<TorqueModule> torqueModules;

	[FoldoutGroup("Modules", 0)]
	public List<ResourceModule.Grup> resourceGrups;

	[FoldoutGroup("Modules", 0)]
	public List<RcsModule> rcsModules;

	[FoldoutGroup("Modules", 0)]
	public List<FairingModule> fairingModules;
}
