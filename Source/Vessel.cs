using System;
using System.Collections.Generic;
using NewBuildSystem;
using SFSML.HookSystem.ReWork.BaseHooks.VesselHooks;
using Sirenix.OdinInspector;
using UnityEngine;

public class Vessel : MonoBehaviour
{
	public void SetThrottle(Vessel.Throttle newThrottle)
	{
		this.throttle = newThrottle;
		for (int i = 0; i < this.partsManager.engineModules.Count; i++)
		{
			this.partsManager.engineModules[i].UpdateEngineThrottle(newThrottle);
		}
		bool flag = Ref.mainVessel == this;
		if (flag)
		{
			Ref.controller.throttleOnUI.text = ((!this.throttle.throttleOn) ? "Off" : "On");
			Ref.controller.throttleBar.fillAmount = this.throttle.throttleRaw;
			Ref.controller.throttleColorMove.SetTargetTime((float)((!this.throttle.throttleOn) ? 0 : 1));
			float num = Mathf.Pow(this.throttle.throttleRaw, 1.6f) * 100f;
			bool flag2 = num > 10f;
			if (flag2)
			{
				Ref.controller.throttlePercentUI.text = ((int)num).ToString() + "%";
			}
			else
			{
				bool flag3 = num >= 0.1f;
				if (flag3)
				{
					Ref.controller.throttlePercentUI.text = ((int)num).ToString() + "." + ((int)(num % 1f * 10f)).ToString() + "%";
				}
				else
				{
					Ref.controller.throttlePercentUI.text = ((num <= 0f) ? "0%" : "0.1%");
				}
			}
		}
	}

	public static Vessel CreateVessel(Part startPart, Vector2 velocity, float angularVelocity, Vessel.Throttle throttle, List<string> vesselArchivments, Transform mapIconHolder)
	{
		Vessel component = UnityEngine.Object.Instantiate<Transform>(Ref.controller.vesselPrefab).GetComponent<Vessel>();
		Ref.controller.vessels.Add(component);
		component.partsManager.GetConnectedParts(startPart, component);
		component.partsManager.rb2d.velocity = velocity;
		component.partsManager.rb2d.angularVelocity = angularVelocity;
		component.mapIcon = UnityEngine.Object.Instantiate<Transform>(Ref.map.mapVesselIconPrefab, mapIconHolder);
		Ref.map.UpdateVesselsMapIcons();
		component.SetThrottle(throttle);
		component.vesselAchievements = new List<string>(vesselArchivments);
		return component;
	}

	private void FixedUpdate()
	{
		bool flag = this.state == Vessel.State.RealTime;
		if (flag)
		{
			this.partsManager.UpdateCenterOfMass();
			this.ApplyPhysics();
		}
	}

