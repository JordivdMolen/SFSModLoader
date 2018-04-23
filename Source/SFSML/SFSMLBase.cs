using System;
using SFSML.HookSystem.ReWork.BaseHooks.FrameHooks;
using UnityEngine;

namespace SFSML
{
	public class SFSMLBase : MonoBehaviour
	{
		public static void initialize()
		{
			bool flag = SFSMLBase.mainLoader == null;
			if (flag)
			{
				try
				{
					SFSMLBase.mainLoader = new ModLoader();
				}
				catch (Exception e)
				{
					ModLoader.mainConsole.logError(e);
				}
				SFSMLBase.mainLoader.myConsole.tryLogCustom(ModLoader.translation.autoFormat("@ModloaderInitated", new object[]
				{
					"SFSMLBase"
				}), "Base", LogType.Generic);
				SFSMLBase.mainLoader.startLoadProcedure();
				GameObject gameObject = new GameObject();
				gameObject.AddComponent<SFSMLBase>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				gameObject.SetActive(true);
				SFSMLBase.root = gameObject;
			}
		}

		public static ModLoader myModLoader()
		{
			return SFSMLBase.mainLoader;
		}

		public SFSMLBase()
		{
			base.enabled = true;
		}

		public void Update()
		{
			SFSMLBase.mainLoader.RunUpdate();
		}

		public void Awake()
		{
		}

		public void OnGUI()
		{
			new MyOnGuiHook(Ref.currentScene).executeDefault();
		}

		public static ModLoader mainLoader;

		public static GameObject root;
	}
}
