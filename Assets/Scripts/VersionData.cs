using UnityEngine;

// Agregamos un men� para crear este ScriptableObject desde el editor
[CreateAssetMenu(fileName = "VersionData", menuName = "Game/Version Data", order = 1)]
public class VersionData : ScriptableObject
{
    [SerializeField] private string versionNumber = "1.0.0";
    [SerializeField] [TextArea(3, 5)] private string versionNotes = "Initial version";

    // Propiedad para acceder a la versi�n
    public string VersionNumber => versionNumber;

    // Propiedad para acceder a las notas de versi�n
    public string VersionNotes => versionNotes;

    // Si necesitas una versi�n formateada espec�fica
    public string FormattedVersion => $"v{versionNumber}";
}