	private void Update()
	{
		this.CheckLoadDistance();
		Double3 @double = this.GetGlobalPosition;
		bool flag = this.state == Vessel.State.RealTime;
		if (flag)
		{
			@double = Ref.positionOffset + this.partsManager.rb2d.worldCenterOfMass;
			bool flag2 = Time.time > this.archivmentCheckTime;
			if (flag2)
			{
				this.archivmentCheckTime = Time.time + 1f;
				this.CheckForArchivments();
				this.CheckOrbitArchivments(new Orbit(@double, this.GetGlobalVelocity, Ref.controller.loadedPlanet));
			}
			bool flag3 = this.OnSurface && !this.castedHook;
			if (flag3)
			{
				new MyVesselLandedHook(this, Ref.controller.loadedPlanet.parentBody).executeDefault();
				this.castedHook = true;
			}
			else
			{
				bool flag4 = !this.OnSurface && this.castedHook;
				if (flag4)
				{
					this.castedHook = false;
				}
			}
			bool flag5 = @double.sqrMagnitude2d > Ref.controller.loadedPlanet.orbitData.SOI * Ref.controller.loadedPlanet.orbitData.SOI;
			if (flag5)
			{
				this.AddArchivment("Escaped " + Ref.controller.loadedPlanet.bodyName + " sphere of influence");
				CelestialBodyData parentBody = Ref.controller.loadedPlanet.parentBody;
				this.mapIcon.parent = Ref.map.mapRefs[parentBody].holder;
				bool flag6 = Ref.mainVessel == this;
				if (flag6)
				{
					Ref.controller.EnterTimeWarpMode();
					this.EnterNextOrbit();
					Ref.controller.ExitTimeWarpMode();
					@double = this.GetGlobalPosition;
				}
				else
				{
					this.partsManager.ReCenter();
					Double3 posIn = Ref.controller.loadedPlanet.GetPosOut(Ref.controller.globalTime) + @double;
					Double3 velIn = Ref.controller.loadedPlanet.GetVelOut(Ref.controller.globalTime) + this.GetGlobalVelocity;
					this.partsManager.rb2d.bodyType = RigidbodyType2D.Static;
					this.partsManager.rb2d.gameObject.SetActive(false);
					this.orbits = Orbit.CalculateOrbits(posIn, velIn, parentBody);
					this.state = Vessel.State.OnRailsUnloaded;
				}
			}
			foreach (CelestialBodyData celestialBodyData in Ref.controller.loadedPlanet.satellites)
			{
				Double3 double2 = @double - celestialBodyData.GetPosOut(Ref.controller.globalTime);
				bool flag7 = double2.sqrMagnitude2d < celestialBodyData.orbitData.SOI * celestialBodyData.orbitData.SOI;
				if (flag7)
				{
					this.AddArchivment("Entered " + celestialBodyData.bodyName + " sphere of influence");
					this.mapIcon.parent = Ref.map.mapRefs[celestialBodyData].holder;
					Double3 posIn2 = double2;
					Double3 velIn2 = this.GetGlobalVelocity - celestialBodyData.GetVelOut(Ref.controller.globalTime);
					bool flag8 = Ref.mainVessel == this;
					if (flag8)
					{
						Ref.controller.EnterTimeWarpMode();
						this.EnterNextOrbit();
						Ref.controller.ExitTimeWarpMode();
						@double = this.GetGlobalPosition;
					}
					else
					{
						this.partsManager.ReCenter();
						this.partsManager.rb2d.bodyType = RigidbodyType2D.Static;
						this.partsManager.rb2d.gameObject.SetActive(false);
						this.orbits = Orbit.CalculateOrbits(posIn2, velIn2, celestialBodyData);
						this.state = Vessel.State.OnRailsUnloaded;
					}
				}
			}
			bool mapView = Ref.mapView;
			if (mapView)
			{
				this.mapIcon.localPosition = (@double / 10000.0).toVector3;
				this.mapIcon.localRotation = this.partsManager.parts[0].transform.rotation;
			}
		}
		else
		{
			bool flag9 = this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded;
			if (flag9)
			{
				bool flag10 = this.orbits[0].orbitEndTime < Ref.controller.globalTime;
				if (flag10)
				{
					this.EnterNextOrbit();
					@double = this.GetGlobalPosition;
				}
				bool flag11 = this.orbits[0].calculatePassesTime != double.PositiveInfinity && Ref.controller.globalTime > this.orbits[0].calculatePassesTime - this.orbits[0]._period * 0.9 && this.orbits[0].UpdatePass();
				if (flag11)
				{
					this.orbits = Orbit.CalculateOrbits(this.orbits);
				}
				bool mapView2 = Ref.mapView;
				if (mapView2)
				{
					this.mapIcon.localPosition = (@double / 10000.0).toVector3;
				}
				else
				{
					bool flag12 = this.state == Vessel.State.OnRails;
					if (flag12)
					{
						base.transform.position = (@double - Ref.positionOffset).toVector3;
					}
				}
				bool flag13 = Ref.mainVessel != this && @double.magnitude2d - this.orbits[0].planet.radius < this.orbits[0].planet.GetTerrainSampleAtAngle(Math.Atan2(@double.y, @double.x) * 57.2958);
				if (flag13)
				{
					this.DestroyVessel();
				}
			}
		}
	}

