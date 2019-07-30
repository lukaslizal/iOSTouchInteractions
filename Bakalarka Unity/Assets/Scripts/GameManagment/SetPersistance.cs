/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPersistance : MonoBehaviour {
	void Start () {
        DontDestroyOnLoad(this);
	}
}
