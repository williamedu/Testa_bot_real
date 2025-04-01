using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DropdownUserSelection : MonoBehaviour
{
    public TMP_InputField userInput;
    public TMP_InputField passwordInput;
    public TMP_Dropdown userDropdown;
    public Toggle rememberUserToggle;  // Toggle para recordar el usuario

    // Clave para guardar la selecci�n del usuario en PlayerPrefs
    private const string USER_PREF_KEY = "SelectedUserIndex";
    private const string REMEMBER_USER_KEY = "RememberUser";

    private void Start()
    {
        // Cargar el estado del toggle desde PlayerPrefs
        if (PlayerPrefs.HasKey(REMEMBER_USER_KEY))
        {
            rememberUserToggle.isOn = PlayerPrefs.GetInt(REMEMBER_USER_KEY) == 1;
        }

        // Primero configuramos el evento del dropdown
        userDropdown.onValueChanged.AddListener(OnUserDropdownChanged);

        // Luego cargamos la �ltima selecci�n si el toggle est� activado
        if (rememberUserToggle.isOn && PlayerPrefs.HasKey(USER_PREF_KEY))
        {
            int savedIndex = PlayerPrefs.GetInt(USER_PREF_KEY);
            // Establecer el valor sin activar el evento
            userDropdown.SetValueWithoutNotify(savedIndex);
        }

        // Por �ltimo, inicializamos los campos basados en la selecci�n actual
        SetUserData(userDropdown.value);

        // Configurar el evento del toggle
        rememberUserToggle.onValueChanged.AddListener(OnRememberToggleChanged);
    }

    private void OnUserDropdownChanged(int value)
    {
        // Guardar la selecci�n si el toggle est� activado
        if (rememberUserToggle.isOn)
        {
            PlayerPrefs.SetInt(USER_PREF_KEY, value);
            PlayerPrefs.Save();
        }

        // Actualizar los campos con los datos del usuario
        SetUserData(value);
    }

    private void OnRememberToggleChanged(bool isOn)
    {
        // Guardar el estado del toggle
        PlayerPrefs.SetInt(REMEMBER_USER_KEY, isOn ? 1 : 0);

        if (isOn)
        {
            // Si se activa, guardar la selecci�n actual
            PlayerPrefs.SetInt(USER_PREF_KEY, userDropdown.value);
        }
        else
        {
            // Si se desactiva, se podr�a borrar la preferencia guardada
            // PlayerPrefs.DeleteKey(USER_PREF_KEY);
            // Comentado porque quiz�s quieras mantener la �ltima selecci�n aunque el toggle est� desactivado
        }

        PlayerPrefs.Save();
    }

    void SetUserData(int userIndex)
    {
        switch (userIndex)
        {
            case 0: // William Hiciano
                userInput.text = "william.hiciano";
                passwordInput.text = "Eldiablaso06";
                break;
            case 1: // Wellington Hiciano
                userInput.text = "wellington.hiciano";
                passwordInput.text = "Elperro25";
                break;
                // Puedes a�adir m�s casos seg�n sea necesario
        }
    }
}
