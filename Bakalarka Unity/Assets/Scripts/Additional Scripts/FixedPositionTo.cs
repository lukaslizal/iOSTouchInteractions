/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Copies position of tracked object to object attached object
/// Good for breaking position link of parent/children objects.
/// </summary>
public class FixedPositionTo : MonoBehaviour {
	public GameObject parentPositionObject;
	public Vector3 parentPosition;
	private GameObject reference;
	void Start()
	{
		if(!parentPositionObject)
		{
			reference = new GameObject();
			reference.name = "UIParentPivot";
			reference.transform.position = parentPosition;
		}
		else
		{
			reference = parentPositionObject;
		}
	}
	void LateUpdate () {
		transform.position = reference.transform.position;
		transform.rotation = reference.transform.rotation;
		transform.localScale = reference.transform.localScale;
	}
}
