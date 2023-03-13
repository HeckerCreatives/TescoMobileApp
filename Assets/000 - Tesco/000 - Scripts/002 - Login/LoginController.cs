using MyBox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UserData userData;
    [SerializeField] private GameObject loadingNoBg;
    [SerializeField] private ErrorController errorController;

    [Header("ALPHA CANVAS")]
    [SerializeField] private CanvasGroup loginCG;
    [SerializeField] private CanvasGroup dashboardCG;

    [Header("LOGIN")]
    [SerializeField] private TMP_InputField usernameTMP;
    [SerializeField] private TMP_InputField passwordTMP;
    [SerializeField] private Toggle userRoleToggle;
    [SerializeField] private Button loginBtn;

    [Header("DEBUGGER")]
    [ReadOnly] [SerializeField] private string userRole;

    private void Awake()
    {
        gameManager.OnAppStateChange += AppStateChange;
    }

    private void AppStateChange(object sender, EventArgs e)
    {
        ShowLogin();
    }

    private void ShowLogin()
    {
        if (gameManager.CurrentAppState != GameManager.AppState.LOGIN)
            return;

        ResetLogin();

        gameManager.animations.ShowHideFadeCG(loginCG, dashboardCG, UserRole);
    }

    private void ResetLogin()
    {
        usernameTMP.text = "";
        passwordTMP.text = "";
        loginBtn.enabled = true;
    }
    private IEnumerator Login()
    {
        loadingNoBg.SetActive(true);

        if (usernameTMP.text == "")
        {
            loadingNoBg.SetActive(false);
            gameManager.errorController.ShowError("Please input your username", null);
            yield break;
        }

        if (passwordTMP.text == "")
        {
            loadingNoBg.SetActive(false);
            gameManager.errorController.ShowError("Please input your password", null);
            yield break;
        }
        //  show loading here

        if (!gameManager.DebugMode)
        {
            //  API login here

            string httpRequest = "https://tescowebappapi.onrender.com/api/login";

            UnityWebRequest loginRequest = UnityWebRequest.Post(httpRequest, UnityWebRequest.kHttpVerbPOST);
            byte[] credBytes = Encoding.UTF8.GetBytes(gameManager.DataSerializer(new List<string>()
            {
                "username",
                "password",
                "role"
            }, new List<object>()
            {
                usernameTMP.text,
                passwordTMP.text,
                userRole
            }));

            loginRequest.SetRequestHeader("Content-Type", "application/json");

            UploadHandler uploadHandler = new UploadHandlerRaw(credBytes);

            loginRequest.uploadHandler = uploadHandler;

            yield return loginRequest.SendWebRequest();

            if (loginRequest.result == UnityWebRequest.Result.Success)
            {
                string response = loginRequest.downloadHandler.text;
                
                if (response[0] == '{' && response[response.Length - 1] == '}')
                {
                    try
                    {
                        Dictionary<string, object> dataResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                        if (dataResponse.ContainsKey("refreshToken"))
                        {
                            userData.LoginDataSet(response);
                            StartCoroutine(GetTopic(dataResponse["data"].ToString()));
                        }
                        else
                        {
                            errorController.ShowError("Error logging in there's an error on response. Error: " + response, () =>
                            {
                                ResetLogin();
                                loadingNoBg.SetActive(false);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        errorController.ShowError("Error logging in there's an error on response. Error: " + ex.Message, () =>
                        {
                            ResetLogin();
                            loadingNoBg.SetActive(false);
                        });
                    }
                }
                else
                {
                    errorController.ShowError("Error logging in! please check your internet connection and try again. Error: " + response, () =>
                    {
                        ResetLogin();
                        loadingNoBg.SetActive(false);
                    });
                }
            }
            else
            {
                errorController.ShowError("Error logging in! please check your internet connection and try again. Error: " + loginRequest.result, () =>
                {
                    ResetLogin();
                    loadingNoBg.SetActive(false);
                });
            }
        }
    }

    private IEnumerator GetTopic(string id)
    {
        string httpRequest = "https://tescowebappapi.onrender.com/api/topics/" + id;

        UnityWebRequest topicRequest = UnityWebRequest.Get(httpRequest);
        topicRequest.SetRequestHeader("Content-Type", "application/json");
        yield return topicRequest.SendWebRequest();
        if (topicRequest.result == UnityWebRequest.Result.Success)
        {
            string response = topicRequest.downloadHandler.text;

            if (response[0] == '{' && response[response.Length - 1] == '}')
            {
                try
                {
                    Dictionary<string, object> dataResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                    if (dataResponse.ContainsKey("data"))
                    {
                        var topicList = new List<Data>();
                        var topics = new List<string>();
                        topicList = JsonConvert.DeserializeObject<List<Data>>(dataResponse["data"].ToString());
                        if (topicList.Count > 0)
                        {
                            foreach (var value in topicList)
                            {
                                topics.Add(value.topic);
                            }

                            userData.TopicDataSet(topics);
                        }

                        StartCoroutine(GetQuestionnaires(usernameTMP.text));
                    }
                    else
                    {
                        errorController.ShowError("Error logging in there's an error on response. Error: " + response, () =>
                        {
                            ResetLogin();
                            loadingNoBg.SetActive(false);
                        });
                    }
                }
                catch (Exception ex)
                {
                    errorController.ShowError("Error logging in there's an error on response. Error: " + ex.Message, () =>
                    {
                        ResetLogin();
                        loadingNoBg.SetActive(false);
                    });
                }
            }
            else
            {
                errorController.ShowError("Error logging in! please check your internet connection and try again. Error: " + response, () =>
                {
                    ResetLogin();
                    loadingNoBg.SetActive(false);
                });
            }
        }
        else
        {
            errorController.ShowError("Error logging in! please check your internet connection and try again. Error: " + topicRequest.result, () =>
            {
                ResetLogin();
                loadingNoBg.SetActive(false);
            });
        }
    }

    private IEnumerator GetQuestionnaires(string username)
    {
        string httpRequest = "https://tescowebappapi.onrender.com/api/questions/filter/" + username;

        UnityWebRequest questionnairesRequest = UnityWebRequest.Get(httpRequest);
        questionnairesRequest.SetRequestHeader("Content-Type", "application/json");
        yield return questionnairesRequest.SendWebRequest();

        if (questionnairesRequest.result == UnityWebRequest.Result.Success)
        {
            string response = questionnairesRequest.downloadHandler.text;
            if (response[0] == '{' && response[response.Length - 1] == '}')
            {
                try
                {
                    Dictionary<string, object> dataResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                    if (dataResponse.ContainsKey("data"))
                    {
                        var questionnaireList = new List<QuestionnaireData>();
                        questionnaireList = JsonConvert.DeserializeObject<List<QuestionnaireData>>(dataResponse["data"].ToString());

                        if (questionnaireList.Count > 0)
                            userData.QuestionnaireDataSet(questionnaireList);

                        gameManager.CurrentAppState = GameManager.AppState.DASHBOARD;
                        loadingNoBg.SetActive(false);
                    }
                    else
                    {
                        errorController.ShowError("Error logging in there's an error on response. Error: " + response, () =>
                        {
                            ResetLogin();
                            loadingNoBg.SetActive(false);
                        });
                    }
                }
                catch (Exception ex)
                {
                    errorController.ShowError("Error logging in there's an error on response. Error: " + ex.Message, () =>
                    {
                        ResetLogin();
                        loadingNoBg.SetActive(false);
                    });
                }
            }
            else
            {
                errorController.ShowError("Error logging in! please check your internet connection and try again. Error: " + response, () =>
                {
                    ResetLogin();
                    loadingNoBg.SetActive(false);
                });
            }
        }
        else
        {
            errorController.ShowError("Error logging in! please check your internet connection and try again. Error: " + questionnairesRequest.result, () =>
            {
                ResetLogin();
                loadingNoBg.SetActive(false);
            });
        }
    }

    #region BUTTONS


    public void LoginButton()
    {
        if (!gameManager.CanUseButton)
            return;

        gameManager.CanUseButton = false;

        StartCoroutine(Login());
    }

    public void UserRole()
    {
        if (userRoleToggle.isOn) userRole = "admin";
        else userRole = "teacher";
    }

    #endregion
}

[System.Serializable]
public class Data
{
    public string topic;
}

[System.Serializable]
public class QuestionnaireData
{
    public string questionnaire_id;
    public string topic_name;
    public string questionnaire_title;
}
