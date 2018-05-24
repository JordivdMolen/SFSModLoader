using System;
using System.Collections.Generic;
using System.Reflection;
using JMtech.Translations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Addons.Translations
{
	public class TranslationRefrence : MonoBehaviour
	{
		public List<string> TransTargets
		{
			get
			{
				if (Translation.mainTranslation == null)
				{
					Translation.loadTranslations<SFST>();
				}
				return Translation.mainTranslation.getTranslatables();
			}
		}

		private void UpdateRef()
		{
			if (SFST.T != null)
			{
				this.TranslationPreview = SFST.T.getTranslations()[this.TranslationTarget];
			}
			else
			{
				this.TranslationPreview = "<Empty>";
			}
		}

		private void Start()
		{
			Text component = base.gameObject.GetComponent<Text>();
			string translationTarget = this.TranslationTarget;
			FieldInfo field = SFST.T.GetType().GetField(translationTarget, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				component.text = "Failed to translate " + translationTarget;
			}
			component.text = field.GetValue(SFST.T).ToString();
		}

		[ValueDropdown("TransTargets")]
		[OnValueChanged("UpdateRef", false)]
		public string TranslationTarget;

		[ReadOnly]
		public string TranslationPreview;
	}
}
