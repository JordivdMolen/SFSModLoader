using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

    public void TargetTime(float none)
    {
        base.enabled = true;
    }

    private void Update()
    {
        float num = Time.deltaTime / this.animationTime;
        if (Mathf.Abs(this.targetTime.floatValue - this.time.floatValue) < num)
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

    public void AddToTargetTime(float change)
    {
        this.targetTime.floatValue += change;
        base.enabled = true;
    }

    public void AddToTargetTimeClamped(string data)
    {
        string[] array = data.Split(new char[]
        {
            ","[0]
        });
        if (array.Length != 3)
        {
            return;
        }
        this.targetTime.floatValue = Mathf.Clamp(this.targetTime.floatValue + float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
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
                    this.animationElements[i].transform.localEulerAngles = new Vector3(0f, 0f, this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset));
                    break;
                case MoveModule.Type.Scale:
                    this.animationElements[i].transform.localScale = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset), this.animationElements[i].Y.Evaluate(this.time.floatValue - this.animationElements[i].offset), 0f);
                    break;
                case MoveModule.Type.Position:
                    this.animationElements[i].transform.localPosition = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset), this.animationElements[i].Y.Evaluate(this.time.floatValue - this.animationElements[i].offset), 0f);
                    break;
                case MoveModule.Type.CenterOfMass:
                    this.part.centerOfMass = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset), this.animationElements[i].Y.Evaluate(this.time.floatValue - this.animationElements[i].offset), 0f);
                    break;
                case MoveModule.Type.CenterOfDrag:
                    this.part.centerOfDrag = new Vector3(this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset), this.animationElements[i].Y.Evaluate(this.time.floatValue - this.animationElements[i].offset), 0f);
                    break;
                case MoveModule.Type.SpriteColor:
                    this.animationElements[i].spriteRenderer.color = this.animationElements[i].gradient.Evaluate(this.time.floatValue - this.animationElements[i].offset);
                    break;
                case MoveModule.Type.ImageColor:
                    this.animationElements[i].image.color = this.animationElements[i].gradient.Evaluate(this.time.floatValue - this.animationElements[i].offset);
                    break;
                case MoveModule.Type.Active:
                    if (this.animationElements[i].transform.gameObject.activeSelf != this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset) > 0f)
                    {
                        this.animationElements[i].transform.gameObject.SetActive(this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset) > 0f);
                    }
                    break;
                case MoveModule.Type.Inactive:
                    if (this.animationElements[i].transform.gameObject.activeSelf != this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset) <= 0f)
                    {
                        this.animationElements[i].transform.gameObject.SetActive(this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset) <= 0f);
                    }
                    break;
                case MoveModule.Type.SoundVolume:
                    this.animationElements[i].audioSource.volume = this.animationElements[i].X.Evaluate(this.time.floatValue - this.animationElements[i].offset);
                    break;
            }
        }
    }

    public string pourpose;

    [InlineProperty]
    [FoldoutGroup("", 0)]
    public FloatValueHolder time;

    [InlineProperty]
    [FoldoutGroup("", 0)]
    public FloatValueHolder targetTime;

    [FoldoutGroup("", 0)]
    public float animationTime;

    [Space]
    [InlineProperty]
    [FoldoutGroup("", 0)]
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

        private bool ShowTransform()
        {
            return (new bool[]
            {
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                true,
                true,
                false
            })[(int)this.type];
        }

        private bool ShowSpriteRenderer()
        {
            return this.type == MoveModule.Type.SpriteColor;
        }

        private bool ShowImage()
        {
            return this.type == MoveModule.Type.ImageColor;
        }

        private bool ShowCurveX()
        {
            return (new bool[]
            {
                true,
                true,
                true,
                true,
                true,
                false,
                false,
                true,
                true,
                true
            })[(int)this.type];
        }

        private bool ShowCurveY()
        {
            return (new bool[]
            {
                false,
                true,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                false
            })[(int)this.type];
        }

        private bool ShowGradient()
        {
            return this.type == MoveModule.Type.SpriteColor || this.type == MoveModule.Type.ImageColor;
        }

        private bool ShowAudio()
        {
            return this.type == MoveModule.Type.SoundVolume;
        }

        public MoveModule.Type type;

        public float offset;

        [ShowIf("ShowTransform", true)]
        public Transform transform;

        [ShowIf("ShowSpriteRenderer", true)]
        public SpriteRenderer spriteRenderer;

        [ShowIf("ShowImage", true)]
        public Image image;

        [ShowIf("ShowCurveX", true)]
        public AnimationCurve X;

        [ShowIf("ShowCurveY", true)]
        public AnimationCurve Y;

        [ShowIf("ShowGradient", true)]
        public Gradient gradient;

        [ShowIf("ShowAudio", true)]
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
