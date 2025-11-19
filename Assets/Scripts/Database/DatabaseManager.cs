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
                        ultima_partida_guardada DATETIME DEFAULT CURRENT_TIMESTAMP,
                        max_health INTEGER DEFAULT 200,      
                        current_health INTEGER DEFAULT 200,  
                        power INTEGER DEFAULT 10,            
                        speed INTEGER DEFAULT 5,             
                        gold INTEGER DEFAULT 0,
                        level INTEGER DEFAULT 1,
                        current_exp INTEGER DEFAULT 0,
                        exp_to_level INTEGER DEFAULT 100
                    );
                    CREATE TABLE IF NOT EXISTS Lore_Descubierto (
                        perfil_id INTEGER, lore_id INTEGER,
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
                        perfil_id INTEGER, item_id INTEGER, cantidad INTEGER,
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
                                            nivel_ingenieria = @nivelIng, 
                                            highest_level_unlocked = @highest, 
                                            ultima_partida_guardada = CURRENT_TIMESTAMP,
                                            max_health = @maxHealth,         
                                            current_health = @currentHealth,   
                                            power = @power,                  
                                            speed = @speed,                  
                                            gold = @gold,
                                            level = @level,
                                            current_exp = @currentExp,
                                            exp_to_level = @expToLevel
                                        WHERE id = @profileId;";
                        cmd.Parameters.Add(new SqliteParameter("@nivelIng", data.playerEngineeringLevel));
                        cmd.Parameters.Add(new SqliteParameter("@highest", data.highestLevelUnlocked));
                        cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                        cmd.Parameters.Add(new SqliteParameter("@maxHealth", data.maxHealth));
                        cmd.Parameters.Add(new SqliteParameter("@currentHealth", data.currentHealth));
                        cmd.Parameters.Add(new SqliteParameter("@power", data.power));
                        cmd.Parameters.Add(new SqliteParameter("@speed", data.speed));
                        cmd.Parameters.Add(new SqliteParameter("@gold", data.gold));
                        cmd.Parameters.Add(new SqliteParameter("@level", data.level));
                        cmd.Parameters.Add(new SqliteParameter("@currentExp", data.currentExp));
                        cmd.Parameters.Add(new SqliteParameter("@expToLevel", data.expToLevel));
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Lore_Descubierto WHERE perfil_id = @profileId;";
                        cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                        cmd.ExecuteNonQuery();
                        foreach (var loreId in data.discoveredLoreIDs)
                        {
                            var pLore = cmd.CreateParameter();
                            pLore.ParameterName = "@loreId";
                            pLore.Value = loreId;
                            cmd.Parameters.Add(pLore);
                            cmd.CommandText = "INSERT INTO Lore_Descubierto (perfil_id, lore_id) VALUES (@profileId, @loreId);";
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
                            var pObj = cmd.CreateParameter();
                            pObj.ParameterName = "@objectiveId";
                            pObj.Value = objectiveId;
                            cmd.Parameters.Add(pObj);
                            cmd.CommandText = "INSERT INTO Objetivo_Completado (perfil_id, objetivo_id) VALUES (@profileId, @objectiveId);";
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
                            var pName = cmd.CreateParameter();
                            pName.ParameterName = "@puzzleName";
                            pName.Value = request.puzzleName;
                            var pReq = cmd.CreateParameter();
                            pReq.ParameterName = "@fullRequest";
                            pReq.Value = request.fullRequest;
                            cmd.Parameters.Add(pName);
                            cmd.Parameters.Add(pReq);
                            cmd.CommandText = "INSERT INTO Request_Aprendido (perfil_id, puzzle_name, full_request) VALUES (@profileId, @puzzleName, @fullRequest);";
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
                            var pId = cmd.CreateParameter();
                            pId.ParameterName = "@itemId";
                            pId.Value = itemData.itemID;
                            var pQty = cmd.CreateParameter();
                            pQty.ParameterName = "@cantidad";
                            pQty.Value = itemData.quantity;
                            cmd.Parameters.Add(pId);
                            cmd.Parameters.Add(pQty);
                            cmd.CommandText = "INSERT INTO Inventario (perfil_id, item_id, cantidad) VALUES (@profileId, @itemId, @cantidad);";
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
                cmd.CommandText = "SELECT nivel_ingenieria, highest_level_unlocked, max_health, current_health, power, speed, gold, level, current_exp, exp_to_level FROM Perfil_Jugador WHERE id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        data.playerEngineeringLevel = reader.GetInt32(0);
                        data.highestLevelUnlocked = reader.GetInt32(1);
                        data.maxHealth = reader.GetInt32(2);
                        data.currentHealth = reader.GetInt32(3);
                        data.power = reader.GetInt32(4);
                        data.speed = reader.GetInt32(5);
                        data.gold = reader.GetInt32(6);
                        data.level = reader.GetInt32(7);
                        data.currentExp = reader.GetInt32(8);
                        data.expToLevel = reader.GetInt32(9);
                    }
                }
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT lore_id FROM Lore_Descubierto WHERE perfil_id = @profileId";
                cmd.Parameters.Add(new SqliteParameter("@profileId", currentProfileID));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) data.discoveredLoreIDs.Add(reader.GetInt32(0));
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
                            itemID = reader.GetInt32(0),
                            quantity = reader.GetInt32(1)
                        });
                    }
                }
            }
        }
        return data;
    }
}