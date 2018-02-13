using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewBuildSystem
{
	public class PickPartGrid : MonoBehaviour
	{
		[BoxGroup]
		public int width;

		[BoxGroup]
		public int height;

		public Transform iconPrefab;

		[BoxGroup("2", false, false, 0)]
		public int selectedListId;

		[BoxGroup("2", false, false, 0)]
		public PartDatabase[] pickList;

		public Orientation orientation;

		public List<GameObject> icons;

		public Transform[] buttons;

		public void SelectPickList(int newListId)
		{
			this.selectedListId = newListId;
			this.LoadIcons();
		}

		public Transform PointCastButtons(Vector2 mousePos)
		{
			for (int i = 0; i < this.buttons.Length; i++)
			{
				if (this.buttons[i].gameObject.activeInHierarchy)
				{
					if (Utility.IsInsideRange(mousePos.x, this.buttons[i].position.x, this.buttons[i].position.x + this.buttons[i].GetChild(0).localScale.x, true))
					{
						if (Utility.IsInsideRange(mousePos.y, this.buttons[i].position.y, this.buttons[i].position.y + this.buttons[i].GetChild(0).localScale.y, true))
						{
							return this.buttons[i];
						}
					}
				}
			}
			return null;
		}

		public Build.HoldingPart TryTakePart(Vector2 mousePos, Vector2 mousePos0z)
		{
			if (!this.IsInsidePickArea(mousePos))
			{
				return null;
			}
			Vector2 vector = (Vector3)mousePos - base.transform.position;
			int num = (int)vector.x;
			int num2 = (int)(-(int)vector.y);
			int num3 = num * this.height + num2;
			if (num3 > this.pickList[this.selectedListId].parts.Count - 1)
			{
				return null;
			}
			Vector2 a = Utility.ToDepth(base.transform.TransformPoint(new Vector3((float)num + 0.5f, (float)(-(float)num2) - 0.5f, 0f)), -Camera.main.transform.position.z) - mousePos0z;
			Ref.inputController.PlayClickSound(0.2f);
			return new Build.HoldingPart(a - this.pickList[this.selectedListId].parts[num3].centerOfRotation * this.orientation, new PlacedPart(null, Vector2.zero, this.orientation.DeepCopy(), this.pickList[this.selectedListId].parts[num3]));
		}

		public bool IsInsidePickArea(Vector2 mousePos)
		{
			Vector2 vector = (Vector3)mousePos - base.transform.position;
			return Utility.IsInsideRange(vector.x, 0f, (float)this.width, true) && Utility.IsInsideRange(vector.y, 0f, (float)(-(float)this.height), true);
		}

		public bool IsInsideDropArea(Vector2 mousePos)
		{
			Vector2 vector = (Vector3)mousePos - base.transform.position;
			return Utility.IsInsideRange(vector.x, -1f, (float)this.width, true) && Utility.IsInsideRange(vector.y, 0f, (float)(-(float)this.height - 1), true);
		}

		[Button(ButtonSizes.Medium)]
		public void LoadIcons()
		{
			this.DeleteIcons();
			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
					int num = i * this.height + j;
					if (num <= this.pickList[this.selectedListId].parts.Count - 1)
					{
						Transform transform = PartGrid.LoadIcon(this.iconPrefab, this.pickList[this.selectedListId].parts[num].prefab, new Vector2((float)i + 0.5f, (float)(-(float)j) - 0.5f) - this.pickList[this.selectedListId].parts[num].centerOfRotation * this.orientation * this.pickList[this.selectedListId].parts[num].pickGridScale, Vector2.one, base.transform, 50);
						Orientation.ApplyOrientation(transform, this.orientation);
						transform.localScale *= this.pickList[this.selectedListId].parts[num].pickGridScale;
						this.icons.Add(transform.gameObject);
						for (int k = 0; k < this.pickList[this.selectedListId].parts[num].attachmentSprites.Length; k++)
						{
							Utility.GetByPath(transform, this.pickList[this.selectedListId].parts[num].attachmentSprites[k].path).gameObject.SetActive(this.pickList[this.selectedListId].parts[num].attachmentSprites[k].showInPick);
						}
					}
				}
			}
		}

		private void DeleteIcons()
		{
			while (this.icons.Count > 0)
			{
				UnityEngine.Object.Destroy(this.icons[0]);
				this.icons.RemoveAt(0);
			}
		}
	}
}
