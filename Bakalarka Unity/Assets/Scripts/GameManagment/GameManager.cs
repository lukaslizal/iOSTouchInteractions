/*
 * @author Lukáš Lízal 2018
 */
using UnityEngine;

public class GameManager : MonoBehaviour
{
	void Awake()
	{
		Application.targetFrameRate = 60;
		Application.backgroundLoadingPriority = ThreadPriority.Low;
	}
}
