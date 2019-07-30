/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Demonstration of input raycating. Detection of game objects in 3d scene (touch pointers, mouse pointer).true For purposes of theoretical part.
/// </summary>
public class RaycastExample : MonoBehaviour
{
    void Update()
    {
        // Translate touch input into a scene ray cast and show debug red ray in scen view
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            Vector3 pointerPosNearPlane = new Vector3(t.position.x,
                                                        t.position.y,
                                                        Camera.main.nearClipPlane);
            Vector3 pointerPosFarPlane = new Vector3(t.position.x,
                                                        t.position.y,
                                                        Camera.main.farClipPlane);
            Vector3 pointerPosWorldNearPlane = Camera.main.ScreenToWorldPoint(pointerPosNearPlane);
            Vector3 pointerPosWorldFarPlane = Camera.main.ScreenToWorldPoint(pointerPosFarPlane);
            RaycastHit hit;
            if (Physics.Raycast(pointerPosWorldNearPlane, pointerPosWorldFarPlane - pointerPosWorldNearPlane, out hit))
            {
                Debug.Log("Raycast hit game object: " + hit.transform.gameObject.name);
            }
            Debug.DrawRay(pointerPosWorldNearPlane, pointerPosWorldFarPlane - pointerPosWorldNearPlane, Color.red, 5f);
        }
        // Translate left mouse click input into a scene ray cast and show debug red ray in scen view
        if (Input.GetMouseButton(0))
        {
            Vector2 m = Input.mousePosition;
            Vector3 pointerPosNearPlane = new Vector3(m.x,
                                                        m.y,
                                                        Camera.main.nearClipPlane);
            Vector3 pointerPosFarPlane = new Vector3(m.x,
                                                        m.y,
                                                        Camera.main.farClipPlane);
            Vector3 pointerPosWorldNearPlane = Camera.main.ScreenToWorldPoint(pointerPosNearPlane);
            Vector3 pointerPosWorldFarPlane = Camera.main.ScreenToWorldPoint(pointerPosFarPlane);
            RaycastHit hit;
            if (Physics.Raycast(pointerPosWorldNearPlane, pointerPosWorldFarPlane - pointerPosWorldNearPlane, out hit))
            {
                Debug.Log("Raycast hit game object: " + hit.transform.gameObject.name);
            }
            Debug.DrawRay(pointerPosWorldNearPlane, pointerPosWorldFarPlane - pointerPosWorldNearPlane, Color.red, 5f);
        }
    }
}