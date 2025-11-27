#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class DebugTools : MonoBehaviour
{
    [MenuItem("Herramientas/Borrar Base de Datos (Forzar)")]
    public static void DeleteDatabase()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");

        if (File.Exists(dbPath))
        {
            // Forzar el cierre de conexiones SQLite (Garbage Collection)
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            try
            {
                File.Delete(dbPath);
                Debug.Log($"<color=green>✅ ÉXITO: Base de datos borrada en: {dbPath}</color>");
                Debug.Log("Ahora dale a Play para generar una nueva limpia.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ ERROR: No se pudo borrar el archivo. Cierra Unity y bórralo manualmente.\n{e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró base de datos en: {dbPath}. \nProbablemente ya está borrada.");
        }
    }
}
#endif