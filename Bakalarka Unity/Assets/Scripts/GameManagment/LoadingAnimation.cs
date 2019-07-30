/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour
{
    public float offset;
    public float radius;
    public float alpha;
    public float speed;
    void OnGUI()
    {
        GetComponent<Text>().color = new Color(GetComponent<Text>().color.r,
                                                GetComponent<Text>().color.g,
                                                GetComponent<Text>().color.b,
                                                offset + radius * Mathf.Sin(alpha));
        alpha += speed*Time.deltaTime;
    }
}
