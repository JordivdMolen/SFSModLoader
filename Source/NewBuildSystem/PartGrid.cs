using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace NewBuildSystem
{
	public class PartGrid : MonoBehaviour
	{
		public float width
		{
			get
			{
				return (!Ref.hasPartsExpansion && !Ref.hasHackedExpansion) ? this.buildGridSizeSmall.x : this.buildGridSizeExtended.x;
			}
		}

		public float height
		{
			get
			{
				return (!Ref.hasPartsExpansion && !Ref.hasHackedExpansion) ? this.buildGridSizeSmall.y : this.buildGridSizeExtended.y;
			}
		}

		private void Start()
		{
			this.SetBuildGridSize();
		}

		[Button(0)]
		public void SetBuildGridSize()
		{
			this.gridSprite1.size = new Vector2(this.width, this.height - 0.5f) * 2f;
			this.gridSprite2.size = new Vector2(this.width, this.height - 0.5f) / 2f;
			Transform transform = this.gridSprite1.transform;
			Vector3 position = new Vector2(this.width, this.height - 0.5f) / 2f;
			this.gridSprite1.transform.position = position;
			transform.position = position;
		}

		public void CreateUndoSave()
		{
			bool flag = this.undoSaves.Count >= 15;
			if (flag)
			{
				this.undoSaves.RemoveAt(0);
			}
			this.undoSaves.Add(new PartGrid.UndoSave(this.parts));
			this.undoIcon.color = new Color(1f, 1f, 1f, 0.9f);
		}

		public void Undo()
		{
			bool flag = this.undoSaves.Count == 0;
			if (!flag)
			{
				this.LoadBuild(this.undoSaves[this.undoSaves.Count - 1].parts);
				this.undoSaves.RemoveAt(this.undoSaves.Count - 1);
				this.undoIcon.color = ((this.undoSaves.Count != 0) ? new Color(1f, 1f, 1f, 0.9f) : new Color(1f, 1f, 1f, 0.6f));
			}
		}

		public Build.HoldingPart TryTakePart(Vector2 mousePos, bool remove)
		{
			Vector2 vector = mousePos - (Vector2)base.transform.position;
			PlacedPart placedPart = PartGrid.PointCastParts(vector, this.parts);
			bool flag = placedPart == null;
			Build.HoldingPart result;
			if (flag)
			{
				result = null;
			}
			else
			{
				this.CreateUndoSave();
				if (remove)
				{
					this.RemovePart(placedPart);
				}
				Ref.inputController.PlayClickSound(0.2f);
				result = new Build.HoldingPart(placedPart.position - vector, new PlacedPart(null, placedPart.position, placedPart.orientation.DeepCopy(), placedPart.partData));
			}
			return result;
		}

		public void PlacePart(PlacedPart newPart)
		{
			this.ClampToLimits(newPart);
			this.CreateUndoSave();
			foreach (PlacedPart placedPart in this.parts)
			{
				bool flag = PlacedPart.IsOverplaing(newPart, placedPart);
				if (flag)
				{
					bool flag2 = !(placedPart.partIcon.name == "Inactive");
					if (flag2)
					{
						UnityEngine.Object.Destroy(placedPart.partIcon.gameObject);
						placedPart.partIcon = PartGrid.LoadIcon(this.iconPrefab, placedPart.partData.prefab, placedPart.position, placedPart.partData.prefab.localScale, base.transform, -50, placedPart.orientation, Color.white, true);
						placedPart.partIcon.name = "Inactive";
					}
				}
			}
			this.parts.Add(newPart);
			newPart.partIcon = PartGrid.LoadIcon(this.iconPrefab, newPart.partData.prefab, newPart.position, newPart.partData.prefab.localScale, base.transform, 0, newPart.orientation, Color.white, true);
			newPart.UpdateConnected(this.parts);
			foreach (PartData.AttachmentSurface surfaceA in newPart.partData.attachmentSurfaces)
			{
				foreach (PlacedPart placedPart2 in this.parts)
				{
					foreach (PartData.AttachmentSurface surfaceB in placedPart2.partData.attachmentSurfaces)
					{
						bool flag3 = PlacedPart.SurfacesConnect(newPart, surfaceA, placedPart2, surfaceB) > 0f;
						if (flag3)
						{
							placedPart2.UpdateConnected(this.parts);
						}
					}
				}
			}
			Ref.inputController.PlayClickSound(0.6f);
			for (int k = 0; k < newPart.partData.areas.Length; k++)
			{
				Vector2 vector = newPart.position + newPart.partData.areas[k].start * newPart.orientation;
				Utility.DrawSquare(vector, vector + newPart.partData.areas[k].size * newPart.orientation, 1f, (!newPart.partData.areas[k].hitboxOnly) ? Color.red : Color.blue);
			}
		}

		public bool Overlap(PlacedPart part)
		{
			for (int i = this.parts.Count - 1; i >= 0; i--)
			{
				bool flag = this.parts[i].partIcon.name != "Inactive" && PlacedPart.IsOverplaing(part, this.parts[i]);
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		public void ClampToLimits(PlacedPart part)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			foreach (PartData.Area area in part.partData.areas)
			{
				Vector2 vector = part.position + area.start * part.orientation;
				Vector2 vector2 = part.position + (area.start + area.size) * part.orientation;
				zero = new Vector2(Mathf.Max(new float[]
				{
					zero.x,
					vector.x,
					vector2.x
				}), Mathf.Max(new float[]
				{
					zero.y,
					vector.y,
					vector2.y
				}));
				zero2 = new Vector2(Mathf.Min(new float[]
				{
					zero2.x,
					vector.x,
					vector2.x
				}), Mathf.Min(new float[]
				{
					zero2.y,
					vector.y,
					vector2.y
				}));
			}
			part.position -= new Vector2(Mathf.Min(0f, zero2.x), Mathf.Min(0f, zero2.y)) + new Vector2(Mathf.Max(0f, zero.x - this.width), Mathf.Max(0f, zero.y - this.height));
		}

		public void RemovePart(PlacedPart partToRemove)
		{
			bool flag = partToRemove.partIcon != null;
			if (flag)
			{
				UnityEngine.Object.Destroy(partToRemove.partIcon.gameObject);
			}
			this.parts.Remove(partToRemove);
			for (int i = 0; i < this.parts.Count; i++)
			{
				bool flag2 = !(this.parts[i].partIcon.name != "Inactive");
				if (flag2)
				{
					bool flag3 = false;
					for (int j = i + 1; j < this.parts.Count; j++)
					{
						bool flag4 = PlacedPart.IsOverplaing(this.parts[i], this.parts[j]);
						if (flag4)
						{
							flag3 = true;
							break;
						}
					}
					bool flag5 = !flag3;
					if (flag5)
					{
						UnityEngine.Object.Destroy(this.parts[i].partIcon.gameObject);
						this.parts[i].partIcon = PartGrid.LoadIcon(this.iconPrefab, this.parts[i].partData.prefab, this.parts[i].position, this.parts[i].partData.prefab.localScale, base.transform, 0, this.parts[i].orientation, Color.white, true);
					}
				}
			}
			foreach (PartData.AttachmentSurface surfaceA in partToRemove.partData.attachmentSurfaces)
			{
				foreach (PlacedPart placedPart in this.parts)
				{
					foreach (PartData.AttachmentSurface surfaceB in placedPart.partData.attachmentSurfaces)
					{
						bool flag6 = PlacedPart.SurfacesConnect(partToRemove, surfaceA, placedPart, surfaceB) > 0f;
						if (flag6)
						{
							placedPart.UpdateConnected(this.parts);
							break;
						}
					}
				}
			}
		}

		public static PlacedPart PointCastParts(Vector2 pointPositionLocal, List<PlacedPart> castParts)
		{
			PlacedPart placedPart = null;
			for (int i = 0; i < castParts.Count; i++)
			{
				bool flag = PlacedPart.IsInside(pointPositionLocal, castParts[i], 0f) && (placedPart == null || castParts[i].partData.depth >= placedPart.partData.depth || (placedPart.partIcon.name == "Inactive" && castParts[i].partIcon.name != "Inactive"));
				if (flag)
				{
					placedPart = castParts[i];
				}
			}
			bool flag2 = placedPart != null;
			PlacedPart result;
			if (flag2)
			{
				result = placedPart;
			}
			else
			{
				for (int j = 0; j < castParts.Count; j++)
				{
					bool flag3 = PlacedPart.IsInside(pointPositionLocal, castParts[j], 0.2f) && (placedPart == null || castParts[j].partData.depth >= placedPart.partData.depth || (placedPart.partIcon.name == "Inactive" && castParts[j].partIcon.name != "Inactive"));
					if (flag3)
					{
						placedPart = castParts[j];
					}
				}
				result = placedPart;
			}
			return result;
		}

		public static void CenterParts(List<PlacedPart> parts, Vector2 targetCenter)
		{
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.one * float.PositiveInfinity;
			foreach (PlacedPart placedPart in parts)
			{
				foreach (PartData.Area area in placedPart.partData.areas)
				{
					Vector2 vector3 = placedPart.position + area.start * placedPart.orientation;
					Vector2 vector4 = placedPart.position + (area.start + area.size) * placedPart.orientation;
					vector = new Vector2(Mathf.Max(new float[]
					{
						vector.x,
						vector3.x,
						vector4.x
					}), Mathf.Max(new float[]
					{
						vector.y,
						vector3.y,
						vector4.y
					}));
					vector2 = new Vector2(Mathf.Min(new float[]
					{
						vector2.x,
						vector3.x,
						vector4.x
					}), Mathf.Min(new float[]
					{
						vector2.y,
						vector3.y,
						vector4.y
					}));
				}
			}
			Vector2 a = Utility.RoundToHalf((vector2 + vector) / 2f + targetCenter);
			for (int j = 0; j < parts.Count; j++)
			{
				parts[j].position += -a;
			}
		}

		public static void PositionForLaunch(List<Build.BuildSave.PlacedPartSave> parts, PartDatabase partDatabase, int rotation)
		{
			Orientation b = new Orientation(1, 1, -rotation);
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].orientation += b;
				parts[i].position *= b;
			}
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.one * float.PositiveInfinity;
			foreach (Build.BuildSave.PlacedPartSave placedPartSave in parts)
			{
				PartData partByName = partDatabase.GetPartByName(placedPartSave.partName);
				bool flag = !(partByName == null);
				if (flag)
				{
					foreach (PartData.Area area in partByName.areas)
					{
						Vector2 vector3 = placedPartSave.position + area.start * placedPartSave.orientation;
						Vector2 vector4 = placedPartSave.position + (area.start + area.size) * placedPartSave.orientation;
						vector = new Vector2(Mathf.Max(new float[]
						{
							vector.x,
							vector3.x,
							vector4.x
						}), Mathf.Max(new float[]
						{
							vector.y,
							vector3.y,
							vector4.y
						}));
						vector2 = new Vector2(Mathf.Min(new float[]
						{
							vector2.x,
							vector3.x,
							vector4.x
						}), Mathf.Min(new float[]
						{
							vector2.y,
							vector3.y,
							vector4.y
						}));
					}
				}
			}
			for (int k = 0; k < parts.Count; k++)
			{
				parts[k].position -= vector2 - new Vector2(Utility.RoundToHalf((vector2.x - vector.x) / 2f), 0f);
			}
		}

		public void LoadBuild(List<PlacedPart> newParts)
		{
			this.DeleteAllIcons();
			this.parts = newParts;
			this.LoadAllIcons();
		}

		public static Transform LoadIcon(Transform iconPrefab, Transform icon, Vector3 localPosition, Vector3 localScale, Transform parent, int sortingOrderAdd, Orientation orientation, Color color, bool forGrid)
		{
			Transform transform = PartGrid.LoadIcon(iconPrefab, icon, localPosition, localScale, parent, sortingOrderAdd, color, forGrid);
			Orientation.ApplyOrientation(transform, orientation);
			return transform;
		}

		public static Transform LoadIcon(Transform iconPrefab, Transform icon, Vector3 localPosition, Vector3 localScale, Transform parent, int sortingOrderAdd, Color color, bool forGrid)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(iconPrefab, parent);
			transform.name = icon.name + " (C)";
			transform.localPosition = localPosition;
			transform.localRotation = icon.localRotation;
			transform.localScale = localScale;
			transform.gameObject.SetActive(icon.gameObject.activeSelf);
			bool flag = icon.GetComponent<SpriteRenderer>() != null;
			if (flag)
			{
				transform.GetComponent<SpriteRenderer>().sprite = icon.GetComponent<SpriteRenderer>().sprite;
				transform.GetComponent<SpriteRenderer>().sortingOrder = icon.GetComponent<SpriteRenderer>().sortingOrder + sortingOrderAdd;
				transform.GetComponent<SpriteRenderer>().drawMode = icon.GetComponent<SpriteRenderer>().drawMode;
				transform.GetComponent<SpriteRenderer>().size = icon.GetComponent<SpriteRenderer>().size;
				transform.GetComponent<SpriteRenderer>().sharedMaterial = icon.GetComponent<SpriteRenderer>().sharedMaterial;
				Color b = Color.white;
				if (forGrid)
				{
					ColorModule component = icon.GetComponent<ColorModule>();
					bool flag2 = component != null;
					if (flag2)
					{
						b = component.buildColor;
					}
				}
				transform.GetComponent<SpriteRenderer>().color = icon.GetComponent<SpriteRenderer>().color * color * b;
			}
			for (int i = 0; i < icon.childCount; i++)
			{
				PartGrid.LoadIcon(iconPrefab, icon.GetChild(i), icon.GetChild(i).localPosition, icon.GetChild(i).localScale, transform, sortingOrderAdd, color, forGrid);
			}
			return transform;
		}

		public void LoadAllIcons()
		{
			this.DeleteAllIcons();
			for (int i = 0; i < this.parts.Count; i++)
			{
				bool flag = !(this.parts[i].partData == null) && !(this.parts[i].partData.prefab == null);
				if (flag)
				{
					bool flag2 = false;
					for (int j = i + 1; j < this.parts.Count; j++)
					{
						bool flag3 = PlacedPart.IsOverplaing(this.parts[i], this.parts[j]);
						if (flag3)
						{
							flag2 = true;
						}
					}
					this.parts[i].partIcon = PartGrid.LoadIcon(this.iconPrefab, this.parts[i].partData.prefab, this.parts[i].position, this.parts[i].partData.prefab.localScale, base.transform, (!flag2) ? 0 : -50, this.parts[i].orientation, Color.white, true);
					bool flag4 = flag2;
					if (flag4)
					{
						this.parts[i].partIcon.name = "Inactive";
					}
				}
			}
			this.UpdateAllConnections();
		}

		public void DeleteAllIcons()
		{
			foreach (PlacedPart placedPart in this.parts)
			{
				bool flag = placedPart.partIcon != null;
				if (flag)
				{
					UnityEngine.Object.DestroyImmediate(placedPart.partIcon.gameObject);
				}
			}
		}

		public void UpdateAllConnections()
		{
			foreach (PlacedPart placedPart in this.parts)
			{
				placedPart.UpdateConnected(this.parts);
			}
		}

		public bool HasAnyParts()
		{
			return this.parts.Count > 0;
		}

		public bool HasControlAuthority()
		{
			for (int i = 0; i < this.parts.Count; i++)
			{
				bool flag = this.parts[i].partIcon.name != "Inactive";
				if (flag)
				{
					for (int j = 0; j < this.parts[i].partData.prefab.GetComponent<Part>().modules.Length; j++)
					{
						bool flag2 = this.parts[i].partData.prefab.GetComponent<Part>().modules[j] is ControlModule;
						if (flag2)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool HasParachute()
		{
			for (int i = 0; i < this.parts.Count; i++)
			{
				bool flag = this.parts[i].partIcon.name != "Inactive";
				if (flag)
				{
					for (int j = 0; j < this.parts[i].partData.prefab.GetComponent<Part>().modules.Length; j++)
					{
						bool flag2 = this.parts[i].partData.prefab.GetComponent<Part>().modules[j] is ParachuteModule;
						if (flag2)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public float GetTWR()
		{
			float thrust = this.GetThrust();
			float num = thrust / 9.8f;
			return num / this.GetMass();
		}

		public float GetThrust()
		{
			float num = 0f;
			foreach (PlacedPart placedPart in this.parts)
			{
				bool flag = !(placedPart.partIcon.name == "Inactive");
				if (flag)
				{
					bool flag2 = !(placedPart.partData.prefab.GetComponent<EngineModule>() == null);
					if (flag2)
					{
						bool flag3 = !Utility.GetByPath(placedPart.partIcon, placedPart.partData.prefab.GetComponent<EngineModule>().interstagePath).gameObject.activeSelf;
						if (flag3)
						{
							num += placedPart.partData.prefab.GetComponent<EngineModule>().thrust;
						}
					}
				}
			}
			return num;
		}

		public float GetMass()
		{
			float num = 0f;
			for (int i = 0; i < this.parts.Count; i++)
			{
				bool flag = this.parts[i].partIcon.name != "Inactive";
				if (flag)
				{
					num += this.parts[i].partData.prefab.GetComponent<Part>().mass;
				}
			}
			return num;
		}

		public PartGrid()
		{
		}

		public Transform iconPrefab;

		[BoxGroup]
		public Vector2 buildGridSizeSmall;

		[BoxGroup]
		public Vector2 buildGridSizeExtended;

		[BoxGroup]
		public SpriteRenderer gridSprite1;

		[BoxGroup]
		public SpriteRenderer gridSprite2;

		public List<PlacedPart> parts;

		[BoxGroup("Undo", true, false, 0)]
		public Image undoIcon;

		[BoxGroup("Undo", true, false, 0)]
		public List<PartGrid.UndoSave> undoSaves;

		[Serializable]
		public class UndoSave
		{
			public UndoSave(List<PlacedPart> parts)
			{
				this.parts = new List<PlacedPart>();
				for (int i = 0; i < parts.Count; i++)
				{
					this.parts.Add(new PlacedPart(null, parts[i].position, parts[i].orientation, parts[i].partData));
				}
			}

			public List<PlacedPart> parts;
		}
	}
}
