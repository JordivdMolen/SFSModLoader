using System;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class PartsManager : MonoBehaviour
{
    public void ApplyThrustForce(float engineGimbal, Vessel.Throttle throttle, Vessel vessel)
    {
        if (Ref.mainVessel == vessel && Ref.mainVesselTerrainHeight < 20.0 && (!throttle.throttleOn || throttle.throttleRaw == 0f))
        {
            engineGimbal = 0f;
        }
        float num = 0f;
        for (int i = 0; i < this.engineModules.Count; i++)
        {
            this.engineModules[i].ApplyThrust(ref num, this.rb2d, (!this.engineModules[i].engineOn.boolValue) ? 0f : engineGimbal, throttle, Ref.mainVessel == vessel);
        }
        if (num == 0f)
        {
            return;
        }
        if (vessel == Ref.mainVessel)
        {
            Ref.controller.cameraOffset = num * 1.5E-05f * UnityEngine.Random.insideUnitCircle;
        }
    }

    public void ApplyDragForce(CelestialBodyData planet, Double3 posToPlanet)
    {
        if (!planet.atmosphereData.hasAtmosphere)
        {
            return;
        }
        Double3 @double = Ref.velocityOffset + this.rb2d.velocity;
        if (1.0 > @double.sqrMagnitude)
        {
            return;
        }
        double num = posToPlanet.magnitude2d - planet.radius;
        if (num > planet.atmosphereData.atmosphereHeightM)
        {
            return;
        }
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
        if (float.IsNaN(vector.x))
        {
            return;
        }
        this.rb2d.AddForceAtPosition(-(@double.normalized2d * @double.sqrMagnitude * d).toVector2 * d2, this.rb2d.GetRelativePoint(vector));
    }

    public void ApplyTorqueForce(bool controlAuthority, ref float horizontalAxis, Vessel vessel)
    {
        if (this.torque <= 0f)
        {
            return;
        }
        if (!controlAuthority)
        {
            if (Ref.mainVessel == vessel && Ref.inputController.horizontalAxis != 0f && MsgController.main.msgText.color.a < 0.6f)
            {
                MsgController.ShowMsg("No control");
            }
            horizontalAxis = 0f;
            return;
        }
        float num = this.torque / this.rb2d.mass / Time.fixedDeltaTime;
        if (Ref.mainVessel == vessel && Ref.inputController.horizontalAxis != 0f)
        {
            horizontalAxis = Ref.inputController.horizontalAxis;
            this.rb2d.angularVelocity -= num * Ref.inputController.horizontalAxis * Time.fixedDeltaTime;
            return;
        }
        horizontalAxis = Mathf.Clamp(this.rb2d.angularVelocity / (num * Time.fixedDeltaTime), -1f, 1f);
        this.rb2d.angularVelocity -= num * horizontalAxis * Time.fixedDeltaTime;
    }

    public void ApplyRcs(Vessel vessel, float horizontalAxis, float rcsInputDirection, bool directionInput)
    {
        if (this.rcsModules.Count == 0)
        {
            vessel.RCS = false;
            return;
        }
        double removeAmount = 0.0;
        for (int i = 0; i < this.rcsModules.Count; i++)
        {
            this.rcsModules[i].UseRcs(this.rb2d, vessel, horizontalAxis, rcsInputDirection, directionInput, ref removeAmount);
        }
        bool flag = this.TakeFromAllFuelGrups(removeAmount);
        if (flag)
        {
            return;
        }
        vessel.RCS = false;
        for (int j = 0; j < this.rcsModules.Count; j++)
        {
            this.rcsModules[j].effect.SetTargetTime(0f);
        }
    }

    public void ToggleRCS(Vessel vessel, bool showMsg)
    {
        if (!vessel.RCS)
        {
            if (this.rcsFuel.Count == 0)
            {
                MsgController.ShowMsg("No fuel source", showMsg);
                return;
            }
            if (!this.TakeFromAllFuelGrups(0.0))
            {
                MsgController.ShowMsg("Out of fuel", 2f, showMsg);
                return;
            }
        }
        vessel.RCS = !vessel.RCS;
        if (!vessel.RCS)
        {
            for (int i = 0; i < this.rcsModules.Count; i++)
            {
                this.rcsModules[i].effect.SetTargetTime(0f);
            }
        }
        MsgController.ShowMsg("RCS " + ((!vessel.RCS) ? "Off" : "On"), showMsg);
    }

    private bool TakeFromAllFuelGrups(double removeAmount)
    {
        if (Ref.infiniteFuel)
        {
            return true;
        }
        int num = 0;
        for (int i = 0; i < this.resourceGrups.Count; i++)
        {
            if (!this.resourceGrups[i].empty)
            {
                num++;
            }
        }
        if (num == 0)
        {
            return false;
        }
        for (int j = 0; j < this.resourceGrups.Count; j++)
        {
            if (!this.resourceGrups[j].empty)
            {
                this.resourceGrups[j].TakeResource(removeAmount / (double)num);
            }
        }
        return true;
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
        if (flag)
        {
            this.ResetsParts();
            List<Part> list = new List<Part>(this.parts);
            this.GetConnectedParts(this.parts[0], vessel);
            List<Part> list2 = new List<Part>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].vessel == null)
                {
                    list2.Add(list[i]);
                }
            }
            List<Vessel> list3 = Ref.controller.CreateVesselsFromParts(list2, this.rb2d.velocity, this.rb2d.angularVelocity, vessel.throttle, vessel.vesselAchievements);
            if (Ref.mainVessel == vessel)
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
                Vector2 a = (Vector2)this.fairingModules[i].part.transform.localPosition + this.fairingModules[i].part.partData.areas[j].start * this.fairingModules[i].part.orientation;
                Vector2 vector = a + this.fairingModules[i].part.partData.areas[j].size * this.fairingModules[i].part.orientation;
                if (Utility.IsInsideRange(localPosition.x, a.x, vector.x, true) && Utility.IsInsideRange(localPosition.y, a.y, vector.y, true))
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
                if (part.vessel == null)
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
        this.SortModules(this.GetModules(), vessel);
        this.UpdateCenterOfMass();
    }

    public Part GetCenterPart()
    {
        for (int i = 0; i < this.parts.Count; i++)
        {
            if (this.parts[i].GetComponent<ControlModule>() != null)
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

    private void SortModules(List<Module> modules, Vessel vessel)
    {
        MonoBehaviour.print(base.name + "  sorted modules");
        this.controlModules.Clear();
        this.torqueModules.Clear();
        this.engineModules.Clear();
        this.fairingModules.Clear();
        this.flowModules.Clear();
        this.rcsModules.Clear();
        this.rcsFuel.Clear();
        while (this.resourceGrups.Count > 0)
        {
            this.resourceGrups[0].DestroyGroup();
            this.resourceGrups.RemoveAt(0);
        }
        List<ResourceModule> list = new List<ResourceModule>();
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i].IsSorted())
            {
                if (modules[i] is ResourceModule)
                {
                    list.Add(modules[i] as ResourceModule);
                }
                else if (modules[i] is FlowModule)
                {
                    this.flowModules.Add(modules[i] as FlowModule);
                }
                else if (modules[i] is TorqueModule)
                {
                    this.torqueModules.Add(modules[i] as TorqueModule);
                }
                else if (modules[i] is EngineModule)
                {
                    this.engineModules.Add(modules[i] as EngineModule);
                }
                else if (modules[i] is ControlModule)
                {
                    this.controlModules.Add(modules[i] as ControlModule);
                }
                else if (modules[i] is RcsModule)
                {
                    this.rcsModules.Add(modules[i] as RcsModule);
                }
                else if (modules[i] is FairingModule)
                {
                    this.fairingModules.Add(modules[i] as FairingModule);
                }
            }
        }
        this.UpdateTorque();
        vessel.controlAuthority = (this.controlModules.Count > 0);
        this.SortResourceGrups(list);
        for (int j = 0; j < this.flowModules.Count; j++)
        {
            this.flowModules[j].GetSource();
        }
        for (int k = 0; k < this.resourceGrups.Count; k++)
        {
            if (this.resourceGrups[k].resourceType == Resource.Type.Fuel)
            {
                this.rcsFuel.Add(this.resourceGrups[k]);
            }
        }
    }

    private void SortResourceGrups(List<ResourceModule> resourceModules)
    {
        for (int i = 0; i < resourceModules.Count; i++)
        {
            if (!(resourceModules[i].resourceGroup != null))
            {
                ResourceGroup resourceGroup = base.gameObject.AddComponent<ResourceGroup>();
                resourceGroup.Initialize(resourceModules, i, resourceModules[i].resourceType);
                if (resourceGroup.resourceModules.Count != 0)
                {
                    this.resourceGrups.Add(resourceGroup);
                }
            }
        }
        if (Ref.mainVessel != null && Ref.mainVessel.partsManager == this)
        {
            Ref.controller.RepositionFuelIcons();
        }
    }

    public void ResetsParts()
    {
        for (int i = 0; i < this.rcsModules.Count; i++)
        {
            this.rcsModules[i].effect.SetTargetTime(0f);
        }
        foreach (ResourceGroup resourceGroup in this.resourceGrups)
        {
            resourceGroup.DestroyGroup();
        }
        this.resourceGrups = new List<ResourceGroup>();
        foreach (Part part in this.parts)
        {
            if (part.gameObject.activeSelf)
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
        if (float.IsNaN(num))
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
    public List<RcsModule> rcsModules;

    [FoldoutGroup("Modules", 0)]
    public List<FairingModule> fairingModules;

    [FoldoutGroup("Resources", 0)]
    public List<FlowModule> flowModules;

    [FoldoutGroup("Resources", 0)]
    public List<ResourceGroup> resourceGrups;

    [FoldoutGroup("Resources", 0)]
    public List<ResourceGroup> rcsFuel;
}
