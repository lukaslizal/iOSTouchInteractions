/*
*   @author Lukáš Lízal 2018
 */
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
/// <summary>
/// Represents Crossroad entity in corridor based elastic pan controller.
/// </summary>
public class Node : MonoBehaviour
{
    [Tooltip("name of crossroad game object")]
    [HideInInspector]
    public string nodeName;
    [Tooltip("specifies type of guiding used when transitioning to any of the neightbours")]
    public GuideType guideType;
    [Tooltip("width of crossroad (X axis)")]
    [Range(1, 50)]
    public int hSize;
    [Tooltip("height of crossroad (Y axis)")]
    [Range(1, 50)]
    public int vSize;
    [Tooltip("horizontal plus neighbour")]
    public Node hPlus;
    [Tooltip("vertical plus neighbour")]
    public Node vPlus;
    [Tooltip("horizontal minus neighbour")]
    public Node hMinus;
    [Tooltip("vertical minus neighbour")]
    public Node vMinus;
    [Tooltip("item selected in editor window is being highlighted")]
    private bool selected;
    [Tooltip("reveal/hide animation reference for specified direction")]
    [HideInInspector]
    public Animator navHPlus;
    [Tooltip("reveal/hide animation reference for specified direction")]
    [HideInInspector]
    public Animator navHMinus;
    [Tooltip("reveal/hide animation reference for specified direction")]
    [HideInInspector]
    public Animator navVPlus;
    [Tooltip("reveal/hide animation reference for specified direction")]
    [HideInInspector]
    public Animator navVMinus;
    [Tooltip("corridor elastic pan controller reference")]
    public CorridorPanController corridorPanController;
    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        corridorPanController = GameObject.FindObjectOfType<CorridorPanController>();
        Transform[] directions = transform.GetComponentsInChildren<Transform>();
        if (directions.Length > 1)
        {
            //get Animator reference for coming in horizontal plus direction navigation
            int i = 0;
            while (i < directions.Length-1 && directions[i].name != "Navigation hPlus")
            {
                i++;
            }
			if(directions[i].name == "Navigation hPlus")
            	navHPlus = directions[i].GetComponentsInChildren<Animator>()[0];
            //get Animator reference for coming in horizontal minus direction navigation
            i = 0;
            while (i < directions.Length-1 && directions[i].name != "Navigation hMinus")
            {
                i++;
            }
			if(directions[i].name == "Navigation hMinus")
            	navHMinus = directions[i].GetComponentsInChildren<Animator>()[0];
            //get Animator reference for coming in vertical plus direction navigation
            i = 0;
            while (i < directions.Length-1 && directions[i].name != "Navigation vPlus")
            {
                i++;
            }
			if(directions[i].name == "Navigation vPlus")
            	navVPlus = directions[i].GetComponentsInChildren<Animator>()[0];
            //get Animator reference for coming in vertical minus direction navigation
            i = 0;
            while (i < directions.Length-1 && directions[i].name != "Navigation vMinus")
            {
                i++;
            }
			if(directions[i].name == "Navigation vMinus")
            	navVMinus = directions[i].GetComponentsInChildren<Animator>()[0];
        }
    }
    /// <summary>
    /// Gets information wheter crossroad's gizmo
    /// is being highligted.
    /// </summary>
    /// <returns>@selected</returns>
    public bool GetHighlightStatus()
    {
        return selected;
    }
    /// <summary>
    /// Reveals visual navigation helper of current crossroad
    /// for all relevant directions.
    /// </summary>
    public void RevealNavigation()
    {
		if(navHPlus)
			navHPlus.SetBool("showArrow", true);
		if(navHMinus)
			navHMinus.SetBool("showArrow", true);
		if(navVPlus)
			navVPlus.SetBool("showArrow", true);
		if(navVMinus)
			navVMinus.SetBool("showArrow", true);
        if (corridorPanController.orientation == PanOrientation.Horizontal)
        {
            if (navHPlus && corridorPanController.direction == PanDirection.Minus)
            {
                navHPlus.SetBool("showArrow", false);
            }
            else if (navHMinus && corridorPanController.direction == PanDirection.Plus)
            {
                navHMinus.SetBool("showArrow", false);
            }
        }
        else if (corridorPanController.orientation == PanOrientation.Vertical)
        {
            if (navVPlus && corridorPanController.direction == PanDirection.Minus)
            {
                navVPlus.SetBool("showArrow", false);
            }
            else if (navVMinus && corridorPanController.direction == PanDirection.Plus)
            {
                navVMinus.SetBool("showArrow", false);
            }
        }
    }
    /// <summary>
    /// Hides all visual navigation helpers of current crossroad.
    /// </summary>
    public void HideNavigation()
    {
		if(navHPlus)
        	navHPlus.SetBool("showArrow", false);
		if(navHMinus)
        	navHMinus.SetBool("showArrow", false);
		if(navVPlus)
        	navVPlus.SetBool("showArrow", false);
		if(navVMinus)
        	navVMinus.SetBool("showArrow", false);
    }
    /// <summary>
    /// Hightlights and Unhighlights crossroad gizmo.
    /// </summary>
    /// <param name="sel"></param>
    public void SetSelectedTo(bool sel)
    {
        selected = sel;
    }
    /// <summary>
    /// Draws visual manipulator for this crossroad in a Scene View.
    /// </summary>
    void OnDrawGizmos()
    {
        if (selected)
        {
            Gizmos.color = new Color(0, 1f, 0, 1f);
        }
        else
        {
            Gizmos.color = new Color(0, 0, 0, 0.5f);
        }
        Gizmos.DrawCube(transform.position, new Vector3(hSize, vSize, vSize));
    }
}
