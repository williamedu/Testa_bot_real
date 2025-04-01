using UnityEngine;

public class ID_GO : MonoBehaviour
{

    public static ID_GO Instance { get; private set; }
    private void Awake()
    {
        // Implementaci�n del patr�n Singleton
        if (Instance == null)
        {
            Instance = this;
        }

    }
}
