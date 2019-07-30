/*
 * @author Lukáš Lízal
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Animates sprite so it is flickering kind of like a CRT monitor does
/// </summary>
public class ScreenFlicker : MonoBehaviour {
	[Range(0,1)]
	public float intensityRadius;
	public float intensityFrequency;
	private float defaultIntensity;
	public bool terminateFlickering;
	private int tickTock;
	public Color spriteColor;
	private SpriteRenderer s;
	void OnEnable()
	{
		tickTock = -1;
		s = this.transform.GetComponent<SpriteRenderer>();
		defaultIntensity = s.color.a;
	}
	void Start()
	{
		spriteColor = transform.GetComponent<SpriteRenderer>().color;
	}
	public void StartFlickering(){
		terminateFlickering = false;
		StartCoroutine(Flicker());
	}
	public void StopFlickering(){
		terminateFlickering = true;
	}
	IEnumerator Flicker()
	{
		while(!terminateFlickering)
		{
			s.color = (new Color(spriteColor.r ,spriteColor.g,spriteColor.b, defaultIntensity + tickTock*defaultIntensity*intensityRadius));
			tickTock *= -1;
			yield return new WaitForSeconds(1f/intensityFrequency);
		}
		s.color = (new Color(spriteColor.r,spriteColor.g,spriteColor.b, defaultIntensity));
	}
}
