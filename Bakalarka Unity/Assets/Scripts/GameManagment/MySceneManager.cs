/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using TouchScript.Layers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public int nextLevel;
    public GameObject fader;
    private AsyncOperation asyncLoad;
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    /// <summary>
    /// called when the game is terminated
    /// </summary>
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    /// <summary>
    /// called second
    /// </summary>
    /// <param name="scene">scene</param>
    /// <param name="mode">laodingType</param>
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
    void Start()
    {
        
        fader.GetComponent<Fader>().FadeIn(1);
    }
    public void LoadSceneNumber(int scene)
    {
        fader.GetComponent<Fader>().FadeIn(scene);
    }
    public void SwitchToNewScene()
    {
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true;
            fader.GetComponent<Fader>().FadeOut();
        }
    }
    public IEnumerator MyLoadSceneAsync(int scene)
    {
        // The Application loads the Scene in the background at the same time as the current Scene.
        //This is particularly good for creating loading screens. You could also load the Scene by build //number.
        asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;
        //Wait until the last operation fully loads to return anything
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        SwitchToNewScene();
    }
    public void SetFaderActive(bool active)
    {
        fader.SetActive(active);
    }
    public void SetTouchActive(bool active)
    {
        if (Camera.main.GetComponent<CameraLayer>())
            Camera.main.GetComponent<CameraLayer>().enabled = active;
    }
}
