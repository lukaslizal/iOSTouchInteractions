/*
 * @author Lukáš Lízal
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Animates color of trail material in all colors of te rainbow
/// </summary>
public class TrailColorAnimation : MonoBehaviour {
	public float transitionTime;
	private float h,s,v;
	private Material trailMaterial;
	void Start () {

		trailMaterial = GetComponent<TrailRenderer>().material;
		Color.RGBToHSV(trailMaterial.color, out h, out s, out v);
		trailMaterial = GetComponent<TrailRenderer>().material;
		LeanTween.value(gameObject, 0f, 1f, transitionTime).setOnUpdate((float val) =>
        {
			h++;
            trailMaterial.color = Color.HSVToRGB(val,s,v);
        }).setRepeat(-1).setLoopPingPong();
	}
}
