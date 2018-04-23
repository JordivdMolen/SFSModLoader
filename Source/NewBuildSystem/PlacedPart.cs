using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NewBuildSystem
{
	[Serializable]
	public class PlacedPart
	{
		public PlacedPart(Transform partIcon, Vector2 position, Orientation orientation, PartData partData)
		{
			this.position = position;
			this.orientation = orientation;
			this.partData = partData;
		}

		public Vector2 GetWorldPosition(Vector2 pos)
		{
			return this.position + pos * this.orientation;
		}

		public static bool IsInside(Vector2 a, PlacedPart b, float edge)
		{
			foreach (PartData.Area area in b.partData.areas)
			{
				bool flag = Utility.IsInsideRange(a.x, b.GetWorldPosition(area.start).x, b.GetWorldPosition(area.start + area.size).x, true, edge);
				bool flag2 = Utility.IsInsideRange(a.y, b.GetWorldPosition(area.start).y, b.GetWorldPosition(area.start + area.size).y, true, edge);
				bool flag3 = flag && flag2;
				if (flag3)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsOverplaing(PlacedPart a, PlacedPart b)
		{
			foreach (PartData.Area area in a.partData.areas)
			{
				foreach (PartData.Area area2 in b.partData.areas)
				{
					bool flag = !area.hitboxOnly && !area2.hitboxOnly;
					if (flag)
					{
						bool flag2 = Utility.LineOverlaps(a.GetWorldPosition(area.start).x, a.GetWorldPosition(area.start + area.size).x, b.GetWorldPosition(area2.start).x, b.GetWorldPosition(area2.start + area2.size).x);
						bool flag3 = Utility.LineOverlaps(a.GetWorldPosition(area.start).y, a.GetWorldPosition(area.start + area.size).y, b.GetWorldPosition(area2.start).y, b.GetWorldPosition(area2.start + area2.size).y);
						bool flag4 = flag2 && flag3;
						if (flag4)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static float SurfacesConnect(PlacedPart partA, PartData.AttachmentSurface surfaceA, PlacedPart partB, PartData.AttachmentSurface surfaceB)
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
				Vector2 worldPosition = partA.GetWorldPosition(surfaceA.start);
				Vector2 worldPosition2 = partB.GetWorldPosition(surfaceB.start);
				bool flag4 = (!flag) ? (Mathf.Abs(worldPosition.x - worldPosition2.x) > 0.1f) : (Mathf.Abs(worldPosition.y - worldPosition2.y) > 0.1f);
				if (flag4)
				{
					result = 0f;
				}
				else
				{
					Vector2 worldPosition3 = partA.GetWorldPosition(surfaceA.start + surfaceA.size);
					Vector2 worldPosition4 = partB.GetWorldPosition(surfaceB.start + surfaceB.size);
					result = ((!flag) ? Utility.Overlap(worldPosition.y, worldPosition3.y, worldPosition2.y, worldPosition4.y) : Utility.Overlap(worldPosition.x, worldPosition3.x, worldPosition2.x, worldPosition4.x));
				}
			}
			return result;
		}

		public void UpdateConnected(List<PlacedPart> parts)
		{
			bool flag = this.partIcon == null;
			if (!flag)
			{
				foreach (PartData.AttachmentSurface surfaceA in this.partData.attachmentSurfaces)
				{
					this.UpdateSuraface(surfaceA, parts);
				}
			}
		}

		public void UpdateSuraface(PartData.AttachmentSurface surfaceA, List<PlacedPart> parts)
		{
			float num = 0f;
			foreach (PlacedPart placedPart in parts)
			{
				bool flag = placedPart.partData != null && placedPart.partIcon.name != "Inactive";
				if (flag)
				{
					foreach (PartData.AttachmentSurface surfaceB in placedPart.partData.attachmentSurfaces)
					{
						bool flag2 = placedPart != this;
						if (flag2)
						{
							num += Mathf.Max(0f, PlacedPart.SurfacesConnect(this, surfaceA, placedPart, surfaceB));
						}
					}
				}
			}
			bool flag3 = num > 0f;
			foreach (PartData.AttachmentSprite attachmentSprite in this.partData.attachmentSprites)
			{
				bool flag4 = attachmentSprite.surfaceId != -1;
				if (flag4)
				{
					bool flag5 = this.partData.attachmentSurfaces[attachmentSprite.surfaceId] == surfaceA;
					if (flag5)
					{
						Utility.GetByPath(this.partIcon, attachmentSprite.path).gameObject.SetActive(flag3 != attachmentSprite.inverse);
					}
				}
				else
				{
					Utility.GetByPath(this.partIcon, attachmentSprite.path).gameObject.SetActive(attachmentSprite.inverse);
				}
			}
		}

		public Transform partIcon;

		[DrawWithUnity]
		[BoxGroup]
		public Vector2 position;

		[BoxGroup]
		[Space]
		public Orientation orientation;

		[Space]
		public PartData partData;
	}
}
