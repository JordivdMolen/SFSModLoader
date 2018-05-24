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
                return (!Ref.hasPartsExpansion) ? this.buildGridSizeSmall.x : this.buildGridSizeExtended.x;
            }
        }

        public float height
        {
            get
            {
                return (!Ref.hasPartsExpansion) ? this.buildGridSizeSmall.y : this.buildGridSizeExtended.y;
            }
        }

        private void Start()
        {
            this.SetBuildGridSize();
        }

        [Button(ButtonSizes.Small)]
        public void SetBuildGridSize()
        {
            this.gridSprite1.size = new Vector2(this.width, this.height - 0.5f) * 2f;
            this.gridSprite2.size = new Vector2(this.width, this.height - 0.5f) / 2f;
            Transform transform = this.gridSprite1.transform;
            Vector3 position = new Vector2(this.width, this.height - 0.5f) / 2f;
            this.gridSprite1.transform.position = position;
            transform.position = position;
            this.topShade.position = new Vector3(0f, this.height - 0.5f, 0f);
            this.rightShade.position = new Vector3(this.width, 0f, 0f);
            this.topShade.localScale = new Vector3(this.width, 22f, 1f);
            this.rightShade.localScale = new Vector3(5f, this.height + 22f - 0.5f, 1f);
        }

        public void CreateUndoSave()
        {
            if (this.undoSaves.Count >= 15)
            {
                this.undoSaves.RemoveAt(0);
            }
            this.undoSaves.Add(new PartGrid.UndoSave(this.parts, this.inactiveParts));
            this.undoIcon.color = new Color(1f, 1f, 1f, 0.9f);
        }

        public void Undo()
        {
            if (this.undoSaves.Count == 0)
            {
                return;
            }
            this.LoadBuild(this.undoSaves[this.undoSaves.Count - 1].parts, this.undoSaves[this.undoSaves.Count - 1].innactiveParts);
            this.undoSaves.RemoveAt(this.undoSaves.Count - 1);
            this.undoIcon.color = ((this.undoSaves.Count != 0) ? new Color(1f, 1f, 1f, 0.9f) : new Color(1f, 1f, 1f, 0.6f));
        }

        public Build.HoldingPart TryTakePart(Vector2 mousePos, bool remove)
        {
            Vector2 vector = mousePos - (Vector2)base.transform.position;
            PlacedPart placedPart = PartGrid.PointCastParts(vector, this.parts);
            if (placedPart == null)
            {
                placedPart = PartGrid.PointCastParts(vector, this.inactiveParts);
            }
            if (placedPart == null)
            {
                return null;
            }
            this.CreateUndoSave();
            if (remove)
            {
                this.RemovePart(placedPart);
            }
            Ref.inputController.PlayClickSound(0.2f);
            return new Build.HoldingPart(placedPart.position - vector, new PlacedPart(null, placedPart.position, placedPart.orientation.DeepCopy(), placedPart.partData));
        }

        public void PlacePart(PlacedPart newPart, bool placeActive)
        {
            this.ClampToLimits(newPart);
            this.CreateUndoSave();
            Ref.inputController.PlayClickSound(0.6f);
            if (placeActive)
            {
                for (int i = 0; i < this.parts.Count; i++)
                {
                    if (PlacedPart.IsOverplaing(newPart, this.parts[i]))
                    {
                        UnityEngine.Object.Destroy(this.parts[i].partIcon.gameObject);
                        this.parts[i].partIcon = PartGrid.LoadIcon(this.iconPrefab, this.parts[i].partData.prefab, this.parts[i].position, this.parts[i].partData.prefab.localScale, base.transform, -50, this.parts[i].orientation, Color.white, true);
                        this.inactiveParts.Add(this.parts[i]);
                        this.parts.RemoveAt(i);
                        i--;
                    }
                }
                this.parts.Add(newPart);
                newPart.partIcon = PartGrid.LoadIcon(this.iconPrefab, newPart.partData.prefab, newPart.position, newPart.partData.prefab.localScale, base.transform, 0, newPart.orientation, Color.white, true);
                foreach (PartData.AttachmentSurface surfaceA in newPart.partData.attachmentSurfaces)
                {
                    foreach (PlacedPart placedPart in this.parts)
                    {
                        foreach (PartData.AttachmentSurface surfaceB in placedPart.partData.attachmentSurfaces)
                        {
                            if (PlacedPart.SurfacesConnect(newPart, surfaceA, placedPart, surfaceB) > 0f)
                            {
                                placedPart.UpdateConnected(this.parts);
                            }
                        }
                    }
                }
            }
            else
            {
                this.inactiveParts.Add(newPart);
                newPart.partIcon = PartGrid.LoadIcon(this.iconPrefab, newPart.partData.prefab, newPart.position, newPart.partData.prefab.localScale, base.transform, -50, newPart.orientation, Color.white, true);
            }
            newPart.UpdateConnected(this.parts);
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
            if (partToRemove.partIcon != null)
            {
                UnityEngine.Object.Destroy(partToRemove.partIcon.gameObject);
            }
            this.parts.Remove(partToRemove);
            this.inactiveParts.Remove(partToRemove);
            this.UpdateInactive();
            foreach (PartData.AttachmentSurface surfaceA in partToRemove.partData.attachmentSurfaces)
            {
                foreach (PlacedPart placedPart in this.parts)
                {
                    foreach (PartData.AttachmentSurface surfaceB in placedPart.partData.attachmentSurfaces)
                    {
                        if (PlacedPart.SurfacesConnect(partToRemove, surfaceA, placedPart, surfaceB) > 0f)
                        {
                            placedPart.UpdateConnected(this.parts);
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateInactive()
        {
            List<PlacedPart> list = new List<PlacedPart>();
            for (int i = 0; i < this.inactiveParts.Count; i++)
            {
                if (Ref.hasPartsExpansion || !this.inactiveParts[i].partData.inFull)
                {
                    bool flag = false;
                    for (int j = 0; j < this.parts.Count; j++)
                    {
                        if (PlacedPart.IsOverplaing(this.inactiveParts[i], this.parts[j]))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        UnityEngine.Object.Destroy(this.inactiveParts[i].partIcon.gameObject);
                        this.inactiveParts[i].partIcon = PartGrid.LoadIcon(this.iconPrefab, this.inactiveParts[i].partData.prefab, this.inactiveParts[i].position, this.inactiveParts[i].partData.prefab.localScale, base.transform, 0, this.inactiveParts[i].orientation, Color.white, true);
                        list.Add(this.inactiveParts[i]);
                        MonoBehaviour.print(this.inactiveParts[i].partData.name);
                        this.parts.Add(this.inactiveParts[i]);
                        this.inactiveParts.RemoveAt(i);
                        i--;
                    }
                }
            }
            for (int k = 0; k < list.Count; k++)
            {
                list[k].UpdateConnected(this.parts);
            }
        }

        public static PlacedPart PointCastParts(Vector2 pointPositionLocal, List<PlacedPart> castParts)
        {
            PlacedPart placedPart = null;
            for (int i = 0; i < castParts.Count; i++)
            {
                if (PlacedPart.IsInside(pointPositionLocal, castParts[i], 0f) && (placedPart == null || castParts[i].partData.depth >= placedPart.partData.depth))
                {
                    placedPart = castParts[i];
                }
            }
            if (placedPart != null)
            {
                return placedPart;
            }
            for (int j = 0; j < castParts.Count; j++)
            {
                if (PlacedPart.IsInside(pointPositionLocal, castParts[j], 0.2f) && (placedPart == null || castParts[j].partData.depth >= placedPart.partData.depth))
                {
                    placedPart = castParts[j];
                }
            }
            return placedPart;
        }

        public static void CenterParts(List<PlacedPart> parts, Vector2 targetCenter)
        {
            Vector2 zero = Vector2.zero;
            Vector2 zero2 = Vector2.zero;
            PartGrid.GetMinMax(parts, ref zero2, ref zero);
            Vector2 b = (zero2 + zero) / 2f - targetCenter;
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].position -= b;
            }
        }

        public static void GetMinMax(List<PlacedPart> parts, ref Vector2 min, ref Vector2 max)
        {
            if (parts.Count == 0)
            {
                return;
            }
            min = new Vector2(1000f, 1000f);
            max = new Vector2(-1000f, -100f);
            foreach (PlacedPart placedPart in parts)
            {
                foreach (PartData.Area area in placedPart.partData.areas)
                {
                    Vector2 vector = placedPart.position + area.start * placedPart.orientation;
                    Vector2 vector2 = placedPart.position + (area.start + area.size) * placedPart.orientation;
                    max = new Vector2(Mathf.Max(new float[]
                    {
                        max.x,
                        vector.x,
                        vector2.x
                    }), Mathf.Max(new float[]
                    {
                        max.y,
                        vector.y,
                        vector2.y
                    }));
                    min = new Vector2(Mathf.Min(new float[]
                    {
                        min.x,
                        vector.x,
                        vector2.x
                    }), Mathf.Min(new float[]
                    {
                        min.y,
                        vector.y,
                        vector2.y
                    }));
                }
            }
        }

        public static void PositionForLaunch(List<Build.BuildSave.PlacedPartSave> parts, PartDatabase partDatabase, int rotation)
        {
            Vector2 zero = Vector2.zero;
            Vector2 a = Vector2.one * float.PositiveInfinity;
            foreach (Build.BuildSave.PlacedPartSave placedPartSave in parts)
            {
                PartData partByName = partDatabase.GetPartByName(placedPartSave.partName);
                if (!(partByName == null))
                {
                    foreach (PartData.Area area in partByName.areas)
                    {
                        Vector2 vector = placedPartSave.position + area.start * placedPartSave.orientation;
                        Vector2 vector2 = placedPartSave.position + (area.start + area.size) * placedPartSave.orientation;
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
            }
            for (int j = 0; j < parts.Count; j++)
            {
                parts[j].position -= a - new Vector2(Utility.RoundToHalf((a.x - zero.x) / 2f), 0f);
            }
        }

        public static void CenterToPositon(List<Build.BuildSave.PlacedPartSave> parts, PartDatabase partDatabase, ref Vector2 size, Vector2 targetCenter)
        {
            Vector2 zero = Vector2.zero;
            Vector2 vector = Vector2.one * float.PositiveInfinity;
            foreach (Build.BuildSave.PlacedPartSave placedPartSave in parts)
            {
                PartData partByName = partDatabase.GetPartByName(placedPartSave.partName);
                if (!(partByName == null))
                {
                    foreach (PartData.Area area in partByName.areas)
                    {
                        Vector2 vector2 = placedPartSave.position + area.start * placedPartSave.orientation;
                        Vector2 vector3 = placedPartSave.position + (area.start + area.size) * placedPartSave.orientation;
                        zero = new Vector2(Mathf.Max(new float[]
                        {
                            zero.x,
                            vector2.x,
                            vector3.x
                        }), Mathf.Max(new float[]
                        {
                            zero.y,
                            vector2.y,
                            vector3.y
                        }));
                        vector = new Vector2(Mathf.Min(new float[]
                        {
                            vector.x,
                            vector2.x,
                            vector3.x
                        }), Mathf.Min(new float[]
                        {
                            vector.y,
                            vector2.y,
                            vector3.y
                        }));
                    }
                }
            }
            Vector2 b = (vector + zero) / 2f;
            Vector2 b2 = targetCenter - b;
            for (int j = 0; j < parts.Count; j++)
            {
                parts[j].position += b2;
            }
            size = zero - vector;
        }

        public void ClearBuildSpace()
        {
            this.DeleteAllIcons();
            this.parts = new List<PlacedPart>();
            this.inactiveParts = new List<PlacedPart>();
        }

        public void LoadBuild(List<PlacedPart> newParts, List<PlacedPart> newInactiveParts)
        {
            this.DeleteAllIcons();
            Vector2 zero = Vector2.zero;
            Vector2 zero2 = Vector2.zero;
            PartGrid.GetMinMax(newParts, ref zero2, ref zero);
            Vector2 vector = new Vector2(Mathf.Max(0f, zero.x - this.width), Mathf.Max(0f, zero.y - (this.height - 0.5f)));
            Vector2 b = new Vector2(Mathf.Min(zero2.x, vector.x), Mathf.Min(zero2.y, vector.y));
            for (int i = 0; i < newParts.Count; i++)
            {
                newParts[i].position -= b;
            }
            if (!Ref.hasPartsExpansion)
            {
                for (int j = 0; j < newParts.Count; j++)
                {
                    if (newParts[j].partData.inFull)
                    {
                        newInactiveParts.Add(newParts[j]);
                        newParts.RemoveAt(j);
                        j--;
                    }
                }
            }
            this.parts = newParts;
            this.inactiveParts = newInactiveParts;
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
            if (icon.GetComponent<SpriteRenderer>() != null)
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
                    if (component != null)
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
                if (this.parts[i].partData != null && this.parts[i].partData.prefab != null)
                {
                    this.parts[i].partIcon = PartGrid.LoadIcon(this.iconPrefab, this.parts[i].partData.prefab, this.parts[i].position, this.parts[i].partData.prefab.localScale, base.transform, 0, this.parts[i].orientation, Color.white, true);
                }
            }
            for (int j = 0; j < this.inactiveParts.Count; j++)
            {
                if (this.inactiveParts[j].partData != null && this.inactiveParts[j].partData.prefab != null)
                {
                    this.inactiveParts[j].partIcon = PartGrid.LoadIcon(this.iconPrefab, this.inactiveParts[j].partData.prefab, this.inactiveParts[j].position, this.inactiveParts[j].partData.prefab.localScale, base.transform, -50, this.inactiveParts[j].orientation, Color.white, true);
                }
            }
            this.UpdateAllConnections();
        }

        public void DeleteAllIcons()
        {
            foreach (PlacedPart placedPart in this.parts)
            {
                if (placedPart.partIcon != null)
                {
                    UnityEngine.Object.DestroyImmediate(placedPart.partIcon.gameObject);
                }
            }
            foreach (PlacedPart placedPart2 in this.inactiveParts)
            {
                if (placedPart2.partIcon != null)
                {
                    UnityEngine.Object.DestroyImmediate(placedPart2.partIcon.gameObject);
                }
            }
        }

        public void UpdateAllConnections()
        {
            foreach (PlacedPart placedPart in this.parts)
            {
                placedPart.UpdateConnected(this.parts);
            }
            foreach (PlacedPart placedPart2 in this.inactiveParts)
            {
                placedPart2.UpdateConnected(new List<PlacedPart>());
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
                if (this.parts[i].partData.prefab.GetComponent<ControlModule>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasParachute()
        {
            for (int i = 0; i < this.parts.Count; i++)
            {
                if (this.parts[i].partData.prefab.GetComponent<ParachuteModule>() != null)
                {
                    return true;
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
                if (!(placedPart.partData.prefab.GetComponent<EngineModule>() == null))
                {
                    if (!Utility.GetByPath(placedPart.partIcon, placedPart.partData.prefab.GetComponent<EngineModule>().interstagePath).gameObject.activeSelf)
                    {
                        num += placedPart.partData.prefab.GetComponent<EngineModule>().thrust;
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

        public Transform iconPrefab;

        [BoxGroup]
        public Vector2 buildGridSizeSmall;

        [BoxGroup]
        public Vector2 buildGridSizeExtended;

        [BoxGroup]
        public SpriteRenderer gridSprite1;

        [BoxGroup]
        public SpriteRenderer gridSprite2;

        [BoxGroup]
        public Transform topShade;

        [BoxGroup]
        public Transform rightShade;

        public List<PlacedPart> parts;

        public List<PlacedPart> inactiveParts;

        [BoxGroup("Undo", true, false, 0)]
        public Image undoIcon;

        [BoxGroup("Undo", true, false, 0)]
        public List<PartGrid.UndoSave> undoSaves;

        [Serializable]
        public class UndoSave
        {
            public UndoSave(List<PlacedPart> parts, List<PlacedPart> innactiveParts)
            {
                this.parts = new List<PlacedPart>();
                for (int i = 0; i < parts.Count; i++)
                {
                    this.parts.Add(new PlacedPart(null, parts[i].position, parts[i].orientation, parts[i].partData));
                }
                this.innactiveParts = new List<PlacedPart>();
                for (int j = 0; j < innactiveParts.Count; j++)
                {
                    this.parts.Add(new PlacedPart(null, innactiveParts[j].position, innactiveParts[j].orientation, innactiveParts[j].partData));
                }
            }

            public List<PlacedPart> parts;

            public List<PlacedPart> innactiveParts;
        }
    }
}
