using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScanExam : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private WebCamTextureToCloudVision webcam;
    [SerializeField] private DashboardController dashboardController;
    [SerializeField] private GameObject loadingNoBg;
    [SerializeField] private GameObject cameraTexture;
    [SerializeField] private CanvasGroup dashboardCG;
    [SerializeField] private CanvasGroup scannerCG;
    [SerializeField] private GameObject successGO;
    [SerializeField] private GameObject failedGO;
    [SerializeField] private TextMeshProUGUI answersTMP;
    [SerializeField] private ErrorController errorController;

    [field: Header("DEBUGGER")]
    [field: ReadOnly] [field: SerializeField] public List<string> Answers { get; set; }
    [field: ReadOnly] [field: SerializeField] public int Score { get; set; }
    [field: ReadOnly] [field: SerializeField] public int Wrong { get; set; }


    //  =========================

    Coroutine scanCoroutine;

    //  =========================

    private void OnEnable()
    {
        gameManager.OnAppStateChange += AppStateChange;
    }

    private void OnDisable()
    {
        gameManager.OnAppStateChange -= AppStateChange;
    }

    private void AppStateChange(object sender, EventArgs e)
    {
        ResetScan();
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.SCANNING) return;

        cameraTexture.SetActive(true);
        webcam.InitializeCamera(AnimatePanels);
    }

    private void AnimatePanels()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.SCANNING) return;

        gameManager.animations.ShowHideFadeCG(scannerCG, dashboardCG, () => loadingNoBg.SetActive(false));
    }

    private void Success()
    {
        answersTMP.text = "";
        StartCoroutine(CheckAnswers());
    }

    IEnumerator CheckAnswers()
    {
        for (int a = 0; a < dashboardController.AnswerList.Count; a++)
        {
            if (Answers[a].Replace(" ", String.Empty).ToLower() == dashboardController.AnswerList[a].Replace(" ", String.Empty).ToLower()) Score++;
            else Wrong++;

            yield return null;
        }

        for (int a = 0; a < Answers.Count; a++)
        {
            try
            {
                answersTMP.text += a + 1 + ".) Answer: " + Answers[a] + "\n" + "Correct Answer: " + dashboardController.AnswerList[a];
            }
            catch(Exception ex) { }

            if (a < Answers.Count - 1)
                answersTMP.text += "\n\n";

            yield return null;
        }

        StopCoroutine(scanCoroutine);

        gameManager.CanUseButton = true;
        loadingNoBg.SetActive(false);
        successGO.SetActive(true);
    }

    private void Failed()
    {
        StopCoroutine(scanCoroutine);

        loadingNoBg.SetActive(false);
        errorController.ShowError("Can't scan the image, please try again!", () =>
        {
            ResetScan();
            gameManager.CanUseButton = true;
        });
    }

    private void ResetScan()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.SCANNING) return;

        Answers.Clear();
        Wrong = 0;
        Score = 0;
    }

    #region BUTTON

    public void Scan()
    {
        if (!gameManager.CanUseButton) return;

        gameManager.CanUseButton = false;

        loadingNoBg.SetActive(true);

        scanCoroutine = StartCoroutine(webcam.Capture(Success, Failed));
    }

    public void Retry()
    {
        if (!gameManager.CanUseButton) return;

        gameManager.CanUseButton = false;

        ResetScan();
        successGO.SetActive(false);

        gameManager.CanUseButton = true;
    }

    public void Results()
    {
        if (!gameManager.CanUseButton) return;

        gameManager.CanUseButton = false;

        successGO.SetActive(false);

        loadingNoBg.SetActive(true);

        webcam.RemoveCamera(() => 
        {
            gameManager.CurrentAppState = GameManager.AppState.RESULT;
        });
    }

    #endregion
}
