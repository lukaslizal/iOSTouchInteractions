/*
 *	@author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using System;
using UnityEngine.Events;
using TMPro;
/// <summary>
/// Defines custom action for a button - responds to TapGesture events
/// </summary>
public class CustomButtonHandler : MonoBehaviour {
	public UnityEvent onTapEvent;
	public bool isSceneLoader;
	public int loadSceneNumber;
	
	private void OnEnable()
	{
		GetComponent<TapGesture>().Tapped += tappedHandler;
	}

    private void OnDisable()
	{
		GetComponent<TapGesture>().Tapped -= tappedHandler;
	}
	void Awake() {
		
        if (onTapEvent == null && !isSceneLoader)
            onTapEvent = new UnityEvent();
	}

	void Start()
	{
	if(isSceneLoader)
		onTapEvent.AddListener(()=>((MySceneManager) FindObjectOfType(typeof(MySceneManager))).LoadSceneNumber(loadSceneNumber));
	}
    private void tappedHandler(object sender, EventArgs e)
    {
        onTapEvent.Invoke();
    }
	
}
