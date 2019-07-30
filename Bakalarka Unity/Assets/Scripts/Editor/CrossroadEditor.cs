/*
*   @author Lukáš Lízal 2018
 */
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using TouchScript.Layers;
using TouchScript.Gestures;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
public class CrossroadEditor : EditorWindow
{
    private static CorridorPanController cc;
    private static List<Node> nodes;
    private static bool[] foldout;
    private static Vector2 scrollPosition;
    private static string crossroadsParentName = "Crossroad Nodes";
    private static int index;
    private static string[] options;
    [Range(5, 1000)]
    private static int nodeSize = 5;
    private static bool delConfirmation;
    private static bool mapInitiated;
    [MenuItem("Elastic Scrolling/Setup Corridor Map", false, 1)]
    public static void SetupCorridorMap()
    {
        EditorWindow.GetWindow(typeof(CrossroadEditor));
        if (!GameObject.FindObjectOfType<CorridorPanController>())
            InitiateCrossroadMap();
    }
    [MenuItem("Elastic Scrolling/Setup Corridor Map", true, 1)]
    public static bool ValidateSetupCorridorMap()
    {
        return !GameObject.FindObjectOfType<CorridorPanController>();
    }
    [MenuItem("Window/Crossroad Editor")]
    [MenuItem("Elastic Scrolling/Open Crossroad Editor", false, 2)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CrossroadEditor));
    }
    [MenuItem("Elastic Scrolling/Select Crossroad Pan Object %&k", false, 3)]
    public static void SelectMenuPanObject()
    {
        if (FindObjectOfType((typeof(CorridorPanController))))
            Selection.activeGameObject = (FindObjectOfType(typeof(CorridorPanController)) as CorridorPanController).transform.gameObject;
    }
    [MenuItem("Elastic Scrolling/Select Crossroad Pan Object %&k", true, 3)]
    public static bool ValidateSelectMenuPanObject()
    {
        return FindObjectOfType((typeof(CorridorPanController)));
    }
    [MenuItem("Elastic Scrolling/Create Corridor Debug View", false, 3)]
    public static void CreateCorridorDebugView()
    {
        CreateCrossroadDebugView();
    }
    [MenuItem("Elastic Scrolling/Create Corridor Debug View", true, 3)]
    public static bool ValidateCreateCorridorDebugView()
    {
        return FindObjectOfType((typeof(CorridorPanController)));
    }
    void OnEnable()
    {
        Enable();
    }
    public static void Enable()
    {
        if (GameObject.FindObjectOfType<CorridorPanController>() != null)
        {
            cc = GameObject.FindObjectOfType<CorridorPanController>();
            if (cc != null)
            {
                mapInitiated = false;
                var list = new List<Node>();
                list.AddRange(cc.transform.GetComponentsInChildren<Node>());
                nodes = list;
            }
            else
            {
                mapInitiated = true;
            }
            if (foldout == null || (foldout != null && foldout.Length != nodes.Count))
            {
                foldout = new bool[nodes.Count];
                for (int i = 0; i < foldout.Length; i++)
                {
                    foldout[i] = true;
                }
            }
        }
    }
    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            Enable();
            if (cc != null)
            {
                var list = new List<Node>();
                list.AddRange(cc.transform.GetComponentsInChildren<Node>());
                nodes = list;
            }
            if (cc && !cc.mapStart)
            {
                Node n = AddNode();
                cc.mapStart = n;
            }
            GUILayout.Space(20);
            GUILayout.Space(10);
            if (cc != null)
            {
                GUILayout.Label("MAP EDITOR");
                GUILayout.BeginVertical("HelpBox");
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUI.BeginChangeCheck();
                cc.mapStart = (Node)EditorGUILayout.ObjectField("Spawn Player At Node", cc.mapStart, typeof(Node), true);
                GUILayout.Space(40);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                nodeSize = EditorGUILayout.IntSlider("Nodes Sizes", nodeSize, 5, 20);
                GUILayout.Space(10);
                if (GUILayout.Button("Apply"))
                {
                    ResetNodeSizesTo(nodeSize, nodeSize);
                }
                GUILayout.Space(40);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUI.BeginChangeCheck();
                cc.enableDebugVizualization = EditorGUILayout.Toggle("Enable Debug Vizualization", cc.enableDebugVizualization);
                if (EditorGUI.EndChangeCheck())
                {
                    if (cc.debugVizualizationObject)
                        cc.debugVizualizationObject.SetActive(cc.enableDebugVizualization);
                }
                GUILayout.Space(40);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Space(60);
                if (GUILayout.Button("Select Map Contoller"))
                {
                    Selection.activeGameObject = GameObject.FindObjectOfType<CorridorPanController>().gameObject;
                }
                GUILayout.Space(60);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(60);
                if (GUILayout.Button("Create/Refresh Debug View"))
                {
                    cc.debugVizualizationObject = CreateCrossroadDebugView();
                    cc.debugVizualizationObject.SetActive(cc.enableDebugVizualization);
                }
                GUILayout.Space(60);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(60);
                if (!delConfirmation)
                {
                    if (GUILayout.Button("Delete All Nodes"))
                    {
                        delConfirmation = true;
                    }
                }
                else
                {
                    GUILayout.Label("Are you sure?");
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    if (GUILayout.Button("Yes"))
                    {
                        DeleteAllNodes();
                        Enable();
                        delConfirmation = false;
                    }
                    if (GUILayout.Button("No"))
                    {
                        delConfirmation = false;
                    }
                    GUILayout.Space(60);
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(60);
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.EndVertical();
                if (foldout != null)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Nodes: " + nodes.Count);
                    if (GUILayout.Button("Collapse all"))
                    {
                        SetMenuFoldoutsTo(false);
                    }
                    if (GUILayout.Button("Expand all"))
                    {
                        SetMenuFoldoutsTo(true);
                    }
                    GUILayout.Space(200);
                    GUILayout.EndHorizontal();
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(this.position.width + 15));
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        GUILayout.BeginVertical("HelpBox");
                        GUILayout.Space(10);

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        nodes[i].nodeName = EditorGUILayout.TextField(nodes[i].nodeName);
                        nodes[i].transform.gameObject.name = nodes[i].nodeName;
                        GUILayout.Space(20);
                        if (GUILayout.Button("Highlight"))
                        {
                            HighlightNode(i);
                            UnityEditor.EditorGUIUtility.PingObject(nodes[i].gameObject);
                        }
                        if (GUILayout.Button("Select"))
                        {
                            Selection.activeGameObject = nodes[i].gameObject;
                        }
                        if (GUILayout.Button("Starting"))
                        {
                            SetStartingNode(nodes[i]);
                        }
                        if (GUILayout.Button("X"))
                        {
                            DeleteNode(nodes[i]);
                        }
                        GUILayout.Space(20);
                        GUILayout.EndHorizontal();
                        foldout[i] = EditorGUILayout.Foldout(foldout[i], "Connections");
                        if (foldout[i])
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(40);
                            nodes[i].hPlus = (Node)EditorGUILayout.ObjectField("Horizontal Plus", nodes[i].hPlus, typeof(Node), true);
                            if (GUILayout.Button("X"))
                            {
                                RemoveConnection(nodes[i], nodes[i].hPlus);
                            }
                            if (GUILayout.Button("+"))
                            {
                                AddHorizontalPlus(nodes[i]);
                            }
                            GUILayout.Space(20);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(40);
                            nodes[i].hMinus = (Node)EditorGUILayout.ObjectField("Horizontal Minus", nodes[i].hMinus, typeof(Node), true);
                            if (GUILayout.Button("X"))
                            {
                                RemoveConnection(nodes[i], nodes[i].hMinus);
                            }
                            if (GUILayout.Button("+"))
                            {
                                AddHorizontalMinus(nodes[i]);
                            }
                            GUILayout.Space(20);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(40);
                            nodes[i].vPlus = (Node)EditorGUILayout.ObjectField("Vertical Plus", nodes[i].vPlus, typeof(Node), true);
                            if (GUILayout.Button("X"))
                            {
                                RemoveConnection(nodes[i], nodes[i].vPlus);
                            }
                            if (GUILayout.Button("+"))
                            {
                                AddVerticalPlus(nodes[i]);
                            }
                            GUILayout.Space(20);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(40);
                            nodes[i].vMinus = (Node)EditorGUILayout.ObjectField("Vertical Minus", nodes[i].vMinus, typeof(Node), true);
                            if (GUILayout.Button("X"))
                            {
                                RemoveConnection(nodes[i], nodes[i].vMinus);
                            }
                            if (GUILayout.Button("+"))
                            {
                                AddVerticalMinus(nodes[i]);
                            }
                            GUILayout.Space(20);
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.Space(10);
                        GUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorUtility.SetDirty(this);
            }
            else
            {
                if (mapInitiated)
                {
                    if (GUILayout.Button("Enable Node Window"))
                        Enable();
                }
                else
                {
                    EditorGUILayout.HelpBox("There is no crossroad map initiated yet. If you would like to setup new crossroad panning map click 'Setup Corridor Map' button or go to toolbar Elastic Scrolling/Setup Corridor Map", MessageType.Warning);
                    if (GUILayout.Button("Setup Corridor Map"))
                    {
                        InitiateCrossroadMap();
                    }
                }
            }
        }
    }
    private static void DestroyOldCrossroad()
    {
        if (GameObject.FindObjectOfType<CorridorPanController>())
        {
            var go = GameObject.FindObjectOfType<CorridorPanController>();
            DestroyImmediate(go);
        }
    }
    private static void InitiateCrossroadMap()
    {
        SceneView.lastActiveSceneView.in2DMode = true;
        if (Camera.main)
        {
            Camera.main.orthographic = true;
            if (!Camera.main.gameObject.GetComponent<CameraLayer>())
                Camera.main.gameObject.AddComponent<CameraLayer>();
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
        mapObject.name = "Map";
        mapObject.AddComponent<CorridorPanHandler>();
        var tg = mapObject.AddComponent<TransformGesture>();
        tg.Type = TransformGesture.TransformType.Translation;
        var coCo = mapObject.GetComponent<CorridorPanController>();
        coCo.inertiaCoefficient = 0.93f;
        coCo.maxPanSpeed = 2f;
        coCo.finishGestureThreshold = 0.001f;
        coCo.enableDebugVizualization = true;
        var touchPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        touchPlane.name = "Touch Recieving Collider";
        touchPlane.transform.parent = mapObject.transform;
        touchPlane.transform.rotation = Quaternion.Euler(-90, 0, 0);
        touchPlane.transform.localScale = new Vector3(100, 1, 100);
        var mr = touchPlane.GetComponent<MeshRenderer>();
        DestroyImmediate(mr);
        var crossroadNodes = new GameObject();
        crossroadNodes.name = "Crossroad Nodes";
        crossroadNodes.transform.parent = mapObject.transform;
        var contents = new GameObject();
        contents.name = "Contents";
        contents.transform.parent = mapObject.transform;
        mapInitiated = true;
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
        Enable();
        InitStartingNode();
    }
    private static void SetMenuFoldoutsTo(bool v)
    {
        for (int i = 0; i < foldout.Length; i++)
        {
            foldout[i] = v;
        }
    }
    private static void AddHorizontalPlus(Node node)
    {
        Node newNode = AddNode();
        newNode.gameObject.transform.position = new Vector3(node.transform.position.x + node.hSize / 2f + newNode.hSize / 2f + 1f, node.transform.position.y, node.transform.position.z);
        if (node.hPlus == null)
        {
            newNode.hMinus = node;
            node.hPlus = newNode;
        }
        else
        {
            node.hPlus.hMinus = newNode;
            newNode.hPlus = node.hPlus;
            newNode.hMinus = node;
            node.hPlus = newNode;
        }
        AddFoldoutItem();
    }
    private static void AddHorizontalMinus(Node node)
    {
        Node newNode = AddNode();
        newNode.gameObject.transform.position = new Vector3(node.transform.position.x - (node.hSize / 2f + newNode.hSize / 2f + 1f), node.transform.position.y, node.transform.position.z);
        if (node.hMinus == null)
        {
            newNode.hPlus = node;
            node.hMinus = newNode;
        }
        else
        {
            node.hMinus.hPlus = newNode;
            newNode.hMinus = node.hMinus;
            newNode.hPlus = node;
            node.hMinus = newNode;
        }
        AddFoldoutItem();
    }
    private static void AddVerticalPlus(Node node)
    {
        Node newNode = AddNode();
        newNode.gameObject.transform.position = new Vector3(node.transform.position.x, node.transform.position.y + (node.vSize / 2f + newNode.vSize / 2f + 1f), node.transform.position.z);
        if (node.vPlus == null)
        {
            newNode.vMinus = node;
            node.vPlus = newNode;
        }
        else
        {
            node.vPlus.vMinus = newNode;
            newNode.vPlus = node.vPlus;
            newNode.vMinus = node;
            node.vPlus = newNode;
        }
        AddFoldoutItem();
    }
    private static void AddVerticalMinus(Node node)
    {
        Node newNode = AddNode();
        newNode.gameObject.transform.position = new Vector3(node.transform.position.x, node.transform.position.y - (node.vSize / 2f + newNode.vSize / 2f + 1f), node.transform.position.z);
        if (node.vMinus == null)
        {
            newNode.vPlus = node;
            node.vMinus = newNode;
        }
        else
        {
            node.vMinus.vPlus = newNode;
            newNode.vMinus = node.vMinus;
            newNode.vPlus = node;
            node.vMinus = newNode;
        }
        AddFoldoutItem();
    }
    private static void RemoveConnection(Node node, Node connectedNode)
    {
        if (connectedNode == null)
            return;
        else
        {
            if (connectedNode == node.hPlus)
            {
                connectedNode.hMinus = null;
                node.hPlus = null;
            }
            if (connectedNode == node.hMinus)
            {
                connectedNode.hPlus = null;
                node.hMinus = null;
            }
            if (connectedNode == node.vPlus)
            {
                connectedNode.vMinus = null;
                node.vPlus = null;
            }
            if (connectedNode == node.vMinus)
            {
                connectedNode.vPlus = null;
                node.vMinus = null;
            }
        }
    }
    private static void AddFoldoutItem()
    {
        var len = foldout.Length;
        bool[] copy = (bool[])foldout.Clone();
        foldout = new bool[len + 1];
        for (int i = 0; i < len; i++)
        {
            foldout[i] = copy[i];
        }
        foldout[len] = true;
    }
    private static void DeleteNode(Node node)
    {
        if (node != cc.mapStart)
        {
            DestroyImmediate(node.gameObject);
        }
        else
        {
            Debug.LogWarning("Can not delete starting node. Set map starting node to different one and then delete this one.");
        }
    }
    private static void SetStartingNode(Node node)
    {
        cc.mapStart = node;
    }
    private static void DeleteAllNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (cc.mapStart != nodes[i])
                DestroyImmediate(nodes[i].gameObject);
        }
        SceneView.RepaintAll();
    }
    private static void ResetNodeSizesTo(int hSize, int vSize)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].GetComponent<Node>().hSize = hSize;
            nodes[i].GetComponent<Node>().vSize = vSize;
        }
        SceneView.RepaintAll();
    }
    private static void HighlightNode(int i)
    {
        if (nodes[i].GetHighlightStatus() == true)
        {
            nodes[i].SetSelectedTo(false);
        }
        else
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j] != null)
                {
                    nodes[j].SetSelectedTo(false);
                }
            }
            nodes[i].SetSelectedTo(true);
            SceneView.RepaintAll();
        }
    }
    private static void AddStartingNode()
    {
        if (cc.mapStart == null)
        {
            foldout = new bool[0];
            Node n = AddNode();
            cc.mapStart = n;
        }
        else
        {
            Debug.LogWarning("Can not initiate a starting node, because it already exists.");
        }
    }
    private static void InitStartingNode()
    {
        foldout = new bool[0];
        Node n = AddNode();
        cc.mapStart = n;
    }
    public static Node AddNode()
    {
        GameObject crossroadsParent = GameObject.Find(crossroadsParentName);
        if (crossroadsParent == null)
        {
            var go = GameObject.Instantiate(new GameObject());
            go.transform.parent = cc.gameObject.transform;
            go.name = crossroadsParentName;
            crossroadsParent = go;
        }
        GameObject node = new GameObject("Node " + (nodes.Count + 1));
        node.AddComponent(typeof(Node));
        node.transform.parent = crossroadsParent.transform;
        node.transform.position = new Vector3(node.transform.position.x,
            node.transform.position.y,
            crossroadsParent.transform.position.z);
        node.GetComponent<Node>().vSize = nodeSize;
        node.GetComponent<Node>().hSize = nodeSize;
        node.GetComponent<Node>().nodeName = node.name;
        nodes.Add(node.GetComponent<Node>());
        AddFoldoutItem();
        return node.GetComponent<Node>();
    }
    private static GameObject CreateCrossroadDebugView()
    {
        GameObject go;
        if (cc.transform.Find("Corridor Visualization (Debugging)"))
            DestroyImmediate(cc.transform.Find("Corridor Visualization (Debugging)").gameObject);
        go = new GameObject();
        go.transform.parent = cc.transform;
        go.name = "Corridor Visualization (Debugging)";
        cc.debugVizualizationObject = go;
        Material mat1 = new Material(Shader.Find("Standard"));
        mat1.color = new Color(1, 0, 1, 0.3f);
        Material mat2 = new Material(Shader.Find("Standard"));
        mat2.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        mat1.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat1.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat1.SetInt("_ZWrite", 0);
        mat1.DisableKeyword("_ALPHATEST_ON");
        mat1.EnableKeyword("_ALPHABLEND_ON");
        mat1.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat1.renderQueue = 3000;
        mat2.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat2.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat2.SetInt("_ZWrite", 0);
        mat2.DisableKeyword("_ALPHATEST_ON");
        mat2.EnableKeyword("_ALPHABLEND_ON");
        mat2.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat2.renderQueue = 3000;
        foreach (Node n in nodes)
        {
            VizualizeNode(n, mat1, mat2);
        }
        return go;
    }
    private static void CreateQuadNode(Vector3 leftBottom, Vector3 rightBottom, Vector3 rightTop, Vector3 leftTop, Material mat) //mesh, Vector3[] v, int[] t)
    {
        var q = new GameObject();
        q.transform.parent = cc.debugVizualizationObject.transform;
        q.name = "Quad";
        var mf = q.AddComponent<MeshFilter>();
        q.AddComponent<MeshRenderer>();
        q.GetComponent<MeshRenderer>().material = mat;
        q.layer = 2;
        var mesh = new Mesh();
        mf.sharedMesh = mesh;
        mesh.Clear();
        Vector3[] v = {
        leftBottom,
        rightBottom,
        rightTop,
        leftTop,
        };
        int[] t = {
            0,2,1,
            0,3,2
        };
        mesh.vertices = v;
        mesh.triangles = t;
    }
    private static void VizualizeNode(Node n, Material mat1, Material mat2) //mesh, Vector3[] v, int[] t)
    {
        var zShift = 0;
        Vector3 leftBottom = n.transform.position + new Vector3(-n.hSize / 2f, -n.vSize / 2f, zShift);
        Vector3 rightBottom = n.transform.position + new Vector3(n.hSize / 2f, -n.vSize / 2f, zShift);
        Vector3 rightTop = n.transform.position + new Vector3(n.hSize / 2f, n.vSize / 2f, zShift);
        Vector3 leftTop = n.transform.position + new Vector3(-n.hSize / 2f, n.vSize / 2f, zShift);
        CreateQuadNode(leftBottom, rightBottom, rightTop, leftTop, mat1);
        if (n.vPlus)
        {
            leftBottom = n.transform.position + new Vector3(-n.hSize / 2f, n.vSize / 2f, zShift);
            rightBottom = n.transform.position + new Vector3(n.hSize / 2f, n.vSize / 2f, zShift);
            rightTop = n.vPlus.transform.position + new Vector3(n.vPlus.hSize / 2f, -n.vPlus.vSize / 2f, zShift);
            leftTop = n.vPlus.transform.position + new Vector3(-n.vPlus.hSize / 2f, -n.vPlus.vSize / 2f, zShift);
            CreateQuadNode(leftBottom, rightBottom, rightTop, leftTop, mat2);
        }
        if (n.hPlus)
        {
            leftBottom = n.transform.position + new Vector3(n.hSize / 2f, -n.vSize / 2f, zShift);
            rightBottom = n.hPlus.transform.position + new Vector3(-n.hPlus.hSize / 2f, -n.hPlus.vSize / 2f, zShift);
            rightTop = n.hPlus.transform.position + new Vector3(-n.hPlus.hSize / 2f, n.hPlus.vSize / 2f, zShift);
            leftTop = n.transform.position + new Vector3(n.hSize / 2f, n.vSize / 2f, zShift);
            CreateQuadNode(leftBottom, rightBottom, rightTop, leftTop, mat2);
        }
        if (n.vMinus)
        {
            leftBottom = n.transform.position + new Vector3(-n.vMinus.hSize / 2f, n.vMinus.vSize / 2f, zShift);
            rightBottom = n.transform.position + new Vector3(n.vMinus.hSize / 2f, n.vMinus.vSize / 2f, zShift);
            rightTop = n.transform.position + new Vector3(n.hSize / 2f, -n.vSize / 2f, zShift);
            leftTop = n.transform.position + new Vector3(-n.hSize / 2f, -n.vSize / 2f, zShift);
            CreateQuadNode(leftBottom, rightBottom, rightTop, leftTop, mat2);
        }
        if (n.hMinus)
        {
            leftBottom = n.transform.position + new Vector3(n.hMinus.hSize / 2f, -n.hMinus.vSize / 2f, zShift);
            rightBottom = n.transform.position + new Vector3(-n.hSize / 2f, -n.vSize / 2f, zShift);
            rightTop = n.transform.position + new Vector3(-n.hSize / 2f, n.vSize / 2f, zShift);
            leftTop = n.transform.position + new Vector3(n.hMinus.hSize / 2f, n.hMinus.vSize / 2f, zShift);
            CreateQuadNode(leftBottom, rightBottom, rightTop, leftTop, mat2);
        }
    }
    private static void AddQuad(Vector3 leftBottom, Vector3 rightBottom, Vector3 rightTop, Vector3 leftTop, List<Vector3> vl1, List<Vector3> vl2, List<int> tl1, List<int> tl2)
    {
        vl2.Add(leftBottom);
        vl2.Add(rightBottom);
        vl2.Add(rightTop);
        vl2.Add(leftTop);
        var tCount = tl1.Count / 3 + tl2.Count / 3;
        tl2.Add(tCount + 0);
        tl2.Add(tCount + 2);
        tl2.Add(tCount + 1);
        tl2.Add(tCount + 0);
        tl2.Add(tCount + 3);
        tl2.Add(tCount + 2);
        var shift = new Vector3(-70, -80, 0);
        vl2.Add(leftBottom + shift);
        vl2.Add(rightBottom + shift);
        vl2.Add(rightTop + shift);
        vl2.Add(leftTop + shift);
        tCount = tl1.Count / 3 + tl2.Count / 3;
        tl2.Add(tCount + 0);
        tl2.Add(tCount + 2);
        tl2.Add(tCount + 1);
        tl2.Add(tCount + 0);
        tl2.Add(tCount + 3);
        tl2.Add(tCount + 2);
    }
}