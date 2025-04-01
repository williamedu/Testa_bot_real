using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject popup;
    [SerializeField] private Toggle rememberMeToggle;

    [Header("API Configuration")]
    [SerializeField] private string apiUrl;

    // Class to serialize login data
    [System.Serializable]
    private class LoginData
    {
        public string username;
        public string password;
    }

    // Class to deserialize response
    [System.Serializable]
    private class LoginResponse
    {
        public bool success;
        public string message;
        public UserData user_data;
    }

    [System.Serializable]
    private class UserData
    {
        public int id;
        public long acc_id;
        public string nombre_usuario;
        public long mt5_acc; // Nuevo campo para el ID de MT5

    }

    private void Start()
    {
        // Add listener to the login button
        loginButton.onClick.AddListener(OnLoginButtonClick);

        // Clear status text
        if (statusText != null)
            statusText.text = "";

        // Set up password input to hide characters
        if (passwordInput != null)
            passwordInput.contentType = TMP_InputField.ContentType.Password;

        // Initialize popup - make sure it's hidden at start
        if (popup != null)
            popup.transform.localScale = Vector3.zero;

        // Load saved credentials if remember me was enabled
        LoadSavedCredentials();
    }

    private void LoadSavedCredentials()
    {
        // Check if Remember Me was enabled in previous session
        bool rememberMeEnabled = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        if (rememberMeEnabled && rememberMeToggle != null)
        {
            // Set toggle state
            rememberMeToggle.isOn = true;

            // Load saved credentials
            string savedUsername = PlayerPrefs.GetString("SavedUsername", "");
            string savedPassword = PlayerPrefs.GetString("SavedPassword", "");

            // Fill the input fields
            if (!string.IsNullOrEmpty(savedUsername) && usernameInput != null)
            {
                usernameInput.text = savedUsername;
            }

            if (!string.IsNullOrEmpty(savedPassword) && passwordInput != null)
            {
                passwordInput.text = savedPassword;
            }
        }
    }

    public void OnLoginButtonClick()
    {
        // Validate input fields
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowStatus("Usuario y contraseña son requeridos", Color.red);
            ShowPopup("Usuario y contraseña son requeridos");
            return;
        }

        // Start login process
        StartCoroutine(LoginUser(usernameInput.text, passwordInput.text));
    }

    private IEnumerator LoginUser(string username, string password)
    {
        // Show loading state
        ShowStatus("Verificando credenciales...", Color.yellow);

        // Disable login button during the request
        loginButton.interactable = false;

        // Create login data object
        LoginData loginData = new LoginData
        {
            username = username,
            password = password
        };

        // Convert to JSON
        string jsonData = JsonConvert.SerializeObject(loginData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // Create request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send request
        yield return request.SendWebRequest();

        // Re-enable login button
        loginButton.interactable = true;

        // Handle response
        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorMessage = "Error de conexión: " + request.error;
            ShowStatus(errorMessage, Color.red);
            ShowPopup(errorMessage);
            Debug.LogError("Error connecting to API: " + request.error);
        }
        else
        {
            try
            {
                // Parse response
                LoginResponse response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);

                // Y luego modificar la sección donde procesas el login exitoso:
                if (response.success)
                {
                    Debug.Log("Respuesta JSON completa: " + request.downloadHandler.text);
                    // Login successful
                    ShowStatus("Login exitoso", Color.green);
                    Debug.Log("Logged in as: " + response.user_data.nombre_usuario);

                    // Save the username to PlayerPrefs
                    PlayerPrefs.SetString("Username", usernameInput.text);
                    PlayerPrefs.SetInt("UserID", response.user_data.id);
                    PlayerPrefs.SetString("NombreUsuario", response.user_data.nombre_usuario);

                    // Guardar el ID de la cuenta MT5 (si existe, de lo contrario usamos un valor predeterminado)
                    long mt5AccountId = response.user_data.acc_id;
                    print("El accID que obtuvimos fue: " + response.user_data.acc_id);

                    // Asignar ID basado en el ID de usuario si acc_id es 0
                    if (mt5AccountId == 0)
                    {
                        // Asignar ID basado en el ID de usuario
                        if (response.user_data.id == 1)
                            mt5AccountId = 590423106; // Primera cuenta
                        else if (response.user_data.id == 2)
                            mt5AccountId = 590454233; // Segunda cuenta
                        else
                            mt5AccountId = 590423106; // Valor predeterminado para otros usuarios
                    }
                    PlayerPrefs.SetString("CurrentAccountID", mt5AccountId.ToString());

                    // Si remember me toggle está activado, guardar credenciales
                    if (rememberMeToggle != null && rememberMeToggle.isOn)
                    {
                        PlayerPrefs.SetString("SavedUsername", usernameInput.text);
                        PlayerPrefs.SetString("SavedPassword", passwordInput.text);
                        PlayerPrefs.SetInt("RememberMe", 1);
                    }
                    else
                    {
                        // Limpiar credenciales guardadas
                        PlayerPrefs.DeleteKey("SavedUsername");
                        PlayerPrefs.DeleteKey("SavedPassword");
                        PlayerPrefs.SetInt("RememberMe", 0);
                    }

                    PlayerPrefs.Save();

                    // Load the Operations scene
                    SceneManager.LoadScene("new_operations");
                }
                else
                {
                    // Login failed
                    ShowStatus("Credenciales incorrectas", Color.red);
                    ShowPopup(response.message ?? "Credenciales incorrectas");
                }
            }
            catch (System.Exception e)
            {
                string errorMessage = "Error al procesar la respuesta";
                ShowStatus(errorMessage, Color.red);
                ShowPopup(errorMessage);
                Debug.LogError("Error parsing API response: " + e.Message);
                Debug.LogError("Response text: " + request.downloadHandler.text);
            }
        }
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }

    private void ShowPopup(string errorMessage)
    {
        if (popup != null)
        {
            // Find the title and message components in the popup
            Transform titleTransform = popup.GetComponentInChildren<titleGO>().transform;
            Transform messageTransform = popup.GetComponentInChildren<messageGO>().transform;

            if (titleTransform != null)
            {
                TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = "WARNING";
                }
            }

            if (messageTransform != null)
            {
                TextMeshProUGUI messageText = messageTransform.GetComponent<TextMeshProUGUI>();
                if (messageText != null)
                {
                    messageText.text = errorMessage;
                }
            }

            // Animate the popup using DOTween
            popup.transform.localScale = Vector3.zero;
            popup.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }

    public void deactivatepopUp()
    {
        popup.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutBack);
    }

    // Method to clear saved credentials (can be called from a UI button if needed)
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