using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using NativeWebSocket;
using Newtonsoft.Json;

/// <summary>
/// Gestiona la conexión con el servidor WebSocket de MT5 y procesa los datos recibidos
/// </summary>
public class MT5DataManager : MonoBehaviour
{
    [Header("Configuración WebSocket")]
    [SerializeField] private string webSocketUrl;
    [SerializeField] private float reconnectInterval = 5f;
    [SerializeField] private int maxReconnectAttempts = 10;

    [Header("IDs de Cuentas MT5")]
    [SerializeField] private long[] accountIds = { 590423106, 590454233 };

    [Header("Referencias UI (Opcional)")]
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private TextMeshProUGUI lastUpdateText;

    [Header("Eventos")]
    [SerializeField] public UnityEvent<string> onConnectionStatusChanged;
    [SerializeField] public UnityEvent<Dictionary<long, MT5AccountData>> onDataReceived;
    [SerializeField] public UnityEvent<MT5AccountData> onAccount1DataReceived;
    [SerializeField] public UnityEvent<MT5AccountData> onAccount2DataReceived;

    // WebSocket
    private WebSocket webSocket;
    private bool isConnected = false;
    private int reconnectAttempts = 0;
    private Dictionary<long, MT5AccountData> accountsData = new Dictionary<long, MT5AccountData>();

    // Para acceder a este gestor desde otros scripts
    public static MT5DataManager Instance { get; private set; }

