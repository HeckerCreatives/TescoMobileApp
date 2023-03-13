//using Microsoft.IdentityModel.Tokens;
using MyBox;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum AppState
    {
        NONE,
        LOGIN,
        DASHBOARD,
        SCANNING,
        RESULT
    }
    private event EventHandler AppStateChange;
    public event EventHandler OnAppStateChange
    {
        add
        {
            if (AppStateChange == null || !AppStateChange.GetInvocationList().Contains(value))
                AppStateChange += value;
        }
        remove { AppStateChange -= value; }
    }
    public AppState CurrentAppState
    {
        get => currentAppState;
        set
        {
            currentAppState = value;
            AppStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CanUseButton
    {
        get => canUseButton;
        set => canUseButton = value;
    }

    public bool DebugMode
    {
        get => debugMode;
        set => debugMode = value;
    }

    //  ===============================================

    [SerializeField] private bool debugMode;

    [Header("SCRIPTS")]
    public AnimationsLT animations;
    public ErrorController errorController;

    [Header("DEBUGGER")]
    [ReadOnly] [SerializeField] private AppState currentAppState;
    [ReadOnly] [SerializeField] private bool canUseButton;

    //  ===============================================

    private void Start()
    {
        CurrentAppState = AppState.LOGIN;
    }

    public string DataSerializer(List<string> keyString, List<object> objValue)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        for (int a = 0; a < keyString.Count; a++)
            data.Add(keyString[a], objValue[a]);

        return JsonConvert.SerializeObject(data);
    }

    //protected string GetName(string token)
    //{
    //    string secret = "iamscrete";
    //    var key = Encoding.ASCII.GetBytes(secret);
    //    var handler = new JwtSecurityTokenHandler();
    //    var validations = new TokenValidationParameters
    //    {
    //        ValidateIssuerSigningKey = true,
    //        IssuerSigningKey = new SymmetricSecurityKey(key),
    //        ValidateIssuer = false,
    //        ValidateAudience = false
    //    };
    //    var claims = handler.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6ImFkbWluI" +
    //        "iwicm9sZSI6ImFkbWluIiwiZmlyc3RuYW1lIjoiamFja3NvbiIsImlkIjoiNjNjNzg4MjdkOTljODA5MGQ0MDViZmNjIiw" +
    //        "iaWF0IjoxNjc1MDUwMTQ2fQ.H_e5HDFzn9HpNGYTKsjzj63fmcVRqCk49aU9UyS0NO0", validations, out var tokenSecure);
    //    return claims.Identity.Name;
    //}
}
