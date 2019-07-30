/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Examples of LeanTween function calls for puroposes of theroetical part.
/// </summary>
public class LeanTweenExample : MonoBehaviour
{
    void Start()
    {
        var go = new GameObject();
        var from = 0f;
        var to = 1f;
        var time = 1f;
        // examples of LeanTween function calls for puroposes of theroetical part.
        // move game object 'go', on axis Y, to destination position 'to', in time 'time', with easing curve 'easeOutExpo' 
        LeanTween.moveY(go, to, time).setEase(LeanTweenType.easeOutExpo);
        // further settings of tween function can be chained
        LeanTween.moveY(go, to, time)
                    .setEase(LeanTweenType.easeOutBounce)
                    .setDelay(1f)
                    .setRepeat(3);
        // leanTween manages to animate all different sorts of properties like colors, or even sound
        LeanTween.color(gameObject, Color.yellow, 1f);
        // example of tween function with use of an anonymous method
        LeanTween.value(go, from, to, time).setOnUpdate((float val) =>
                        {
                            GameObject enemy = GameObject.Find("enemy");
                            if (enemy)
                            {
                                enemy.transform.position = new Vector3(val, val / 2, val / 3);
                            }
                        }).setEase(LeanTweenType.easeOutBounce);
    }
}
