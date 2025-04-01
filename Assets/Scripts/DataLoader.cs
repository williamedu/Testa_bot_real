using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DataLoader : MonoBehaviour
{
    [Header("User Info Display")]
    [SerializeField] private TextMeshProUGUI usernameDisplay;
    [SerializeField] private TextMeshProUGUI fullNameDisplay;
    [SerializeField] private Image userProfileImage;
    [SerializeField] private TextMeshProUGUI mt5AccountDisplay; // Nuevo campo para MT5 Account ID

    [Header("Configuration")]
    [SerializeField] private string welcomePrefix = "Bienvenido, ";
    [SerializeField] private bool showUserID = false;
    [SerializeField] private TextMeshProUGUI userIDDisplay;
    [SerializeField] private string mt5AccountFormat = "MT5 Account: {0}"; // Formato para MT5 Account

    [Header("Default Values")]
    [SerializeField] private string defaultUsername = "Usuario";
    [SerializeField] private Sprite defaultProfileImage;

    private void Start()
    {
        // Load data from PlayerPrefs as soon as the scene starts
        LoadUserData();
    }

    public void LoadUserData()
    {
        // Get username from PlayerPrefs
        string username = PlayerPrefs.GetString("Username", defaultUsername);
        string nombreUsuario = PlayerPrefs.GetString("NombreUsuario", username);
        int userID = PlayerPrefs.GetInt("UserID", 0);
        string mt5AccountId = PlayerPrefs.GetString("CurrentAccountID", "N/A"); // Obtener MT5 Account ID

        // Display username if component exists
        if (usernameDisplay != null)
        {
            usernameDisplay.text = username;
        }

        // Display full name if component exists
        if (fullNameDisplay != null)
        {
            fullNameDisplay.text = welcomePrefix + nombreUsuario;
        }

        // Display user ID if enabled and component exists
        if (showUserID && userIDDisplay != null)
        {
            userIDDisplay.text = "User ID: " + userID.ToString();
        }

        // Display MT5 Account ID if component exists
        if (mt5AccountDisplay != null)
        {
            mt5AccountDisplay.text = string.Format(mt5AccountFormat, mt5AccountId);
        }

        // Set default profile image if available
        if (userProfileImage != null && defaultProfileImage != null)
        {
            userProfileImage.sprite = defaultProfileImage;
        }

        Debug.Log("Usuario cargado: " + username + " (ID: " + userID + ", MT5 Account: " + mt5AccountId + ")");
    }

    // Method to reload data - can be called from UI events or other scripts
    public void RefreshUserData()
    {
        LoadUserData();
    }

    // Method to check if user is logged in - can be used for scene protection
    public bool IsUserLoggedIn()
    {
        string username = PlayerPrefs.GetString("Username", "");
        return !string.IsNullOrEmpty(username);
    }

    // Method to redirect to login screen if not logged in
    public void ValidateUserSession()
    {
        if (!IsUserLoggedIn())
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
        }
    }

    // Optional: Method to clear user data on logout
    public void ClearUserData()
    {
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("NombreUsuario");
        PlayerPrefs.DeleteKey("UserID");
        PlayerPrefs.DeleteKey("CurrentAccountID"); // También limpiar MT5 Account ID
        PlayerPrefs.Save();

        // You can keep the RememberMe setting and saved credentials
        // This way users don't have to re-enter credentials, but session data is cleared

        Debug.Log("Datos de usuario eliminados - sesión cerrada");
    }
}
