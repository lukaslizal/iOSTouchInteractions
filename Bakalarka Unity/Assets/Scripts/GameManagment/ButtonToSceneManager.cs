/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonToSceneManager : MonoBehaviour {
	public int LoadSceneNumber;
	void Start () {
		GetComponent<Button>().onClick.AddListener(()=>((MySceneManager) FindObjectOfType(typeof(MySceneManager))).LoadSceneNumber(LoadSceneNumber));
	}
}
