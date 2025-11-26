using System;
using UnityEngine;
using UnityEngine.UI;
using VirtueSky.GameService;

public class SignInControl : MonoBehaviour
{
    public StatusLoginVariable statusLoginVariable;
    public Button button;

    private void OnEnable()
    {
        switch (statusLoginVariable.Value)
        {
            case StatusLogin.NotLoggedIn:
                Login();
                break;
            case StatusLogin.Successful:
                break;
            case StatusLogin.Failed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        button.onClick.AddListener(Login);
    }

    public void Login()
    {
        Debug.Log("TODO: Login", this);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Login);
    }
}
