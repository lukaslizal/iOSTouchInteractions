/*
 *	@author Lukáš Lízal
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Makes sure displayed area is the right size (iPhone vs iPad screen ratio)
/// </summary>
public class CameraAspectResolver : MonoBehaviour {
	void Awake()
	{
		var camera = GetComponent<Camera>();
		if(camera.aspect < (16f/9f))
			camera.orthographicSize += camera.orthographicSize*((16/9f)-camera.aspect);
	}
}
