using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewBuildSystem
{
	public class PartGrid : MonoBehaviour
	{
		public Transform iconPrefab;

		[BoxGroup]
		public float width;

		[BoxGroup]
		public float height;

		public List<PlacedPart> parts;

		public Build.HoldingPart TryTakePart(Vector2 mousePos, bool remove)
		{
			Vector2 vector = (Vector3)mousePos - base.transform.position;
			PlacedPart placedPart = PartGrid.PointCastParts(vector, this.parts);
			if (placedPart == null)
			{
				return null;
			}
			if (remove)
			{
				this.RemovePart(placedPart);
			}
			Ref.inputController.PlayClickSound(0.2f);
			return new Build.HoldingPart(placedPart.position - vector, new PlacedPart(null, placedPart.position, placedPart.orientation.DeepCopy(), placedPart.partData));
		}

		public void PlacePart(PlacedPart newPart)
		{
			this.ClampToLimits(newPart);
			for (int i = this.parts.Count - 1; i >= 0; i--)
			{
				if (PlacedPart.IsOverplaing(newPart, this.parts[i]))
				{
					this.RemovePart(this.parts[i]);
				}
			}
			if (newPart.partData.prefab != null)
			{
				newPart.partIcon = PartGrid.LoadIcon(this.iconPrefab, newPart.partData.prefab, newPart.position, newPart.partData.prefab.localScale, base.transform, 0, newPart.orientation);
			}
			this.parts.Add(newPart);
			this.UpdateAllConnections();
			Ref.inputController.PlayClickSound(0.6f);
		}

		public bool Overlap(PlacedPart part)
		{
			for (int i = this.parts.Count - 1; i >= 0; i--)
			{
				if (PlacedPart.IsOverplaing(part, this.parts[i]))
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
			PartData.Area[] areas = part.partData.areas;
			for (int i = 0; i < areas.Length; i++)
			{
				PartData.Area area = areas[i];
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
			if (partToRemove.partIcon != null)
			{
				UnityEngine.Object.Destroy(partToRemove.partIcon.gameObject);
			}
			this.parts.Remove(partToRemove);
			this.UpdateAllConnections();
		}

		public static PlacedPart PointCastParts(Vector2 pointPositionLocal, List<PlacedPart> castParts)
		{
			for (int i = 0; i < castParts.Count; i++)
			{
				if (PlacedPart.IsInside(pointPositionLocal, castParts[i]))
				{
					return castParts[i];
				}
			}
			return null;
		}

		public static void CenterParts(List<PlacedPart> parts, Vector2 targetCenter)
		{
			Vector2 zero = Vector2.zero;
			Vector2 a = Vector2.one * float.PositiveInfinity;
			foreach (PlacedPart current in parts)
			{
				PartData.Area[] areas = current.partData.areas;
				for (int i = 0; i < areas.Length; i++)
				{
					PartData.Area area = areas[i];
					Vector2 vector = current.position + area.start * current.orientation;
					Vector2 vector2 = current.position + (area.start + area.size) * current.orientation;
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
					a = new Vector2(Mathf.Min(new float[]
					{
						a.x,
						vector.x,
						vector2.x
					}), Mathf.Min(new float[]
					{
						a.y,
						vector.y,
						vector2.y
					}));
				}
			}
			Vector2 a2 = Utility.RoundToHalf((a + zero) / 2f + targetCenter);
			for (int j = 0; j < parts.Count; j++)
			{
				parts[j].position += -a2;
			}
		}

		public static void PositionForLaunch(List<Build.BuildSave.PlacedPartSave> parts, PartDatabase partDatabase)
		{
			Vector2 zero = Vector2.zero;
			Vector2 a = Vector2.one * float.PositiveInfinity;
			foreach (Build.BuildSave.PlacedPartSave current in parts)
			{
				PartData.Area[] areas = partDatabase.GetPartByName(current.partName).areas;
				for (int i = 0; i < areas.Length; i++)
				{
					PartData.Area area = areas[i];
					Vector2 vector = current.position + area.start * current.orientation;
					Vector2 vector2 = current.position + (area.start + area.size) * current.orientation;
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
					a = new Vector2(Mathf.Min(new float[]
					{
						a.x,
						vector.x,
						vector2.x
					}), Mathf.Min(new float[]
					{
						a.y,
						vector.y,
						vector2.y
					}));
				}
			}
			for (int j = 0; j < parts.Count; j++)
			{
				parts[j].position -= a - new Vector2(Utility.RoundToHalf((a.x - zero.x) / 2f), 0f);
			}
		}

		public void LoadBuild(List<PlacedPart> newParts)
		{
			foreach (PlacedPart current in this.parts)
			{
				if (current.partIcon != null)
				{
					UnityEngine.Object.DestroyImmediate(current.partIcon.gameObject);
				}
			}
			this.parts = newParts;
			this.LoadAllIcons();
		}

		public static Transform LoadIcon(Transform iconPrefab, Transform icon, Vector3 localPosition, Vector3 localScale, Transform parent, int sortingOrderAdd, Orientation orientation)
		{
			Transform transform = PartGrid.LoadIcon(iconPrefab, icon, localPosition, localScale, parent, sortingOrderAdd);
			Orientation.ApplyOrientation(transform, orientation);
			return transform;
		}

		public static Transform LoadIcon(Transform iconPrefab, Transform icon, Vector3 localPosition, Vector3 localScale, Transform parent, int sortingOrderAdd)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(iconPrefab, parent);
			transform.name = icon.name + " (C)";
			transform.localPosition = localPosition;
			transform.localRotation = icon.localRotation;
			transform.localScale = localScale;
			transform.gameObject.SetActive(icon.gameObject.activeSelf);
			if (icon.GetComponent<SpriteRenderer>() != null)
			{
				transform.GetComponent<SpriteRenderer>().sprite = icon.GetComponent<SpriteRenderer>().sprite;
				transform.GetComponent<SpriteRenderer>().sortingOrder = icon.GetComponent<SpriteRenderer>().sortingOrder + sortingOrderAdd;
				transform.GetComponent<SpriteRenderer>().drawMode = icon.GetComponent<SpriteRenderer>().drawMode;
				transform.GetComponent<SpriteRenderer>().size = icon.GetComponent<SpriteRenderer>().size;
				transform.GetComponent<SpriteRenderer>().color = icon.GetComponent<SpriteRenderer>().color;
			}
			for (int i = 0; i < icon.childCount; i++)
			{
				PartGrid.LoadIcon(iconPrefab, icon.GetChild(i), icon.GetChild(i).localPosition, icon.GetChild(i).localScale, transform, sortingOrderAdd);
			}
			return transform;
		}

		public void LoadAllIcons()
		{
			this.DeleteAllIcons();
			foreach (PlacedPart current in this.parts)
			{
				if (current.partData != null && current.partData.prefab != null)
				{
					current.partIcon = PartGrid.LoadIcon(this.iconPrefab, current.partData.prefab, current.position, current.partData.prefab.localScale, base.transform, 0, current.orientation);
				}
			}
			this.UpdateAllConnections();
		}

		public void DeleteAllIcons()
		{
			foreach (PlacedPart current in this.parts)
			{
				if (current.partIcon != null)
				{
					UnityEngine.Object.DestroyImmediate(current.partIcon.gameObject);
				}
			}
		}

		public void UpdateAllConnections()
		{
			foreach (PlacedPart current in this.parts)
			{
				current.UpdateConnected(this.parts);
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
				for (int j = 0; j < this.parts[i].partData.prefab.GetComponent<Part>().modules.Length; j++)
				{
					if (this.parts[i].partData.prefab.GetComponent<Part>().modules[j] is ControlModule)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool HasParachute()
		{
			for (int i = 0; i < this.parts.Count; i++)
			{
				for (int j = 0; j < this.parts[i].partData.prefab.GetComponent<Part>().modules.Length; j++)
				{
					if (this.parts[i].partData.prefab.GetComponent<Part>().modules[j] is ParachuteModule)
					{
						return true;
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
			foreach (PlacedPart current in this.parts)
			{
				if (!(current.partData.prefab.GetComponent<EngineModule>() == null))
				{
					if (!Utility.GetByPath(current.partIcon, current.partData.prefab.GetComponent<EngineModule>().interstagePath).gameObject.activeSelf)
					{
						num += current.partData.prefab.GetComponent<EngineModule>().thrust;
					}
				}
			}
			return num;
		}

		private float GetMass()
		{
			float num = 0f;
			for (int i = 0; i < this.parts.Count; i++)
			{
				num += this.parts[i].partData.prefab.GetComponent<Part>().mass;
			}
			return num;
		}
	}
}
