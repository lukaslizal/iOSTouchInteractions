/*
 * @author Lukáš Lízal
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using System;
using UnityEngine.Events;
using TMPro;
/// <summary>
/// Special button funcionality for menu items, for loading scenes or
/// controlling appearance moving hands animations.
/// </summary>
public class CustomMenuButtonHandler : MonoBehaviour
{
    public UnityEvent onTapEvent;
    public bool isSceneLoader;
    public int loadSceneNumber;
    public bool isInteractionAnimated;
    public float pressTimeModifier;
    public float handTimeModifier;
    public float directionArrowsTimeModifier;
    public GameObject handObject;
    public GameObject directionArrowsObject;
    private GameObject[] trails;
    public TextMeshProUGUI explanation;
    public GameObject screen;
    public GameObject CircleAnimationObject;
    public TransformGesture transformGesture;
    public bool switchHand;
    public bool startingItem;
    private SpriteRenderer[] hands;
    private SpriteRenderer[] directionArrows;
    private int currentHand;
    private void OnEnable()
    {
        GetComponent<TapGesture>().Tapped += tappedHandler;
        GetComponent<PressGesture>().Pressed += pressedHandler;
        GetComponent<ReleaseGesture>().Released += releasedHandler;
        if (transformGesture)
        {
            transformGesture.TransformStarted += transformStartedHandler;
            transformGesture.TransformCompleted += transformCompletedHandler;
        }
    }
    private void OnDisable()
    {
        GetComponent<TapGesture>().Tapped -= tappedHandler;
        GetComponent<PressGesture>().Pressed -= pressedHandler;
        GetComponent<ReleaseGesture>().Released -= releasedHandler;
        if (transformGesture)
        {
            transformGesture.TransformStarted -= transformStartedHandler;
            transformGesture.TransformCompleted -= transformCompletedHandler;
        }
    }
    void Awake()
    {
            if (onTapEvent == null && !isSceneLoader)
            onTapEvent = new UnityEvent();
        if (handObject)
        {
            hands = handObject.GetComponentsInChildren<SpriteRenderer>();
            trails = new GameObject[hands.Length];
            for (int i = 0; i < hands.Length; i++)
            {
                trails[i] = hands[i].transform.gameObject.GetComponentInChildren<TrailRenderer>().transform.gameObject;
            }
            currentHand = -1;
        }
        if (directionArrowsObject)
        {
            directionArrows = directionArrowsObject.GetComponentsInChildren<SpriteRenderer>();
        }
        if (startingItem)
        {
            switchHand = true;
            screen.GetComponentInChildren<ScreenFlicker>().terminateFlickering = false;
            ShowFirstHandOnly();
        }
        else
        {
            switchHand = false;
            HideHand();
            HideDirectionArrows();
        }
    }
    void Start()
    {
        if (isSceneLoader)
        {
            onTapEvent.AddListener(() => ((MySceneManager)FindObjectOfType(typeof(MySceneManager))).LoadSceneNumber(loadSceneNumber));
        }
    }
    private void transformStartedHandler(object sender, EventArgs e)
    {
        HideHand();
    }
    private void transformCompletedHandler(object sender, EventArgs e)
    {
    }
    private void tappedHandler(object sender, EventArgs e)
    {
        onTapEvent.Invoke();
    }
    private void tappedStateChangedHandler(object sender, EventArgs e)
    {
    }
    private void pressedHandler(object sender, EventArgs e)
    {
        ShowItem();
        if (isSceneLoader)
        {
            CircleAnimationObject.GetComponent<Animator>().SetTrigger("Pressed");
            onTapEvent.Invoke();
        }
    }
    private void releasedHandler(object sender, EventArgs e)
    {
        HideItem();
        if (isSceneLoader)
        {
            CircleAnimationObject.GetComponent<Animator>().SetTrigger("Released");
        }
    }
    private void ShowItem()
    {
        HideHand();
    }
    private void HideItem()
    {
        ShowHand();
    }
    private void HideExplanation()
    {
        if (explanation)
        {
            explanation.CrossFadeAlpha(0f, pressTimeModifier, true);
        }
    }
    private void ShowExplanation()
    {
        if (explanation)
        {
            explanation.CrossFadeAlpha(1f, pressTimeModifier, true);
        }
    }
    public void ShowHand()
    {
        if (hands.Length > 0 && switchHand && !startingItem)
        {
            var number = 0;
            do
            {
                number = UnityEngine.Random.Range(0, hands.Length);
            } while (number == currentHand);
            currentHand = number;
            LeanTween.value(hands[currentHand].gameObject, hands[currentHand].color.a, 1f, handTimeModifier).setOnUpdate((float val) =>
                      {
                          hands[currentHand].color = new Color(hands[currentHand].color.r, hands[currentHand].color.g, hands[currentHand].color.b, val);
                      }).setEaseInOutCubic();
                              if (trails[currentHand])
            {
                trails[currentHand].SetActive(true);
            }
        }
        else if (startingItem)
        {
            startingItem = false;
        }
    }
    public void HideHand()
    {
        for (int i = 0; i < hands.Length; i++)
        {
            var hand = hands[i];
            LeanTween.value(hand.gameObject, hand.color.a, 0f, handTimeModifier).setOnUpdate((float val) =>
                      {
                          hand.color = new Color(hand.color.r, hand.color.g, hand.color.b, val);
                      }).setEaseInOutCubic();
            if (trails[i])
            {
                trails[i].SetActive(false);
            }
        }
    }
    public void ShowDirectionArrows()
    {
        if (directionArrowsObject && !startingItem)
        {
            for (int i = 0; i < directionArrows.Length; i++)
            {
                var arrow = directionArrows[i];
                LeanTween.value(arrow.gameObject, arrow.color.a, 1f, directionArrowsTimeModifier).setOnUpdate((float val) =>
                          {
                              arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, val);
                          }).setEaseInOutCubic();
            }
        }
    }
    public void HideDirectionArrows()
    {
        if (directionArrowsObject)
        {
            for (int i = 0; i < directionArrows.Length; i++)
            {
                var arrow = directionArrows[i];
                LeanTween.value(arrow.gameObject, arrow.color.a, 0f, directionArrowsTimeModifier/3f).setOnUpdate((float val) =>
                        {
                            arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, val);
                        }).setEaseInOutCubic();
            }
        }
    }
    public void ShowFirstHandOnly()
    {
        currentHand = 0;
        for (int i = 0; i < hands.Length; i++)
        {
            hands[i].color = new Color(hands[i].color.r, hands[i].color.g, hands[i].color.b, 0f);
            if (trails[i])
            {
                trails[i].SetActive(false);
            }
        }
        if (trails[currentHand] && switchHand)
        {
            trails[currentHand].SetActive(true);
        }
        if (hands.Length > 0 && switchHand)
        {
            hands[currentHand].color = new Color(hands[currentHand].color.r, hands[currentHand].color.g, hands[currentHand].color.b, 1f);
        }
    }
}
