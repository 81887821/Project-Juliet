using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    #region Child components
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text content;
    [SerializeField]
    private Text leftButtonText;
    [SerializeField]
    private Text rightButtonText;
    [SerializeField]
    private Button leftButton;
    [SerializeField]
    private Button rightButton;
    #endregion

    public string Title
    {
        get
        {
            return title.text;
        }

        set
        {
            title.text = value;
        }
    }

    public string Content
    {
        get
        {
            return content.text;
        }

        set
        {
            content.text = value;
        }
    }

    public string LeftButton
    {
        get
        {
            return leftButtonText.text;
        }

        set
        {
            leftButtonText.text = value;
        }
    }

    public string RightButton
    {
        get
        {
            return rightButtonText.text;
        }

        set
        {
            rightButtonText.text = value;
        }
    }

    public UnityEvent OnLeftButtonClick
    {
        get
        {
            return leftButton.onClick;
        }
    }

    public UnityEvent OnRightButtonClick
    {
        get
        {
            return rightButton.onClick;
        }
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
