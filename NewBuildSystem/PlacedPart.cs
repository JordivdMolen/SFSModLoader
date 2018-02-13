using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewBuildSystem
{
	[Serializable]
	public class PlacedPart
	{
		public Transform partIcon;

		[BoxGroup, DrawWithUnity]
		public Vector2 position;

		[BoxGroup, Space]
		public Orientation orientation;

		[Space]
		public PartData partData;

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

		public static bool IsInside(Vector2 a, PlacedPart b)
		{
			PartData.Area[] areas = b.partData.areas;
			for (int i = 0; i < areas.Length; i++)
			{
				PartData.Area area = areas[i];
				bool flag = Utility.IsInsideRange(a.x, b.GetWorldPosition(area.start).x, b.GetWorldPosition(area.start + area.size).x, true);
				bool flag2 = Utility.IsInsideRange(a.y, b.GetWorldPosition(area.start).y, b.GetWorldPosition(area.start + area.size).y, true);
				if (flag && flag2)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsOverplaing(PlacedPart a, PlacedPart b)
		{
			PartData.Area[] areas = a.partData.areas;
			for (int i = 0; i < areas.Length; i++)
			{
				PartData.Area area = areas[i];
				PartData.Area[] areas2 = b.partData.areas;
				for (int j = 0; j < areas2.Length; j++)
				{
					PartData.Area area2 = areas2[j];
					bool flag = Utility.LineOverlaps(a.GetWorldPosition(area.start).x, a.GetWorldPosition(area.start + area.size).x, b.GetWorldPosition(area2.start).x, b.GetWorldPosition(area2.start + area2.size).x);
					bool flag2 = Utility.LineOverlaps(a.GetWorldPosition(area.start).y, a.GetWorldPosition(area.start + area.size).y, b.GetWorldPosition(area2.start).y, b.GetWorldPosition(area2.start + area2.size).y);
					if (flag && flag2)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static float SurfacesConnect(PlacedPart partA, PartData.AttachmentSurface surfaceA, PlacedPart partB, PartData.AttachmentSurface surfaceB)
		{
			bool flag = Mathf.Abs((surfaceA.size * partA.orientation).x) > 0.1f;
			bool flag2 = Mathf.Abs((surfaceB.size * partB.orientation).x) > 0.1f;
			if (flag != flag2)
			{
				return 0f;
			}
			Vector2 worldPosition = partA.GetWorldPosition(surfaceA.start);
			Vector2 worldPosition2 = partB.GetWorldPosition(surfaceB.start);
			if ((!flag) ? (Mathf.Abs(worldPosition.x - worldPosition2.x) > 0.1f) : (Mathf.Abs(worldPosition.y - worldPosition2.y) > 0.1f))
			{
				return 0f;
			}
			Vector2 worldPosition3 = partA.GetWorldPosition(surfaceA.start + surfaceA.size);
			Vector2 worldPosition4 = partB.GetWorldPosition(surfaceB.start + surfaceB.size);
			return (!flag) ? Utility.Overlap(worldPosition.y, worldPosition3.y, worldPosition2.y, worldPosition4.y) : Utility.Overlap(worldPosition.x, worldPosition3.x, worldPosition2.x, worldPosition4.x);
		}

		public void UpdateConnected(List<PlacedPart> parts)
		{
			if (this.partIcon == null)
			{
				return;
			}
			PartData.AttachmentSurface[] attachmentSurfaces = this.partData.attachmentSurfaces;
			for (int i = 0; i < attachmentSurfaces.Length; i++)
			{
				PartData.AttachmentSurface surfaceA = attachmentSurfaces[i];
				this.UpdateSuraface(surfaceA, parts);
			}
		}

		public void UpdateSuraface(PartData.AttachmentSurface surfaceA, List<PlacedPart> parts)
		{
			float num = 0f;
			foreach (PlacedPart current in parts)
			{
				if (current.partData != null)
				{
					PartData.AttachmentSurface[] attachmentSurfaces = current.partData.attachmentSurfaces;
					for (int i = 0; i < attachmentSurfaces.Length; i++)
					{
						PartData.AttachmentSurface surfaceB = attachmentSurfaces[i];
						if (current != this)
						{
							num += Mathf.Max(0f, PlacedPart.SurfacesConnect(this, surfaceA, current, surfaceB));
						}
					}
				}
			}
			bool flag = num > 0f;
			PartData.AttachmentSprite[] attachmentSprites = this.partData.attachmentSprites;
			for (int j = 0; j < attachmentSprites.Length; j++)
			{
				PartData.AttachmentSprite attachmentSprite = attachmentSprites[j];
				if (this.partData.attachmentSurfaces[attachmentSprite.surfaceId] == surfaceA)
				{
					Utility.GetByPath(this.partIcon, attachmentSprite.path).gameObject.SetActive(flag != attachmentSprite.inverse);
				}
			}
		}
	}
}
