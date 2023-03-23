using MyBox;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DashboardController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UserData userData;
    [SerializeField] private GameObject loadingNoBG;
    [SerializeField] private ErrorController errorController;

    [Header("CANVAS GROUP")]
    [SerializeField] private CanvasGroup loginCG;
    [SerializeField] private CanvasGroup dashboardCG;

    [Header("DROPDOWNS")]
    [SerializeField] private TMP_Dropdown topicDropdown;
    [SerializeField] private TMP_Dropdown questionnaireDropdown;

    [Header("TMP")]
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private TextMeshProUGUI occupationTMP;

    [field: Header("DEBUGGER")]
    [field: ReadOnly] [field: SerializeField] public List<string> AnswerList { get; set; }
    [field: ReadOnly] [field: SerializeField] public List<string> QuestionIdList { get; set; }
    [field: ReadOnly] [field: SerializeField] public string QuestionId { get; set; }
    [field: ReadOnly] [field: SerializeField] public Int64 QuestionnaireId { get; set; }
    [field: ReadOnly] [field: SerializeField] public string TopicName { get; set; }
    [field: ReadOnly] [field: SerializeField] public string Instructor { get; set; }
    [field: ReadOnly] [field: SerializeField] public string FullName { get; set; }

    //  ===============================================================

    Coroutine topicListCoroutine;
    Coroutine questionnaireListCoroutine;

    //  ===============================================================

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
        AnimatePanels();
        TestDashboard();
        TopicList();
        QuestionnaireList();
        ResetDashboard();
    }

    private void AnimatePanels()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.DASHBOARD) return;

        gameManager.animations.ShowHideFadeCG(dashboardCG, loginCG, null);
    }

    private void TestDashboard()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.DASHBOARD) return;

        Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(userData.LoginData);
        FullName = data["fullname"].ToString().ToUpper();
        nameTMP.text = data["fullname"].ToString().ToUpper();
        occupationTMP.text = data["message"].ToString().ToUpper();
    }

    private void TopicList()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.DASHBOARD) return;

        topicDropdown.ClearOptions();
        topicDropdown.interactable = false;

        if (topicListCoroutine != null)
            StopCoroutine(topicListCoroutine);

        topicListCoroutine = StartCoroutine(Topic());
    }

    IEnumerator Topic()
    {
        if (userData.TopicData.Count <= 0)
            yield break;

        topicDropdown.AddOptions(userData.TopicData);
        topicDropdown.RefreshShownValue();
        topicDropdown.Show();

        topicDropdown.interactable = true;
    }

    public void QuestionnaireList()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.DASHBOARD) return;

        questionnaireDropdown.ClearOptions();
        questionnaireDropdown.interactable = false;

        if (questionnaireListCoroutine != null)
            StopCoroutine(questionnaireListCoroutine);

        questionnaireListCoroutine = StartCoroutine(Questionnaire());
    }

    IEnumerator Questionnaire()
    {
        if (userData.Questionnaires.Count <= 0)
            yield break;

        List<string> questionnaireData = new List<string>();

        foreach (var data in userData.Questionnaires)
        {
            if (data.topic_name == topicDropdown.options[topicDropdown.value].text)
                questionnaireData.Add(data.questionnaire_title);

            yield return null;
        }

        if (questionnaireData.Count <= 0) yield break;

        questionnaireDropdown.AddOptions(questionnaireData);
        questionnaireDropdown.RefreshShownValue();
        questionnaireDropdown.Show();
        questionnaireDropdown.interactable = true;
    }

    public void ToScanner()
    {
        if (!gameManager.CanUseButton) return;

        loadingNoBG.SetActive(true);

        gameManager.CanUseButton = false;

        if (topicDropdown.options.Count <= 0)
        {
            loadingNoBG.SetActive(false);

            errorController.ShowError("Please select your topic and questionnaire to continue.", () =>
            {
                gameManager.CanUseButton = true;
            });
            return;
        }
        else if (questionnaireDropdown.options.Count <= 0)
        {
            loadingNoBG.SetActive(false);
            errorController.ShowError("Please select your topic and questionnaire to continue.", () =>
            {
                gameManager.CanUseButton = true;
            });
            return;
        }
        else
        {
            if (topicDropdown.options[topicDropdown.value].text == "" && questionnaireDropdown.options[questionnaireDropdown.value].text == "")
            {
                loadingNoBG.SetActive(false);
                errorController.ShowError("Please select your topic and questionnaire to continue.", () =>
                {
                    gameManager.CanUseButton = true;
                });
                return;
            }
        }

        StartCoroutine(GetQuestions());
    }
    
    IEnumerator GetQuestions()
    {
        string questionnaireId = "";
        string username;

        Dictionary<string, object> dataUser = JsonConvert.DeserializeObject<Dictionary<string, object>>(userData.LoginData);
        username = dataUser["username"].ToString();

        foreach (var data in userData.Questionnaires)
        {
            if (data.topic_name == topicDropdown.options[topicDropdown.value].text &&
                data.questionnaire_title == questionnaireDropdown.options[questionnaireDropdown.value].text)
            {
                questionnaireId = data.questionnaire_id;
                break;
            }

            yield return null;
        }

        string httpRequest = $"https://tescowebappapi.onrender.com/api/questions/{username}/{topicDropdown.options[topicDropdown.value].text}/{questionnaireId}";

        UnityWebRequest questionRequest = UnityWebRequest.Get(httpRequest);
        questionRequest.SetRequestHeader("Content-Type", "application/json");
        yield return questionRequest.SendWebRequest();
        Debug.Log(questionRequest.result);
        if (questionRequest.result == UnityWebRequest.Result.Success)
        {
            string response = questionRequest.downloadHandler.text;
            if (response[0] == '{' && response[response.Length - 1] == '}')
            {
                try
                {
                    Dictionary<string, object> dataResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                    AnswerList.Clear();

                    if (dataResponse.ContainsKey("data"))
                    {
                        List<QuestionData> questionData = new List<QuestionData>();
                        questionData = JsonConvert.DeserializeObject<List<QuestionData>>(dataResponse["data"].ToString());

                        if (questionData.Count > 0)
                        {
                            QuestionId = questionData[0]._id;
                            QuestionnaireId = questionData[0].questionnaire_id;
                            TopicName = questionData[0].topic_name;
                            Instructor = questionData[0].instructor;

                            foreach (var data in questionData[0].questions)
                            {
                                AnswerList.Add(data.answer);
                                QuestionIdList.Add(data._id);
                            }

                            gameManager.CurrentAppState = GameManager.AppState.SCANNING;
                        }
                        else
                        {
                            loadingNoBG.SetActive(false);
                            errorController.ShowError("You don't have Questions in your Questionnaire, Please input first then try again.", () =>
                            {
                                gameManager.CanUseButton = true;
                            });
                        }
                    }
                    else
                    {
                        loadingNoBG.SetActive(false);
                        errorController.ShowError("There's a problem with the server. Please try again later. Error: " + response, () =>
                        {
                            gameManager.CanUseButton = true;
                        });
                    }
                }
                catch(Exception ex)
                {
                    loadingNoBG.SetActive(false);
                    errorController.ShowError("There's a problem with the server. Please try again later. Error: " + ex.Message, () =>
                    {
                        gameManager.CanUseButton = true;
                    });
                }
            }
            else
            {
                loadingNoBG.SetActive(false);
                errorController.ShowError("There's a problem with the server. Please try again later. Error: " + response, () =>
                {
                    gameManager.CanUseButton = true;
                });
            }
        }
        else
        {
            loadingNoBG.SetActive(false);
            errorController.ShowError("There's a problem with your network, please try again later. Error: " + questionRequest.error, () =>
            {
                gameManager.CanUseButton = true;
            });
        }
    }

    private void ResetDashboard()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.DASHBOARD) return;

        QuestionId = "";
        QuestionnaireId = 0;
        TopicName = "";
        Instructor = "";
        AnswerList.Clear();
        QuestionIdList.Clear();
    }

    public void Logout()
    {
        gameManager.CurrentAppState = GameManager.AppState.LOGIN;
    }
}

[System.Serializable]
public class QuestionData
{
    public string _id;
    public Int64 questionnaire_id;
    public string topic_name;
    public string questionnaire_title;
    public string instructor;
    public Questions[] questions;
}

[System.Serializable]
public class Questions
{
    public string type;
    public string question;
    public string? choice1;
    public string? choice2;
    public string? choice3;
    public string number;
    public string answer;
    public string _id;
}