using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AutoLoginManager : MonoBehaviour
{
    [Header("Login Form References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Toggle rememberMeToggle;
    [SerializeField] private Button loginButton;

    [Header("Configuration")]
    [SerializeField] private bool autoLoginIfCredentialsExist = false;
    [SerializeField] private float autoLoadDelay = 0.5f;

    private void Start()
    {
        // Wait a short moment before loading credentials to ensure all UI elements are initialized
        StartCoroutine(LoadSavedCredentialsWithDelay());
    }

    private IEnumerator LoadSavedCredentialsWithDelay()
    {
        yield return new WaitForSeconds(autoLoadDelay);
        LoadSavedCredentials();
    }

    private void LoadSavedCredentials()
    {
        // Check if Remember Me was enabled in previous session
        bool rememberMeEnabled = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        if (rememberMeEnabled)
        {
            // Load saved credentials if they exist
            string savedUsername = PlayerPrefs.GetString("SavedUsername", "");
            string savedPassword = PlayerPrefs.GetString("SavedPassword", "");

            // Fill in the input fields
            if (!string.IsNullOrEmpty(savedUsername) && usernameInput != null)
            {
                usernameInput.text = savedUsername;
            }

            if (!string.IsNullOrEmpty(savedPassword) && passwordInput != null)
            {
                passwordInput.text = savedPassword;
            }

            // Set the toggle state
            if (rememberMeToggle != null)
            {
                rememberMeToggle.isOn = true;
            }

            // Auto login if configured and credentials exist
            if (autoLoginIfCredentialsExist &&
                !string.IsNullOrEmpty(savedUsername) &&
                !string.IsNullOrEmpty(savedPassword) &&
                loginButton != null)
            {
                // Click the login button automatically
                loginButton.onClick.Invoke();
            }
        }
    }

    // Public method to clear saved credentials - can be called from a UI button
    public void ClearSavedCredentials()
    {
        PlayerPrefs.DeleteKey("SavedUsername");
        PlayerPrefs.DeleteKey("SavedPassword");
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.Save();

        if (rememberMeToggle != null)
        {
            rememberMeToggle.isOn = false;
        }

        Debug.Log("Saved credentials cleared");
    }
}
