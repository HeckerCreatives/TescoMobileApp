using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("GameObject")]
    [SerializeField] private GameObject errorPanelObj;
    [SerializeField] private GameObject confirmationPanelObj;

    [Header("TMP")]
    [SerializeField] private TextMeshProUGUI errorTMP;
    [SerializeField] private TextMeshProUGUI confirmationTMP;

    public Action currentAction, closeAction;

    #region CONFIRMATION

    public void ShowConfirmation(string statusText, Action currentConfirmationAction, Action closeConfirmationAction)
    {
        confirmationTMP.text = statusText;
        confirmationPanelObj.SetActive(true);
        currentAction = currentConfirmationAction;
        closeAction = closeConfirmationAction;
    }

    public void ConfirmedAction()
    {
        //GameManager.Instance.Sound.SetSFXAudio(acceptClip);

        gameManager.CanUseButton = true;
        confirmationPanelObj.SetActive(false);
        confirmationTMP.text = "";

        if (currentAction != null)
        {
            currentAction();
            currentAction = null;
        }
    }

    public void CloseConfirmationAction()
    {
        //GameManager.Instance.Sound.SetSFXAudio(backClip);

        gameManager.CanUseButton = true;
        confirmationPanelObj.SetActive(false);
        confirmationTMP.text = "";

        if (closeAction != null)
        {
            closeAction();
            closeAction = null;

        }
    }

    #endregion

    #region ERROR

    public void ShowError(string statusText, Action closeConfirmationAction)
    {
        errorTMP.text = statusText;
        errorPanelObj.SetActive(true);
        closeAction = closeConfirmationAction;
    }

    public void CloseErrorAction()
    {
        //GameManager.Instance.Sound.SetSFXAudio(backClip);

        gameManager.CanUseButton = true;
        errorPanelObj.SetActive(false);
        errorTMP.text = "";

        if (closeAction != null)
        {
            closeAction();
            closeAction = null;
        }
    }


    #endregion
}
