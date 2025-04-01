using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;

public class PhpConnectionTest : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiUrl = "http://44.201.81.192/test.php";

    // Estructura para enviar datos al servidor
    [System.Serializable]
    private class TestData
    {
        public string message = "Hola desde Unity";
        public string timestamp;
        public string device_info;
    }

    // Estructura para recibir la respuesta
    [System.Serializable]
    private class ServerResponse
    {
        public bool success;
        public string message;
        public TestData received_data;
        public string timestamp;
        public ServerInfo server_info;
    }

    [System.Serializable]
    private class ServerInfo
    {
        public string php_version;
        public string server_software;
        public string ip_address;
    }

    private void Start()
    {
        // Esperar un momento para asegurarse de que Unity está completamente iniciado
        Invoke("TestConnection", 1.0f);
    }

    public void TestConnection()
    {
        Debug.Log("Iniciando prueba de conexión con el servidor PHP...");
        StartCoroutine(SendTestRequest());
    }

    private IEnumerator SendTestRequest()
    {
        // Crear datos de prueba
        TestData testData = new TestData
        {
            message = "Hola desde Unity",
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            device_info = $"Unity {Application.unityVersion}, {SystemInfo.operatingSystem}"
        };

        // Convertir a JSON
        string jsonData = JsonConvert.SerializeObject(testData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // Crear request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"Enviando solicitud a: {apiUrl}");
        Debug.Log($"Datos enviados: {jsonData}");

        // Enviar request
        yield return request.SendWebRequest();

        // Manejar respuesta
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error de conexión: {request.error}");
            Debug.LogError($"URL: {apiUrl}");
            Debug.LogError($"Código de estado: {request.responseCode}");
        }
        else
        {
            try
            {
                Debug.Log("Respuesta recibida del servidor:");
                Debug.Log(request.downloadHandler.text);

                // Parsear la respuesta JSON
                ServerResponse response = JsonConvert.DeserializeObject<ServerResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    Debug.Log($"Conexión exitosa: {response.message}");
                    Debug.Log($"Servidor PHP versión: {response.server_info.php_version}");
                    Debug.Log($"Software del servidor: {response.server_info.server_software}");
                    Debug.Log($"Respuesta generada a las: {response.timestamp}");
                }
                else
                {
                    Debug.LogWarning($"El servidor respondió, pero reportó un error: {response.message}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al procesar la respuesta: {e.Message}");
                Debug.LogError($"Texto de la respuesta: {request.downloadHandler.text}");
            }
        }
    }
}