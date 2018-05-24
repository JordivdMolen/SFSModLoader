using System;
using UnityEngine;

public class SortingLayerExposer : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.GetComponent<MeshRenderer>().sortingLayerName = this.SortingLayerName;
		base.gameObject.GetComponent<MeshRenderer>().sortingOrder = this.SortingOrder;
	}

	public string SortingLayerName = "Default";

	public int SortingOrder;
}
