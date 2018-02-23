using NewBuildSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EngineModule : Module
{
	[Serializable]
	public struct FuelIcon
	{
		public Image icon;

		public Image bar;

		public FuelIcon(Image icon, Image bar)
		{
			this.icon = icon;
			this.bar = bar;
		}
	}

	[Serializable]
	public struct Flame
	{
		public Transform flame;

		public Vector4 sizeBounds;
	}

	[Serializable]
	public struct FlameParticle
	{
		public ParticleSystem particle;

		public ParticleSystem.MainModule particleMain;

		public FlameParticle(ParticleSystem particle)
		{
			this.particle = particle;
			this.particleMain = particle.main;
		}
	}

	[BoxGroup("1", false, false, 0)]
	public BoolValueHolder engineOn;

	[BoxGroup("1", false, false, 0)]
	public BoolValueHolder displayFuel;

	[BoxGroup("2", false, false, 0), SuffixLabel("kn", false)]
	public float thrust;

	[BoxGroup("2", false, false, 0)]
	public Resource resourceType;

	[BoxGroup("2", false, false, 0), SuffixLabel("/s", false)]
	public float resourceConsuption;

	[BoxGroup("3", false, false, 0)]
	public ResourceModule.Grup resourceSource;

	[BoxGroup("4", false, false, 0)]
	public int[] interstagePath;

	[BoxGroup("4", false, false, 0)]
	public Part interstagePrefab;

	[Space]
	public MoveModule nozzleMove;

	public MoveModule throttleMove;

	[Space]
	public AudioSource audioSource;

	public EngineModule.FuelIcon fuelIcon;

	[FoldoutGroup("Flame Data", 0)]
	public EngineModule.Flame[] flamesData;

	[FoldoutGroup("Flame Data", 0)]
	public Transform deflectedParticleHolder;

	[FoldoutGroup("Flame Data", 0)]
	public float flameLenght;

	private EngineModule.FlameParticle deflectedParticle1;

	private EngineModule.FlameParticle deflectedParticle2;

	[FoldoutGroup("Flame Data", 0)]
	public LayerMask particleCollisionMask;

	public bool showParametersDescription = true;

	public override List<object> SaveVariables
	{
		get
		{
			return new List<object>
			{
				this.engineOn,
				this.displayFuel
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
				"Isp: " + (int)(this.thrust / this.resourceConsuption / 9.8f) + "s"
			};
		}
	}

	private void Start()
	{
		this.deflectedParticle1 = new EngineModule.FlameParticle(this.deflectedParticleHolder.GetChild(0).GetComponent<ParticleSystem>());
		this.deflectedParticle2 = new EngineModule.FlameParticle(this.deflectedParticleHolder.GetChild(1).GetComponent<ParticleSystem>());
	}

	public override void OnPartUsed()
	{
		if (!Ref.timeWarping)
		{
			this.ToggleEngine(true);
		}
		else
		{
			Ref.controller.ShowMsg("Cannot toggle engine while time warping");
		}
	}

	private bool IsCovered()
	{
		return Utility.GetByPath(base.transform, this.interstagePath).gameObject.activeSelf;
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
				Ref.controller.ShowMsg("Engine Off");
			}
			this.nozzleMove.SetTargetTime(0f);
		}
		else
		{
			if (!this.CheckResourceSource(true))
			{
				return;
			}
			this.engineOn.boolValue = true;
			this.displayFuel.boolValue = true;
			Ref.controller.RepositionFuelIcons();
			if (showMsg)
			{
				Ref.controller.ShowMsg("Engine On");
			}
		}
		this.UpdateEngineThrottle(base.transform.parent.GetComponentInParent<Vessel>().throttle);
	}

	public bool CheckResourceSource(bool showMsg)
	{
		if (!this.resourceSource.isValid)
		{
			this.GetResourceSource();
		}
		if (!this.resourceSource.isValid)
		{
			if (showMsg)
			{
				Ref.controller.ShowMsg("No fuel source");
			}
			return false;
		}
		if (this.resourceSource.resourceAmount == 0f)
		{
			if (showMsg)
			{
				Ref.controller.ShowMsg("Out of fuel");
			}
			return false;
		}
		return true;
	}

	public void UpdateEngineThrottle(Vessel.Throttle throttle)
	{
		this.audioSource.volume = throttle.throttleRaw * 0.6f;
		bool flag = this.engineOn.boolValue && throttle.throttleOn && throttle.throttleRaw > 0f && !Ref.timeWarping;
		float targetTime = (!flag) ? 0f : throttle.throttle;
		this.throttleMove.SetTargetTime(targetTime);
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

	public bool GetResourceSource()
	{
		for (int i = 0; i < this.part.joints.Count; i++)
		{
			if (this.part.joints[i].fuelFlow)
			{
				Part part = (!(this.part.joints[i].fromPart == this.part)) ? this.part.joints[i].fromPart : this.part.joints[i].toPart;
				ResourceModule resourceModule = part.GetResourceModule();
				if (!(resourceModule == null))
				{
					if (resourceModule.resourceGrup == null || !resourceModule.resourceGrup.isValid)
					{
						this.resourceSource = new ResourceModule.Grup(resourceModule);
					}
					else
					{
						this.resourceSource = resourceModule.resourceGrup;
					}
					return true;
				}
			}
		}
		return false;
	}

	public void ApplyThrust(ref float totalThrust, Rigidbody2D rb2d, float xAxis, Vessel.Throttle throttle)
	{
		if (!this.engineOn.boolValue)
		{
			return;
		}
		xAxis *= (float)(this.part.orientation.x * this.part.orientation.y);
		this.nozzleMove.SetTargetTime(xAxis);
		if (!throttle.throttleOn)
		{
			return;
		}
		if (throttle.throttle == 0f)
		{
			return;
		}
		if (!this.CheckResourceSource(true))
		{
			this.ToggleEngine(false);
			return;
		}
		this.resourceSource.TakeResource(this.resourceConsuption * throttle.throttle * Time.fixedDeltaTime);
		if (this.fuelIcon.bar != null)
		{
			this.fuelIcon.bar.fillAmount = this.resourceSource.resourcePercent;
		}
		float num = this.thrust * throttle.throttle;
		totalThrust += num;
		Vector2 a = Quaternion.Euler(0f, 0f, base.transform.rotation.eulerAngles.z) * new Vector2(0f, (float)this.part.orientation.y);
		Vector2 force = a * num;
		Vector2 a2 = (base.transform.localPosition + this.part.centerOfMass * this.part.orientation) * num;
		rb2d.AddForceAtPosition(force, rb2d.GetRelativePoint(a2 / num));
		EngineModule.Flame[] array = this.flamesData;
		for (int i = 0; i < array.Length; i++)
		{
			EngineModule.Flame flame = array[i];
			flame.flame.transform.localScale = new Vector3(UnityEngine.Random.Range(flame.sizeBounds.x, flame.sizeBounds.y), UnityEngine.Random.Range(flame.sizeBounds.z, flame.sizeBounds.w), 1f);
		}
		bool flag = false;
		if (Ref.mainVesselTerrainHeight < 100.0)
		{
			RaycastHit2D hit = Physics2D.Raycast(base.transform.position, -a, this.flameLenght, this.particleCollisionMask);
			if (hit)
			{
				flag = true;
				this.deflectedParticle1.particle.transform.localPosition = new Vector3(0f, -hit.distance / 1.5f + 0.45f, 0f);
				this.deflectedParticle2.particle.transform.localPosition = new Vector3(0f, -hit.distance / 1.5f + 0.45f, 0f);
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
}
