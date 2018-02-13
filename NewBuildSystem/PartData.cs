using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewBuildSystem
{
	[CreateAssetMenu]
	public class PartData : ScriptableObject
	{
		[Serializable]
		public class Area
		{
			[HorizontalGroup(0f, 0, 0, 0), LabelWidth(50f)]
			public Vector2 size;

			[HorizontalGroup(0f, 0, 0, 0), LabelWidth(50f)]
			public Vector2 start;
		}

		[Serializable]
		public class AttachmentSurface
		{
			[HorizontalGroup(0f, 0, 0, 0), LabelWidth(50f)]
			public Vector2 size;

			[HorizontalGroup(0f, 0, 0, 0), LabelWidth(50f)]
			public Vector2 start;

			[HorizontalGroup(0f, 0, 0, 0)]
			public bool fuelFlow;

			public float surfaceLenght
			{
				get
				{
					return Mathf.Abs(this.size.x + this.size.y);
				}
			}

			public AttachmentSurface(Vector2 size, Vector2 start)
			{
				this.size = size;
				this.start = start;
			}
		}

		[Serializable]
		public class AttachmentSprite
		{
			public int surfaceId;

			public bool showInPick;

			public bool inverse;

			public int[] path;
		}

		[Serializable]
		public class DragSurface
		{
			[BoxGroup]
			public int surfaceId = -1;

			[BoxGroup]
			public bool point;

			[BoxGroup, ShowIf("ShowAngleDeg", true)]
			public float angleDeg;

			[BoxGroup, HideIf("point", true)]
			public float angleRad;

			[BoxGroup]
			public float size;

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
		}

		public string displayName;

		[TextArea(1, 10)]
		public string description;

		public Transform prefab;

		public float breakVelocity = 5f;

		[Space]
		public PartData.Area[] areas;

		[FoldoutGroup("Surfaces", 0), Space]
		public PartData.AttachmentSurface[] attachmentSurfaces;

		[FoldoutGroup("Surfaces", 0), Space]
		public PartData.AttachmentSprite[] attachmentSprites;

		[FoldoutGroup("Surfaces", 0), Space]
		public float dragMultiplier = 1f;

		[FoldoutGroup("Surfaces", 0), Space]
		public PartData.DragSurface[] dragSurfaces;

		[BoxGroup("Build Data", true, false, 0)]
		public Vector2 centerOfRotation;

		[BoxGroup("Build Data", true, false, 0)]
		public float pickGridScale = 1f;

		[Button(ButtonSizes.Small), FoldoutGroup("Surfaces", 0)]
		public void TransferSurfaces()
		{
			List<PartData.DragSurface> list = new List<PartData.DragSurface>();
			for (int i = 0; i < this.attachmentSurfaces.Length; i++)
			{
				float num = Mathf.Atan2(this.attachmentSurfaces[i].size.y, this.attachmentSurfaces[i].size.x);
				if (Vector3.Cross(this.attachmentSurfaces[i].start - this.centerOfRotation, this.attachmentSurfaces[i].size).z > 0f)
				{
					num += 3.14159274f;
				}
				list.Add(new PartData.DragSurface(i, false, num, this.attachmentSurfaces[i].size.magnitude));
			}
			for (int j = 0; j < this.dragSurfaces.Length; j++)
			{
				if (this.dragSurfaces[j].surfaceId == -1)
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

		[Button(ButtonSizes.Small)]
		public void OnValidate()
		{
			if (this.prefab != null)
			{
				this.prefab.GetComponent<Part>().partData = this;
			}
			for (int i = 0; i < this.dragSurfaces.Length; i++)
			{
				if (this.dragSurfaces[i].surfaceId == -1)
				{
					this.dragSurfaces[i].angleRad = this.dragSurfaces[i].angleDeg * 0.0174532924f;
				}
			}
			for (int j = 0; j < this.dragSurfaces.Length; j++)
			{
				Debug.DrawRay(new Vector3(Mathf.Cos(this.dragSurfaces[j].angleRad - 3.926991f), Mathf.Sin(this.dragSurfaces[j].angleRad - 3.926991f)) * (this.dragSurfaces[j].size * 0.707f), new Vector3(Mathf.Cos(this.dragSurfaces[j].angleRad), Mathf.Sin(this.dragSurfaces[j].angleRad)) * this.dragSurfaces[j].size, Color.cyan);
			}
		}
	}
}