	private void EnterNextOrbit()
	{
		bool flag = this.orbits[0].orbitType == Orbit.Type.Escape;
		if (flag)
		{
			this.AddArchivment("Escaped " + this.orbits[0].planet.bodyName + " sphere of influence.");
		}
		else
		{
			bool flag2 = this.orbits[0].orbitType == Orbit.Type.Encounter;
			if (flag2)
			{
				this.AddArchivment("Entered " + this.orbits[0].nextPlanet.bodyName + " sphere of influence.");
			}
		}
		this.orbits.RemoveAt(0);
		this.orbits = Orbit.CalculateOrbits(this.orbits);
		Double3 posOut = this.orbits[0].GetPosOut(Ref.controller.globalTime);
		this.mapIcon.parent = Ref.map.mapRefs[this.orbits[0].planet].holder;
		bool flag3 = Ref.mainVessel == this;
		if (flag3)
		{
			Ref.planetManager.SwitchLocation(this.orbits[0].planet, posOut, true, false, 0.0);
		}
	}

	private void CheckLoadDistance()
	{
		bool flag = Ref.mainVessel == null;
		if (!flag)
		{
			bool flag2 = Ref.mainVessel == this;
			if (flag2)
			{
				bool flag3 = !Ref.timeWarping && this.state == Vessel.State.OnRails;
				if (flag3)
				{
					this.SetVesselState(Vessel.ToState.ToRealTime);
					MonoBehaviour.print("Corrected");
				}
				bool flag4 = Ref.timeWarping && this.state == Vessel.State.OnRailsUnloaded;
				if (flag4)
				{
					this.SetVesselState(Vessel.ToState.ToTimewarping);
					MonoBehaviour.print("Corrected");
				}
			}
			else
			{
				bool flag5 = Ref.controller.loadedPlanet == this.GetVesselPlanet && (this.GetGlobalPosition - Ref.mainVessel.GetGlobalPosition).sqrMagnitude2d < 25000000.0;
				if (flag5)
				{
					bool flag6 = this.state == Vessel.State.OnRailsUnloaded || this.state == Vessel.State.StationaryUnloaded;
					if (flag6)
					{
						this.SetVesselState((!Ref.timeWarping) ? Vessel.ToState.ToRealTime : Vessel.ToState.ToTimewarping);
					}
				}
				else
				{
					bool flag7 = this.state == Vessel.State.RealTime || this.state == Vessel.State.OnRails || this.state == Vessel.State.Stationary;
					if (flag7)
					{
						this.SetVesselState(Vessel.ToState.ToUnloaded);
					}
				}
			}
		}
	}

	private void CheckForArchivments()
	{
		bool flag = Ref.controller.loadedPlanet.bodyName != Ref.controller.startAdress;
		if (flag)
		{
			bool onSurface = this.OnSurface;
			if (onSurface)
			{
				this.AddArchivment("Landed on " + Ref.controller.loadedPlanet.bodyName + " surface.");
			}
		}
		else
		{
			Double3 @double = Ref.positionOffset + this.partsManager.rb2d.worldCenterOfMass;
			bool flag2 = @double.magnitude2d > Ref.controller.loadedPlanet.radius + Ref.controller.loadedPlanet.atmosphereData.atmosphereHeightM;
			if (flag2)
			{
				this.AddArchivment("Passed the Karman Line, leaving the /atmosphere and reaching space.");
			}
			else
			{
				bool flag3 = @double.magnitude2d > Ref.controller.loadedPlanet.radius + 15000.0;
				if (flag3)
				{
					this.AddArchivment("Reached 15 km altitude.");
				}
				else
				{
					bool flag4 = @double.magnitude2d > Ref.controller.loadedPlanet.radius + 10000.0;
					if (flag4)
					{
						this.AddArchivment("Reached 10 km altitude.");
					}
					else
					{
						bool flag5 = @double.magnitude2d > Ref.controller.loadedPlanet.radius + 5000.0;
						if (flag5)
						{
							this.AddArchivment("Reached 5 km altitude.");
						}
					}
				}
			}
		}
	}

