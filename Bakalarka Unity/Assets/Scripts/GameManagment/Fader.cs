/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour
{
    public float transitionTime;
	private MySceneManager sceneManager;
    void Awake()
    {
		sceneManager = (MySceneManager)FindObjectOfType(typeof(MySceneManager));
    }
    public void FadeIn(int scene)
    {
        sceneManager.SetFaderActive(true);
        sceneManager.SetTouchActive(false);

        LeanTween.cancel(transform.gameObject);
        LeanTween.value(gameObject, GetComponentInChildren<Image>().color.a, 1f, transitionTime).setOnUpdate((float val) =>
        {
            GetComponentInChildren<Image>().color = new Color(0f, 0f, 0f, val);
        }).setOnComplete(() =>
        {
            StartCoroutine(sceneManager.MyLoadSceneAsync(scene));
        });
    }
    public void FadeOut()
	{
        // LeanTween.cancel(transform.gameObject);
		LeanTween.value(gameObject, GetComponentInChildren<Image>().color.a, 0f, transitionTime).setOnUpdate((float val) =>
        {
            GetComponentInChildren<Image>().color = new Color(0f, 0f, 0f, val);
        }).setOnComplete(() =>
        {
            sceneManager.SetFaderActive(false);
            sceneManager.SetTouchActive(true);
        }).setDelay(0.5f);
	}
}
