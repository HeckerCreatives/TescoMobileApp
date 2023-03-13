using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserData", menuName = "Tesco/Data/UserData")]
public class UserData : ScriptableObject
{
    [field: ReadOnly] [field: SerializeField] public string LoginData { get; private set; }
    [field: ReadOnly] [field: SerializeField] public List<string> TopicData { get; private set; }
    [field: ReadOnly] [field: SerializeField] public List<QuestionnaireData> Questionnaires { get; private set; }

    private void OnEnable()
    {
        LoginData = "";
        TopicData.Clear();
        Questionnaires.Clear();
    }

    #region SETTERS

    public void LoginDataSet(string data)
    {
        LoginData = data;
    }

    public void TopicDataSet(List<string> data)
    {
        TopicData = data;
    }

    public void QuestionnaireDataSet(List<QuestionnaireData> data)
    {
        Questionnaires = data;
    }

    #endregion
}
