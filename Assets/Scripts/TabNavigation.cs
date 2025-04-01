using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TabNavigation : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;

    void Start()
    {
        // Verificar que ambos campos estén asignados
        if (usernameField == null || passwordField == null)
        {
            Debug.LogError("Error: Debes asignar los campos de username y password en el Inspector.");
            return;
        }

        // Configurar los eventos de Tab para cada campo
        ConfigurarNavegacionTab();

        // Colocar el foco en el campo de username al iniciar
        StartCoroutine(SeleccionarCampoInicial());
    }

    private System.Collections.IEnumerator SeleccionarCampoInicial()
    {
        // Esperar un frame para asegurar que la UI esté lista
        yield return null;

        // Seleccionar el campo de username y activar el teclado
        usernameField.Select();
        usernameField.ActivateInputField();
    }

    private void ConfigurarNavegacionTab()
    {
        // Configurar el evento para el campo de username
        usernameField.onValidateInput += (text, charIndex, addedChar) =>
        {
            // Si se presiona Tab, cambiar al campo de password
            if (addedChar == '\t')
            {
                passwordField.Select();
                passwordField.ActivateInputField();
                // Devolver un carácter nulo para evitar que el Tab se añada al texto
                return '\0';
            }
            return addedChar;
        };

        // Configurar el evento para el campo de password
        passwordField.onValidateInput += (text, charIndex, addedChar) =>
        {
            // Si se presiona Tab, cambiar al campo de username
            if (addedChar == '\t')
            {
                usernameField.Select();
                usernameField.ActivateInputField();
                // Devolver un carácter nulo para evitar que el Tab se añada al texto
                return '\0';
            }
            return addedChar;
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            loginButton.onClick.Invoke(); // Simula un clic en el botón
        }
        // Método alternativo usando el evento de teclado en Update
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Obtener el objeto actualmente seleccionado
            GameObject objetoSeleccionado = EventSystem.current.currentSelectedGameObject;

            if (objetoSeleccionado != null)
            {
                // Cambiar entre los campos usando Tab
                if (objetoSeleccionado == usernameField.gameObject)
                {
                    passwordField.Select();
                    passwordField.ActivateInputField();
                }
                else if (objetoSeleccionado == passwordField.gameObject)
                {
                    usernameField.Select();
                    usernameField.ActivateInputField();
                }
            }
            else
            {
                // Si no hay objeto seleccionado, seleccionar el campo de username
                usernameField.Select();
                usernameField.ActivateInputField();
            }
        }
    }
}
