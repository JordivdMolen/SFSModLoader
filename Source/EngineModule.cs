using System;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class EngineModule : Module
{
    public override bool IsSorted()
    {
        return true;
    }

    public override List<object> SaveVariables
    {
        get
        {
            return new List<object>
            {
                this.engineOn
            };
        }
    }

    public override List<string> DescriptionVariables
    {
        get
        {
            return (!this.showParametersDescription) ? new List<string>() : new List<string>
            {
                "Thrust: " + this.thrust.ToString() + "kN",
                "Isp (efficiency): " + (int)(this.thrust / this.consumption.floatValue / 9.8f) + "s"
            };
        }
    }

    private void Start()
    {
        if (this.deflectedParticleHolder != null)
        {
            this.deflectedParticle1 = new EngineModule.FlameParticle(this.deflectedParticleHolder.GetChild(0).GetComponent<ParticleSystem>());
            this.deflectedParticle2 = new EngineModule.FlameParticle(this.deflectedParticleHolder.GetChild(1).GetComponent<ParticleSystem>());
        }
    }

    public override void OnPartUsed()
    {
        if (!Ref.timeWarping)
        {
            this.ToggleEngine(true);
        }
        else
        {
            MsgController.ShowMsg("Cannot toggle engine while time warping");
        }
    }

    private bool IsCovered()
    {
        return this.interstagePath.Length == 0 || Utility.GetByPath(base.transform, this.interstagePath).gameObject.activeSelf;
    }

    public void ToggleEngine(bool showMsg)
    {
        if (this.IsCovered())
        {
            return;
        }
        if (this.engineOn.boolValue)
        {
            this.engineOn.boolValue = false;
            if (showMsg)
            {
                MsgController.ShowMsg("Engine Off");
            }
            if (this.nozzleMove != null)
            {
                this.nozzleMove.SetTargetTime(0f);
            }
        }
        else
        {
            for (int i = 0; i < this.resourceSources.Length; i++)
            {
                if (this.resourceSources[i].state != FlowModule.State.Flowing)
                {
                    MsgController.ShowMsg((this.resourceSources[i].state != FlowModule.State.NoSource) ? ("Out of " + Resource.GetResourceName(this.resourceSources[i].resourceType).ToLower()) : ("No " + Resource.GetResourceName(this.resourceSources[i].resourceType).ToLower() + " source"));
                    return;
                }
            }
            this.engineOn.boolValue = true;
            if (showMsg)
            {
                MsgController.ShowMsg("Engine On");
            }
        }
        this.UpdateEngineThrottle(base.transform.parent.GetComponentInParent<Vessel>().throttle);
    }

    public void OnSourceStateChange()
    {
        if (this.throttle.floatValue == 0f)
        {
            return;
        }
        if (FlowModule.ConfirmSources(this.resourceSources, Ref.mainVessel == this.part.vessel))
        {
            return;
        }
        if (this.engineOn.boolValue)
        {
            this.ToggleEngine(false);
        }
    }

    public void UpdateEngineThrottle(Vessel.Throttle newThrottle)
    {
        bool flag = this.engineOn.boolValue && newThrottle.throttleOn && newThrottle.throttleRaw > 0f && !Ref.timeWarping;
        this.throttle.floatValue = ((!flag) ? 0f : newThrottle.throttle);
        this.audioSource.volume = this.throttle.floatValue * 0.6f;
        if (flag != this.audioSource.isPlaying)
        {
            if (this.audioSource.isPlaying)
            {
                this.audioSource.Stop();
            }
            else
            {
                this.audioSource.Play();
                if (Ref.mainVessel != null)
                {
                    for (int i = 0; i < Ref.mainVessel.partsManager.engineModules.Count; i++)
                    {
                        Ref.mainVessel.partsManager.engineModules[i].audioSource.time = 0f;
                    }
                }
            }
        }
    }

    public void ApplyThrust(ref float totalThrust, Rigidbody2D rb2d, float xAxis, Vessel.Throttle throttle, bool mainVessel)
    {
        if (!this.engineOn.boolValue)
        {
            return;
        }
        xAxis *= (float)(this.part.orientation.x * this.part.orientation.y);
        if (this.nozzleMove != null)
        {
            this.nozzleMove.SetTargetTime(xAxis);
        }
        if (!throttle.throttleOn)
        {
            return;
        }
        if (throttle.throttle == 0f)
        {
            return;
        }
        float num = this.thrust * throttle.throttle;
        totalThrust += num;
        Vector2 a = Quaternion.Euler(0f, 0f, base.transform.rotation.eulerAngles.z) * new Vector2(0f, (float)this.part.orientation.y);
        Vector2 force = a * num;
        Vector2 a2 = (base.transform.localPosition + this.part.centerOfMass * this.part.orientation) * num;
        rb2d.AddForceAtPosition(force, rb2d.GetRelativePoint(a2 / num));
        if (this.deflectedParticleHolder == null)
        {
            return;
        }
        bool flag = false;
        if (Ref.mainVesselTerrainHeight < 100.0)
        {
            RaycastHit2D hit = Physics2D.Raycast(base.transform.position, -a, this.flameLenght, this.particleCollisionMask);
            if (hit)
            {
                flag = true;
                this.deflectedParticle1.particle.transform.localPosition = new Vector3(0f, -hit.distance / this.deflectedParticle1.particle.transform.lossyScale.y * Mathf.Abs(this.deflectedParticle1.particle.transform.localScale.y), 0f);
                this.deflectedParticle2.particle.transform.localPosition = new Vector3(0f, -hit.distance / this.deflectedParticle2.particle.transform.lossyScale.y * Mathf.Abs(this.deflectedParticle2.particle.transform.localScale.y), 0f);
                this.deflectedParticle1.particleMain.startColor = new Color(1f, 1f, 1f, throttle.throttleRaw * (1f - hit.distance / this.flameLenght));
                this.deflectedParticle2.particleMain.startColor = new Color(1f, 1f, 1f, throttle.throttleRaw * (1f - hit.distance / this.flameLenght));
            }
        }
        if (this.deflectedParticle1.particle.gameObject.activeSelf != flag)
        {
            this.deflectedParticle1.particle.gameObject.SetActive(flag);
            this.deflectedParticle2.particle.gameObject.SetActive(flag);
        }
    }

    [BoxGroup("1", false, false, 0)]
    [InlineProperty]
    public BoolValueHolder engineOn;

    [BoxGroup("2", false, false, 0)]
    [SuffixLabel("kn", false)]
    public float thrust;

    public FlowModule[] resourceSources;

    [InlineProperty]
    public FloatValueHolder consumption;

    [Space]
    public MoveModule nozzleMove;

    [InlineProperty]
    public FloatValueHolder throttle;

    public AudioSource audioSource;

    [BoxGroup("Flame Data", false, false, 0)]
    public Transform deflectedParticleHolder;

    [BoxGroup("Flame Data", false, false, 0)]
    public float flameLenght;

    [BoxGroup("Flame Data", false, false, 0)]
    public LayerMask particleCollisionMask;

    [BoxGroup("4", false, false, 0)]
    public int[] interstagePath;

    private EngineModule.FlameParticle deflectedParticle1;

    private EngineModule.FlameParticle deflectedParticle2;

    public bool showParametersDescription = true;

    [Serializable]
    public struct Flame
    {
        public Transform flame;

        public Vector4 sizeBounds;
    }

    [Serializable]
    public struct FlameParticle
    {
        public FlameParticle(ParticleSystem particle)
        {
            this.particle = particle;
            this.particleMain = particle.main;
        }

        public ParticleSystem particle;

        public ParticleSystem.MainModule particleMain;
    }
}
