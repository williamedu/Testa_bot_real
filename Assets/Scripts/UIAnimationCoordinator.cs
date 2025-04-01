using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Esta clase servirá como coordinador centralizado para las animaciones de UI
public class UIAnimationCoordinator : MonoBehaviour
{
    // Singleton
    public static UIAnimationCoordinator Instance { get; private set; }

    // Eventos para coordinar la carga de datos
    public UnityEvent onDataLoadingStarted = new UnityEvent();
    public UnityEvent onDataLoadingCompleted = new UnityEvent();

    // Evento con parámetro para retrasar las animaciones
    [Serializable]
    public class AnimationEvent : UnityEvent<float> { }
    public AnimationEvent onStartAnimations = new AnimationEvent();

    [Header("Timing Settings")]
    [SerializeField] private float animationDelay = 0.2f;

    private bool balanceDataReady = false;
    private bool tradingDataReady = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
       
    }

    // Llamado cuando comienza la carga de datos
    public void StartLoading()
    {
        // Resetear flags de preparación
        balanceDataReady = false;
        tradingDataReady = false;

        // Notificar que la carga comenzó
        onDataLoadingStarted.Invoke();
    }

    // Llamado cuando LoadTradingData termina de cargar
    public void TradingDataReady()
    {
        tradingDataReady = true;
        CheckAllDataReady();
    }

    // Llamado cuando MT5BalanceOnly termina de cargar
    public void BalanceDataReady()
    {
        balanceDataReady = true;
        CheckAllDataReady();
    }

    // Comprobar si todos los datos están listos
    private void CheckAllDataReady()
    {
        if (balanceDataReady && tradingDataReady)
        {
            // Notificar que todos los datos están completos
            onDataLoadingCompleted.Invoke();

            // Iniciar secuencia de animaciones con un pequeño retraso
            StartCoroutine(TriggerAnimationsWithDelay());
        }
    }

    private IEnumerator TriggerAnimationsWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        onStartAnimations.Invoke(animationDelay);
    }

    // Método público para sincronizar manualmente si es necesario
    public void ForceAnimationSync()
    {
        StartCoroutine(TriggerAnimationsWithDelay());
    }
}
