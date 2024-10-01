using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectPlatformPopup : MonoBehaviour
{
    [SerializeField] private Button googleButton;
    [SerializeField] private Button appleButton;
    [SerializeField] private Button guestButton;
    [SerializeField] private Button emailButton;
    [SerializeField] private TMP_InputField emailInputField;
    
    private System.Action<EPlatform> callback;

    private void Awake()
    {
        googleButton.onClick.AddListener(() =>
        {
            callback.Invoke(EPlatform.Google);
        });
        appleButton.onClick.AddListener(() =>
        {
            callback.Invoke(EPlatform.Apple);
        });
        guestButton.onClick.AddListener(() =>
        {
            callback.Invoke(EPlatform.Guest);
        });
        emailButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(emailInputField.text))
                return;
            AuthManager.Instance.EMail = emailInputField.text;
            callback.Invoke(EPlatform.Email);
        });
    }
    public void Set(System.Action<EPlatform> _callback)
    {
        callback = _callback;
    }
}
