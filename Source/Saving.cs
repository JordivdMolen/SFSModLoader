using System;
using System.Collections;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Saving : MonoBehaviour
{
	private void Start()
	{
		this.UpdateLoadButtonColor();
	}

	public void OnKeyboardClick(Vector2 clickPosPixel)
	{
		GameObject gameObject = Ref.inputController.PointCastUI(clickPosPixel, this.keys);
		bool flag = gameObject == null;
		if (!flag)
		{
			base.StartCoroutine(Ref.inputController.ClickGlow(gameObject.transform.GetChild(0).gameObject));
			Ref.inputController.PlayClickSound(0.4f);
			this.OnKey(gameObject.GetComponentInChildren<Text>().text);
		}
	}

	public void ToggleCaps()
	{
		string text = this.keys[0].GetComponentInChildren<Text>().text;
		bool flag = text == text.ToLower();
		for (int i = 0; i < this.keys.Length; i++)
		{
			this.keys[i].GetComponentInChildren<Text>().text = ((!flag) ? this.keys[i].GetComponentInChildren<Text>().text.ToLower() : this.keys[i].GetComponentInChildren<Text>().text.ToUpper());
		}
	}

	public void OnKey(string key)
	{
		this.keyboardText.text = this.keyboardText.text.Replace("|", string.Empty) + key + ((Time.unscaledTime % 1f <= 0.5f) ? string.Empty : "|");
		string text = this.keys[0].GetComponentInChildren<Text>().text;
	}

	public void Backspace()
	{
		this.keyboardText.text = this.keyboardText.text.Replace("|", string.Empty);
		this.keyboardText.text = this.keyboardText.text.Substring(0, Mathf.Max(this.keyboardText.text.Length - 1, 0)) + ((Time.unscaledTime % 1f <= 0.5f) ? string.Empty : "|");
	}

	public void StartSaveProcess()
	{
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag = currentScene != Ref.SceneType.Build;
		if (flag)
		{
			bool flag2 = currentScene == Ref.SceneType.Game;
			if (flag2)
			{
				bool flag3 = Ref.mainVessel == null;
				if (flag3)
				{
					Ref.controller.ShowMsg("Cannot save while not controlling a rocket");
					return;
				}
				Time.timeScale = 0f;
				bool activeSelf = Ref.inputController.menuHolder.activeSelf;
				if (activeSelf)
				{
					Ref.inputController.ToggleDropdownMenu();
				}
			}
		}
		else
		{
			bool flag4 = !Build.main.buildGrid.HasAnyParts();
			if (flag4)
			{
				return;
			}
		}
		this.savingMenuHolder.SetActive(true);
		this.keyboardText.text = string.Empty;
		this.keyboardHolder.SetActive(true);
		this.keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default);
		this.keyboard.active = true;
	}

	public void CompleteSaveProcess()
	{
		string text = this.keyboardText.text.Replace("|", string.Empty);
		bool flag = text == string.Empty;
		if (flag)
		{
			text = "Unnamed Save";
		}
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag2 = currentScene != Ref.SceneType.Build;
		if (flag2)
		{
			bool flag3 = currentScene == Ref.SceneType.Game;
			if (flag3)
			{
				GameSaving.Quicksaves.AddQuicksave(GameSaving.GetGameSaveData(text));
				Ref.controller.ShowMsg("Game Saved");
			}
		}
		else
		{
			Build.BuildQuicksaves.AddQuicksave(new Build.BuildSave(text, Camera.main.transform.position, Build.main.buildGrid.parts, 0));
		}
		this.CloseMenus();
		this.UpdateLoadButtonColor();
	}

	private bool CanLoad()
	{
		return ((Ref.currentScene != Ref.SceneType.Build) ? GameSaving.Quicksaves.LoadQuicksaves().QuicksavesCount : Build.BuildQuicksaves.LoadBuildQuicksaves().QuicksavesCount) > 0;
	}

	private void UpdateLoadButtonColor()
	{
		this.loadButton.GetComponentInChildren<Text>().color = ((!this.CanLoad()) ? new Color(1f, 1f, 1f, 0.6f) : Color.white);
	}

	public void TryStartLoadProcess()
	{
		bool flag = !this.CanLoad();
		if (!flag)
		{
			this.StartLoadProcess();
		}
	}

	private void StartLoadProcess()
	{
		this.loadingMenuHolder.SetActive(true);
		this.selectedSaveId = -1;
		this.LoadSaveFilesIcons();
		bool flag = Ref.currentScene == Ref.SceneType.Build;
		if (flag)
		{
			Build.main.DisableDescription();
		}
		else
		{
			bool flag2 = Ref.currentScene == Ref.SceneType.Game;
			if (flag2)
			{
				Ref.controller.HideArrowButtons();
				Ref.controller.fuelIconsHolder.gameObject.SetActive(false);
				bool activeSelf = Ref.inputController.menuHolder.activeSelf;
				if (activeSelf)
				{
					Ref.inputController.ToggleDropdownMenu();
				}
			}
		}
	}

	public void TrySelectLoad(Vector2 clickPosPixel)
	{
		bool flag = this.selectedSaveId != -1 && this.selectedSaveId < this.saveFilesIcons.Count;
		if (flag)
		{
			this.saveFilesIcons[this.selectedSaveId].transform.GetChild(0).gameObject.SetActive(false);
		}
		this.selectedSaveId = -1;
		GameObject gameObject = Ref.inputController.PointCastUI(clickPosPixel, this.saveFilesIcons.ToArray());
		for (int i = 0; i < this.saveFilesIcons.Count; i++)
		{
			bool flag2 = this.saveFilesIcons[i].gameObject == gameObject;
			if (flag2)
			{
				this.selectedSaveId = i;
				gameObject.transform.GetChild(0).gameObject.SetActive(true);
				Ref.inputController.PlayClickSound(0.6f);
				break;
			}
		}
	}

	public void CompleteLoadProcess()
	{
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag = currentScene != Ref.SceneType.Build;
		if (flag)
		{
			bool flag2 = currentScene == Ref.SceneType.Game;
			if (flag2)
			{
				GameSaving.Quicksaves quicksaves = GameSaving.Quicksaves.LoadQuicksaves();
				bool flag3 = this.selectedSaveId != -1 && this.selectedSaveId < quicksaves.QuicksavesCount;
				if (flag3)
				{
					GameSaving.LoadGame(quicksaves.quicksaves[this.selectedSaveId]);
					Ref.controller.ShowMsg("Game Loaded");
					this.CloseMenus();
				}
			}
		}
		else
		{
			Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
			bool flag4 = this.selectedSaveId != -1 && this.selectedSaveId < buildQuicksaves.QuicksavesCount;
			if (flag4)
			{
				Build.main.LoadSave(buildQuicksaves.buildSaves[this.selectedSaveId]);
				this.CloseMenus();
			}
		}
	}

	public void DeleteSelectedQuicksave()
	{
		bool flag = this.selectedSaveId == -1;
		if (!flag)
		{
			Ref.SceneType currentScene = Ref.currentScene;
			bool flag2 = currentScene != Ref.SceneType.Build;
			if (flag2)
			{
				bool flag3 = currentScene == Ref.SceneType.Game;
				if (flag3)
				{
					UnityEngine.Object.Destroy(this.saveFilesIcons[this.selectedSaveId].gameObject);
					this.saveFilesIcons.RemoveAt(this.selectedSaveId);
					GameSaving.Quicksaves.RemoveQuicksaveAt(this.selectedSaveId);
					this.iconsAnimation = this.MoveSaveFilesIcons();
					base.StartCoroutine(this.iconsAnimation);
				}
			}
			else
			{
				UnityEngine.Object.Destroy(this.saveFilesIcons[this.selectedSaveId].gameObject);
				this.saveFilesIcons.RemoveAt(this.selectedSaveId);
				Build.BuildQuicksaves.RemoveQuicksaveAt(this.selectedSaveId);
				this.iconsAnimation = this.MoveSaveFilesIcons();
				base.StartCoroutine(this.iconsAnimation);
			}
			this.selectedSaveId = -1;
		}
	}

	public void CloseMenus()
	{
		Time.timeScale = 1f;
		this.savingMenuHolder.SetActive(false);
		this.loadingMenuHolder.SetActive(false);
		while (this.saveFilesIcons.Count > 0)
		{
			UnityEngine.Object.Destroy(this.saveFilesIcons[0].gameObject);
			this.saveFilesIcons.RemoveAt(0);
		}
		bool flag = this.keyboard != null;
		if (flag)
		{
			this.keyboard.active = false;
		}
		this.keyboardHolder.SetActive(false);
		this.keyboard = null;
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag2 = currentScene != Ref.SceneType.Build;
		if (flag2)
		{
			bool flag3 = currentScene == Ref.SceneType.Game;
			if (flag3)
			{
				Ref.inputController.leftArrow.parent.gameObject.SetActive(true);
				Ref.controller.fuelIconsHolder.gameObject.SetActive(true);
			}
		}
		else
		{
			Build.main.EnableDescription();
		}
		this.UpdateLoadButtonColor();
	}

	private void LoadSaveFilesIcons()
	{
		while (this.saveFilesIcons.Count > 0)
		{
			UnityEngine.Object.Destroy(this.saveFilesIcons[0].gameObject);
			this.saveFilesIcons.RemoveAt(0);
		}
		bool flag = Ref.inputController.canvasScalers[0].referenceResolution.x == 750f;
		int num = (!flag) ? 8 : 12;
		List<string> list = new List<string>();
		bool flag2 = Ref.currentScene == Ref.SceneType.Build;
		if (flag2)
		{
			Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
			for (int i = 0; i < Mathf.Min(buildQuicksaves.QuicksavesCount, 24); i++)
			{
				list.Add(buildQuicksaves.buildSaves[i].saveName);
			}
		}
		else
		{
			bool flag3 = Ref.currentScene == Ref.SceneType.Game;
			if (flag3)
			{
				GameSaving.Quicksaves quicksaves = GameSaving.Quicksaves.LoadQuicksaves();
				for (int j = 0; j < Mathf.Min(quicksaves.QuicksavesCount, 24); j++)
				{
					list.Add(quicksaves.quicksaves[j].saveName);
				}
			}
		}
		Transform child = this.loadingMenuHolder.transform.GetChild(1);
		for (int k = 0; k < list.Count; k++)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(this.saveFileIconPrefab, child);
			transform.localPosition = new Vector3((float)(k / num * 365), (float)(k % num * -66), 0f);
			transform.GetChild(1).GetComponent<Text>().text = list[k];
			this.saveFilesIcons.Add(transform.GetComponent<BoxCollider2D>());
		}
		child.localPosition = new Vector3(-182.5f * (float)((list.Count - 1) / num), child.localPosition.y);
	}

	private IEnumerator MoveSaveFilesIcons()
	{
		bool movingIcons = true;
		while (movingIcons)
		{
			yield return new WaitForEndOfFrame();
			movingIcons = false;
			int collumIndex = 0;
			int collumsCount = 0;
			int num3;
			for (int i = 0; i < this.saveFilesIcons.Count; i = num3 + 1)
			{
				bool flag = i > 0 && this.saveFilesIcons[i].transform.localPosition.x != this.saveFilesIcons[i - 1].transform.localPosition.x;
				if (flag)
				{
					collumIndex = 0;
					num3 = collumsCount;
					collumsCount = num3 + 1;
				}
				float num = (float)(collumIndex * -66);
				num3 = collumIndex;
				collumIndex = num3 + 1;
				float num2 = (25f + 5f * Mathf.Abs(this.saveFilesIcons[i].transform.localPosition.y - num)) * Time.deltaTime;
				bool flag2 = Mathf.Abs(this.saveFilesIcons[i].transform.localPosition.y - num) > num2;
				if (flag2)
				{
					this.saveFilesIcons[i].transform.localPosition = new Vector3(this.saveFilesIcons[i].transform.localPosition.x, this.saveFilesIcons[i].transform.localPosition.y - num2 * Mathf.Sign(this.saveFilesIcons[i].transform.localPosition.y - num));
					movingIcons = true;
				}
				else
				{
					this.saveFilesIcons[i].transform.localPosition = new Vector3(this.saveFilesIcons[i].transform.localPosition.x, num);
				}
				num3 = i;
			}
			float targetPositionX = -182.5f * (float)collumsCount;
			float diff = this.loadingMenuHolder.transform.GetChild(2).localPosition.x - targetPositionX;
			float moveAmountX = (40f + 5f * Mathf.Abs(diff)) * Time.deltaTime;
			bool flag3 = Mathf.Abs(diff) > moveAmountX;
			if (flag3)
			{
				this.loadingMenuHolder.transform.GetChild(2).localPosition = new Vector3(this.loadingMenuHolder.transform.GetChild(2).localPosition.x - moveAmountX * Mathf.Sign(diff), this.loadingMenuHolder.transform.GetChild(2).localPosition.y);
				movingIcons = true;
			}
			else
			{
				this.loadingMenuHolder.transform.GetChild(2).localPosition = new Vector3(targetPositionX, this.loadingMenuHolder.transform.GetChild(2).localPosition.y);
			}
		}
		yield break;
	}

	private void Update()
	{
		bool activeSelf = this.savingMenuHolder.activeSelf;
		if (activeSelf)
		{
			this.keyboardText.text = this.keyboardText.text.Replace("|", string.Empty) + ((Time.unscaledTime % 1f <= 0.5f) ? string.Empty : "|");
			bool anyKeyDown = Input.anyKeyDown;
			if (anyKeyDown)
			{
				this.OnKey(Input.inputString);
			}
			bool keyDown = Input.GetKeyDown(KeyCode.Backspace);
			if (keyDown)
			{
				this.Backspace();
			}
			this.keyboardText.transform.localPosition = new Vector3((Time.unscaledTime % 1f <= 0.5f) ? 0f : 4.5f, 30f, 0f);
		}
	}

	public static void SaveSetting(Saving.SettingKey settingKey, bool value)
	{
		PlayerPrefs.SetInt(settingKey.ToString(), (!value) ? 0 : 1);
		PlayerPrefs.Save();
	}

	public static bool LoadSetting(Saving.SettingKey settingKey)
	{
		return PlayerPrefs.GetInt(settingKey.ToString(), 0) == 1;
	}

	public static bool HasSetting(Saving.SettingKey settingKey)
	{
		return PlayerPrefs.HasKey(settingKey.ToString());
	}

	public static void ToggleSetting(Saving.SettingKey settingKey)
	{
		Saving.SaveSetting(settingKey, !Saving.LoadSetting(settingKey));
	}

	public Saving()
	{
	}

	public Text keyboardText;

	private TouchScreenKeyboard keyboard;

	public GameObject savingMenuHolder;

	public GameObject loadButton;

	public GameObject loadingMenuHolder;

	public GameObject confirmMenuHolder;

	public int selectedSaveId;

	public List<BoxCollider2D> saveFilesIcons;

	public Transform saveFileIconPrefab;

	private IEnumerator iconsAnimation;

	[BoxGroup]
	public GameObject keyboardHolder;

	[BoxGroup]
	public BoxCollider2D[] keys;

	public enum SettingKey
	{
		seenBuildInstructions,
		seenGameInstructions,
		seenMapInstructions,
		seenTimewarpInstructions,
		hidePartDescription,
		disableMusic,
		disableSound,
		disableAutoRotate,
		disable60fps,
		hasPartDLC,
		hasHackedDLC,
		infiniteFuel,
		noDrag
	}

	public enum SaveKey
	{
		ToLaunch,
		PersistantGameSave,
		Settings,
		BuildQuicksaves,
		GameQuicksaves
	}
}
