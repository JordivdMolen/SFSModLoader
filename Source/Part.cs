using System;
using System.Collections.Generic;
using NewBuildSystem;
using SFSML.HookSystem.ReWork;
using SFSML.HookSystem.ReWork.BaseHooks.PartHooks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Text;

[DisallowMultipleComponent]
public class Part : MonoBehaviour
{
	private void Awake()
	{
		this.modules = base.GetComponents<Module>();
	}

	private void OnValidate()
	{
		this.modules = base.GetComponents<Module>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		this.Collision(collision);
	}

	public virtual void UsePart()
	{
        MyPartUseHook myPartUseHook = new MyPartUseHook(this);
        myPartUseHook = MyHookSystem.executeHook<MyPartUseHook>(myPartUseHook);

		bool flag = myPartUseHook.isCanceled();
		if (!flag)
		{
			bool flag2 = this.useEvent;
			if (flag2)
			{
				this.onPartUsed.Invoke();
			}
			for (int i = 0; i < this.modules.Length; i++)
			{
				this.modules[i].OnPartUsed();
			}
			new MyPartUsedHook(this).executeDefault();
		}
	}

	public void Collision(Collision2D collision)
	{
		bool flag = !this.vessel.partsManager.partToPartDamage;
		if (!flag)
		{
			bool flag2 = collision.relativeVelocity.sqrMagnitude < 30f;
			if (!flag2)
			{
				this.DestroyPart(true, false);
			}
		}
	}

