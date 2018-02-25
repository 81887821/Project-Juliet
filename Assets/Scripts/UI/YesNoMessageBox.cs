using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class YesNoMessageBox : MonoBehaviour
{
    #region Child components
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text content;
    [SerializeField]
    private Button noButton;
    [SerializeField]
    private Button yesButton;
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

    public UnityEvent OnNoButtonClick
    {
        get
        {
            return noButton.onClick;
        }
    }

    public UnityEvent OnYesButtonClick
    {
        get
        {
            return yesButton.onClick;
        }
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
