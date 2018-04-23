using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveModule : Module
{
	public override List<object> SaveVariables
	{
		get
		{
			return new List<object>
			{
				this.time,
				this.targetTime
			};
		}
	}

	private void Start()
	{
		this.UpdateAnimation();
	}

	private void Update()
	{
		float num = Time.deltaTime / this.animationTime;
		bool flag = Mathf.Abs(this.targetTime.floatValue - this.time.floatValue) < num;
		if (flag)
		{
			this.time.floatValue = this.targetTime.floatValue;
			base.enabled = false;
		}
		else
		{
			this.time.floatValue = this.time.floatValue + num * Mathf.Sign(this.targetTime.floatValue - this.time.floatValue);
		}
		this.UpdateAnimation();
	}

	public void SetTargetTime(float newTargetTime)
	{
		this.targetTime.floatValue = newTargetTime;
		base.enabled = true;
	}

	public void SetTime(float newTime)
	{
		this.time.floatValue = newTime;
		this.UpdateAnimation();
		base.enabled = true;
	}

	public void UpdateAnimation()
	{
		for (int i = 0; i < this.animationElements.Length; i++)
		{
			switch (this.animationElements[i].type)
			{
			case MoveModule.Type.RotationZ:
				this.animationElements[i].transform.localEulerAngles = new Vector3(0f, 0f, this.animationElements[i].X.Evaluate(this.time.floatValue));
				break;
			case MoveModule.Type.Scale:
				this.animationElements[i].transform.localScale = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue), this.animationElements[i].Y.Evaluate(this.time.floatValue), 0f);
				break;
			case MoveModule.Type.Position:
				this.animationElements[i].transform.localPosition = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue), this.animationElements[i].Y.Evaluate(this.time.floatValue), 0f);
				break;
			case MoveModule.Type.CenterOfMass:
				this.part.centerOfMass = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue), this.animationElements[i].Y.Evaluate(this.time.floatValue), 0f);
				break;
			case MoveModule.Type.CenterOfDrag:
				this.part.centerOfDrag = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue), this.animationElements[i].Y.Evaluate(this.time.floatValue), 0f);
				break;
			case MoveModule.Type.SpriteColor:
				this.animationElements[i].spriteRenderer.color = this.animationElements[i].gradient.Evaluate(this.time.floatValue);
				break;
			case MoveModule.Type.ImageColor:
				this.animationElements[i].image.color = this.animationElements[i].gradient.Evaluate(this.time.floatValue);
				break;
			case MoveModule.Type.Active:
			{
				bool flag = this.animationElements[i].transform.gameObject.activeSelf != this.animationElements[i].X.Evaluate(this.time.floatValue) > 0f;
				if (flag)
				{
					this.animationElements[i].transform.gameObject.SetActive(this.animationElements[i].X.Evaluate(this.time.floatValue) > 0f);
				}
				break;
			}
			case MoveModule.Type.Inactive:
			{
				bool flag2 = this.animationElements[i].transform.gameObject.activeSelf != this.animationElements[i].X.Evaluate(this.time.floatValue) <= 0f;
				if (flag2)
				{
					this.animationElements[i].transform.gameObject.SetActive(this.animationElements[i].X.Evaluate(this.time.floatValue) <= 0f);
				}
				break;
			}
			case MoveModule.Type.SoundVolume:
				this.animationElements[i].audioSource.volume = this.animationElements[i].X.Evaluate(this.time.floatValue);
				break;
			}
		}
	}

	public MoveModule()
	{
	}

	public string pourpose;

	public FloatValueHolder time;

	public FloatValueHolder targetTime;

	public float animationTime;

	public MoveModule.MoveData[] animationElements = new MoveModule.MoveData[0];

	[Serializable]
	public class MoveData
	{
		public MoveData()
		{
			this.X = new AnimationCurve();
			this.Y = new AnimationCurve();
			this.gradient = new Gradient();
		}

		public MoveModule.Type type;

		public Transform transform;

		public SpriteRenderer spriteRenderer;

		public Image image;

		public AnimationCurve X;

		public AnimationCurve Y;

		public Gradient gradient;

		public AudioSource audioSource;
	}

	public enum Type
	{
		RotationZ,
		Scale,
		Position,
		CenterOfMass,
		CenterOfDrag,
		SpriteColor,
		ImageColor,
		Active,
		Inactive,
		SoundVolume
	}
}
