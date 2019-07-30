/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Reveals and hides navigation arrows and animated animated hands from
/// elastic pan menu scene with animated inforgraphics.
/// </summary>
public class HandToggler : MonoBehaviour {
	public CustomMenuButtonHandler customMenuButtonHandler;
	public ScreenFlicker screenFlicker;
	void Awake(){
		customMenuButtonHandler = transform.parent.GetComponent<CustomMenuButtonHandler>();
		screenFlicker = transform.parent.GetComponentInChildren<ScreenFlicker>();
	}
	void OnTriggerEnter(Collider col)
	{
		if(col.transform.gameObject.tag == "MainCamera")
		{
			customMenuButtonHandler.switchHand = true;
			customMenuButtonHandler.ShowHand();
			customMenuButtonHandler.ShowDirectionArrows();
			screenFlicker.StartFlickering();
		}
	}
	void OnTriggerExit(Collider col)
	{
		if(col.transform.gameObject.tag == "MainCamera")
		{
			customMenuButtonHandler.switchHand = false;
			customMenuButtonHandler.HideHand();
			customMenuButtonHandler.HideDirectionArrows();
			screenFlicker.StopFlickering();
		}
	}
}
