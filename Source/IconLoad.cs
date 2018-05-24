using System;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class IconLoad : MonoBehaviour
{
	[Button(ButtonSizes.Small)]
	private void Start()
	{
		PartGrid.LoadIcon(this.iconPrefab, this.partToLoad.prefab, -(this.partToLoad.centerOfRotation * this.partToLoad.pickGridScale), Vector2.one * this.partToLoad.pickGridScale, base.transform, 50, Color.white, false);
	}

	public PartData partToLoad;

	public Transform iconPrefab;
}
