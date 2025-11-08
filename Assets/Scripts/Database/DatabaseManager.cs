using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private string dbPath;

    // El ID del perfil se puede gestionar desde el menú principal o la pantalla de carga en el futuro.
    // Por ahora, lo dejamos fijo en 1.
    public int currentProfileID = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // La ruta a la base de datos ahora se define aquí.
            dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");
            Debug.Log($"Database path: {dbPath}");

            // La inicialización solo se encarga de crear el archivo y las tablas si no existen.
            // Ya no mantiene una conexión abierta.
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        // El bloque 'using' abre la conexión y la cierra AUTOMÁTICAMENTE al terminar,
        // incluso si hay un error. Esto es crucial.
        using (var connection = new SqliteConnection($"URI=file:{dbPath}"))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Perfil_Jugador (
                        id INTEGER PRIMARY KEY,
                        nombre_perfil TEXT NOT NULL,
                        nivel_ingenieria INTEGER DEFAULT 1,
                        highest_level_unlocked INTEGER DEFAULT 0,
                        ultima_partida_guardada DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE TABLE IF NOT EXISTS Lore_Descubierto (
                        perfil_id INTEGER, lore_id TEXT,
                        PRIMARY KEY (perfil_id, lore_id),
                        FOREIGN KEY (perfil_id) REFERENCES Perfil_Jugador(id)
                    );
                    CREATE TABLE IF NOT EXISTS Objetivo_Completado (
                        perfil_id INTEGER, objetivo_id TEXT,
                        PRIMARY KEY (perfil_id, objetivo_id),
                        FOREIGN KEY (perfil_id) REFERENCES Perfil_Jugador(id)
                    );
                    CREATE TABLE IF NOT EXISTS Request_Aprendido (
                        perfil_id INTEGER, puzzle_name TEXT, full_request TEXT,
                        PRIMARY KEY (perfil_id, puzzle_name),
                        FOREIGN KEY (perfil_id) REFERENCES Perfil_Jugador(id)
                    );
                    -- Insertar perfil por defecto si no existe
                    INSERT OR IGNORE INTO Perfil_Jugador (id, nombre_perfil) VALUES (1, 'Default Profile');
                ";
                command.ExecuteNonQuery();
            }
        }
    }

    // AHORA CADA MÉTODO GESTIONA SU PROPIA CONEXIÓN. ESTO ES MÁS SEGURO Y EFICIENTE.

    public void SaveGameData(SaveData data)
    {
        using (var connection = new SqliteConnection($"URI=file:{dbPath}"))
        {
            connection.Open();
            // Usamos una 'transacción' para asegurarnos de que todos los datos se guarden juntos.
            // Si algo falla, nada se guarda, evitando datos corruptos.
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Guardar datos del perfil
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Perfil_Jugador SET 
                                            nivel_ingenieria = @nivel, 
                                            highest_level_unlocked = @highest, 
                                            ultima_partida_guardada = CURRENT_TIMESTAMP 
                                          WHERE id = @profileId;";
                        cmd.Parameters.Add(new SqliteParameter("@nivel", data.playerEngineeringLevel));
                        cmd.Parameters.Add(new SqliteParameter("@highest", data.highestLevelUnlocked));
                        cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Guardar Lore (Borrar los antiguos y añadir los nuevos)
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Lore_Descubierto WHERE perfil_id = @profileId;";
                        cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                        cmd.ExecuteNonQuery();

                        foreach (var loreId in data.discoveredLoreIDs)
                        {
                            cmd.CommandText = "INSERT INTO Lore_Descubierto (perfil_id, lore_id) VALUES (@profileId, @loreId);";
                            cmd.Parameters.Add(new SqliteParameter("@loreId", loreId));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3. Guardar Objetivos Completados
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Objetivo_Completado WHERE perfil_id = @profileId;";
                        cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                        cmd.ExecuteNonQuery();

                        foreach (var objectiveId in data.completedObjectiveIDs)
                        {
                            cmd.CommandText = "INSERT INTO Objetivo_Completado (perfil_id, objetivo_id) VALUES (@profileId, @objectiveId);";
                            cmd.Parameters.Add(new SqliteParameter("@objectiveId", objectiveId));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 4. Guardar Requests Aprendidos
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Request_Aprendido WHERE perfil_id = @profileId;";
                        cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                        cmd.ExecuteNonQuery();

                        foreach (var request in data.learnedRequests)
                        {
                            cmd.CommandText = "INSERT INTO Request_Aprendido (perfil_id, puzzle_name, full_request) VALUES (@profileId, @puzzleName, @fullRequest);";
                            cmd.Parameters.Add(new SqliteParameter("@puzzleName", request.puzzleName));
                            cmd.Parameters.Add(new SqliteParameter("@fullRequest", request.fullRequest));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Si todo fue bien, confirma los cambios.
                    transaction.Commit();
                    Debug.Log("Datos del juego guardados correctamente.");
                }
                catch (System.Exception ex)
                {
                    // Si algo falló, deshaz todos los cambios.
                    transaction.Rollback();
                    Debug.LogError($"Error al guardar los datos: {ex.Message}");
                }
            }
        }
    }

    public SaveData LoadGameData()
    {
        SaveData data = new SaveData();

        using (var connection = new SqliteConnection($"URI=file:{dbPath}"))
        {
            connection.Open();

            // Cargar datos del perfil
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT nivel_ingenieria, highest_level_unlocked FROM Perfil_Jugador WHERE id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        data.playerEngineeringLevel = reader.GetInt32(0);
                        data.highestLevelUnlocked = reader.GetInt32(1);
                    }
                }
            }

            // Cargar lore
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT lore_id FROM Lore_Descubierto WHERE perfil_id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) data.discoveredLoreIDs.Add(reader.GetString(0));
                }
            }

            // Cargar objetivos
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT objetivo_id FROM Objetivo_Completado WHERE perfil_id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) data.completedObjectiveIDs.Add(reader.GetString(0));
                }
            }

            // Cargar requests
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT puzzle_name, full_request FROM Request_Aprendido WHERE perfil_id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.learnedRequests.Add(new RequestEntry
                        {
                            puzzleName = reader.GetString(0),
                            fullRequest = reader.GetString(1)
                        });
                    }
                }
            }
        }

        Debug.Log("Datos del juego cargados.");
        return data;
    }
}