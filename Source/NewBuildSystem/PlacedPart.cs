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
                if (flag && flag2)
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
                    if (!area.hitboxOnly && !area2.hitboxOnly)
                    {
                        bool flag = Utility.LineOverlaps(a.GetWorldPosition(area.start).x, a.GetWorldPosition(area.start + area.size).x, b.GetWorldPosition(area2.start).x, b.GetWorldPosition(area2.start + area2.size).x);
                        bool flag2 = Utility.LineOverlaps(a.GetWorldPosition(area.start).y, a.GetWorldPosition(area.start + area.size).y, b.GetWorldPosition(area2.start).y, b.GetWorldPosition(area2.start + area2.size).y);
                        if (flag && flag2)
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
            foreach (PartData.AttachmentSurface surfaceA in this.partData.attachmentSurfaces)
            {
                this.UpdateSuraface(surfaceA, parts);
            }
        }

        public void UpdateSuraface(PartData.AttachmentSurface surfaceA, List<PlacedPart> parts)
        {
            float num = 0f;
            foreach (PlacedPart placedPart in parts)
            {
                if (placedPart.partData != null)
                {
                    foreach (PartData.AttachmentSurface surfaceB in placedPart.partData.attachmentSurfaces)
                    {
                        if (placedPart != this)
                        {
                            num += Mathf.Max(0f, PlacedPart.SurfacesConnect(this, surfaceA, placedPart, surfaceB));
                        }
                    }
                }
            }
            bool flag = num > 0f;
            foreach (PartData.AttachmentSprite attachmentSprite in this.partData.attachmentSprites)
            {
                if (attachmentSprite.surfaceId != -1)
                {
                    if (this.partData.attachmentSurfaces[attachmentSprite.surfaceId] == surfaceA)
                    {
                        Utility.GetByPath(this.partIcon, attachmentSprite.path).gameObject.SetActive(flag != attachmentSprite.inverse);
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