	private void ApplyPhysics()
	{
		CelestialBodyData loadedPlanet = Ref.controller.loadedPlanet;
		Double3 getGlobalPosition = this.GetGlobalPosition;
		double d = loadedPlanet.mass / (getGlobalPosition.x * getGlobalPosition.x + getGlobalPosition.y * getGlobalPosition.y) * (double)Time.fixedDeltaTime;
		this.partsManager.rb2d.velocity -= (getGlobalPosition.normalized2d * d).toVector2;
		bool flag = this.throttle.throttleOn && this.throttle.throttleRaw > 0f;
		if (flag)
		{
			this.partsManager.ApplyThrustForce(this.horizontalAxis, this.throttle, this);
		}
		bool flag2 = !Ref.noDrag;
		if (flag2)
		{
			this.partsManager.ApplyDragForce(loadedPlanet, getGlobalPosition);
		}
		this.partsManager.ApplyTorqueForce(this.controlAuthority, ref this.horizontalAxis, this);
		Vector2 vector = (!(this == Ref.mainVessel)) ? Vector2.zero : this.GetRcsInput();
		bool flag3 = vector.sqrMagnitude > 0f;
		bool rcs = this.RCS;
		if (rcs)
		{
			this.partsManager.ApplyRcs(this, this.horizontalAxis, (!flag3 || !this.controlAuthority) ? 0f : (Mathf.Atan2(vector.y, vector.x) * 57.29578f + Ref.cam.transform.eulerAngles.z), flag3);
		}
	}

	private Vector2 GetRcsInput()
	{
		return new Vector2(Input.GetAxisRaw("RcsX"), Input.GetAxisRaw("RcsY"));
	}

	public Double3 GetGlobalPosition
	{
		get
		{
			bool flag = this.state == Vessel.State.RealTime;
			Double3 result;
			if (flag)
			{
				result = Ref.positionOffset + this.partsManager.rb2d.worldCenterOfMass;
			}
			else
			{
				bool flag2 = (this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded) && this.orbits.Count > 0;
				if (flag2)
				{
					result = this.orbits[0].GetPosOut(Ref.controller.globalTime);
				}
				else
				{
					result = this.stationaryData.posToPlane;
				}
			}
			return result;
		}
	}

	public Double3 GetGlobalVelocity
	{
		get
		{
			bool flag = this.state == Vessel.State.RealTime;
			Double3 result;
			if (flag)
			{
				result = Ref.velocityOffset + this.partsManager.rb2d.velocity;
			}
			else
			{
				bool flag2 = (this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded) && this.orbits.Count > 0;
				if (flag2)
				{
					result = this.orbits[0].GetVelOut(Ref.controller.globalTime);
				}
				else
				{
					result = Double3.zero;
				}
			}
			return result;
		}
	}

	public CelestialBodyData GetVesselPlanet
	{
		get
		{
			bool flag = this.state == Vessel.State.RealTime;
			CelestialBodyData result;
			if (flag)
			{
				result = Ref.controller.loadedPlanet;
			}
			else
			{
				bool flag2 = this.state == Vessel.State.Stationary || this.state == Vessel.State.StationaryUnloaded;
				if (flag2)
				{
					result = this.stationaryData.planet;
				}
				else
				{
					bool flag3 = (this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded) && this.orbits.Count > 0;
					if (flag3)
					{
						result = this.orbits[0].planet;
					}
					else
					{
						result = Ref.controller.loadedPlanet;
					}
				}
			}
			return result;
		}
	}

