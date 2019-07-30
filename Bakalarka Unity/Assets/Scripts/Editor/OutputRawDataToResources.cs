/*
 * @author Lukáš Lízal 2018
 */
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// This Component pulls raw data about scroll poistion and lastic pan states
/// when using it in editor play mode. This tool as been used for producing
/// detailed graphs in the written part of this thesis on pages Příloha 9-12.
/// Recording first detected input after this script has been activated
/// Recording ends @endedReading after first elastic pan routine results
/// in Done state.
/// </summary>
public class OutputGraphData : MonoBehaviour
{
    [Tooltip("output path inside Resources folder")]
    public string path;
    [Tooltip("output file name")]
    public string fileName;
    [Tooltip("activate read and write script")]
    public bool readAndSave;
    public CorridorPanHandler p;
    [Tooltip("for writing output to")]
    private string s;
    [Tooltip("Elastic pan stages")]
    private string states;
    [Tooltip("number of frame since recording started")]
    private int frameCounter;
    [Tooltip("recording starts after first pan input is detected")]
    private bool startedReading;
    [Tooltip("determining")]
    private bool endedReading;
    [Tooltip("previous position")]
    private float lastPos;
    [Tooltip("speed")]
    private float difference;
    [Tooltip("previous speed")]
    private float lastDifference; 
    [Tooltip("distance traveled")]
    private float distance;
    [Tooltip("acceleration")]
    private float acceleration;
    private NodeState prevNodeState;
    private GestureState prevGestureState;
    /// <summary>
    /// Every frame gather data and write them to
    /// file @filename in Resources folder path @path
    /// </summary>
    void Update(){
        WriteString();
    }
    /// <summary>
    /// Find Crossroad Elastic Controller reference in scene
    /// </summary>
    void Start()
    {
        p = GetComponent<CorridorPanHandler>();
        prevNodeState = NodeState.AttractionZone;
        prevGestureState = GestureState.Done;
    }

    /// <summary>
    /// Get player position data to string.
    /// </summary>
    void WriteString()
    {
        if(readAndSave && !startedReading && (p.c.GetGestureState() == GestureState.Start))
        {
            startedReading = true;
            frameCounter = 0;
            lastPos = transform.position.x;
            distance = 0f;
            acceleration =  0f;

            s += "frame (time); ";
            s += "delta position (speed); ";
            s += "delta speed (acceleration); ";
            s += "position";
            s += System.Environment.NewLine;
        }
        if(!endedReading && startedReading && (p.c.GetGestureState() == GestureState.Done) && !LeanTween.isTweening())
        {
            states += frameCounter+";";
            states += p.c.GetGestureState();
            states += System.Environment.NewLine;
            s += states;
            endedReading = true;
            WriteStringToFile(s);
            Debug.Log(s);
        }
        if(readAndSave && !endedReading && startedReading)
        {
            frameCounter++;
            if(prevGestureState != p.c.GetGestureState())
            {
                states += frameCounter+";";
                states += p.c.GetGestureState();
                states += System.Environment.NewLine;
                prevGestureState = p.c.GetGestureState();
            }
            if(prevNodeState != p.c.GetOccurrance())
            {
                states += frameCounter+";";
                states += p.c.GetOccurrance();
                states += System.Environment.NewLine;
                prevNodeState = p.c.GetOccurrance();
            }
            lastDifference = difference;
            difference = transform.position.x - lastPos;
            distance += difference;
            acceleration = difference-lastDifference;
            s += frameCounter+";";
            s += difference.ToString("0.00000")+";";
            s += acceleration.ToString("0.00000")+";";
            s += distance.ToString("0.00000");
            s += System.Environment.NewLine;

            lastPos = transform.position.x;
            // Debug.Log(frameCounter);
        }
    }
    /// <summary>
    /// Output string to a text file
    /// </summary>
    /// <param name="s">String to write</param>
    void WriteStringToFile(string s){
        s = s.Replace('.',',');

        StreamWriter writer = new StreamWriter(path+"/"+fileName, true);
        writer.WriteLine(s);
        writer.Close();
    }

}