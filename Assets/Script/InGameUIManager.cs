using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This Script controls in-game UI like
//Specifically Movement, Action, Transformation, Health Icons, Diary Icons
//Setting Buttons should be controlled by Global UI Manager. Keep in Mind.

//Singleton Class
public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    //Movement Variables
    [Header("Movement")]
    public Scrollbar movementScrollbar; //joystick for movement

    [Header("Buttons")]
    public Button ActionButton;
    public Button TransformationButton;

    //Health & Diary Variables
    [Header("Health & Diary Sprite")]
    public Sprite blankHeartImage;
    public Sprite halfHeaertImage;
    public Sprite fullHeartImage;
    
    public Sprite diaryImage;

    [Header("Health & Diary Set")]
    public GameObject heartParentObject; //Object Which Contains heart Objects
    public GameObject diaryParentObject; //Object Which Contains diary Objects

    private List<Image> heartImageList;
    private List<Image> diaryImageList;


    private void Awake()
    {
        //Basic way to set Singleton Pattern
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        heartImageList = new List<Image>();
        diaryImageList = new List<Image>();

        foreach(var itr in heartParentObject.GetComponentsInChildren<Image>())
        {
            if (itr != null)
            {
                heartImageList.Add(itr);
            }
        }

        foreach (var itr in diaryParentObject.GetComponentsInChildren<Image>())
        {
            if (itr != null)
            {
                diaryImageList.Add(itr);
            }
        }

        InitUIElements();

    }

    public void InitUIElements()
    {

        //0.5f = Center, 0 = Left, 1 = Right
        movementScrollbar.value = 0.5f;
        foreach(var itr in heartImageList)
        {
            itr.sprite = fullHeartImage;
        }

        foreach(var itr in diaryImageList)
        {
            itr.sprite = diaryImage;
        }

        //TODO: Apply saved data to UI


    }

    //amount: current number of heart
    public void SetCurrentHeartNum(int amount)
    {
        for(int i = 0; i < heartImageList.Count; ++i)
        {
            if(i < (amount / 2))
            {
                heartImageList[i].sprite = fullHeartImage;
            }
            else
            {
                if((amount - i * 2) == 1)
                {
                    heartImageList[i].sprite = halfHeaertImage;
                }
                else
                {
                    heartImageList[i].sprite = blankHeartImage;
                }    
            }
            
        }
    }

    //amount: current number of diary
    public void SetCurrentDiaryNum(int amount)
    {
        for(int i = 0; i < diaryImageList.Count; ++i)
        {
            if(i < amount)
            {
                diaryImageList[i].color = new Color(1, 1, 1, 1);
            }
            else
            {
                diaryImageList[i].color = new Color(1, 1, 1, 0.5f);
            }
        }
    }
}
