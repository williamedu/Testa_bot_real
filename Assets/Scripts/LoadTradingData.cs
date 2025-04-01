using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using DG.Tweening;

public class LoadTradingData : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiUrl = "http://52.91.175.173/get_trading_data.php";
    [SerializeField] private float dataLoadDelay = 2.0f; // Delay in seconds before loading data

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI profitLossText;
    [SerializeField] private TextMeshProUGUI averageProfitLossText;
    [SerializeField] private TextMeshProUGUI numberOfTradesText;
    [SerializeField] private TextMeshProUGUI averageTradeDurationText;
    [SerializeField] private TextMeshProUGUI averageTradeVolumeText;
    [SerializeField] private TextMeshProUGUI bestTradeText;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TextMeshProUGUI errorMessageText;

    [Header("Format Settings")]
    [SerializeField] private string profitLossPrefix = "$";
    [SerializeField] private string tradeCountPrefix = "Trades: ";
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;

    [Header("Animation Coordinator")]
    [SerializeField] private bool useAnimationCoordinator = true;

    [SerializeField] private TextMeshProUGUI mt5AccountIdText;
    [SerializeField] private string mt5AccountFormat = "MT5 Account: {0}";

    private TradingData cachedTradingData;
    private bool animationsTriggered = false;

    // Class to serialize request data
    [System.Serializable]
    private class RequestData
    {
        public string username;
    }

    // Class to deserialize response
    [System.Serializable]
    private class TradingData
    {
        public float profit_loss;
        public float average_profit_loss;
        public int number_of_trades;
        public string average_trade_duration;
        public float average_trade_volume;
        public float best_trade;
        public bool success;
        public string message;

        // Override ToString for debugging
        public override string ToString()
        {
            return string.Format(
                "Trading Data:\n" +
                "Success: {0}\n" +
                "Message: {1}\n" +
                "Profit/Loss: {2}\n" +
                "Avg Profit/Loss: {3}\n" +
                "Number of Trades: {4}\n" +
                "Avg Duration: {5}\n" +
                "Avg Volume: {6}\n" +
                "Best Trade: {7}",
                success, message, profit_loss, average_profit_loss,
                number_of_trades, average_trade_duration,
                average_trade_volume, best_trade
            );
        }
    }

    private void Start()
    {
        // Hide error message by default
        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        // Display MT5 Account ID if available
        DisplayMT5AccountId();

        // Start loading data with delay
        StartCoroutine(LoadDataWithDelay());
    }

    private void OnEnable()
    {
        // Suscribirse a eventos del coordinador si está disponible
        if (useAnimationCoordinator && UIAnimationCoordinator.Instance != null)
        {
            UIAnimationCoordinator.Instance.onStartAnimations.AddListener(OnCoordinatedAnimationStart);
        }


    }

    private void OnDisable()
    {
        // Desuscribirse de eventos
        if (UIAnimationCoordinator.Instance != null)
        {
            UIAnimationCoordinator.Instance.onStartAnimations.RemoveListener(OnCoordinatedAnimationStart);
        }
    }

    private void OnCoordinatedAnimationStart(float delay)
    {
        // Animar los elementos con los datos almacenados
        if (cachedTradingData != null && cachedTradingData.success)
        {
            // Animar con retraso coordinado
            AnimateUIElementsWithDelay(cachedTradingData, delay);
        }
    }

    private void DisplayMT5AccountId()
    {
        if (mt5AccountIdText != null)
        {
            string mt5AccountId = PlayerPrefs.GetString("CurrentAccountID", "No disponible");
            mt5AccountIdText.text = string.Format(mt5AccountFormat, mt5AccountId);
        }
    }

    private IEnumerator LoadDataWithDelay()
    {
        // Si estamos usando el coordinador, notificar el inicio de carga
        if (useAnimationCoordinator && UIAnimationCoordinator.Instance != null)
        {
            UIAnimationCoordinator.Instance.StartLoading();
        }

        // Wait for specified delay before loading data
        yield return new WaitForSeconds(dataLoadDelay);

        // Get username from PlayerPrefs
        string username = PlayerPrefs.GetString("Username", "");

        // Start the loading process if username exists
        if (!string.IsNullOrEmpty(username))
        {
            // Comparar si el usuario es wellington.hiciano (ignorando mayúsculas/minúsculas)
            if (username.ToLower() == "wellington.hiciano")
            {
                // Cambiar temporalmente al nombre completo en mayúsculas
                username = "WELLINGTON EDUARDO HICIANO MORALES";
            }

            StartCoroutine(LoadTradingDataFromServer(username));
        }
        else
        {
            Debug.LogError("No username found in PlayerPrefs");
            ShowError("No se ha encontrado información de usuario. Por favor, inicie sesión nuevamente.");
        }
    }

    public void RefreshData()
    {
        // Actualizar ID de MT5 por si cambió
        DisplayMT5AccountId();

        // Resetear estado de animación
        animationsTriggered = false;

        // Si estamos usando el coordinador, notificar el inicio de carga
        if (useAnimationCoordinator && UIAnimationCoordinator.Instance != null)
        {
            UIAnimationCoordinator.Instance.StartLoading();
        }

        string username = PlayerPrefs.GetString("Username", "");

        if (!string.IsNullOrEmpty(username))
        {
            // Comparar si el usuario es wellington.hiciano (ignorando mayúsculas/minúsculas)
            if (username.ToLower() == "wellington.hiciano")
            {
                // Cambiar temporalmente al nombre completo en mayúsculas
                username = "WELLINGTON EDUARDO HICIANO MORALES";
            }

            StartCoroutine(LoadTradingDataFromServer(username));
        }
    }

    private IEnumerator LoadTradingDataFromServer(string username)
    {
        // Show loading indicator
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        // Hide error message
        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        // Format username: replace dot with space
        string formattedUsername = username.Replace(".", " ");

        // Create request data object
        RequestData requestData = new RequestData
        {
            username = formattedUsername
        };

        // Convert to JSON
        string jsonData = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // Create request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send request
        yield return request.SendWebRequest();

        // Hide loading indicator
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        // Handle response
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error connecting to API: " + request.error);
            ShowError("Error de conexión: " + request.error);
        }
        else
        {
            try
            {
                // Parse response
                TradingData tradingData = JsonConvert.DeserializeObject<TradingData>(request.downloadHandler.text);

                // Debug: Log the full JSON response with pretty formatting
                string prettyJson = JsonConvert.SerializeObject(
                    JsonConvert.DeserializeObject(request.downloadHandler.text),
                    Formatting.Indented
                );
                Debug.Log("JSON Response:\n" + prettyJson);

                // Log the parsed object values
                Debug.Log(tradingData.ToString());

                if (tradingData.success)
                {
                    // Cache the data
                    cachedTradingData = tradingData;

                    // Si estamos usando el coordinador
                    if (useAnimationCoordinator && UIAnimationCoordinator.Instance != null)
                    {
                        // Actualizar texto sin animaciones todavía
                        UpdateUITextOnly(tradingData);

                        // Notificar al coordinador que los datos están listos
                        UIAnimationCoordinator.Instance.TradingDataReady();
                    }
                    else
                    {
                        // Update UI elements with retrieved data (comportamiento normal)
                        UpdateUIElements(tradingData);
                    }
                }
                else
                {
                    Debug.LogError("API returned error: " + tradingData.message);
                    ShowError("Error: " + tradingData.message);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing API response: " + e.Message);
                Debug.LogError("Response text: " + request.downloadHandler.text);
                ShowError("Error al procesar la respuesta del servidor");
            }
        }
    }

    // Método que solo actualiza el texto sin animaciones
    private void UpdateUITextOnly(TradingData data)
    {
        // Update Profit/Loss
        if (profitLossText != null)
        {
            profitLossText.text = profitLossPrefix + data.profit_loss.ToString("F2");
            profitLossText.color = data.profit_loss >= 0 ? positiveColor : negativeColor;
            // Establecer alpha a 0 para posterior animación
            //profitLossText.alpha = 0f;
        }

        // Update Average Profit/Loss
        if (averageProfitLossText != null)
        {
            averageProfitLossText.text = profitLossPrefix + data.average_profit_loss.ToString("F2");
            averageProfitLossText.color = data.average_profit_loss >= 0 ? positiveColor : negativeColor;
            // averageProfitLossText.alpha = 0f;
        }

        // Update Number of Trades
        if (numberOfTradesText != null)
        {
            numberOfTradesText.text = tradeCountPrefix + data.number_of_trades.ToString();
            // numberOfTradesText.alpha = 0f;
        }

        // Update Average Trade Duration
        if (averageTradeDurationText != null)
        {
            averageTradeDurationText.text = data.average_trade_duration;
            //averageTradeDurationText.alpha = 0f;
        }

        // Update Average Trade Volume
        if (averageTradeVolumeText != null)
        {
            averageTradeVolumeText.text = data.average_trade_volume.ToString("F2");
            // averageTradeVolumeText.alpha = 0f;
        }

        // Update Best Trade
        if (bestTradeText != null)
        {
            bestTradeText.text = profitLossPrefix + data.best_trade.ToString("F2");
            bestTradeText.color = positiveColor;
            // bestTradeText.alpha = 0f;
        }
    }

    private void UpdateUIElements(TradingData data)
    {
        // Update Profit/Loss
        if (profitLossText != null)
        {
            profitLossText.text = profitLossPrefix + data.profit_loss.ToString("F2");
            profitLossText.color = data.profit_loss >= 0 ? positiveColor : negativeColor;
        }

        // Update Average Profit/Loss
        if (averageProfitLossText != null)
        {
            averageProfitLossText.text = profitLossPrefix + data.average_profit_loss.ToString("F2");
            averageProfitLossText.color = data.average_profit_loss >= 0 ? positiveColor : negativeColor;
        }

        // Update Number of Trades
        if (numberOfTradesText != null)
        {
            numberOfTradesText.text = tradeCountPrefix + data.number_of_trades.ToString();
        }

        // Update Average Trade Duration
        if (averageTradeDurationText != null)
        {
            averageTradeDurationText.text = data.average_trade_duration;
        }

        // Update Average Trade Volume
        if (averageTradeVolumeText != null)
        {
            averageTradeVolumeText.text = data.average_trade_volume.ToString("F2");
        }

        // Update Best Trade
        if (bestTradeText != null)
        {
            bestTradeText.text = profitLossPrefix + data.best_trade.ToString("F2");
            bestTradeText.color = positiveColor;
        }

        // Animate the UI elements (comportamiento estándar)
        AnimateUIElements();
    }

    private void AnimateUIElements()
    {
        // If animations already triggered, don't repeat
        if (animationsTriggered)
            return;

        animationsTriggered = true;

        // You can add animations here using DOTween
        // For example, scale up the profit/loss text
        if (profitLossText != null)
        {
            profitLossText.transform.localScale = Vector3.zero;
            profitLossText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        // Fade in other elements
        TextMeshProUGUI[] allTexts = new TextMeshProUGUI[]
        {
            averageProfitLossText,
            numberOfTradesText,
            averageTradeDurationText,
            averageTradeVolumeText,
            bestTradeText
        };

        for (int i = 0; i < allTexts.Length; i++)
        {
            if (allTexts[i] != null)
            {
                allTexts[i].alpha = 0f;
                allTexts[i].DOFade(1f, 0.3f).SetDelay(i * 0.1f);
            }
        }
    }

    private void AnimateUIElementsWithDelay(TradingData data, float delayBetweenElements)
    {
        // If animations already triggered, don't repeat
        if (animationsTriggered)
            return;

        animationsTriggered = true;

        // Animar con el retraso especificado
        if (profitLossText != null)
        {
            profitLossText.transform.localScale = Vector3.zero;
            profitLossText.alpha = 1f;
            profitLossText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        // Fade in other elements with the specified delay
        TextMeshProUGUI[] allTexts = new TextMeshProUGUI[]
        {
            averageProfitLossText,
            numberOfTradesText,
            averageTradeDurationText,
            averageTradeVolumeText,
            bestTradeText
        };

        for (int i = 0; i < allTexts.Length; i++)
        {
            if (allTexts[i] != null)
            {
                allTexts[i].alpha = 0f;
                // Usar el retraso coordinado entre elementos
                allTexts[i].DOFade(1f, 0.3f).SetDelay(i * delayBetweenElements);
            }
        }
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
        }
    }
}