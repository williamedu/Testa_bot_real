using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;


public class MT5BalanceOnly : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI equityText;
    [SerializeField] private TextMeshProUGUI floatingPLText;

    [Header("Format Settings")]
    [SerializeField] private string balanceFormat = "Balance: ${0:N2}";
    [SerializeField] private string equityFormat = "Equity: ${0:N2}";
    [SerializeField] private string floatingPLFormat = "Floating P/L: ${0:N2}";
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;

    [Header("Animation Settings")]
    [SerializeField] private bool useAnimations = true;
    [Tooltip("Slight delay between animations for better visual effect")]
    [SerializeField] private float animationDelay = 0.1f;
    [Tooltip("Threshold for detecting value changes (to avoid floating point precision issues)")]
    [SerializeField] private double changeThreshold = 0.001;
    [Tooltip("Use the animation coordinator for synchronized animations")]
    [SerializeField] private bool useAnimationCoordinator = true;

    // Animadores de texto - se asignarán automáticamente o manualmente
    private TextAnimator balanceAnimator;
    private TextAnimator equityAnimator;
    private TextAnimator floatingPLAnimator;

    private string currentAccountId;

    // Variables para almacenar los valores anteriores y compararlos
    private double previousBalance;
    private double previousEquity;
    private double previousFloatingPL;
    private bool isFirstUpdate = true;

    // Variable para almacenar temporalmente los datos de la cuenta mientras se espera la sincronización
    private MT5AccountData cachedAccountData;

    private void Start()
    {
        // Configurar animadores si las animaciones están habilitadas
        if (useAnimations)
        {
            SetupAnimators();
        }

        // Obtener el ID guardado en PlayerPrefs
        currentAccountId = PlayerPrefs.GetString("CurrentAccountID", "0");
        Debug.Log("MT5BalanceOnly: Iniciado con cuenta ID: " + currentAccountId);

        // Suscribirse al evento del WebSocket
        if (MT5DataManager.Instance != null)
        {
            MT5DataManager.Instance.onDataReceived.AddListener(OnDataReceived);
            Debug.Log("MT5BalanceOnly: Suscrito a eventos del WebSocket");
        }
        else
        {
            Debug.LogError("MT5BalanceOnly: MT5DataManager.Instance es NULL");
        }

        // Suscribirse a eventos del coordinador de animaciones si está disponible
        if (useAnimationCoordinator && UIAnimationCoordinator.Instance != null)
        {
            UIAnimationCoordinator.Instance.onDataLoadingStarted.AddListener(OnCoordinatedLoadingStarted);
            UIAnimationCoordinator.Instance.onStartAnimations.AddListener(OnCoordinatedAnimationStart);
        }
    }

    private void SetupAnimators()
    {
        // Configurar animador para balance
        if (balanceText != null)
        {
            balanceAnimator = balanceText.GetComponent<TextAnimator>();
            if (balanceAnimator == null)
            {
                balanceAnimator = balanceText.gameObject.AddComponent<TextAnimator>();
            }
        }

        // Configurar animador para equity
        if (equityText != null)
        {
            equityAnimator = equityText.GetComponent<TextAnimator>();
            if (equityAnimator == null)
            {
                equityAnimator = equityText.gameObject.AddComponent<TextAnimator>();
            }
        }

        // Configurar animador para floating P/L
        if (floatingPLText != null)
        {
            floatingPLAnimator = floatingPLText.GetComponent<TextAnimator>();
            if (floatingPLAnimator == null)
            {
                floatingPLAnimator = floatingPLText.gameObject.AddComponent<TextAnimator>();
            }
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento
        if (MT5DataManager.Instance != null)
        {
            MT5DataManager.Instance.onDataReceived.RemoveListener(OnDataReceived);
        }

        // Desuscribirse de eventos del coordinador
        if (UIAnimationCoordinator.Instance != null)
        {
            UIAnimationCoordinator.Instance.onDataLoadingStarted.RemoveListener(OnCoordinatedLoadingStarted);
            UIAnimationCoordinator.Instance.onStartAnimations.RemoveListener(OnCoordinatedAnimationStart);
        }
    }

    // Este método se llama cada vez que llegan nuevos datos del WebSocket
    private void OnDataReceived(Dictionary<long, MT5AccountData> accountsData)
    {
        Debug.Log("MT5BalanceOnly: Datos recibidos. Cuentas disponibles: " + accountsData.Count);

        // Mostrar todas las claves para depuración
        foreach (var key in accountsData.Keys)
        {
            Debug.Log("MT5BalanceOnly: Cuenta disponible: " + key);
        }

        // Obtener solo los datos de la cuenta actual
        if (long.TryParse(currentAccountId, out long accountId))
        {
            Debug.Log("MT5BalanceOnly: Buscando cuenta: " + accountId);

            if (accountsData.TryGetValue(accountId, out MT5AccountData accountData))
            {
                Debug.Log("MT5BalanceOnly: Cuenta encontrada. Balance: " + accountData.balance + ", Equity: " + accountData.equity);

                // Si estamos usando el coordinador, almacenar datos temporalmente
                if (useAnimationCoordinator && UIAnimationCoordinator.Instance != null)
                {
                    // Almacenar datos para uso posterior
                    cachedAccountData = accountData;

                    // Notificar al coordinador que los datos están listos
                    UIAnimationCoordinator.Instance.BalanceDataReady();
                }
                else
                {
                    // Comportamiento normal, actualizar UI inmediatamente
                    UpdateUI(accountData);
                }
            }
            else
            {
                Debug.LogError("MT5BalanceOnly: Cuenta no encontrada en los datos recibidos");
            }
        }
        else
        {
            Debug.LogError("MT5BalanceOnly: No se pudo convertir el ID: " + currentAccountId);
        }
    }

    private void UpdateUI(MT5AccountData accountData)
    {
        // Si las animaciones están habilitadas, usar corrutina para secuenciar las animaciones
        if (useAnimations)
        {
            StartCoroutine(AnimateUIUpdates(accountData));
        }
        else
        {
            // Actualización estándar sin animaciones
            UpdateUIWithoutAnimation(accountData);
        }
    }

    private IEnumerator AnimateUIUpdates(MT5AccountData accountData)
    {
        // Calcular valores actuales
        double currentBalance = accountData.balance;
        double currentEquity = accountData.equity;
        double currentFloatingPL = accountData.equity - accountData.balance;

        // Verificar cambios y animar solo si es necesario
        bool animateBalance = isFirstUpdate || Math.Abs(currentBalance - previousBalance) > changeThreshold;
        bool animateEquity = isFirstUpdate || Math.Abs(currentEquity - previousEquity) > changeThreshold;
        bool animateFloatingPL = isFirstUpdate || Math.Abs(currentFloatingPL - previousFloatingPL) > changeThreshold;

        // Actualizar balance con animación si cambió
        if (balanceText != null && balanceAnimator != null)
        {
            string formattedBalance = string.Format(balanceFormat, currentBalance);
            balanceText.text = formattedBalance; // Siempre actualizar el texto

            if (animateBalance)
            {
                balanceAnimator.Animate(formattedBalance, neutralColor);
                yield return new WaitForSeconds(animationDelay);
            }
        }

        // Actualizar equity con animación si cambió
        if (equityText != null && equityAnimator != null)
        {
            string formattedEquity = string.Format(equityFormat, currentEquity);
            Color equityColor = neutralColor;

            if (currentEquity > currentBalance)
                equityColor = positiveColor;
            else if (currentEquity < currentBalance)
                equityColor = negativeColor;

            equityText.text = formattedEquity; // Siempre actualizar el texto
            equityText.color = equityColor; // Siempre actualizar el color

            if (animateEquity)
            {
                equityAnimator.Animate(formattedEquity, equityColor);
                yield return new WaitForSeconds(animationDelay);
            }
        }

        // Actualizar floating profit/loss con animación si cambió
        if (floatingPLText != null && floatingPLAnimator != null)
        {
            string formattedPL = string.Format(floatingPLFormat, currentFloatingPL);
            Color plColor = currentFloatingPL >= 0 ? positiveColor : negativeColor;

            floatingPLText.text = formattedPL; // Siempre actualizar el texto
            floatingPLText.color = plColor; // Siempre actualizar el color

            if (animateFloatingPL)
            {
                floatingPLAnimator.Animate(formattedPL, plColor);
            }
        }

        // Guardar los valores actuales para la próxima comparación
        previousBalance = currentBalance;
        previousEquity = currentEquity;
        previousFloatingPL = currentFloatingPL;

        // Ya no es la primera actualización
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
        }
    }

    private void UpdateUIWithoutAnimation(MT5AccountData accountData)
    {
        // Actualizar balance
        if (balanceText != null)
        {
            balanceText.text = string.Format(balanceFormat, accountData.balance);
            balanceText.color = neutralColor;
        }

        // Actualizar equity
        if (equityText != null)
        {
            equityText.text = string.Format(equityFormat, accountData.equity);

            // Colorear el equity basado en comparación con el balance
            if (accountData.equity > accountData.balance)
                equityText.color = positiveColor;
            else if (accountData.equity < accountData.balance)
                equityText.color = negativeColor;
            else
                equityText.color = neutralColor;
        }

        // Actualizar floating profit/loss
        if (floatingPLText != null)
        {
            double floatingPL = accountData.equity - accountData.balance;
            floatingPLText.text = string.Format(floatingPLFormat, floatingPL);
            floatingPLText.color = floatingPL >= 0 ? positiveColor : negativeColor;
        }
    }

    // Métodos para la coordinación de animaciones
    private void OnCoordinatedLoadingStarted()
    {
        // Resetear el estado de primera actualización para forzar animación
        isFirstUpdate = true;
    }

    private void OnCoordinatedAnimationStart(float delayOverride)
    {
        // Usar los datos almacenados para actualizar la UI
        if (cachedAccountData != null)
        {
            // Usar el retraso proporcionado por el coordinador si es distinto de cero
            float delayToUse = delayOverride > 0 ? delayOverride : animationDelay;

            // Guardar el valor actual y restaurarlo después
            float originalDelay = animationDelay;
            animationDelay = delayToUse;

            // Actualizar la UI con animaciones
            UpdateUI(cachedAccountData);

            // Restaurar el valor original
            animationDelay = originalDelay;
        }
    }
}