	public void SetVesselState(Vessel.ToState toState)
	{
		bool flag = toState == Vessel.ToState.ToTimewarping;
		if (flag)
		{
			bool flag2 = this.state == Vessel.State.RealTime;
			if (flag2)
			{
				this.state = this.GoOnRails(Ref.controller.loadedPlanet, true);
				base.name = "Vessel (" + this.state.ToString() + ")";
				return;
			}
			bool flag3 = this.state == Vessel.State.OnRailsUnloaded;
			if (flag3)
			{
				this.state = Vessel.State.OnRails;
			}
			bool flag4 = this.state == Vessel.State.StationaryUnloaded;
			if (flag4)
			{
				this.state = Vessel.State.Stationary;
			}
			this.partsManager.rb2d.gameObject.SetActive(true);
		}
		bool flag5 = toState == Vessel.ToState.ToUnloaded;
		if (flag5)
		{
			bool flag6 = this.state == Vessel.State.RealTime;
			if (flag6)
			{
				this.state = this.GoOnRails(Ref.controller.loadedPlanet, true);
			}
			bool flag7 = this.state == Vessel.State.OnRails;
			if (flag7)
			{
				this.state = Vessel.State.OnRailsUnloaded;
			}
			bool flag8 = this.state == Vessel.State.Stationary;
			if (flag8)
			{
				this.state = Vessel.State.StationaryUnloaded;
			}
			this.partsManager.rb2d.gameObject.SetActive(false);
			this.partsManager.rb2d.position = Vector3.zero;
		}
		bool flag9 = toState == Vessel.ToState.ToRealTime;
		if (flag9)
		{
			bool flag10 = this.state == Vessel.State.RealTime;
			if (flag10)
			{
				return;
			}
			Ref.planetManager.totalDistanceMoved += 5000.0;
			this.partsManager.rb2d.gameObject.SetActive(true);
			this.GoOffRails();
			this.state = Vessel.State.RealTime;
		}
		base.name = "Vessel (" + this.state.ToString() + ")";
	}

	public Vessel.State GoOnRails(CelestialBodyData initialPlanet, bool reCenter)
	{
		if (reCenter)
		{
			this.partsManager.ReCenter();
		}
		Double3 getGlobalPosition = this.GetGlobalPosition;
		Double3 getGlobalVelocity = this.GetGlobalVelocity;
		this.partsManager.rb2d.bodyType = RigidbodyType2D.Static;
		foreach (EngineModule engineModule in this.partsManager.engineModules)
		{
			engineModule.nozzleMove.SetTargetTime(0f);
			engineModule.UpdateEngineThrottle(this.throttle);
		}
		this.stationaryData.posToPlane = getGlobalPosition;
		this.stationaryData.planet = initialPlanet;
		bool flag = Math.Abs(getGlobalVelocity.x) < 0.3 && Math.Abs(getGlobalVelocity.y) < 0.3;
		Vessel.State result;
		if (flag)
		{
			this.orbits.Clear();
			this.mapIcon.localPosition = (this.stationaryData.posToPlane / 10000.0).toVector3;
			result = Vessel.State.Stationary;
		}
		else
		{
			this.orbits = Orbit.CalculateOrbits(getGlobalPosition, getGlobalVelocity, initialPlanet);
			bool flag2 = double.IsNaN(this.orbits[0].meanMotion);
			if (flag2)
			{
				MonoBehaviour.print("Cannot orbit NaN, went stationary instead");
				this.orbits.Clear();
				this.mapIcon.localPosition = (this.stationaryData.posToPlane / 10000.0).toVector3;
				result = Vessel.State.Stationary;
			}
			else
			{
				this.CheckOrbitArchivments(this.orbits[0]);
				result = Vessel.State.OnRails;
			}
		}
		return result;
	}

	public void GoOffRails()
	{
		Double3 getGlobalPosition = this.GetGlobalPosition;
		Vector3 toVector = (this.GetGlobalPosition - Ref.positionOffset).toVector3;
		double num = Math.Pow(Ref.controller.loadedPlanet.radius + Ref.controller.loadedPlanet.terrainData.maxTerrainHeight + 10.0, 2.0);
		bool flag = getGlobalPosition.sqrMagnitude2d < num && Ref.velocityOffset.sqrMagnitude2d > 0.0;
		if (flag)
		{
			Ref.controller.OffsetSceneVelocity(-Ref.velocityOffset.toVector2);
		}
		Vector2 toVector2 = (this.GetGlobalVelocity - Ref.velocityOffset).toVector2;
		base.transform.position = toVector;
		this.partsManager.rb2d.bodyType = RigidbodyType2D.Dynamic;
		this.partsManager.rb2d.velocity = toVector2;
		foreach (EngineModule engineModule in this.partsManager.engineModules)
		{
			engineModule.UpdateEngineThrottle(this.throttle);
		}
		this.orbits.Clear();
		this.stationaryData.posToPlane = Double3.zero;
		this.stationaryData.planet = null;
	}

