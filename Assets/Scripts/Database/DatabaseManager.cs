using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }
    private string dbPath;
    public int currentProfileID = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            dbPath = Path.Combine(Application.persistentDataPath, "game_database.db");
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
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
                    CREATE TABLE IF NOT EXISTS Inventario (
                        perfil_id INTEGER, item_id TEXT, cantidad INTEGER,
                        PRIMARY KEY (perfil_id, item_id),
                        FOREIGN KEY (perfil_id) REFERENCES Perfil_Jugador(id)
                    );
                    INSERT OR IGNORE INTO Perfil_Jugador (id, nombre_perfil) VALUES (1, 'Default Profile');
                ";
                command.ExecuteNonQuery();
            }
        }
    }

    public void SaveGameData(SaveData data)
    {
        using (var connection = new SqliteConnection($"URI=file:{dbPath}"))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
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

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Inventario WHERE perfil_id = @profileId;";
                        cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                        cmd.ExecuteNonQuery();
                        foreach (var itemData in data.inventoryItems)
                        {
                            cmd.CommandText = "INSERT INTO Inventario (perfil_id, item_id, cantidad) VALUES (@profileId, @itemId, @cantidad);";
                            cmd.Parameters.Add(new SqliteParameter("@itemId", itemData.itemID));
                            cmd.Parameters.Add(new SqliteParameter("@cantidad", itemData.quantity));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
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

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT lore_id FROM Lore_Descubierto WHERE perfil_id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) data.discoveredLoreIDs.Add(reader.GetString(0));
                }
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT objetivo_id FROM Objetivo_Completado WHERE perfil_id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) data.completedObjectiveIDs.Add(reader.GetString(0));
                }
            }

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

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT item_id, cantidad FROM Inventario WHERE perfil_id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.inventoryItems.Add(new InventoryItemData
                        {
                            itemID = reader.GetString(0),
                            quantity = reader.GetInt32(1)
                        });
                    }
                }
            }
        }

        return data;
    }
}