	public void DestroyPart(bool createExplosion, bool disablePartToPartDamage)
	{
		MyPartBeforeDestroyHook myPartBeforeDestroyHook = new MyPartBeforeDestroyHook(this).execute<MyPartBeforeDestroyHook>();
		bool flag = myPartBeforeDestroyHook.isCanceled();
		if (!flag)
		{
			while (this.joints.Count > 0)
			{
				Part.DestroyJoint(this.joints[0], disablePartToPartDamage);
			}
			this.vessel.partsManager.parts.Remove(this);
			base.gameObject.SetActive(false);
			this.vessel.partsManager.UpdatePartsGruping(disablePartToPartDamage, this.vessel);
			if (createExplosion)
			{
				Transform transform = UnityEngine.Object.Instantiate<Transform>(Ref.controller.explosionParticle, base.transform.TransformPoint(this.centerOfMass), Quaternion.identity);
				transform.localScale = Vector3.one * (this.mass * 1.8f + 0.5f);
				UnityEngine.Object.Destroy(transform.gameObject, 8f);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void GetDrag(float velocityAngle, float rot, ref float sumedDrag, ref Vector2 sumedCenterOfDrag)
	{
		float num = 0f;
		for (int i = 0; i < this.activeDragSurfaces.Count; i++)
		{
			bool point = this.activeDragSurfaces[i].point;
			if (point)
			{
				num += this.activeDragSurfaces[i].size;
			}
			else
			{
				float num2 = Mathf.Cos(this.activeDragSurfaces[i].angleRad + rot - velocityAngle);
				bool flag = num2 > 0f;
				if (flag)
				{
					num += num2 * num2 * this.activeDragSurfaces[i].size;
				}
			}
		}
		sumedDrag += num * this.partData.dragMultiplier;
		sumedCenterOfDrag.x += (base.transform.localPosition.x + this.centerOfDrag.x) * num;
		sumedCenterOfDrag.y += (base.transform.localPosition.y + this.centerOfDrag.y) * num;
	}

	public static void DestroyJoint(Part.Joint jointToDestroy, bool disablePartToPartDamage)
	{
		jointToDestroy.fromPart.joints.Remove(jointToDestroy);
		jointToDestroy.toPart.joints.Remove(jointToDestroy);
		jointToDestroy.fromPart.UpdateConnected();
		jointToDestroy.toPart.UpdateConnected();
		jointToDestroy.fromPart.vessel.partsManager.UpdatePartsGruping(disablePartToPartDamage, jointToDestroy.fromPart.vessel);
	}

	public void UpdateConnected()
	{
		float[] surfacesCoveredAmount = this.GetSurfacesCoveredAmount();
		for (int i = 0; i < this.partData.attachmentSprites.Length; i++)
		{
			bool flag = surfacesCoveredAmount[this.partData.attachmentSprites[i].surfaceId] > 0f;
			Utility.GetByPath(base.transform, this.partData.attachmentSprites[i].path).gameObject.SetActive(flag != this.partData.attachmentSprites[i].inverse);
		}
		this.UpdateDragSurfaces(surfacesCoveredAmount);
	}

	public float[] GetSurfacesCoveredAmount()
	{
		float[] array = new float[this.partData.attachmentSurfaces.Length];
		for (int i = 0; i < this.joints.Count; i++)
		{
			int num = (!(this.joints[i].fromPart == this)) ? this.joints[i].toSurfaceIndex : this.joints[i].fromSurfaceIndex;
			bool flag = array.Length > num;
			if (flag)
			{
				array[num] += this.joints[i].coveredAmount;
			}
			else
			{
				MonoBehaviour.print("surface index");
			}
		}
		return array;
	}

	public void UpdateDragSurfaces(float[] surfaceCoverAmount)
	{
		this.activeDragSurfaces.Clear();
		bool flag = this.vessel.partsManager.InsideFairing(base.transform.localPosition + this.centerOfMass * this.orientation);
		if (!flag)
		{
			for (int i = 0; i < this.partData.dragSurfaces.Length; i++)
			{
				bool flag2 = this.partData.dragSurfaces[i].surfaceId != -1;
				if (flag2)
				{
					bool flag3 = this.partData.dragSurfaces[i].size > surfaceCoverAmount[this.partData.dragSurfaces[i].surfaceId];
					if (flag3)
					{
						this.activeDragSurfaces.Add(new Part.ActiveDragSurafce(this.partData.dragSurfaces[i], this.orientation, surfaceCoverAmount[this.partData.dragSurfaces[i].surfaceId]));
					}
				}
				else
				{
					this.activeDragSurfaces.Add(new Part.ActiveDragSurafce(this.partData.dragSurfaces[i], this.orientation, 0f));
				}
			}
		}
	}

	public void SetMass(float newMass)
	{
		this.mass = newMass;
	}

	public bool HasEngineModule()
	{
		for (int i = 0; i < this.modules.Length; i++)
		{
			bool flag = this.modules[i] is EngineModule;
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFairingModule()
	{
		for (int i = 0; i < this.modules.Length; i++)
		{
			bool flag = this.modules[i] is FairingModule;
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasDockingPortModule()
	{
		for (int i = 0; i < this.modules.Length; i++)
		{
			bool flag = this.modules[i] is DockingPortModule;
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public Vector2 GetPartPosition(Rigidbody2D rb2d)
	{
		return this.GetPartPosition(rb2d.worldCenterOfMass, rb2d.centerOfMass, rb2d.rotation);
	}

	public Vector2 GetPartPosition(Vector2 worldCenterOfMass, Vector2 localCenterOfMass, float rotation)
	{
		rotation *= 0.0174532924f;
		float num = Mathf.Cos(rotation);
		float num2 = Mathf.Sin(rotation);
		Vector2 vector = (Vector2)base.transform.localPosition - localCenterOfMass;
		return worldCenterOfMass + new Vector2(vector.x * num - vector.y * num2, vector.x * num2 + vector.y * num);
	}

	public static int GetPartListIndex(Part part, List<Part> parts)
	{
		for (int i = 0; i < parts.Count; i++)
		{
			bool flag = parts[i] == part;
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public ResourceModule GetResourceModule()
	{
		for (int i = 0; i < this.modules.Length; i++)
		{
			bool flag = this.modules[i] is ResourceModule;
			if (flag)
			{
				return this.modules[i] as ResourceModule;
			}
		}
		return null;
	}

	public Part()
	{
	}

	[FoldoutGroup("Info", 0)]
	public Vessel vessel;

	[FoldoutGroup("Info", 0)]
	public int connectionCheckId;

	[FoldoutGroup("Part Data", 0)]
	[ReadOnly]
	public PartData partData;

	[FoldoutGroup("Part Data", 0)]
	public float mass;

	[FoldoutGroup("Part Data", 0)]
	public Vector3 centerOfMass;

	[FoldoutGroup("Part Data", 0)]
	public Vector2 centerOfDrag;

	[FoldoutGroup("Part Data", 0)]
	public Orientation orientation;

	[FoldoutGroup("Joints and Active Drag Surface", 0)]
	[TableList]
	public List<Part.Joint> joints;

	[FoldoutGroup("Joints and Active Drag Surface", 0)]
	[TableList]
	public List<Part.ActiveDragSurafce> activeDragSurfaces;

	[BoxGroup]
	public Module[] modules;

	public bool useEvent;

	[ShowIf("useEvent", true)]
	public UnityEvent onPartUsed;

	[Serializable]
	public class Joint
	{
		public Joint(Vector2 anchor, Part fromPart, Part toPart, int fromSurfaceIndex, int toSurfaceIndex, bool fuelFlow)
		{
			this.fromPart = fromPart;
			this.toPart = toPart;
			this.fromSurfaceIndex = fromSurfaceIndex;
			this.toSurfaceIndex = toSurfaceIndex;
			this.anchor = anchor;
			this.fuelFlow = fuelFlow;
			bool flag = fromPart.partData.attachmentSurfaces.Length > fromSurfaceIndex && toPart.partData.attachmentSurfaces.Length > toSurfaceIndex;
			if (flag)
			{
				this.coveredAmount = Part.Joint.SurfacesConnect(anchor, fromPart, fromPart.partData.attachmentSurfaces[fromSurfaceIndex], toPart, toPart.partData.attachmentSurfaces[toSurfaceIndex]);
			}
			else
			{
				MonoBehaviour.print("from " + fromSurfaceIndex);
				MonoBehaviour.print("to " + toSurfaceIndex);
			}
			fromPart.joints.Add(this);
			toPart.joints.Add(this);
		}

		public static float SurfacesConnect(Vector2 anchor, Part partA, PartData.AttachmentSurface surfaceA, Part partB, PartData.AttachmentSurface surfaceB)
		{
			bool flag = Mathf.Abs((surfaceA.size * partA.orientation).x) > 0.1f;
			bool flag2 = Mathf.Abs((surfaceB.size * partB.orientation).x) > 0.1f;
			bool flag3 = flag != flag2;
			float result;
			if (flag3)
			{
				result = 0f;
			}
			else
			{
				Vector2 vector = surfaceA.start * partA.orientation;
				Vector2 vector2 = anchor + surfaceB.start * partB.orientation;
				bool flag4 = (!flag) ? (Mathf.Abs(vector.x - vector2.x) > 0.1f) : (Mathf.Abs(vector.y - vector2.y) > 0.1f);
				if (flag4)
				{
					result = 0f;
				}
				else
				{
					Vector2 vector3 = (surfaceA.start + surfaceA.size) * partA.orientation;
					Vector2 vector4 = anchor + (surfaceB.start + surfaceB.size) * partB.orientation;
					result = ((!flag) ? Utility.Overlap(vector.y, vector3.y, vector2.y, vector4.y) : Utility.Overlap(vector.x, vector3.x, vector2.x, vector4.x));
				}
			}
			return result;
		}

		public Vector2 RelativeAnchor(Part part)
		{
			return (!(this.fromPart == part)) ? (-this.anchor) : this.anchor;
		}

		public Part fromPart;

		public Part toPart;

		public int fromSurfaceIndex;

		public int toSurfaceIndex;

		public Vector2 anchor;

		public float coveredAmount;

		public bool fuelFlow;

		[Serializable]
		public class Save
		{
			public Save(int fromPartId, int toPartId, Vector2 anchor, int fromSurfaceIndex, int toSurfaceIndex, bool fuelFlow)
			{
				this.fromPartId = fromPartId;
				this.toPartId = toPartId;
				this.anchor = anchor;
				this.fromSurfaceIndex = fromSurfaceIndex;
				this.toSurfaceIndex = toSurfaceIndex;
				this.fuelFlow = fuelFlow;
            }

			public static int GetToPartIndex(Part.Joint joint, List<Part> parts)
			{
				for (int i = 0; i < parts.Count; i++)
				{
					bool flag = parts[i] == joint.toPart;
					if (flag)
					{
						return i;
					}
				}
				return -1;
			}

			public int fromPartId;

			public int toPartId;

			public Vector2 anchor;

			public int fromSurfaceIndex;

			public int toSurfaceIndex;

			public bool fuelFlow;
		}
	}

	[Serializable]
	public class ActiveDragSurafce
	{
		public ActiveDragSurafce(PartData.DragSurface dragSurface, Orientation orientation, float coveredArea)
		{
			this.point = dragSurface.point;
			this.size = dragSurface.size - coveredArea;
			this.angleRad = ((!this.point) ? (dragSurface.angleRad * orientation) : float.NaN);
		}

		public bool point;

		[HideIf("point", true)]
		public float angleRad;

		public float size;
	}

	[Serializable]
	public class Save
	{
		public Save(Part part, Orientation orientation, List<Part> parts)
		{
			this.partName = part.partData.name;
			this.orientation = orientation;
			Module.Save[] array = new Module.Save[part.modules.Length];
			for (int i = 0; i < part.modules.Length; i++)
			{
				array[i] = new Module.Save(part.modules[i].GetType().ToString(), part.modules[i].SaveVariables);
			}
			this.moduleSaves = array;


            StringBuilder sb = new StringBuilder();
            foreach (string text in part.partData.tags.Keys)
            {
                sb.Append(string.Concat(new string[]
                {
                    part.partData.tags[text].GetType().AssemblyQualifiedName,
                    "#",
                    text,
                    "#",
                    JsonUtility.ToJson(part.partData.tags[text]),
                    "|"
                }));
            }
            this.tagsString = sb.ToString();
            this.GUID = part.partData.GUID;
        }

		[Header("Part")]
		public string partName;

		public Orientation orientation;

		public Module.Save[] moduleSaves;

        public string tagsString;

        public Guid GUID;
    }
}