	public void DestroyVessel()
	{
		MyVesselOnDestroyHook myVesselOnDestroyHook = new MyVesselOnDestroyHook(this).execute<MyVesselOnDestroyHook>();
		bool flag = myVesselOnDestroyHook.isCanceled();
		if (!flag)
		{
			Ref.controller.vessels.Remove(this);
			this.partsManager.parts.Clear();
			bool flag2 = Ref.mainVessel == this;
			if (flag2)
			{
				Ref.mainVessel = null;
				Ref.controller.RepositionFuelIcons();
			}
			bool flag3 = Ref.mainVessel == this;
			if (flag3)
			{
				Ref.mainVessel = null;
				Ref.map.mainLines.HideAll();
			}
			bool flag4 = Ref.selectedVessel == this;
			if (flag4)
			{
				Ref.selectedVessel = null;
				Ref.map.selectedLines.HideAll();
			}
			bool flag5 = this.mapIcon != null;
			if (flag5)
			{
				UnityEngine.Object.Destroy(this.mapIcon.gameObject);
			}
			bool flag6 = base.gameObject != null;
			if (flag6)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public void MergeVessel(Vessel otherVessel)
	{
		bool flag = otherVessel == this;
		if (!flag)
		{
			bool flag2 = otherVessel == Ref.mainVessel;
			if (flag2)
			{
				Ref.mainVessel = this;
			}
			bool flag3 = otherVessel == Ref.selectedVessel;
			if (flag3)
			{
				Ref.selectedVessel = this;
			}
			this.partsManager.parts.AddRange(otherVessel.partsManager.parts);
			this.partsManager.GetConnectedParts(this.partsManager.parts[0], this);
			for (int i = 0; i < otherVessel.vesselAchievements.Count; i++)
			{
				this.AddArchivment(otherVessel.vesselAchievements[i]);
			}
			otherVessel.DestroyVessel();
		}
	}

	public void DisasembleVessel()
	{
		Ref.controller.vessels.Remove(this);
		this.partsManager.ResetsParts();
		List<Vessel> list = Ref.controller.CreateVesselsFromParts(this.partsManager.parts, this.partsManager.rb2d.velocity, this.partsManager.rb2d.angularVelocity, this.throttle, this.vesselAchievements);
		bool flag = Ref.mainVessel == this && list.Count > 0;
		if (flag)
		{
			Ref.controller.SwitchVessel(list[0]);
		}
		this.DestroyVessel();
	}

	public bool OnSurface
	{
		get
		{
			bool flag = this.state == Vessel.State.Stationary || this.state == Vessel.State.StationaryUnloaded;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = this.partsManager.rb2d == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					Collider2D[] array = new Collider2D[5];
					this.partsManager.rb2d.GetContacts(array);
					foreach (Collider2D collider2D in array)
					{
						bool flag3 = collider2D != null && collider2D.gameObject.layer == LayerMask.NameToLayer("Celestial Body");
						if (flag3)
						{
							return true;
						}
					}
					result = false;
				}
			}
			return result;
		}
	}

