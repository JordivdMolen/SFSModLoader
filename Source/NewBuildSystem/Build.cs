using System;
using System.Collections.Generic;
using SFSML.HookSystem.ReWork;
using SFSML.HookSystem.ReWork.BaseHooks.BuildHooks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using SFSML;

namespace NewBuildSystem
{
	public class Build : MonoBehaviour
	{
		public float maxZoom
		{
			get
			{
				return (!Ref.hasPartsExpansion && !Ref.hasHackedExpansion) ? this.maxZoomBasic : this.maxZoomFull;
			}
		}

		private void Awake()
		{
			Build.main = this;
		}

		private void Start()
		{
			this.PositionCameraStart();
			Ref.partShader.SetFloat("_Intensity", 1.35f);
			if (Ref.lastScene == Ref.SceneType.Game && GameSaving.GameSave.LoadPersistant().mainVesselId != -1)
			{
				this.exitButton.text = "Resume";
			}
			this.pickGrid.LoadIcons();
			this.buildGrid.LoadAllIcons();
			this.pickGrid.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height, this.pickMenuDistance)) + (Vector3)this.pickMenuPosition;
			bool loadLaunchedRocket = Ref.loadLaunchedRocket;
			if (loadLaunchedRocket)
			{
				Ref.loadLaunchedRocket = false;
				this.LoadSave(JsonUtility.FromJson<Build.BuildSave>(Ref.LoadJsonString(Saving.SaveKey.ToLaunch)));
			}
			this.descriptionMoveModule.SetTargetTime((float)((!Saving.LoadSetting(Saving.SettingKey.hidePartDescription)) ? 1 : 0));
			this.descriptionMoveModule.SetTime((float)((!Saving.LoadSetting(Saving.SettingKey.hidePartDescription)) ? 1 : 0));
			bool flag2 = !Saving.LoadSetting(Saving.SettingKey.seenBuildInstructions);
			if (flag2)
			{
				this.dragAndDropInstruction.gameObject.SetActive(true);
				Saving.SaveSetting(Saving.SettingKey.seenBuildInstructions, true);
			}
		}

		public void PositionCameraStart()
		{
			Ref.cam.transform.position = ((!Ref.hasPartsExpansion && !Ref.hasHackedExpansion) ? this.camPosBasic : this.camPosExtended);
		}

		private void LateUpdate()
		{
            if (Input.GetKeyDown(KeyCode.A))
            {
                this.Rotate90();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                this.FlipX();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                this.FlipY();
            }
        }

		public void FixOrientation()
		{
            for (int i = 0; i < this.holdingParts.Length; i++)
            {
                if (!Build.HoldingPart.isNull(this.holdingParts[i]) && this.holdingParts[i].part.partData.simX)
                {
                    Vector2 vector = new Vector2(-1f, 0f) * this.holdingParts[i].part.orientation;
                    if (vector.x == 1f)
                    {
                        this.holdingParts[i].FlipX(this.roundStrength);
                    }
                    else if (vector.y == -1f)
                    {
                        this.holdingParts[i].FlipY(this.roundStrength);
                    }
                }
            }
        }

		private void Update()
		{
            this.MoveCamera(new Vector3(0f, 0f, -Ref.inputController.horizontalAxis * Time.deltaTime));
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; i < this.holdingParts.Length; i++)
            {
                if (!Build.HoldingPart.isNull(this.holdingParts[i]))
                {
                    flag = true;
                    Vector2 v = (Vector2)this.holdingParts[i].part.partIcon.position + this.holdingParts[i].part.partData.centerOfRotation * this.holdingParts[i].part.orientation;
                    flag2 = this.pickGrid.IsInsideDropArea(Utility.ToDepth(v, this.pickMenuDistance));
                    break;
                }
            }
            if (this.zoomButtonsHolder.activeSelf != !flag)
            {
                this.zoomButtonsHolder.SetActive(!flag);
            }
            if (this.descriptionHolder.activeSelf != flag2)
            {
                this.descriptionHolder.SetActive(flag2);
            }
            if (Input.GetKeyDown("u"))
            {
                Ref.inputController.leftArrow.transform.parent.gameObject.SetActive(!Ref.inputController.leftArrow.transform.parent.gameObject.activeSelf);
            }
        }

		public int GetOrientation()
		{
			return 0;
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
            Transform transform = this.pickGrid.PointCastButtons(Utility.ToDepth(touchPosWorld, this.pickMenuDistance));
            if (transform != null)
            {
                Ref.inputController.touchesInfo[fingerId].touchState = global::Touch.TouchState.OnUI;
                Ref.inputController.touchesInfo[fingerId].touchDownButton = transform;
                return;
            }
            this.TryTakePart(fingerId, touchPosWorld);
        }

		public void OnTouchStay(int fingerId, Vector2 touchPosWorld, Vector2 deltaPixel)
		{
            Vector2 v = default(Vector2);
            v = Camera.main.ScreenToWorldPoint((Vector3)deltaPixel + new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2), -Camera.main.transform.position.z)) - Camera.main.transform.position;
            this.roundStrength = Mathf.Clamp01(this.roundStrength + v.magnitude * this.smoothIncrease - this.smoothDecay * Time.deltaTime);
            if (Build.HoldingPart.isNull(this.holdingParts[fingerId]))
            {
                this.MoveCamera(v);
                this.pickGrid.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height, this.pickMenuDistance)) + (Vector3)this.pickMenuPosition;
                return;
            }
            Vector2 vector = touchPosWorld + this.holdingParts[fingerId].holdingOffset;
            if (this.pickGrid.IsInsideDropArea(Utility.ToDepth(vector + this.holdingParts[fingerId].part.partData.centerOfRotation * this.holdingParts[fingerId].part.orientation, this.pickMenuDistance)))
            {
                this.roundStrength = 1f;
            }
            Vector2 autoCorrect = this.GetAutoCorrect(vector, fingerId);
            if (this.holdingParts[fingerId].part.partIcon != null)
            {
                PlacedPart part = this.holdingParts[fingerId].part;
				Vector3 newPos = Vector3.Lerp(vector, autoCorrect, 1f - this.roundStrength);
				MyPartDragHook myPartDragHook = new MyPartDragHook(part, newPos);
				bool flag3 = myPartDragHook.isCanceled();
				if (!flag3)
				{
					this.holdingParts[fingerId].part.partIcon.position = myPartDragHook.pos;
				}				
			}
		}

		public void OnTouchEnd(int fingerId, Vector2 touchPosWorld)
		{
            if (Build.HoldingPart.isNull(this.holdingParts[fingerId]))
            {
                return;
            }
            Vector2 vector = touchPosWorld + (Build.HoldingPart.isNull(this.holdingParts[fingerId]) ? Vector2.zero : this.holdingParts[fingerId].holdingOffset);
            Vector2 vector2 = this.GetAutoCorrect(vector, fingerId);
            Vector2 a = Vector3.Lerp(vector, vector2, 1f - this.roundStrength);
            Vector2 v = a + (Build.HoldingPart.isNull(this.holdingParts[fingerId]) ? Vector2.zero : (this.holdingParts[fingerId].part.partData.centerOfRotation * this.holdingParts[fingerId].part.orientation));
            bool flag = Utility.IsInsideRange(v.x - this.buildGrid.transform.position.x, -0.5f, this.buildGrid.width + 0.5f, true) && Utility.IsInsideRange(v.y - this.buildGrid.transform.position.y, -0.5f, this.buildGrid.height + 0.5f, true);
            bool flag2 = this.pickGrid.IsInsideDropArea(Utility.ToDepth(v, this.pickMenuDistance));
            if (flag && !flag2)
            {
                PlacedPart targetPart = new PlacedPart(null, vector2 - (Vector2)this.buildGrid.transform.position, this.holdingParts[fingerId].part.orientation.DeepCopy(), this.holdingParts[fingerId].part.partData);
				MyPartCreatedHook myPartCreatedHook = new MyPartCreatedHook(targetPart);
				myPartCreatedHook = MyHookSystem.executeHook<MyPartCreatedHook>(myPartCreatedHook);
				bool flag5 = myPartCreatedHook.isCanceled();
				if (flag5)
				{
					return;
				}
                if (this.holdingParts[fingerId].part.partData.flip2stickX && this.ConnectedSurface(vector2, fingerId) == 0f)
                {
                    this.FlipX();
                    Vector2 vector3 = vector2;
                    vector = touchPosWorld + (Build.HoldingPart.isNull(this.holdingParts[fingerId]) ? Vector2.zero : this.holdingParts[fingerId].holdingOffset);
                    vector2 = this.GetAutoCorrect(vector, fingerId);
                    if (this.ConnectedSurface(vector2, fingerId) == 0f)
                    {
                        this.FlipX();
                        vector2 = vector3;
                    }
                }
                bool placeActive = !this.holdingParts[fingerId].part.partData.inFull || Ref.hasPartsExpansion;
                this.buildGrid.PlacePart(new PlacedPart(null, vector2 - (Vector2)this.buildGrid.transform.position, this.holdingParts[fingerId].part.orientation.DeepCopy(), this.holdingParts[fingerId].part.partData), placeActive);
            }
            if (this.holdingParts[fingerId].part.partIcon != null)
            {
                UnityEngine.Object.Destroy(this.holdingParts[fingerId].part.partIcon.gameObject);
            }
            this.holdingParts[fingerId] = null;

        }

		public Vector2 GetAutoCorrect(Vector2 partPos, int fingerId)
		{
            Vector2 vector = Utility.RoundToHalf(partPos);
            Vector2 offset = partPos - vector;
            bool flag = this.buildGrid.Overlap(new PlacedPart(null, vector, this.holdingParts[fingerId].part.orientation, this.holdingParts[fingerId].part.partData));
            if (flag)
            {
                vector = this.GetAutoCorrectForOverlaping(vector, offset, fingerId);
            }
            return this.GetAutoCorrectForSnap(vector, fingerId);
        }

        public Vector2 GetAutoCorrectForOverlaping(Vector2 partPosRounded, Vector2 offset, int fingerId)
		{
            if (offset.y > 0f && this.TryCorrect(partPosRounded, new Vector2(0f, 0.5f), fingerId))
            {
                return partPosRounded + new Vector2(0f, 0.5f);
            }
            if (offset.y < 0f && this.TryCorrect(partPosRounded, new Vector2(0f, -0.5f), fingerId))
            {
                return partPosRounded + new Vector2(0f, -0.5f);
            }
            if (offset.x > 0f && this.TryCorrect(partPosRounded, new Vector2(0.5f, 0f), fingerId))
            {
                return partPosRounded + new Vector2(0.5f, 0f);
            }
            if (offset.x < 0f && this.TryCorrect(partPosRounded, new Vector2(-0.5f, 0f), fingerId))
            {
                return partPosRounded + new Vector2(-0.5f, 0f);
            }
            if ((double)offset.y > -0.35 && this.TryCorrect(partPosRounded, new Vector2(0f, 0.5f), fingerId))
            {
                return partPosRounded + new Vector2(0f, 0.5f);
            }
            if ((double)offset.y < 0.35 && this.TryCorrect(partPosRounded, new Vector2(0f, -0.5f), fingerId))
            {
                return partPosRounded + new Vector2(0f, -0.5f);
            }
            if ((double)offset.x > -0.35 && this.TryCorrect(partPosRounded, new Vector2(0.5f, 0f), fingerId))
            {
                return partPosRounded + new Vector2(0.5f, 0f);
            }
            if ((double)offset.x < 0.35 && this.TryCorrect(partPosRounded, new Vector2(-0.5f, 0f), fingerId))
            {
                return partPosRounded + new Vector2(-0.5f, 0f);
            }
            return partPosRounded;
        }

		public Vector2 GetAutoCorrectForSnap(Vector2 partPos, int fingerId)
		{
            Vector2 zero = Vector2.zero;
            foreach (PartData.SnapPoint snapPoint in this.holdingParts[fingerId].part.partData.snapPoints)
            {
                Vector2 vector = partPos + snapPoint.position * this.holdingParts[fingerId].part.orientation;
                Debug.DrawLine(Vector3.zero, vector);
                foreach (PlacedPart placedPart in this.buildGrid.parts)
                {
                    foreach (PartData.SnapPoint snapPoint2 in placedPart.partData.snapPoints)
                    {
                        if (snapPoint.type == snapPoint2.type)
                        {
                            Vector2 b = placedPart.position + snapPoint2.position * placedPart.orientation;
                            Vector2 b2 = vector - b;
                            if (b2.x == 0f && b2.y == 0f)
                            {
                                return partPos;
                            }
                            if (Mathf.Abs(b2.x) <= 0.51f && Mathf.Abs(b2.y) <= 0.51f)
                            {
                                bool flag = ((!this.holdingParts[fingerId].part.orientation.InversedAxis()) ? snapPoint.snapX : snapPoint.snapY) && ((!placedPart.orientation.InversedAxis()) ? snapPoint2.snapX : snapPoint2.snapY);
                                bool flag2 = (this.holdingParts[fingerId].part.orientation.InversedAxis() ? snapPoint.snapX : snapPoint.snapY) && (placedPart.orientation.InversedAxis() ? snapPoint2.snapX : snapPoint2.snapY);
                                if (Mathf.Abs(b2.x) <= 0f || flag)
                                {
                                    if (Mathf.Abs(b2.y) <= 0f || flag2)
                                    {
                                        if (flag || flag2)
                                        {
                                            if (!this.buildGrid.Overlap(new PlacedPart(null, partPos - b2, this.holdingParts[fingerId].part.orientation, this.holdingParts[fingerId].part.partData)))
                                            {
                                                zero = new Vector2((!flag) ? 0f : b2.x, (!flag2) ? 0f : b2.y);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return partPos - zero;
        }

		public float ConnectedSurface(Vector2 position, int fingerId)
		{
			float num = 0f;
			PlacedPart placedPart = new PlacedPart(null, position, this.holdingParts[fingerId].part.orientation, this.holdingParts[fingerId].part.partData);
			foreach (PartData.AttachmentSurface surfaceA in placedPart.partData.attachmentSurfaces)
			{
				foreach (PlacedPart placedPart2 in this.buildGrid.parts)
				{
					bool flag = placedPart2.partIcon.name != "Inactive";
					if (flag)
					{
						foreach (PartData.AttachmentSurface surfaceB in placedPart2.partData.attachmentSurfaces)
						{
							num += Mathf.Max(0f, PlacedPart.SurfacesConnect(placedPart, surfaceA, placedPart2, surfaceB));
						}
					}
				}
			}
			return num;
		}

		public bool TryCorrect(Vector2 original, Vector2 change, int fingerId)
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
            this.FixOrientation();
            if (flag)
            {
                return;
            }
            this.pickGrid.orientation.Rotate90();
            this.pickGrid.LoadIcons();
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
            this.FixOrientation();
            if (flag)
            {
                return;
            }
            this.pickGrid.orientation.FlipX();
            this.pickGrid.LoadIcons();
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
            this.FixOrientation();
            if (flag)
            {
                return;
            }
            this.pickGrid.orientation.FlipY();
            this.pickGrid.LoadIcons();
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
                this.FixOrientation();
                if (this.buildGrid.parts.Count > 0 && this.buildInstruction.gameObject.activeSelf)
                {
                    this.buildInstruction.InvokeEvenets();
                }
                if (this.dragAndDropInstruction.gameObject.activeInHierarchy)
                {
                    this.dragAndDropInstruction.InvokeEvenets();
                    Transform transform = Ref.cam.transform;
                    transform.position = new Vector3(this.camPosBasic.x, this.camPosBasic.y, transform.position.z);
                    this.pickGrid.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height, this.pickMenuDistance)) + (Vector3)this.pickMenuPosition;
                }
            }
        }

		public void LoadHoldingIcon(int fingerId, Vector2 touchPosWorld)
		{
			this.LoadPartDescription(this.holdingParts[fingerId].part.partData);
			this.holdingParts[fingerId].part.partIcon = PartGrid.LoadIcon(this.buildGrid.iconPrefab, this.holdingParts[fingerId].part.partData.prefab, touchPosWorld, Vector3.one, null, 100, Color.white, false);
			this.holdingParts[fingerId].SetAsPickIcon();
			this.holdingParts[fingerId].FlipX(0f);
			this.holdingParts[fingerId].FlipX(0f);
		}

		public bool IsHolding(int fingerId)
		{
			return !Build.HoldingPart.isNull(this.holdingParts[fingerId]) && this.holdingParts[fingerId].part.partIcon != null;
		}

		public void LoadSave(Build.BuildSave buildSaveToLoad)
		{
            MyVesselLoadedHook myVesselLoadedHook = new MyVesselLoadedHook(Build.BuildSave.PlacedPartSave.FromSave(buildSaveToLoad.parts, this.partDatabase));
            myVesselLoadedHook = MyHookSystem.executeHook<MyVesselLoadedHook>(myVesselLoadedHook);
            if (myVesselLoadedHook.isCanceled())
            {
                return;
            }
            this.buildGrid.DeleteAllIcons();
			Camera.main.transform.position = buildSaveToLoad.cameraPosition;
            this.buildGrid.parts = myVesselLoadedHook.parts;
			this.buildGrid.LoadAllIcons();
			this.MoveCamera(Vector3.zero);
		}

		public void ExitBuildScene()
		{
			base.Invoke("AskExit", 0.1f);
		}

		public void AskExit()
		{
            if (this.buildGrid.parts.Count == 0)
            {
                this.Exit();
                return;
            }
            string warning = Utility.SplitLines("Are you sure you want to exit?/All non saved build progress will be lost");
            Ref.warning.ShowWarning(warning, new Vector2(1200f, 265f), "Exit", new Warning.EmptyDelegate(this.Exit), "Cancel", null, 160);
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

        public void OpenSalePage()
        {
            Ref.openSalePage = true;
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
                this.DisableUI();
                string warning = Utility.SplitLines("Your rocket does not have a capsule and will be uncontrollable");
                Ref.warning.ShowWarning(warning, new Vector2(1200f, 175f), "Launch Anyway", new Warning.EmptyDelegate(this.GoForLaunch), "Cancel Launch", new Warning.EmptyDelegate(this.EnableUI), 275);
                return;
            }
            if (this.buildGrid.GetTWR() < 1f)
            {
                this.DisableUI();
                string warning2 = Utility.SplitLines("Your engines are not powerful enough to lift this rocket//Either use more powerful engines or reduce the mass of the rocket");
                Ref.warning.ShowWarning(warning2, new Vector2(1220f, 395f), "Launch Anyway", new Warning.EmptyDelegate(this.GoForLaunch), "Cancel Launch", new Warning.EmptyDelegate(this.EnableUI), 275);
                return;
            }
            if (!this.buildGrid.HasParachute())
            {
                this.DisableUI();
                string warning3 = Utility.SplitLines("Your rocket has no parachute");
                Ref.warning.ShowWarning(warning3, new Vector2(1050f, 95f), "Launch Anyway", new Warning.EmptyDelegate(this.GoForLaunch), "Cancel Launch", new Warning.EmptyDelegate(this.EnableUI), 275);
                return;
            }
            this.GoForLaunch();
        }

        private void GoForLaunch()
		{
			MyRocketToLaunchpadHook myRocketToLaunchpadHook = MyHookSystem.executeHook<MyRocketToLaunchpadHook>(new MyRocketToLaunchpadHook(this.buildGrid.parts));
			bool flag = myRocketToLaunchpadHook.isCanceled();
			if (!flag)
			{
				string jsonString = JsonUtility.ToJson(new Build.BuildSave("To Launch", Ref.cam.transform.position, myRocketToLaunchpadHook.rocket, this.GetOrientation()));
				Ref.SaveJsonString(jsonString, Saving.SaveKey.ToLaunch);
				Ref.LoadScene(Ref.SceneType.Game);
			}
		}

        public string GetBuildJson(string name)
        {
            return JsonUtility.ToJson(new Build.BuildSave(name, Ref.cam.transform.position, this.buildGrid.parts, this.GetOrientation()));
        }

        public static Texture2D CreatePreviewIcon(string jsonData)
        {
            Build.BuildSave buildSave = JsonUtility.FromJson<Build.BuildSave>(jsonData);
            Vector2 zero = Vector2.zero;
            PartGrid.CenterToPositon(buildSave.parts, Build.main.partDatabase, ref zero, new Vector2(0f, -200f));
            List<GameObject> list = new List<GameObject>();
            foreach (Build.BuildSave.PlacedPartSave placedPartSave in buildSave.parts)
            {
                PartData partByName = Build.main.partDatabase.GetPartByName(placedPartSave.partName);
                if (partByName != null)
                {
                    list.Add(PartGrid.LoadIcon(Build.main.buildGrid.iconPrefab, partByName.prefab, placedPartSave.position, partByName.prefab.localScale, null, 0, placedPartSave.orientation, Color.white, true).gameObject);
                }
            }
            Build.main.previewIconCamera.transform.position = new Vector3(0f, -200f, Mathf.Clamp(-Mathf.Max(zero.x, zero.y), -100f, -1f) * 0.52f);
            Build.main.previewIconCamera.Render();
            while (list.Count > 0)
            {
                UnityEngine.Object.DestroyImmediate(list[0]);
                list.RemoveAt(0);
            }
            Texture2D texture2D = new Texture2D(Build.main.previewIconCamera.targetTexture.width, Build.main.previewIconCamera.targetTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = Build.main.previewIconCamera.targetTexture;
            texture2D.ReadPixels(new Rect(0f, 0f, (float)Build.main.previewIconCamera.targetTexture.width, (float)Build.main.previewIconCamera.targetTexture.height), 0, 0);
            texture2D.Apply();
            return texture2D;
        }

        public void LoadPartDescription(PartData partData)
        {
            this.partName.text = partData.displayName;
            this.partDescription.text = Utility.SplitLines(partData.GetDescriptionRaw());
            this.descriptionBackground.sizeDelta = new Vector2(445f, (float)(Utility.GetLinesCount(partData.GetDescriptionRaw()) * 32 + 55));
        }

        public void DisableUI()
        {
        }

        public void EnableUI()
        {
        }

        public static Build main;

		public PickPartGrid pickGrid;

		public PartGrid buildGrid;

		public PartDatabase partDatabase;

		[BoxGroup]
		[Range(0f, 1f)]
		public float roundStrength;

		[BoxGroup]
		public float smoothIncrease;

		[BoxGroup]
		public float smoothDecay;

		[BoxGroup]
		[Space]
		public float pickMenuDistance;

		[BoxGroup]
		public Vector2 pickMenuPosition;

		public float minZoom;

		public float maxZoomBasic;

		public float maxZoomFull;

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
        public GameObject zoomButtonsHolder;

        [BoxGroup("UI", true, false, 0)]
        public GameObject descriptionHolder;

        [BoxGroup("Instructions", true, false, 0)]
        public CustomEvent dragAndDropInstruction;

        [BoxGroup("Instructions", true, false, 0)]
        public CustomEvent buildInstruction;

        public Vector3 camPosBasic;

		public Vector3 camPosExtended;

        [BoxGroup("Icon Generation", true, false, 0)]
        public Camera previewIconCamera;

        public Texture2D tex2d;

        [Serializable]
		public class HoldingPart
		{
			public HoldingPart(Vector2 holdingOffset, PlacedPart part)
			{
				this.holdingOffset = holdingOffset;
				this.part = part;
			}

			public void Rotate90(float roundStregth)
			{
				Vector2 a = (Vector2)this.part.partIcon.position + this.part.partData.centerOfRotation * this.part.orientation;
				Vector2 a2 = this.holdingOffset + this.part.partData.centerOfRotation * this.part.orientation;
				this.part.orientation.Rotate90();
				this.holdingOffset = a2 - this.part.partData.centerOfRotation * this.part.orientation;
				Orientation.ApplyOrientation(this.part.partIcon, this.part.orientation);
				this.part.partIcon.transform.position = a - this.part.partData.centerOfRotation * this.part.orientation;
			}

			public void FlipX(float roundStregth)
			{
				Vector2 a = (Vector2)this.part.partIcon.position + this.part.partData.centerOfRotation * this.part.orientation;
				Vector2 a2 = this.holdingOffset + this.part.partData.centerOfRotation * this.part.orientation;
				this.part.orientation.FlipX();
				this.holdingOffset = a2 - this.part.partData.centerOfRotation * this.part.orientation;
				Orientation.ApplyOrientation(this.part.partIcon, this.part.orientation);
				this.part.partIcon.transform.position = a - this.part.partData.centerOfRotation * this.part.orientation;
			}

			public void FlipY(float roundStregth)
			{
				Vector2 a = (Vector2)this.part.partIcon.position + this.part.partData.centerOfRotation * this.part.orientation;
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

			public Vector2 holdingOffset;

			public PlacedPart part;
		}

		[Serializable]
		public class BuildQuicksaves
		{
			public static void AddQuicksave(Build.BuildSave newSave)
			{
                MyVesselSavedHook myVesselSavedHook = new MyVesselSavedHook(newSave);
                myVesselSavedHook = MyHookSystem.executeHook<MyVesselSavedHook>(myVesselSavedHook);
                if (myVesselSavedHook.isCanceled())
                {
                    return;
                }
                Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
				buildQuicksaves.buildSaves.Add(newSave);
				Build.BuildQuicksaves.SaveBuildQuicksaves(buildQuicksaves);
			}

			public static void RemoveQuicksaveAt(int index)
			{
				Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
				bool flag = index != -1 && index < buildQuicksaves.QuicksavesCount;
				if (flag)
				{
					buildQuicksaves.buildSaves.RemoveAt(index);
				}
				Build.BuildQuicksaves.SaveBuildQuicksaves(buildQuicksaves);
			}

			public int QuicksavesCount
			{
				get
				{
					return Build.BuildQuicksaves.LoadBuildQuicksaves().buildSaves.Count;
				}
			}

			public static void SaveBuildQuicksaves(Build.BuildQuicksaves buildQuicksaves)
			{
				Ref.SaveJsonString(JsonUtility.ToJson(buildQuicksaves), Saving.SaveKey.BuildQuicksaves);
			}

			public static Build.BuildQuicksaves LoadBuildQuicksaves()
			{
				string text = Ref.LoadJsonString(Saving.SaveKey.BuildQuicksaves);
				return (!(text == string.Empty)) ? JsonUtility.FromJson<Build.BuildQuicksaves>(text) : new Build.BuildQuicksaves();
			}

			public List<Build.BuildSave> buildSaves = new List<Build.BuildSave>();
		}

		[Serializable]
		public class BuildSave
		{
			public BuildSave(string saveName, Vector3 cameraPosition, List<PlacedPart> parts, int rotation)
			{
				this.saveName = saveName;
				this.cameraPosition = cameraPosition;
				this.parts = Build.BuildSave.PlacedPartSave.ToSave(parts);
				this.rotation = rotation;
			}

			public string saveName;

			public Vector3 cameraPosition;

			public List<Build.BuildSave.PlacedPartSave> parts;

			public int rotation;

			[Serializable]
			public class PlacedPartSave
			{
				public PlacedPartSave(PlacedPart part)
				{
					this.partName = part.partData.name;
					this.position = part.position;
					this.orientation = part.orientation;
                    this.GUID = part.partData.GUID;

                    //Tags formatting for save
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (string text in part.partData.tags.Keys)
                    {
                        stringBuilder.Append(string.Concat(new string[]
                        {
                            part.partData.tags[text].GetType().AssemblyQualifiedName,
                            "#",
                            text,
                            "#",
                            JsonUtility.ToJson(part.partData.tags[text]),
                            "|"
                        }));
                    }
                    this.tagsString = stringBuilder.ToString();
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
						PartData partByName = partDatabase.GetPartByName(parts[i].partName);
						bool flag = partByName != null;
						if (flag)
						{
                            partByName.GUID = parts[i].GUID;
                            //Tags parsing from text back to a Dictionary
                            partByName.tags = new Dictionary<string, object>();
                            try
                            {
                                foreach (string text in parts[i].tagsString.Split(new char[]
                                {
                                    '|'
                                }))
                                {
                                    if (!(text == ""))
                                    {
                                        Type type = Type.GetType(text.Split(new char[]
                                        {
                                            '#'
                                        })[0]);
                                        string key = text.Split(new char[]
                                        {
                                            '#'
                                        })[1];
                                        //ModLoader.mainConsole.log(text.Split('#')[2]);

                                        object obj = JsonUtility.FromJson(text.Split(new char[]
                                        {
                                            '#'
                                        })[2], type);

                                        //ModLoader.mainConsole.log(obj.ToString(), "Tags");
                                        partByName.tags.Add(key, obj);
                                    }
                                }
                                /*foreach (string text2 in partByName.tags.Keys)
                                {
                                    ModLoader.mainConsole.log(text2 + " " + partByName.tags[text2].ToString(), "Tags");
                                }*/
                            }
                            catch (Exception e)
                            {
                                ModLoader.mainConsole.logError(e);
                            }

                            list.Add(new PlacedPart(null, parts[i].position, parts[i].orientation, partByName));
						}
					}
					return list;
				}

				public string partName;

				public Vector2 position;

				public Orientation orientation;

                //Custom field for custom values saving
                public string tagsString;

                public Guid GUID;
            }
		}
	}
}
