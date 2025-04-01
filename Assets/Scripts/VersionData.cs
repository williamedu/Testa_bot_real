using UnityEngine;

// Agregamos un menú para crear este ScriptableObject desde el editor
[CreateAssetMenu(fileName = "VersionData", menuName = "Game/Version Data", order = 1)]
public class VersionData : ScriptableObject
{
    [SerializeField] private string versionNumber = "1.0.0";
    [SerializeField] [TextArea(3, 5)] private string versionNotes = "Initial version";

    // Propiedad para acceder a la versión
    public string VersionNumber => versionNumber;

    // Propiedad para acceder a las notas de versión
    public string VersionNotes => versionNotes;

    // Si necesitas una versión formateada específica
    public string FormattedVersion => $"v{versionNumber}";
}
