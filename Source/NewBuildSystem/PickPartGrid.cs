using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NewBuildSystem
{
	public class PickPartGrid : MonoBehaviour
	{
		public void SelectPickList(int newListId)
		{
			this.selectedListId = newListId;
			this.LoadIcons();
		}

		public Transform PointCastButtons(Vector2 mousePos)
		{
			for (int i = 0; i < this.buttons.Length; i++)
			{
				bool activeInHierarchy = this.buttons[i].gameObject.activeInHierarchy;
				if (activeInHierarchy)
				{
					bool flag = Utility.IsInsideRange(mousePos.x, this.buttons[i].position.x, this.buttons[i].position.x + this.buttons[i].GetChild(0).localScale.x, true);
					if (flag)
					{
						bool flag2 = Utility.IsInsideRange(mousePos.y, this.buttons[i].position.y, this.buttons[i].position.y + this.buttons[i].GetChild(0).localScale.y, true);
						if (flag2)
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
			bool flag = !this.IsInsidePickArea(mousePos);
			Build.HoldingPart result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Vector2 vector = mousePos - (Vector2)base.transform.position;
				int num = (int)vector.x;
				int num2 = -(int)vector.y;
				int num3 = num * this.height + num2;
				bool flag2 = num3 > this.pickList[this.selectedListId].parts.Count - 1;
				if (flag2)
				{
					result = null;
				}
				else
				{
					bool flag3 = this.pickList[this.selectedListId].parts[num3].inFull && !Ref.hasPartsExpansion && (!Ref.hasHackedExpansion || this.pickList[this.selectedListId].parts[num3].disabledOnHacked);
					if (flag3)
					{
						Build.main.LoadPartDescription(this.pickList[this.selectedListId].parts[num3]);
						result = null;
					}
					else
					{
						Vector2 a = new Vector2(0f, 2f);
						Ref.inputController.PlayClickSound(0.2f);
						result = new Build.HoldingPart(a - this.pickList[this.selectedListId].parts[num3].centerOfRotation * this.orientation, new PlacedPart(null, Vector2.zero, this.orientation.DeepCopy(), this.pickList[this.selectedListId].parts[num3]));
					}
				}
			}
			return result;
		}

		public bool IsInsidePickArea(Vector2 mousePos)
		{
			Vector2 vector = mousePos - (Vector2)base.transform.position;
			return Utility.IsInsideRange(vector.x, 0f, (float)this.width, true) && Utility.IsInsideRange(vector.y, 0f, -(float)this.height, true);
		}

		public bool IsInsideDropArea(Vector2 mousePos)
		{
			Vector2 vector = mousePos - (Vector2)base.transform.position;
			return Utility.IsInsideRange(vector.x, -1f, (float)this.width, true) && Utility.IsInsideRange(vector.y, 0f, -(float)this.height - 1f, true);
		}

		[Button(22)]
		public void LoadIcons()
		{
			this.DeleteIcons();
			for (int i = 0; i < this.width; i++)
			{
				for (int j = 0; j < this.height; j++)
				{
					int num = i * this.height + j;
					bool flag = num <= this.pickList[this.selectedListId].parts.Count - 1;
					if (flag)
					{
						bool flag2 = !this.pickList[this.selectedListId].parts[num].inFull || Ref.hasPartsExpansion || (Ref.hasHackedExpansion && !this.pickList[this.selectedListId].parts[num].disabledOnHacked);
						Transform transform = PartGrid.LoadIcon(this.iconPrefab, this.pickList[this.selectedListId].parts[num].prefab, new Vector2((float)i + 0.5f, -(float)j - 0.5f) - this.pickList[this.selectedListId].parts[num].centerOfRotation * this.orientation * this.pickList[this.selectedListId].parts[num].pickGridScale, Vector2.one, base.transform, 50, new Color(1f, 1f, 1f, (!flag2) ? 0.5f : 1f), false);
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

		public void DeleteIcons()
		{
			while (this.icons.Count > 0)
			{
				UnityEngine.Object.Destroy(this.icons[0]);
				this.icons.RemoveAt(0);
			}
		}

		public PickPartGrid()
		{
		}

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
	}
}