	public void AddArchivment(string newArchivment)
	{
		for (int i = 0; i < this.vesselAchievements.Count; i++)
		{
			bool flag = this.vesselAchievements[i] == newArchivment;
			if (flag)
			{
				return;
			}
		}
		this.vesselAchievements.Add(newArchivment);
		bool flag2 = Ref.mainVessel != this;
		if (flag2)
		{
			return;
		}
		this.CheckGooglePlayAchievements(newArchivment);
		bool flag3 = newArchivment == "Reached 5 km altitude.";
		if (flag3)
		{
			return;
		}
		bool flag4 = newArchivment == "Reached 10 km altitude.";
		if (flag4)
		{
			return;
		}
		bool flag5 = newArchivment == "Reached 15 km altitude.";
		if (flag5)
		{
			return;
		}
		bool flag6 = newArchivment == "Passed the Karman Line, leaving the /atmosphere and reaching space.";
		if (flag6)
		{
			newArchivment = "Passed the Karman Line, leaving the atmosphere and reaching space.";
			bool flag7 = !Saving.LoadSetting(Saving.SettingKey.seenMapInstructions);
			if (flag7)
			{
				bool flag8 = !Ref.mapView;
				if (flag8)
				{
					Ref.inputController.instructionsMap.SetActive(true);
					Ref.inputController.CloseDropdownMenu();
				}
				Saving.SaveSetting(Saving.SettingKey.seenMapInstructions, true);
				return;
			}
		}
		bool flag9 = newArchivment == "Reached low Earth orbit." && !Saving.LoadSetting(Saving.SettingKey.seenTimewarpInstructions);
		if (flag9)
		{
			Ref.inputController.instructionsTimewarp.SetActive(true);
			Saving.SaveSetting(Saving.SettingKey.seenTimewarpInstructions, true);
		}
		newArchivment = newArchivment.Replace(".", "!");
		Ref.controller.ShowMsg(Utility.SplitLines(newArchivment), 6f);
	}

	private void CheckGooglePlayAchievements(string newArchivment)
	{
		bool flag = newArchivment == "Passed the Karman Line, leaving the /atmosphere and reaching space.";
		if (!flag)
		{
			bool flag2 = newArchivment == "Reached low Earth orbit.";
			if (!flag2)
			{
				bool flag3 = newArchivment == "Entered Moon sphere of influence.";
				if (!flag3)
				{
					bool flag4 = newArchivment == "Landed on Moon surface.";
					if (!flag4)
					{
						bool flag5 = newArchivment == "Escaped Earth sphere of influence.";
						if (!flag5)
						{
							bool flag6 = newArchivment == "Entered Mars sphere of influence." || newArchivment == "Entered Venus sphere of influence." || newArchivment == "Entered Mercury sphere of influence.";
							if (!flag6)
							{
								bool flag7 = newArchivment == "Landed on Mars surface.";
								if (flag7)
								{
								}
							}
						}
					}
				}
			}
		}
	}

	private void CheckOrbitArchivments(Orbit orbit)
	{
		bool flag = orbit.orbitType == Orbit.Type.Eternal && orbit.periapsis > orbit.planet.radius + Math.Max(orbit.planet.atmosphereData.atmosphereHeightM, orbit.planet.minTimewarpHeightKm * 1000.0);
		if (flag)
		{
			this.AddArchivment(((!(orbit.planet.bodyName == Ref.controller.startAdress)) ? "Entered " : "Reached low ") + Ref.controller.loadedPlanet.bodyName + " orbit.");
		}
	}

	public Vessel()
	{
	}

	private bool castedHook = false;

	[BoxGroup]
	public bool controlAuthority;

	[BoxGroup]
	public float horizontalAxis;

	[BoxGroup]
	public bool RCS;

	public PartsManager partsManager;

	[FoldoutGroup("State", 0)]
	[Space]
	public Vessel.State state;

	[FoldoutGroup("State", 0)]
	[Space]
	public List<Orbit> orbits;

	[FoldoutGroup("State", 0)]
	public Vessel.StationaryData stationaryData;

	[Space]
	public Transform mapIcon;

	[Space]
	public Vessel.Throttle throttle;

	[Space]
	public List<string> vesselAchievements;

	private float archivmentCheckTime;

	[Serializable]
	public class Throttle
	{
		public Throttle(bool throttleOn, float throttleRaw)
		{
			this.throttleOn = throttleOn;
			this.throttle = Mathf.Pow(throttleRaw, 1.6f);
			this.throttleRaw = throttleRaw;
		}

		public bool throttleOn;

		public float throttle;

		public float throttleRaw;
	}

	[Serializable]
	public class StationaryData
	{
		public StationaryData()
		{
		}

		public Double3 posToPlane;

		public CelestialBodyData planet;
	}

	public enum State
	{
		RealTime,
		OnRails,
		Stationary,
		OnRailsUnloaded,
		StationaryUnloaded
	}

	public enum ToState
	{
		ToRealTime,
		ToTimewarping,
		ToUnloaded
	}
}
