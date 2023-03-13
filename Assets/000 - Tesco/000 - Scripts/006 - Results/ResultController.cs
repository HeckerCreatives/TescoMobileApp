using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ResultController : MonoBehaviour
{
    [SerializeField] private UserData userData;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DashboardController dashboardController;
    [SerializeField] private ScanExam scanExam;
    [SerializeField] private TextMeshProUGUI instructorTMP;
    [SerializeField] private TextMeshProUGUI topicTMP;
    [SerializeField] private TextMeshProUGUI dateTMP;
    [SerializeField] private Image percentImg;
    [SerializeField] private TextMeshProUGUI percentTMP;
    [SerializeField] private TextMeshProUGUI correctTMP;
    [SerializeField] private TextMeshProUGUI wrongTMP;
    [SerializeField] private CanvasGroup resultObj;
    [SerializeField] private CanvasGroup scanObj;
    [SerializeField] private GameObject loadingNoBG;
    [SerializeField] private TMP_InputField firstNameTMP;
    [SerializeField] private TMP_InputField middleNameTMP;
    [SerializeField] private TMP_InputField lastNameTMP;
    [SerializeField] private GameObject enterStudentNameGO;
    [SerializeField] private ErrorController errorController;

    private void Awake()
    {
        gameManager.OnAppStateChange += AppStateChange;
    }

    private void OnDisable()
    {
        gameManager.OnAppStateChange -= AppStateChange;
    }

    private void AppStateChange(object sender, EventArgs e)
    {
        SetDataOnPanel();
        AnimatePanels();
    }

    private void SetDataOnPanel()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.RESULT) return;

        instructorTMP.text = "Instructor Name: " + dashboardController.FullName;
        topicTMP.text = "Topic: " + dashboardController.TopicName.ToUpper();
        dateTMP.text = "Date: " + DateTime.Now.ToString("MM/dd/yyyy h:m:ss:tt");
        percentImg.fillAmount = 0;
        percentTMP.text = ((scanExam.Score / dashboardController.AnswerList.Count) * 100 ).ToString() + "%";
        correctTMP.text = scanExam.Score.ToString();
        wrongTMP.text = scanExam.Wrong.ToString();
        loadingNoBG.SetActive(false);
    }

    private void AnimatePanels()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.RESULT) return;

        gameManager.animations.ShowHideFadeCG(resultObj, scanObj, AnimatePercentImg);
    }

    private void AnimatePercentImg()
    {
        LeanTween.value(percentImg.gameObject, fill => percentImg.fillAmount = fill, 0f, 1f, 0.25f).setEase(LeanTweenType.easeOutCubic);
    }

    IEnumerator SendScore()
    {
        loadingNoBG.SetActive(true);

        Dictionary<string, string>[] data = new Dictionary<string, string>[dashboardController.AnswerList.Count];
        Dictionary<string, object> answerResult = new Dictionary<string, object>();

        for (int a = 0; a < dashboardController.AnswerList.Count; a++)
        {
            data[a] = new Dictionary<string, string>();
            data[a].Add("questionId", dashboardController.QuestionIdList[a]);
            data[a].Add("answer", scanExam.Answers[a]);
            yield return null;
        }

        answerResult.Add("questionaireId", dashboardController.QuestionId);
        answerResult.Add("firstname", firstNameTMP.text.ToUpper());
        answerResult.Add("lastname", middleNameTMP.text.ToUpper());
        answerResult.Add("middlename", lastNameTMP.text.ToUpper());
        answerResult.Add("score", scanExam.Score);
        answerResult.Add("topic", dashboardController.TopicName);
        answerResult.Add("Generated Code", dashboardController.QuestionnaireId);
        answerResult.Add("teacher", dashboardController.Instructor);
        answerResult.Add("answer", data);

        string httpRequest = "https://tescowebappapi.onrender.com/api/create-result";

        UnityWebRequest sendScoreRequest = UnityWebRequest.Post(httpRequest, UnityWebRequest.kHttpVerbPOST);
        byte[] credBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answerResult));
        sendScoreRequest.SetRequestHeader("Content-Type", "application/json");

        UploadHandler uploadHandler = new UploadHandlerRaw(credBytes);

        sendScoreRequest.uploadHandler = uploadHandler;

        yield return sendScoreRequest.SendWebRequest();

        if (sendScoreRequest.result == UnityWebRequest.Result.Success)
        {
            string response = sendScoreRequest.downloadHandler.text;

            if (response[0] == '{' && response[response.Length - 1] == '}')
            {
                try
                {
                    Dictionary<string, object> dataResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                    if (dataResponse.ContainsKey("message") )
                    {
                        if (dataResponse["message"].ToString() == "Result data added")
                        {
                            loadingNoBG.SetActive(false);
                            errorController.ShowError("Save Successful.", () =>
                            {
                                resultObj.gameObject.SetActive(false);
                                enterStudentNameGO.SetActive(false);
                                firstNameTMP.text = "";
                                middleNameTMP.text = "";
                                lastNameTMP.text = "";
                                gameManager.CurrentAppState = GameManager.AppState.DASHBOARD;
                            });
                        }
                        else
                        {
                            loadingNoBG.SetActive(false);
                            errorController.ShowError("There's a problem with the server or internet connection. Please try again later!", () =>
                            {
                                gameManager.CanUseButton = true;
                            });
                        }
                    }
                }
                catch(Exception ex)
                {
                    loadingNoBG.SetActive(false);
                    errorController.ShowError("There's a problem with the server or internet connection. Please try again later!", () =>
                    {
                        gameManager.CanUseButton = true;
                    });
                }
            }
            else
            {
                loadingNoBG.SetActive(false);
                errorController.ShowError("There's a problem with the server or internet connection. Please try again later!", () =>
                {
                    gameManager.CanUseButton = true;
                });
            }
        }
        else
        {
            loadingNoBG.SetActive(false);
            errorController.ShowError("There's a problem with the server or internet connection. Please try again later!", () =>
            {
                gameManager.CanUseButton = true;
            });
        }
    }

    #region BUTTONS

    public void EnterStudentName()
    {
        if (!gameManager.CanUseButton) return;

        gameManager.CanUseButton = false;

        enterStudentNameGO.SetActive(true);

        gameManager.CanUseButton = true;
    }

    public void SendScoreButton()
    {
        if (!gameManager.CanUseButton) return;

        gameManager.CanUseButton = false;

        if (firstNameTMP.text == "")
        {
            errorController.ShowError("Please enter student first name", () =>
            {
                gameManager.CanUseButton = true;
            });
            return;
        }

        else if (middleNameTMP.text == "")
        {
            errorController.ShowError("Please enter student middle name", () =>
            {
                gameManager.CanUseButton = true;
            });
            return;
        }

        else if (lastNameTMP.text == "")
        {
            errorController.ShowError("Please enter student last name", () =>
            {
                gameManager.CanUseButton = true;
            });
            return;
        }

        StartCoroutine(SendScore());
    }

    #endregion
}