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
        if (Ref.mainVessel == this)
        {
            Ref.controller.throttleOnUI.text = ((!this.throttle.throttleOn) ? "Off" : "On");
            Ref.controller.throttleBar.fillAmount = this.throttle.throttleRaw;
            Ref.controller.throttleColorMove.SetTargetTime((float)((!this.throttle.throttleOn) ? 0 : 1));
            float num = Mathf.Pow(this.throttle.throttleRaw, 1.6f) * 100f;
            if (num > 10f)
            {
                Ref.controller.throttlePercentUI.text = ((int)num).ToString() + "%";
            }
            else if (num >= 0.1f)
            {
                Ref.controller.throttlePercentUI.text = ((int)num).ToString() + "." + ((int)(num % 1f * 10f)).ToString() + "%";
            }
            else
            {
                Ref.controller.throttlePercentUI.text = ((num <= 0f) ? "0%" : "0.1%");
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
        if (this.state == Vessel.State.RealTime)
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
            if (Time.time > this.archivmentCheckTime)
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


            if (@double.sqrMagnitude2d > Ref.controller.loadedPlanet.orbitData.SOI * Ref.controller.loadedPlanet.orbitData.SOI)
            {
                this.AddArchivment("Escaped " + Ref.controller.loadedPlanet.bodyName + " sphere of influence");
                CelestialBodyData parentBody = Ref.controller.loadedPlanet.parentBody;
                this.mapIcon.parent = Ref.map.mapRefs[parentBody].holder;
                if (Ref.mainVessel == this)
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
                if (double2.sqrMagnitude2d < celestialBodyData.orbitData.SOI * celestialBodyData.orbitData.SOI)
                {
                    this.AddArchivment("Entered " + celestialBodyData.bodyName + " sphere of influence");
                    this.mapIcon.parent = Ref.map.mapRefs[celestialBodyData].holder;
                    Double3 posIn2 = double2;
                    Double3 velIn2 = this.GetGlobalVelocity - celestialBodyData.GetVelOut(Ref.controller.globalTime);
                    if (Ref.mainVessel == this)
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
            if (Ref.mapView)
            {
                this.mapIcon.localPosition = (@double / 10000.0).toVector3;
                this.mapIcon.localRotation = this.partsManager.parts[0].transform.rotation;
            }
        }
        else if (this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded)
        {
            if (this.orbits[0].orbitEndTime < Ref.controller.globalTime)
            {
                this.EnterNextOrbit();
                @double = this.GetGlobalPosition;
            }
            if (this.orbits[0].calculatePassesTime != double.PositiveInfinity && Ref.controller.globalTime > this.orbits[0].calculatePassesTime - this.orbits[0]._period * 0.9 && this.orbits[0].UpdatePass())
            {
                this.orbits = Orbit.CalculateOrbits(this.orbits);
            }
            if (Ref.mapView)
            {
                this.mapIcon.localPosition = (@double / 10000.0).toVector3;
            }
            else if (this.state == Vessel.State.OnRails)
            {
                base.transform.position = (@double - Ref.positionOffset).toVector3;
            }
            if (Ref.mainVessel != this && @double.magnitude2d - this.orbits[0].planet.radius < this.orbits[0].planet.GetTerrainSampleAtAngle(Math.Atan2(@double.y, @double.x) * 57.2958))
            {
                this.DestroyVessel();
            }
        }
    }

    private void EnterNextOrbit()
    {
        if (this.orbits[0].orbitType == Orbit.Type.Escape)
        {
            this.AddArchivment("Escaped " + this.orbits[0].planet.bodyName + " sphere of influence.");
        }
        else if (this.orbits[0].orbitType == Orbit.Type.Encounter)
        {
            this.AddArchivment("Entered " + this.orbits[0].nextPlanet.bodyName + " sphere of influence.");
        }
        this.orbits.RemoveAt(0);
        this.orbits = Orbit.CalculateOrbits(this.orbits);
        Double3 posOut = this.orbits[0].GetPosOut(Ref.controller.globalTime);
        this.mapIcon.parent = Ref.map.mapRefs[this.orbits[0].planet].holder;
        if (Ref.mainVessel == this)
        {
            Ref.planetManager.SwitchLocation(this.orbits[0].planet, posOut, true, false, 0.0);
        }
    }

    private void CheckLoadDistance()
    {
        if (Ref.mainVessel == null)
        {
            return;
        }
        if (Ref.mainVessel == this)
        {
            if (!Ref.timeWarping && this.state == Vessel.State.OnRails)
            {
                this.SetVesselState(Vessel.ToState.ToRealTime);
                MonoBehaviour.print("Corrected");
            }
            if (Ref.timeWarping && this.state == Vessel.State.OnRailsUnloaded)
            {
                this.SetVesselState(Vessel.ToState.ToTimewarping);
                MonoBehaviour.print("Corrected");
            }
            return;
        }
        if (Ref.controller.loadedPlanet == this.GetVesselPlanet && (this.GetGlobalPosition - Ref.mainVessel.GetGlobalPosition).sqrMagnitude2d < 25000000.0)
        {
            if (this.state == Vessel.State.OnRailsUnloaded || this.state == Vessel.State.StationaryUnloaded)
            {
                this.SetVesselState((!Ref.timeWarping) ? Vessel.ToState.ToRealTime : Vessel.ToState.ToTimewarping);
            }
            return;
        }
        if (this.state == Vessel.State.RealTime || this.state == Vessel.State.OnRails || this.state == Vessel.State.Stationary)
        {
            this.SetVesselState(Vessel.ToState.ToUnloaded);
        }
    }

    private void CheckForArchivments()
    {
        if (Ref.controller.loadedPlanet.bodyName != Ref.controller.startAdress)
        {
            if (this.OnSurface)
            {
                this.AddArchivment("Landed on " + Ref.controller.loadedPlanet.bodyName + " surface.");
            }
        }
        else
        {
            Double3 @double = Ref.positionOffset + this.partsManager.rb2d.worldCenterOfMass;
            if (@double.magnitude2d > Ref.controller.loadedPlanet.radius + Ref.controller.loadedPlanet.atmosphereData.atmosphereHeightM)
            {
                this.AddArchivment("Passed the Karman Line, leaving the /atmosphere and reaching space.");
            }
            else if (@double.magnitude2d > Ref.controller.loadedPlanet.radius + 15000.0)
            {
                this.AddArchivment("Reached 15 km altitude.");
            }
            else if (@double.magnitude2d > Ref.controller.loadedPlanet.radius + 10000.0)
            {
                this.AddArchivment("Reached 10 km altitude.");
            }
            else if (@double.magnitude2d > Ref.controller.loadedPlanet.radius + 5000.0)
            {
                this.AddArchivment("Reached 5 km altitude.");
            }
        }
    }

    private void ApplyPhysics()
    {
        CelestialBodyData loadedPlanet = Ref.controller.loadedPlanet;
        Double3 getGlobalPosition = this.GetGlobalPosition;
        if (!Ref.noGravity)
        {
            double d = loadedPlanet.mass / (getGlobalPosition.x * getGlobalPosition.x + getGlobalPosition.y * getGlobalPosition.y) * (double)Time.fixedDeltaTime;
            this.partsManager.rb2d.velocity -= (getGlobalPosition.normalized2d * d).toVector2;
        }
        if (this.throttle.throttleOn && this.throttle.throttleRaw > 0f)
        {
            this.partsManager.ApplyThrustForce(this.horizontalAxis, this.throttle, this);
        }
        if (!Ref.noDrag)
        {
            this.partsManager.ApplyDragForce(loadedPlanet, getGlobalPosition);
        }
        this.partsManager.ApplyTorqueForce(this.controlAuthority, ref this.horizontalAxis, this);
        Vector2 vector = (!(this == Ref.mainVessel)) ? Vector2.zero : this.GetRcsInput();
        bool flag = vector.sqrMagnitude > 0f;
        if (this.RCS)
        {
            this.partsManager.ApplyRcs(this, this.horizontalAxis, (!flag || !this.controlAuthority) ? 0f : (Mathf.Atan2(vector.y, vector.x) * 57.29578f + Ref.cam.transform.eulerAngles.z), flag);
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
            if (this.state == Vessel.State.RealTime)
            {
                return Ref.positionOffset + this.partsManager.rb2d.worldCenterOfMass;
            }
            if ((this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded) && this.orbits.Count > 0)
            {
                return this.orbits[0].GetPosOut(Ref.controller.globalTime);
            }
            return this.stationaryData.posToPlane;
        }
    }

    public Double3 GetGlobalVelocity
    {
        get
        {
            if (this.state == Vessel.State.RealTime)
            {
                return Ref.velocityOffset + this.partsManager.rb2d.velocity;
            }
            if ((this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded) && this.orbits.Count > 0)
            {
                return this.orbits[0].GetVelOut(Ref.controller.globalTime);
            }
            return Double3.zero;
        }
    }

    public CelestialBodyData GetVesselPlanet
    {
        get
        {
            if (this.state == Vessel.State.RealTime)
            {
                return Ref.controller.loadedPlanet;
            }
            if (this.state == Vessel.State.Stationary || this.state == Vessel.State.StationaryUnloaded)
            {
                return this.stationaryData.planet;
            }
            if ((this.state == Vessel.State.OnRails || this.state == Vessel.State.OnRailsUnloaded) && this.orbits.Count > 0)
            {
                return this.orbits[0].planet;
            }
            return Ref.controller.loadedPlanet;
        }
    }

    public void SetVesselState(Vessel.ToState toState)
    {
        if (toState == Vessel.ToState.ToTimewarping)
        {
            if (this.state == Vessel.State.RealTime)
            {
                this.state = this.GoOnRails(Ref.controller.loadedPlanet, true);
                base.name = "Vessel (" + this.state.ToString() + ")";
                return;
            }
            if (this.state == Vessel.State.OnRailsUnloaded)
            {
                this.state = Vessel.State.OnRails;
            }
            if (this.state == Vessel.State.StationaryUnloaded)
            {
                this.state = Vessel.State.Stationary;
            }
            this.partsManager.rb2d.gameObject.SetActive(true);
        }
        if (toState == Vessel.ToState.ToUnloaded)
        {
            if (this.state == Vessel.State.RealTime)
            {
                this.state = this.GoOnRails(Ref.controller.loadedPlanet, true);
            }
            if (this.state == Vessel.State.OnRails)
            {
                this.state = Vessel.State.OnRailsUnloaded;
            }
            if (this.state == Vessel.State.Stationary)
            {
                this.state = Vessel.State.StationaryUnloaded;
            }
            this.partsManager.rb2d.gameObject.SetActive(false);
            this.partsManager.rb2d.position = Vector3.zero;
        }
        if (toState == Vessel.ToState.ToRealTime)
        {
            if (this.state == Vessel.State.RealTime)
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
            if (engineModule.nozzleMove != null)
            {
                engineModule.nozzleMove.SetTargetTime(0f);
            }
            engineModule.UpdateEngineThrottle(this.throttle);
        }
        this.stationaryData.posToPlane = getGlobalPosition;
        this.stationaryData.planet = initialPlanet;
        if (Math.Abs(getGlobalVelocity.x) < 0.3 && Math.Abs(getGlobalVelocity.y) < 0.3)
        {
            this.orbits.Clear();
            this.mapIcon.localPosition = (this.stationaryData.posToPlane / 10000.0).toVector3;
            return Vessel.State.Stationary;
        }
        this.orbits = Orbit.CalculateOrbits(getGlobalPosition, getGlobalVelocity, initialPlanet);
        if (double.IsNaN(this.orbits[0].meanMotion))
        {
            MonoBehaviour.print("Cannot orbit NaN, went stationary instead");
            this.orbits.Clear();
            this.mapIcon.localPosition = (this.stationaryData.posToPlane / 10000.0).toVector3;
            return Vessel.State.Stationary;
        }
        this.CheckOrbitArchivments(this.orbits[0]);
        return Vessel.State.OnRails;
    }

    public void GoOffRails()
    {
        Double3 getGlobalPosition = this.GetGlobalPosition;
        Vector3 toVector = (this.GetGlobalPosition - Ref.positionOffset).toVector3;
        double num = Math.Pow(Ref.controller.loadedPlanet.radius + Ref.controller.loadedPlanet.terrainData.maxTerrainHeight + 10.0, 2.0);
        if (getGlobalPosition.sqrMagnitude2d < num && Ref.velocityOffset.sqrMagnitude2d > 0.0)
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
            if (Ref.mainVessel == this)
            {
                Ref.mainVessel = null;
                Ref.controller.RepositionFuelIcons();
            }
            if (Ref.mainVessel == this)
            {
                Ref.mainVessel = null;
                Ref.map.mainLines.HideAll();
            }
            if (Ref.selectedVessel == this)
            {
                Ref.selectedVessel = null;
                Ref.map.selectedLines.HideAll();
            }
            if (this.mapIcon != null)
            {
                UnityEngine.Object.Destroy(this.mapIcon.gameObject);
            }
            if (base.gameObject != null)
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }
	}

    public void MergeVessel(Vessel otherVessel)
    {
        if (otherVessel == this)
        {
            return;
        }
        if (otherVessel == Ref.mainVessel)
        {
            Ref.mainVessel = this;
        }
        if (otherVessel == Ref.selectedVessel)
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

    public void DisasembleVessel()
    {
        Ref.controller.vessels.Remove(this);
        this.partsManager.ResetsParts();
        List<Vessel> list = Ref.controller.CreateVesselsFromParts(this.partsManager.parts, this.partsManager.rb2d.velocity, this.partsManager.rb2d.angularVelocity, this.throttle, this.vesselAchievements);
        if (Ref.mainVessel == this && list.Count > 0)
        {
            Ref.controller.SwitchVessel(list[0]);
        }
        this.DestroyVessel();
    }

    public bool OnSurface
    {
        get
        {
            if (this.state == Vessel.State.Stationary || this.state == Vessel.State.StationaryUnloaded)
            {
                return true;
            }
            if (this.partsManager.rb2d == null)
            {
                return false;
            }
            Collider2D[] array = new Collider2D[5];
            this.partsManager.rb2d.GetContacts(array);
            foreach (Collider2D collider2D in array)
            {
                if (collider2D != null && collider2D.gameObject.layer == LayerMask.NameToLayer("Celestial Body"))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public void AddArchivment(string newArchivment)
    {
        for (int i = 0; i < this.vesselAchievements.Count; i++)
        {
            if (this.vesselAchievements[i] == newArchivment)
            {
                return;
            }
        }
        this.vesselAchievements.Add(newArchivment);
        if (Ref.mainVessel != this)
        {
            return;
        }
        if (newArchivment == "Reached 5 km altitude.")
        {
            return;
        }
        if (newArchivment == "Reached 10 km altitude.")
        {
            return;
        }
        if (newArchivment == "Reached 15 km altitude.")
        {
            return;
        }
        if (newArchivment == "Passed the Karman Line, leaving the /atmosphere and reaching space.")
        {
            newArchivment = "Passed the Karman Line, leaving the atmosphere and reaching space.";
            if (!Saving.LoadSetting(Saving.SettingKey.seenMapInstructions))
            {
                if (!Ref.mapView)
                {
                    Ref.inputController.instructionsMap.SetActive(true);
                    Ref.inputController.CloseDropdownMenu();
                }
                Saving.SaveSetting(Saving.SettingKey.seenMapInstructions, true);
                return;
            }
        }
        if (newArchivment == "Reached low Earth orbit." && !Saving.LoadSetting(Saving.SettingKey.seenTimewarpInstructions))
        {
            Ref.inputController.instructionsTimewarp.SetActive(true);
            Saving.SaveSetting(Saving.SettingKey.seenTimewarpInstructions, true);
        }
        newArchivment = newArchivment.Replace(".", "!");
        MsgController.ShowMsg(Utility.SplitLines(newArchivment), 6f, true);
    }

    private void CheckOrbitArchivments(Orbit orbit)
    {
        if (orbit.orbitType == Orbit.Type.Eternal && orbit.periapsis > orbit.planet.radius + Math.Max(orbit.planet.atmosphereData.atmosphereHeightM, orbit.planet.minTimewarpHeightKm * 1000.0))
        {
            this.AddArchivment(((!(orbit.planet.bodyName == Ref.controller.startAdress)) ? "Entered " : "Reached low ") + Ref.controller.loadedPlanet.bodyName + " orbit.");
        }
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
