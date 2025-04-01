using UnityEngine;
using TMPro;

public class VersionDisplay : MonoBehaviour
{
    [SerializeField] private VersionData versionData;
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private bool showFullVersionInfo = false;

    private void Start()
    {
        // Verificar que tenemos las referencias necesarias
        if (versionData == null)
        {
            Debug.LogError("Error: Version Data no asignado en " + gameObject.name);
            return;
        }

        if (versionText == null)
        {
            Debug.LogError("Error: TextMeshProUGUI no asignado en " + gameObject.name);
            return;
        }

        // Mostrar la versión en el texto
        DisplayVersion();
    }

    private void DisplayVersion()
    {
        if (showFullVersionInfo)
        {
            versionText.text = $"{versionData.FormattedVersion}\n{versionData.VersionNotes}";
        }
        else
        {
            versionText.text = versionData.FormattedVersion;
        }
    }

    // Método público para actualizar el texto si necesitas hacerlo manualmente
    public void UpdateVersionText()
    {
        DisplayVersion();
    }
}
