using SFSML;
using SFSML.GameManager.Hooks.BuildRelated;
using SFSML.GameManager.Hooks.FrameRelated;
using SFSML.GameManager.Hooks.UnityRelated;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NewBuildSystem
{
	public class Build : MonoBehaviour
	{
		[Serializable]
		public class HoldingPart
		{
			public Vector2 holdingOffset;

			public PlacedPart part;

			public HoldingPart(Vector2 holdingOffset, PlacedPart part)
			{
				this.holdingOffset = holdingOffset;
				this.part = part;
			}

			public void Rotate90(float roundStregth)
			{
				Vector2 a = this.part.partIcon.position + (Vector3)this.part.partData.centerOfRotation * this.part.orientation;
				Vector2 a2 = this.holdingOffset + this.part.partData.centerOfRotation * this.part.orientation;
				this.part.orientation.Rotate90();
				this.holdingOffset = a2 - this.part.partData.centerOfRotation * this.part.orientation;
				Orientation.ApplyOrientation(this.part.partIcon, this.part.orientation);
				this.part.partIcon.transform.position = a - this.part.partData.centerOfRotation * this.part.orientation;
			}

			public void FlipX(float roundStregth)
			{
				Vector2 a = this.part.partIcon.position + (Vector3)this.part.partData.centerOfRotation * this.part.orientation;
				Vector2 a2 = this.holdingOffset + this.part.partData.centerOfRotation * this.part.orientation;
				this.part.orientation.FlipX();
				this.holdingOffset = a2 - this.part.partData.centerOfRotation * this.part.orientation;
				Orientation.ApplyOrientation(this.part.partIcon, this.part.orientation);
				this.part.partIcon.transform.position = a - this.part.partData.centerOfRotation * this.part.orientation;
			}

			public void FlipY(float roundStregth)
			{
				Vector2 a = this.part.partIcon.position + (Vector3)this.part.partData.centerOfRotation * this.part.orientation;
				Vector2 a2 = this.holdingOffset + this.part.partData.centerOfRotation * this.part.orientation;
				this.part.orientation.FlipY();
				this.holdingOffset = a2 - this.part.partData.centerOfRotation * this.part.orientation;
				Orientation.ApplyOrientation(this.part.partIcon, this.part.orientation);
				this.part.partIcon.transform.position = a - this.part.partData.centerOfRotation * this.part.orientation;
			}

			public void SetAsPickIcon()
			{
				for (int i = 0; i < this.part.partData.attachmentSprites.Length; i++)
				{
					Utility.GetByPath(this.part.partIcon, this.part.partData.attachmentSprites[i].path).gameObject.SetActive(this.part.partData.attachmentSprites[i].showInPick);
				}
			}

			public static bool isNull(Build.HoldingPart holdingPart)
			{
				return holdingPart == null || holdingPart.part == null || holdingPart.part.partData == null;
			}
		}

		[Serializable]
		public class BuildQuicksaves
		{
			public List<Build.BuildSave> buildSaves = new List<Build.BuildSave>();

			public int QuicksavesCount
			{
				get
				{
					return Build.BuildQuicksaves.LoadBuildQuicksaves().buildSaves.Count;
				}
			}

			public static void AddQuicksave(Build.BuildSave newSave)
			{
				Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
				buildQuicksaves.buildSaves.Add(newSave);
				Build.BuildQuicksaves.SaveBuildQuicksaves(buildQuicksaves);
			}

			public static void RemoveQuicksaveAt(int index)
			{
				Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
				if (index != -1 && index < buildQuicksaves.QuicksavesCount)
				{
					buildQuicksaves.buildSaves.RemoveAt(index);
				}
				Build.BuildQuicksaves.SaveBuildQuicksaves(buildQuicksaves);
			}

			private static void SaveBuildQuicksaves(Build.BuildQuicksaves buildQuicksaves)
			{
				Ref.SaveJsonString(JsonUtility.ToJson(buildQuicksaves), Saving.SaveKey.BuildQuicksaves);
			}

			public static Build.BuildQuicksaves LoadBuildQuicksaves()
			{
				string text = Ref.LoadJsonString(Saving.SaveKey.BuildQuicksaves);
				return (!(text == string.Empty)) ? JsonUtility.FromJson<Build.BuildQuicksaves>(text) : new Build.BuildQuicksaves();
			}
		}

		[Serializable]
		public class BuildSave
		{
			[Serializable]
			public class PlacedPartSave
			{
				public string partName;

				public Vector2 position;

				public Orientation orientation;

				public PlacedPartSave(PlacedPart part)
				{
					this.partName = part.partData.name;
					this.position = part.position;
					this.orientation = part.orientation;
				}

				public static List<Build.BuildSave.PlacedPartSave> ToSave(List<PlacedPart> parts)
				{
					List<Build.BuildSave.PlacedPartSave> list = new List<Build.BuildSave.PlacedPartSave>();
					for (int i = 0; i < parts.Count; i++)
					{
						list.Add(new Build.BuildSave.PlacedPartSave(parts[i]));
					}
					return list;
				}

				public static List<PlacedPart> FromSave(List<Build.BuildSave.PlacedPartSave> parts, PartDatabase partDatabase)
				{
					List<PlacedPart> list = new List<PlacedPart>();
					for (int i = 0; i < parts.Count; i++)
					{
						list.Add(new PlacedPart(null, parts[i].position, parts[i].orientation, partDatabase.GetPartByName(parts[i].partName)));
					}
					return list;
				}
			}

			public string saveName;

			public Vector3 cameraPosition;

			public List<Build.BuildSave.PlacedPartSave> parts;

			public BuildSave(string saveName, Vector3 cameraPosition, List<PlacedPart> parts)
			{
				this.saveName = saveName;
				this.cameraPosition = cameraPosition;
				this.parts = Build.BuildSave.PlacedPartSave.ToSave(parts);
			}
		}

		public static Build main;

		public PickPartGrid pickGrid;

		public PartGrid buildGrid;

		public PartDatabase partDatabase;

		[BoxGroup, Range(0f, 1f)]
		public float roundStrength;

		[BoxGroup]
		public float smoothIncrease;

		[BoxGroup]
		public float smoothDecay;

		[BoxGroup, Space]
		public float pickMenuDistance;

		[BoxGroup]
		public Vector2 pickMenuPosition;

		public float minZoom;

		public float maxZoom;

		public Build.HoldingPart[] holdingParts = new Build.HoldingPart[5];

		[BoxGroup("UI", true, false, 0)]
		public Text exitButton;

		[BoxGroup("UI", true, false, 0)]
		public Text partName;

		[BoxGroup("UI", true, false, 0)]
		public Text partDescription;

		[BoxGroup("UI", true, false, 0)]
		public RectTransform descriptionBackground;

		[BoxGroup("UI", true, false, 0)]
		public MoveModule descriptionMoveModule;

		[BoxGroup("Instructions", true, false, 0)]
		public CustomEvent dragAndDropInstruction;

		[BoxGroup("Instructions", true, false, 0)]
		public CustomEvent rotateInstruction;

		private void Start()
		{
			Build.main = this;
			Ref.partShader.SetFloat("_Intensity", 1.35f);
			if (Ref.lastScene == Ref.SceneType.Game && GameSaving.GameSave.LoadPersistant().mainVesselId != -1)
			{
				this.exitButton.text = "Resume";
			}
			this.pickGrid.LoadIcons();
			this.buildGrid.LoadAllIcons();
			this.pickGrid.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height, this.pickMenuDistance)) + (Vector3)this.pickMenuPosition;
			if (Ref.loadLaunchedRocket)
			{
				Ref.loadLaunchedRocket = false;
				this.LoadSave(JsonUtility.FromJson<Build.BuildSave>(Ref.LoadJsonString(Saving.SaveKey.ToLaunch)));
			}
			this.descriptionMoveModule.SetTargetTime((float)((!Saving.LoadSetting(Saving.SettingKey.hidePartDescription)) ? 1 : 0));
			this.descriptionMoveModule.SetTime((float)((!Saving.LoadSetting(Saving.SettingKey.hidePartDescription)) ? 1 : 0));
			if (!Saving.LoadSetting(Saving.SettingKey.seenBuildInstructions))
			{
				this.dragAndDropInstruction.gameObject.SetActive(true);
				Saving.SaveSetting(Saving.SettingKey.seenBuildInstructions, true);
			}
            ModLoader.manager.castHook<MyBuildMenuStartedHook>(new MyBuildMenuStartedHook());
		}

		public void MoveCamera(Vector3 delta)
		{
			Transform transform = Ref.cam.transform;
			transform.position = new Vector3(Mathf.Clamp(transform.position.x + delta.x, 0f, this.buildGrid.width), Mathf.Clamp(transform.position.y + delta.y, 0f, this.buildGrid.height), -Mathf.Clamp(-transform.position.z * (1f - delta.z), this.minZoom, this.maxZoom));
			this.pickGrid.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height, this.pickMenuDistance)) + (Vector3)this.pickMenuPosition;
		}

		public void OnClickUI(Vector2 clickPosPixel)
		{
			Transform transform = this.pickGrid.PointCastButtons(Camera.main.ScreenToWorldPoint((Vector3)clickPosPixel + new Vector3(0f, 0f, this.pickMenuDistance)));
			if (transform == null)
			{
				return;
			}
			if (transform.GetComponent<CustomEvent>() == null)
			{
				return;
			}
			transform.GetComponent<CustomEvent>().InvokeEvenets();
			base.StartCoroutine(Ref.inputController.ClickGlow(transform.GetChild(0).gameObject));
		}

		public void OnTouchStart(int fingerId, Vector2 touchPosWorld)
		{
			Transform x = this.pickGrid.PointCastButtons(Utility.ToDepth(touchPosWorld, this.pickMenuDistance));
			if (x != null)
			{
				Ref.inputController.touchesInfo[fingerId].touchState = global::Touch.TouchState.OnUI;
				return;
			}
			this.TryTakePart(fingerId, touchPosWorld);
		}

		public void OnTouchStay(int fingerId, Vector2 touchPosWorld, Vector2 deltaPixel)
		{
			Vector2 v = Camera.main.ScreenToWorldPoint((Vector3)deltaPixel + new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2), -Camera.main.transform.position.z)) - Camera.main.transform.position;
			this.roundStrength = Mathf.Clamp01(this.roundStrength + v.magnitude * this.smoothIncrease - this.smoothDecay * Time.deltaTime);
			if (Build.HoldingPart.isNull(this.holdingParts[fingerId]))
			{
				this.MoveCamera(v);
				this.pickGrid.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height, this.pickMenuDistance)) + (Vector3)this.pickMenuPosition;
				return;
			}
			Vector2 vector = touchPosWorld + (Build.HoldingPart.isNull(this.holdingParts[fingerId]) ? Vector2.zero : this.holdingParts[fingerId].holdingOffset);
			Vector2 autoCorrect = this.GetAutoCorrect(vector, fingerId);
			if (this.holdingParts[fingerId].part.partIcon != null)
			{
				this.holdingParts[fingerId].part.partIcon.position = Vector3.Lerp(vector, autoCorrect, 1f - this.roundStrength);
			}
		}

		public void OnTouchEnd(int fingerId, Vector2 touchPosWorld)
		{
			if (Build.HoldingPart.isNull(this.holdingParts[fingerId]))
			{
				return;
			}
			Vector2 vector = touchPosWorld + (Build.HoldingPart.isNull(this.holdingParts[fingerId]) ? Vector2.zero : this.holdingParts[fingerId].holdingOffset);
			Vector2 autoCorrect = this.GetAutoCorrect(vector, fingerId);
			Vector2 a = Vector3.Lerp(vector, autoCorrect, 1f - this.roundStrength);
			Vector2 v = a + (Build.HoldingPart.isNull(this.holdingParts[fingerId]) ? Vector2.zero : (this.holdingParts[fingerId].part.partData.centerOfRotation * this.holdingParts[fingerId].part.orientation));
			bool flag = Utility.IsInsideRange(v.x - this.buildGrid.transform.position.x, -0.5f, this.buildGrid.width + 0.5f, true) && Utility.IsInsideRange(v.y - this.buildGrid.transform.position.y, -0.5f, this.buildGrid.height + 0.5f, true);
			bool flag2 = this.pickGrid.IsInsideDropArea(Utility.ToDepth(v, this.pickMenuDistance));
			if (flag && !flag2)
			{
				this.dragAndDropInstruction.InvokeEvenets();
				this.buildGrid.PlacePart(new PlacedPart(null, (Vector3)autoCorrect - this.buildGrid.transform.position, this.holdingParts[fingerId].part.orientation.DeepCopy(), this.holdingParts[fingerId].part.partData));
			}
			if (this.holdingParts[fingerId].part.partIcon != null)
			{
				UnityEngine.Object.Destroy(this.holdingParts[fingerId].part.partIcon.gameObject);
			}
			this.holdingParts[fingerId] = null;
		}

		private Vector2 GetAutoCorrect(Vector2 partPos, int fingerId)
		{
			Vector2 vector = Utility.RoundToHalf(partPos);
			Vector2 vector2 = partPos - vector;
			if (Mathf.Abs(vector2.x) <= 0.05f && Mathf.Abs(vector2.y) <= 0.05f)
			{
				return vector;
			}
			if (!this.buildGrid.Overlap(new PlacedPart(null, vector, this.holdingParts[fingerId].part.orientation, this.holdingParts[fingerId].part.partData)))
			{
				return vector;
			}
			if (vector2.y > 0.05f && this.TryCorrect(vector, new Vector2(0f, 0.5f), fingerId))
			{
				return vector + new Vector2(0f, 0.5f);
			}
			if (vector2.y < -0.05f && this.TryCorrect(vector, new Vector2(0f, -0.5f), fingerId))
			{
				return vector + new Vector2(0f, -0.5f);
			}
			if (vector2.x > 0.05f && this.TryCorrect(vector, new Vector2(0.5f, 0f), fingerId))
			{
				return vector + new Vector2(0.5f, 0f);
			}
			if (vector2.x < -0.05f && this.TryCorrect(vector, new Vector2(-0.5f, 0f), fingerId))
			{
				return vector + new Vector2(-0.5f, 0f);
			}
			return vector;
		}

		private bool TryCorrect(Vector2 original, Vector2 change, int fingerId)
		{
			return !this.buildGrid.Overlap(new PlacedPart(null, original + change, this.holdingParts[fingerId].part.orientation, this.holdingParts[fingerId].part.partData));
		}

		public void Rotate90()
		{
			bool flag = false;
			for (int i = 0; i < this.holdingParts.Length; i++)
			{
				if (!Build.HoldingPart.isNull(this.holdingParts[i]))
				{
					this.holdingParts[i].Rotate90(this.roundStrength);
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			this.pickGrid.orientation.Rotate90();
			this.pickGrid.LoadIcons();
			this.rotateInstruction.InvokeEvenets();
		}

		public void FlipX()
		{
			bool flag = false;
			for (int i = 0; i < this.holdingParts.Length; i++)
			{
				if (!Build.HoldingPart.isNull(this.holdingParts[i]))
				{
					this.holdingParts[i].FlipX(this.roundStrength);
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			this.pickGrid.orientation.FlipX();
			this.pickGrid.LoadIcons();
			this.rotateInstruction.InvokeEvenets();
		}

		public void FlipY()
		{
			bool flag = false;
			for (int i = 0; i < this.holdingParts.Length; i++)
			{
				if (!Build.HoldingPart.isNull(this.holdingParts[i]))
				{
					this.holdingParts[i].FlipY(this.roundStrength);
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			this.pickGrid.orientation.FlipY();
			this.pickGrid.LoadIcons();
			this.rotateInstruction.InvokeEvenets();
		}

		public void TryTakePart(int fingerId, Vector2 touchPosWorld)
		{
			if (this.IsHolding(fingerId))
			{
				UnityEngine.Object.Destroy(this.holdingParts[fingerId].part.partIcon.gameObject);
			}
			if (Build.HoldingPart.isNull(this.holdingParts[fingerId]))
			{
				this.holdingParts[fingerId] = this.pickGrid.TryTakePart(Utility.ToDepth(touchPosWorld, this.pickMenuDistance), touchPosWorld);
			}
			if (Build.HoldingPart.isNull(this.holdingParts[fingerId]))
			{
				this.holdingParts[fingerId] = this.buildGrid.TryTakePart(touchPosWorld, true);
			}
			if (!Build.HoldingPart.isNull(this.holdingParts[fingerId]))
			{
				this.LoadHoldingIcon(fingerId, touchPosWorld);
			}
		}

		private void LoadHoldingIcon(int fingerId, Vector2 touchPosWorld)
		{
			this.LoadPartDescription(this.holdingParts[fingerId].part.partData);
			this.holdingParts[fingerId].part.partIcon = PartGrid.LoadIcon(this.buildGrid.iconPrefab, this.holdingParts[fingerId].part.partData.prefab, touchPosWorld, Vector3.one, null, 100);
			this.holdingParts[fingerId].SetAsPickIcon();
			this.holdingParts[fingerId].FlipX(0f);
			this.holdingParts[fingerId].FlipX(0f);
		}

		private bool IsHolding(int fingerId)
		{
			return !Build.HoldingPart.isNull(this.holdingParts[fingerId]) && this.holdingParts[fingerId].part.partIcon != null;
		}

		public void LoadSave(Build.BuildSave buildSaveToLoad)
		{
			this.buildGrid.DeleteAllIcons();
			Camera.main.transform.position = buildSaveToLoad.cameraPosition;
			this.buildGrid.parts = Build.BuildSave.PlacedPartSave.FromSave(buildSaveToLoad.parts, this.partDatabase);
			this.buildGrid.LoadAllIcons();
		}

		public void ExitBuildScene()
		{
			base.Invoke("Exit", 0.1f);
		}

		private void Exit()
		{
			if (Ref.lastScene == Ref.SceneType.Game && GameSaving.GameSave.LoadPersistant().mainVesselId != -1)
			{
				Ref.LoadScene(Ref.SceneType.Game);
				Ref.lastScene = Ref.SceneType.MainMenu;
				return;
			}
			Ref.LoadScene(Ref.SceneType.MainMenu);
		}

		public void Launch()
		{
			base.Invoke("TryLaunch", 0.1f);
		}

		private void TryLaunch()
		{
			if (!this.buildGrid.HasAnyParts())
			{
				return;
			}
			if (!this.buildGrid.HasControlAuthority())
			{
				this.DisableDescription();
				string warning = Utility.SplitLines("Your rocket does not have a capsule and will be uncontrollable");
				Ref.warning.ShowWarning(warning, new Vector2(1200f, 175f), "Launch Anyway", new Warning.EmptyDelegate(this.GoForLaunch), "Cancel Launch", new Warning.EmptyDelegate(this.EnableDescription), 275);
				return;
			}
			if (this.buildGrid.GetTWR() < 1f)
			{
				this.DisableDescription();
				string warning2 = Utility.SplitLines("Your engines are not powerful enough to lift this rocket//Either use more powerfull engines or reduce the mass of the rocket");
				Ref.warning.ShowWarning(warning2, new Vector2(1220f, 395f), "Launch Anyway", new Warning.EmptyDelegate(this.GoForLaunch), "Cancel Launch", new Warning.EmptyDelegate(this.EnableDescription), 275);
				return;
			}
			if (!this.buildGrid.HasParachute())
			{
				this.DisableDescription();
				string warning3 = Utility.SplitLines("Your rocket has no parachute");
				Ref.warning.ShowWarning(warning3, new Vector2(1050f, 95f), "Launch Anyway", new Warning.EmptyDelegate(this.GoForLaunch), "Cancel Launch", new Warning.EmptyDelegate(this.EnableDescription), 275);
				return;
			}
			this.GoForLaunch();
		}

		private void GoForLaunch()
		{
            MyRocketLaunchHook result = ModLoader.manager.castHook<MyRocketLaunchHook>(new MyRocketLaunchHook(this.buildGrid.parts));
            if (result.isCanceled()) return;
			string jsonString = JsonUtility.ToJson(new Build.BuildSave("To Launch", Ref.cam.transform.position, this.buildGrid.parts));
			Ref.SaveJsonString(jsonString, Saving.SaveKey.ToLaunch);
			Ref.LoadScene(Ref.SceneType.Game);
		}

		public void TogglePartDescription()
		{
			this.descriptionMoveModule.SetTargetTime((float)((this.descriptionMoveModule.targetTime.floatValue != 1f) ? 1 : 0));
			Saving.SaveSetting(Saving.SettingKey.hidePartDescription, this.descriptionMoveModule.targetTime.floatValue != 1f);
		}

		private void LoadPartDescription(PartData partData)
		{
			this.partName.text = partData.displayName;
			this.partDescription.text = Utility.SplitLines(partData.GetDescriptionRaw());
			this.descriptionBackground.sizeDelta = new Vector2(445f, (float)(Utility.GetLinesCount(partData.GetDescriptionRaw()) * 32 + 55));
		}

		public void DisableDescription()
		{
		}

		public void EnableDescription()
		{
		}

        public void OnGUI()
        {
            ModLoader.manager.castHook<MyBuildMenuOnGuiHook>(new MyBuildMenuOnGuiHook());
        }
	}
}
