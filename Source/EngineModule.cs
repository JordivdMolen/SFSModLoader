using System;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class EngineModule : Module
{
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
			List<string> result;
			if (this.showParametersDescription)
			{
				List<string> list = new List<string>();
				list.Add("Thrust: " + this.thrust.ToString() + "kN");
				result = list;
				list.Add("Isp (efficiency): " + (int)(this.thrust / this.resourceConsuption / 9.8f) + "s");
			}
			else
			{
				result = new List<string>();
			}
			return result;
		}
	}

	private void Start()
	{
		this.deflectedParticle1 = new EngineModule.FlameParticle(this.deflectedParticleHolder.GetChild(0).GetComponent<ParticleSystem>());
		this.deflectedParticle2 = new EngineModule.FlameParticle(this.deflectedParticleHolder.GetChild(1).GetComponent<ParticleSystem>());
	}

	public override void OnPartUsed()
	{
		bool flag = !Ref.timeWarping;
		if (flag)
		{
			this.ToggleEngine(true);
		}
		else
		{
			Ref.controller.ShowMsg("Cannot toggle engine while time warping");
		}
	}

	public override void OnPartLoaded()
	{
		bool boolValue = this.displayFuel.boolValue;
		if (boolValue)
		{
			this.GetResourceSource();
		}
	}

	private bool IsCovered()
	{
		return this.interstagePath.Length == 0 || Utility.GetByPath(base.transform, this.interstagePath).gameObject.activeSelf;
	}

	public void ToggleEngine(bool showMsg)
	{
		bool flag = this.IsCovered();
		if (!flag)
		{
			bool boolValue = this.engineOn.boolValue;
			if (boolValue)
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
				bool flag2 = !this.CheckResourceSource(true);
				if (flag2)
				{
					return;
				}
				this.engineOn.boolValue = true;
				this.displayFuel.boolValue = true;
				if (showMsg)
				{
					Ref.controller.ShowMsg("Engine On");
				}
			}
			this.UpdateEngineThrottle(base.transform.parent.GetComponentInParent<Vessel>().throttle);
		}
	}

	public bool CheckResourceSource(bool showMsg)
	{
		bool flag = !this.resourceSource.isValid;
		if (flag)
		{
			this.GetResourceSource();
		}
		bool flag2 = !this.resourceSource.isValid;
		bool result;
		if (flag2)
		{
			if (showMsg)
			{
				Ref.controller.ShowMsg("No fuel source");
			}
			result = false;
		}
		else
		{
			bool flag3 = this.resourceSource.resourceAmount == 0f;
			if (flag3)
			{
				if (showMsg)
				{
					Ref.controller.ShowMsg("Out of fuel");
				}
				result = false;
			}
			else
			{
				result = true;
			}
		}
		return result;
	}

	public void UpdateEngineThrottle(Vessel.Throttle throttle)
	{
		this.audioSource.volume = throttle.throttleRaw * 0.6f;
		bool flag = this.engineOn.boolValue && throttle.throttleOn && throttle.throttleRaw > 0f && !Ref.timeWarping;
		float targetTime = (!flag) ? 0f : throttle.throttle;
		this.throttleMove.SetTargetTime(targetTime);
		bool flag2 = flag != this.audioSource.isPlaying;
		if (flag2)
		{
			bool isPlaying = this.audioSource.isPlaying;
			if (isPlaying)
			{
				this.audioSource.Stop();
			}
			else
			{
				this.audioSource.Play();
				bool flag3 = Ref.mainVessel != null;
				if (flag3)
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
			bool fuelFlow = this.part.joints[i].fuelFlow;
			if (fuelFlow)
			{
				Part part = (!(this.part.joints[i].fromPart == this.part)) ? this.part.joints[i].fromPart : this.part.joints[i].toPart;
				ResourceModule resourceModule = part.GetResourceModule();
				bool flag = !(resourceModule == null);
				if (flag)
				{
					bool flag2 = resourceModule.resourceGrup == null || !resourceModule.resourceGrup.isValid;
					if (flag2)
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

	public void ApplyThrust(ref float totalThrust, Rigidbody2D rb2d, float xAxis, Vessel.Throttle throttle, bool mainVessel)
	{
		bool flag = !this.engineOn.boolValue;
		if (!flag)
		{
			xAxis *= (float)(this.part.orientation.x * this.part.orientation.y);
			this.nozzleMove.SetTargetTime(xAxis);
			bool flag2 = !throttle.throttleOn;
			if (!flag2)
			{
				bool flag3 = throttle.throttle == 0f;
				if (!flag3)
				{
					bool flag4 = !this.CheckResourceSource(mainVessel);
					if (flag4)
					{
						this.ToggleEngine(false);
					}
					else
					{
						bool flag5 = !Ref.infiniteFuel;
						if (flag5)
						{
							this.resourceSource.TakeResource(this.resourceConsuption * throttle.throttle * Time.fixedDeltaTime);
						}
						float num = this.thrust * throttle.throttle;
						totalThrust += num;
						Vector2 a = Quaternion.Euler(0f, 0f, base.transform.rotation.eulerAngles.z) * new Vector2(0f, (float)this.part.orientation.y);
						Vector2 force = a * num;
						Vector2 a2 = (base.transform.localPosition + this.part.centerOfMass * this.part.orientation) * num;
						rb2d.AddForceAtPosition(force, rb2d.GetRelativePoint(a2 / num));
						foreach (EngineModule.Flame flame in this.flamesData)
						{
							flame.flame.transform.localScale = new Vector3(UnityEngine.Random.Range(flame.sizeBounds.x, flame.sizeBounds.y), UnityEngine.Random.Range(flame.sizeBounds.z, flame.sizeBounds.w), 1f);
						}
						bool flag6 = false;
						bool flag7 = Ref.mainVesselTerrainHeight < 100.0;
						if (flag7)
						{
							RaycastHit2D hit = Physics2D.Raycast(base.transform.position, -a, this.flameLenght, this.particleCollisionMask);
							bool flag8 = hit;
							if (flag8)
							{
								flag6 = true;
								this.deflectedParticle1.particle.transform.localPosition = new Vector3(0f, -hit.distance / this.deflectedParticle1.particle.transform.lossyScale.y * Mathf.Abs(this.deflectedParticle1.particle.transform.localScale.y), 0f);
								this.deflectedParticle2.particle.transform.localPosition = new Vector3(0f, -hit.distance / this.deflectedParticle2.particle.transform.lossyScale.y * Mathf.Abs(this.deflectedParticle2.particle.transform.localScale.y), 0f);
								this.deflectedParticle1.particleMain.startColor = new Color(1f, 1f, 1f, throttle.throttleRaw * (1f - hit.distance / this.flameLenght));
								this.deflectedParticle2.particleMain.startColor = new Color(1f, 1f, 1f, throttle.throttleRaw * (1f - hit.distance / this.flameLenght));
							}
						}
						bool flag9 = this.deflectedParticle1.particle.gameObject.activeSelf != flag6;
						if (flag9)
						{
							this.deflectedParticle1.particle.gameObject.SetActive(flag6);
							this.deflectedParticle2.particle.gameObject.SetActive(flag6);
						}
					}
				}
			}
		}
	}

	public EngineModule()
	{
	}

	[BoxGroup("1", false, false, 0)]
	public BoolValueHolder engineOn;

	[BoxGroup("1", false, false, 0)]
	public BoolValueHolder displayFuel;

	[BoxGroup("2", false, false, 0)]
	[SuffixLabel("kn", false)]
	public float thrust;

	[BoxGroup("2", false, false, 0)]
	public Resource resourceType;

	[BoxGroup("2", false, false, 0)]
	[SuffixLabel("/s", false)]
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
