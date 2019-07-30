/*
*   @author Lukáš Lízal 2018
 */
using UnityEngine;
using UnityEditor;
using TouchScript.Layers;
using TouchScript.Gestures;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MenuPanController))]
public class MenuPanControllerInspector : Editor
{
    public static MenuPanController mpc;
    public static bool mapInitiated = true;
    [MenuItem("GameObject/Elastic Pan Menu", false, 12)]
    [MenuItem("Elastic Scrolling/Setup New Elastic Pan Menu", false)]
    public static void SetupManuMap()
    {
        InitiateMenuMap();
    }
    void OnEnable()
    {
        if (GameObject.FindObjectOfType<MenuPanController>() != null)
        {
            mpc = GameObject.FindObjectOfType<MenuPanController>();
            RefreshMenuItems();
        }
    }
    public override void OnInspectorGUI()
    {
        MenuPanController myTarget = (MenuPanController)target;
        GUILayout.Space(10);
        GUILayout.Label("Menu Settings");
        GUILayout.BeginVertical("HelpBox");
        EditorGUILayout.HelpBox("To edit menu items, simply add items into the 'Menu Items' object, then click Refresh Menu Items button below. Items will automatically rearrange into a horizontal menu list.", MessageType.Info);
        GUILayout.BeginHorizontal();
        myTarget.attractor = EditorGUILayout.Toggle("Magnetic Menu Items", myTarget.attractor);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        myTarget.intervalSpacing = EditorGUILayout.Slider("Item Spacing", myTarget.intervalSpacing, 0.1f, 50f);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        myTarget.autoAnimationAreas = EditorGUILayout.Toggle("Animation Controllers", myTarget.autoAnimationAreas);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Refresh Menu Items"))
        {
            RefreshMenuItems();
            if (myTarget.autoAnimationAreas)
                RecalculateAnimationTriggers();
            else
                DestroyAnimationTriggers();
        }
        GUILayout.EndVertical();
        GUILayout.Label("Elastic Scroll Settings");
        GUILayout.BeginVertical("HelpBox");
        GUILayout.BeginHorizontal();
        myTarget.inertiaCoefficient = EditorGUILayout.Slider("Inertia Coefficient", myTarget.inertiaCoefficient, 0f, 1f);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        myTarget.maxPanSpeed = EditorGUILayout.Slider("Max Pan Speed", myTarget.maxPanSpeed, 0f, 2f);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        if (mpc && !mpc.itemContainer)
            EditorGUILayout.HelpBox("There is no Item container reference!", MessageType.Warning);
    }
    private static void DestroyOldMenu()
    {
        if (GameObject.FindObjectOfType<MenuPanController>())
        {
            var go = GameObject.FindObjectOfType<MenuPanController>();
            DestroyImmediate(go);
        }
    }
    private static void InitiateMenuMap()
    {
        SceneView.lastActiveSceneView.in2DMode = true;
        if (Camera.main)
        {
            Camera.main.orthographic = true;
            if (!Camera.main.gameObject.GetComponent<CameraLayer>())
            {
                Camera.main.gameObject.AddComponent<CameraLayer>();
            }
            Camera.main.allowHDR = false;
            Camera.main.allowMSAA = false;
            Camera.main.useOcclusionCulling = false;
            Camera.main.clearFlags = CameraClearFlags.Color;
        }
        else
        {
            Debug.LogError("There is no main camera in tthe scene. Touch layer failed to init. To fix this please add camera to the scene, tag it MainCamera, set its position to 0,0,-20 and delete 'Map' Gameobject and press initialize crossroads button again!");
        }
        var mapObject = new GameObject();
        mapObject.name = "Menu";
        mapObject.AddComponent<MenuPanHandler>();
        var tg = mapObject.AddComponent<TransformGesture>();
        tg.Type = TransformGesture.TransformType.Translation;
        var meCo = mapObject.GetComponent<MenuPanController>();
        meCo.inertiaCoefficient = 0.73f;
        meCo.maxPanSpeed = 2f;
        meCo.finishGestureThreshold = 0.001f;
        meCo.attractor = true;
        meCo.intervalSpacing = 6;
        meCo.bumpExtents = 2.5f;
        var touchPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        touchPlane.name = "Touch Recieving Collider";
        touchPlane.transform.parent = mapObject.transform;
        touchPlane.transform.rotation = Quaternion.Euler(-90, 0, 0);
        touchPlane.transform.localScale = new Vector3(100, 1, 100);
        var mr = touchPlane.GetComponent<MeshRenderer>();
        DestroyImmediate(mr);
        var menuItems = new GameObject();
        menuItems.name = "Menu Items";
        menuItems.transform.parent = mapObject.transform;
        meCo.itemContainer = menuItems.gameObject;
        var animationAreas = new GameObject();
        animationAreas.name = "Animation Control Areas";
        animationAreas.transform.parent = mapObject.transform;
        meCo.animationTriggerContainer = animationAreas.gameObject;
        mapInitiated = true;
        mpc = meCo;
        for (int i = 0; i < 5; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.parent = meCo.itemContainer.transform;
            go.name = "Menu Item " + i;
        }
        List<GameObject> touchInteraceObjects = GameObject.FindObjectsOfType<GameObject>().Where(obj => (obj.GetComponent<CorridorPanController>()) || (obj.GetComponent<MenuPanController>())).ToList();
        float furthestInterfacePosition = 0f;
        foreach (GameObject g in touchInteraceObjects)
        {
            if (g.transform.position.z > furthestInterfacePosition)
                furthestInterfacePosition = g.transform.position.z;
        }
        mapObject.transform.position = new Vector3(mapObject.transform.position.x,
            mapObject.transform.position.y,
            furthestInterfacePosition + 5f);
        RefreshMenuItems();
    }
    public static void RefreshMenuItems()
    {
        if (mpc.itemContainer)
        {
            var list = mpc.itemContainer.transform.GetFirstChildren();
            mpc.itemCount = list.Length;
            for (int i = 1; i < list.Length; i++)
            {
                Vector3 pos = Vector3.zero;
                if (mpc.orientation == PanOrientation.Horizontal)
                {
                    pos = new Vector3(mpc.start + (i) * mpc.intervalSpacing, 0, mpc.transform.position.z);
                }
                else
                {
                    pos = new Vector3(0, mpc.start - (i) * mpc.intervalSpacing, mpc.transform.position.z);
                }
                list[i].transform.position = pos;
            }
        }
        else
        {
            Debug.LogError("item Container in MenuPaCOntroller is missing!");
        }
    }
    public static void RecalculateAnimationTriggers()
    {
        if (mpc.animationTriggerContainer && mpc.itemContainer)
        {
            var oldAnimList = mpc.animationTriggerContainer.transform.GetFirstChildren();
            mpc.itemCount = oldAnimList.Length;
            for (int i = 0; i < oldAnimList.Length; i++)
            {
                if (oldAnimList[i])
                    DestroyImmediate(oldAnimList[i].transform.gameObject);
            }
            var list = mpc.itemContainer.transform.GetFirstChildren();
            mpc.itemCount = list.Length;
            for (int i = 0; i < list.Length; i++)
            {
                var go = AnimationControlsInspector.Create();
                var aca = go.GetComponent<AnimationControlsArea>();
                aca.triggerAnim = false;
                aca.speedAnim = false;
                aca.progressAnim = true;
                aca.minusProgress = 0f;
                aca.plusProgress = 1f;
                aca.animatedObject = list[i].transform.gameObject;
                aca.SetName();
                var c = go.GetComponent<BoxCollider>();
                c.size = new Vector3(mpc.intervalSpacing, 5, 1);
                go.transform.position = list[i].transform.position;
                go.transform.parent = mpc.animationTriggerContainer.transform;
            }
        }
        else
        {
            Debug.LogError("animationContainer in SimplePanController is missing!");
        }
    }
    public static void DestroyAnimationTriggers()
    {
        if (mpc.animationTriggerContainer && mpc.itemContainer)
        {
            var oldAnimList = mpc.animationTriggerContainer.transform.GetFirstChildren();
            mpc.itemCount = oldAnimList.Length;
            for (int i = 0; i < oldAnimList.Length; i++)
            {
                if (oldAnimList[i])
                    DestroyImmediate(oldAnimList[i].transform.gameObject);
            }
        }
        else
        {
            Debug.LogError("animationContainer in SimplePanController is missing!");
        }
    }
}