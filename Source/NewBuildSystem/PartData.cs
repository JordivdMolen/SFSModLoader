using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NewBuildSystem
{
	[CreateAssetMenu]
	public class PartData : ScriptableObject
	{
		[Button(0)]
		[FoldoutGroup("Surfaces", 0)]
		public void TransferSurfaces()
		{
			List<PartData.DragSurface> list = new List<PartData.DragSurface>();
			for (int i = 0; i < this.attachmentSurfaces.Length; i++)
			{
				float num = Mathf.Atan2(this.attachmentSurfaces[i].size.y, this.attachmentSurfaces[i].size.x);
				bool flag = Vector3.Cross(this.attachmentSurfaces[i].start - this.centerOfRotation, this.attachmentSurfaces[i].size).z > 0f;
				if (flag)
				{
					num += 3.14159274f;
				}
				list.Add(new PartData.DragSurface(i, false, num, this.attachmentSurfaces[i].size.magnitude));
			}
			for (int j = 0; j < this.dragSurfaces.Length; j++)
			{
				bool flag2 = this.dragSurfaces[j].surfaceId == -1;
				if (flag2)
				{
					list.Add(this.dragSurfaces[j]);
				}
			}
			this.dragSurfaces = list.ToArray();
		}

		public virtual string GetDescriptionRaw()
		{
			string str = string.Empty;
			str = str + "Mass: " + this.prefab.GetComponent<Part>().mass.ToString() + "t/";
			Part component = this.prefab.GetComponent<Part>();
			List<string> list = new List<string>();
			for (int i = 0; i < component.modules.Length; i++)
			{
				list.AddRange(component.modules[i].DescriptionVariables);
			}
			for (int j = 0; j < list.Count; j++)
			{
				str = str + list[j] + "/";
			}
			return str + this.description;
		}

		[Button(0)]
		public void OnValidate()
		{
			bool flag = this.prefab != null;
			if (flag)
			{
				this.prefab.GetComponent<Part>().partData = this;
			}
			for (int i = 0; i < this.dragSurfaces.Length; i++)
			{
				bool flag2 = this.dragSurfaces[i].surfaceId == -1;
				if (flag2)
				{
					this.dragSurfaces[i].angleRad = this.dragSurfaces[i].angleDeg * 0.0174532924f;
				}
			}
			for (int j = 0; j < this.dragSurfaces.Length; j++)
			{
				Debug.DrawRay(new Vector3(Mathf.Cos(this.dragSurfaces[j].angleRad - 3.926991f), Mathf.Sin(this.dragSurfaces[j].angleRad - 3.926991f)) * (this.dragSurfaces[j].size * 0.707f), new Vector3(Mathf.Cos(this.dragSurfaces[j].angleRad), Mathf.Sin(this.dragSurfaces[j].angleRad)) * this.dragSurfaces[j].size, Color.cyan);
			}
		}

		public string displayName;

		[TextArea(1, 10)]
		public string description;

		public Transform prefab;

		public float breakVelocity = 5f;

        //Custom field for custom values saving
        public Dictionary<string, object> tags = new Dictionary<string, object>();

        public Guid GUID = new Guid();

        [Space]
		public PartData.Area[] areas;

		[Space]
		[FoldoutGroup("Surfaces", 0)]
		public PartData.AttachmentSurface[] attachmentSurfaces;

		[Space]
		[FoldoutGroup("Surfaces", 0)]
		public PartData.AttachmentSprite[] attachmentSprites;

		[Space]
		[FoldoutGroup("Surfaces", 0)]
		public float dragMultiplier = 1f;

		[Space]
		[FoldoutGroup("Surfaces", 0)]
		public PartData.DragSurface[] dragSurfaces;

		[BoxGroup("Build Data", true, false, 0)]
		public Vector2 centerOfRotation;

		[BoxGroup("Build Data", true, false, 0)]
		public float depth;

		[BoxGroup("Build Data", true, false, 0)]
		public float pickGridScale = 1f;

		[BoxGroup("Build Data", true, false, 0)]
		public bool flip2stickX;

		[BoxGroup("Build Data", true, false, 0)]
		public bool flip2stickY;

		[BoxGroup("Build Data", true, false, 0)]
		public bool simX;

		[BoxGroup("Build Data", true, false, 0)]
		public bool simY;

		[BoxGroup("Build Data", true, false, 0)]
		public Vector2 pickOffset = Vector2.up;

		[BoxGroup("Build Data", true, false, 0)]
		public PartData.SnapPoint[] snapPoints = new PartData.SnapPoint[0];

		[BoxGroup("Avalibility Data", true, false, 0)]
		public bool inFull;

		[BoxGroup("Avalibility Data", true, false, 0)]
		public bool disabledOnHacked;

		[Serializable]
		public class Area
		{
			public Area()
			{
			}

			[HorizontalGroup(0f, 0, 0, 0)]
			[LabelWidth(50f)]
			public Vector2 size;

			[HorizontalGroup(0f, 0, 0, 0)]
			[LabelWidth(50f)]
			public Vector2 start;

			public bool hitboxOnly;
		}

		[Serializable]
		public class AttachmentSurface
		{
			public AttachmentSurface(Vector2 size, Vector2 start)
			{
				this.size = size;
				this.start = start;
			}

			public float surfaceLenght
			{
				get
				{
					return Mathf.Abs(this.size.x + this.size.y);
				}
			}

			[HorizontalGroup(0f, 0, 0, 0)]
			[LabelWidth(50f)]
			public Vector2 size;

			[HorizontalGroup(0f, 0, 0, 0)]
			[LabelWidth(50f)]
			public Vector2 start;

			[HorizontalGroup(0f, 0, 0, 0)]
			public bool fuelFlow;
		}

		[Serializable]
		public class AttachmentSprite
		{
			public AttachmentSprite()
			{
			}

			public int surfaceId;

			public bool showInPick;

			public bool inverse;

			public int[] path;
		}

		[Serializable]
		public class DragSurface
		{
			public DragSurface(int surfaceId, bool point, float angleRad, float size)
			{
				this.surfaceId = surfaceId;
				this.point = point;
				this.angleRad = angleRad;
				this.size = size;
			}

			private bool ShowAngleDeg()
			{
				return this.surfaceId == -1 && !this.point;
			}

			[BoxGroup]
			public int surfaceId = -1;

			[BoxGroup]
			public bool point;

			[BoxGroup]
			[ShowIf("ShowAngleDeg", true)]
			public float angleDeg;

			[BoxGroup]
			[HideIf("point", true)]
			public float angleRad;

			[BoxGroup]
			public float size;
		}

		[Serializable]
		public class SnapPoint
		{
			public SnapPoint()
			{
			}

			[BoxGroup]
			public PartData.SnapPoint.Type type;

			[BoxGroup]
			public Vector2 position;

			[BoxGroup]
			public bool snapX;

			[BoxGroup]
			public bool snapY;

			public enum Type
			{
				FuelTank,
				Separator,
				Fairing
			}
		}
	}
}