    // Propiedades públicas
    public bool IsConnected => isConnected;
    public Dictionary<long, MT5AccountData> AccountsData => accountsData;

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        
    }

    private void Start()
    {
        // Iniciar conexión automáticamente al iniciar
        ConnectToWebSocket();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (webSocket != null)
        {
            // Se necesita llamar a DispatchMessageQueue en Update
            webSocket.DispatchMessageQueue();
        }
#endif
    }

    private async void ConnectToWebSocket()
    {
        UpdateConnectionStatus("Conectando...");

        try
        {
            // Crear una nueva instancia de WebSocket
            webSocket = new WebSocket(webSocketUrl);

            // Definir callbacks
            webSocket.OnOpen += OnWebSocketOpen;
            webSocket.OnMessage += OnWebSocketMessage;
            webSocket.OnError += OnWebSocketError;
            webSocket.OnClose += OnWebSocketClose;

            // Iniciar la conexión
            await webSocket.Connect();
        }
        catch (Exception e)
        {
           // Debug.LogError($"Error al conectar al WebSocket: {e.Message}");
          //UpdateConnectionStatus($"Error: {e.Message}");
            TryReconnect();
        }
    }

    private void OnWebSocketOpen()
    {
        isConnected = true;
        reconnectAttempts = 0;
        UpdateConnectionStatus("Conectado");
        Debug.Log("Conexión WebSocket establecida");
    }

    private void OnWebSocketMessage(byte[] data)
    {
        string message = System.Text.Encoding.UTF8.GetString(data);
        ProcessWebSocketMessage(message);
    }

    private void OnWebSocketError(string errorMsg)
    {
        Debug.LogError($"Error en WebSocket: {errorMsg}");
        UpdateConnectionStatus($"Error: {errorMsg}");
    }

    private void OnWebSocketClose(WebSocketCloseCode code)
    {
        isConnected = false;
        UpdateConnectionStatus($"Desconectado (Código: {code})");
        Debug.Log($"Conexión WebSocket cerrada con código: {code}");

        // Intentar reconectar
        TryReconnect();
    }

    private void ProcessWebSocketMessage(string message)
    {
        try
        {
            // Parsear el mensaje JSON
            var data = JsonConvert.DeserializeObject<Dictionary<string, MT5AccountData>>(message);
            if (data != null)
            {
                // Convertir claves de string a long (IDs de cuentas)
                accountsData.Clear();
                foreach (var kvp in data)
                {
                    if (long.TryParse(kvp.Key, out long accountId))
                    {
                        accountsData[accountId] = kvp.Value;
                        // Invocar eventos específicos para cada cuenta
                        if (accountId == accountIds[0] && onAccount1DataReceived != null)
                        {
                            onAccount1DataReceived.Invoke(kvp.Value);
                        }
                        else if (accountId == accountIds[1] && onAccount2DataReceived != null)
                        {
                            onAccount2DataReceived.Invoke(kvp.Value);
                        }
                    }
                }
                // Invocar evento general con todos los datos
                if (onDataReceived != null)
                {
                    onDataReceived.Invoke(accountsData);
                }
                // Actualizar UI de última actualización
                if (lastUpdateText != null)
                {
                    lastUpdateText.text = $"Última actualización: {DateTime.Now.ToString("HH:mm:ss")}";
                }
                Debug.Log($"Datos MT5 actualizados para {accountsData.Count} cuentas");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al procesar mensaje WebSocket: {e.Message}");
        }
    }

        private void TryReconnect()
    {
        if (reconnectAttempts >= maxReconnectAttempts)
        {
            UpdateConnectionStatus($"Reconexión fallida después de {maxReconnectAttempts} intentos");
            return;
        }

        reconnectAttempts++;
        UpdateConnectionStatus($"Reconectando ({reconnectAttempts}/{maxReconnectAttempts})...");

        // Esperar antes de intentar reconectar usando coroutine
        StartCoroutine(ReconnectAfterDelay());
    }

    private IEnumerator ReconnectAfterDelay()
    {
        // Esperar el intervalo configurado
        yield return new WaitForSeconds(reconnectInterval);

        // Intentar nueva conexión
        ConnectToWebSocket();
    }

    private void UpdateConnectionStatus(string status)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = $"Estado: {status}";
        }

        // Invocar evento
        if (onConnectionStatusChanged != null)
        {
            onConnectionStatusChanged.Invoke(status);
        }

        Debug.Log($"Estado de conexión MT5: {status}");
    }

    // Método para solicitar datos de una cuenta específica
    public async void RequestAccountData(long accountId)
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            string request = JsonConvert.SerializeObject(new { request_account = accountId.ToString() });
            await webSocket.SendText(request);
        }
    }

    // Desconexión al cerrar la aplicación
    private async void OnApplicationQuit()
    {
        if (webSocket != null)
        {
            // Desregistrar los callbacks
            webSocket.OnOpen -= OnWebSocketOpen;
            webSocket.OnMessage -= OnWebSocketMessage;
            webSocket.OnError -= OnWebSocketError;
            webSocket.OnClose -= OnWebSocketClose;

            // Cerrar el WebSocket si está abierto
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.Close();
            }
        }
    }

    // Desconexión al destruir el objeto
    private async void OnDestroy()
    {
        if (webSocket != null)
        {
            // Desregistrar los callbacks
            webSocket.OnOpen -= OnWebSocketOpen;
            webSocket.OnMessage -= OnWebSocketMessage;
            webSocket.OnError -= OnWebSocketError;
            webSocket.OnClose -= OnWebSocketClose;

            // Cerrar el WebSocket si está abierto
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.Close();
            }
        }
    }

    // Añade estas propiedades y métodos a la clase MT5DataManager

    // Propiedad para obtener el ID de la cuenta actual
    public long CurrentAccountId
    {
        get
        {
            string accountIdStr = PlayerPrefs.GetString("CurrentAccountID", "0");
            long.TryParse(accountIdStr, out long accountId);
            return accountId;
        }
    }

    // Método para obtener los datos de la cuenta actual
    public MT5AccountData GetCurrentAccountData()
    {
        long accountId = CurrentAccountId;
        if (accountId > 0 && accountsData.TryGetValue(accountId, out MT5AccountData data))
        {
            return data;
        }
        return null;
    }

    // Método para verificar si tenemos datos para la cuenta actual
    public bool HasCurrentAccountData()
    {
        return GetCurrentAccountData() != null;
    }

    // Método para obtener un valor específico de la cuenta actual
    public T GetCurrentAccountValue<T>(System.Func<MT5AccountData, T> selector, T defaultValue)
    {
        var data = GetCurrentAccountData();
        if (data != null)
        {
            return selector(data);
        }
        return defaultValue;
    }

    // Ejemplo de uso:
    // double balance = MT5DataManager.Instance.GetCurrentAccountValue(d => d.balance, 0.0);
}

/// <summary>
/// Clase para almacenar los datos de una cuenta MT5
/// </summary>
[System.Serializable]
public class MT5AccountData
{
    public string name;
    public long login;
    public string server;
    public double balance;
    public double equity;
    public double margin;
    public double free_margin;
    public double profit;
    public string timestamp;
    public List<MT5Position> positions;
    public string error;

    // Constructor
    public MT5AccountData()
    {
        positions = new List<MT5Position>();
    }

    // Propiedad para verificar si la cuenta tiene error
    public bool HasError => !string.IsNullOrEmpty(error);
}

/// <summary>
/// Clase para almacenar una posición de trading
/// </summary>
[System.Serializable]
public class MT5Position
{
    public string symbol;
    public string type;
    public double volume;
    public double open_price;
    public double current_price;
    public double profit;
}