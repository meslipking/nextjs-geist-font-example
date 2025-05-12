using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    [System.Serializable]
    public class GameSaveData
    {
        public string saveDate;
        public int turnNumber;
        public bool isPlayer1Turn;
        public int player1Score;
        public int player2Score;
        public List<AnimalSaveData> player1Animals;
        public List<AnimalSaveData> player2Animals;
    }

    [System.Serializable]
    public class AnimalSaveData
    {
        public AnimalType type;
        public Vector2IntData position;
        public float health;
        public List<SkillSaveData> skills;
    }

    [System.Serializable]
    public class Vector2IntData
    {
        public int x;
        public int y;

        public Vector2IntData(Vector2Int vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }

    [System.Serializable]
    public class SkillSaveData
    {
        public string skillName;
        public float currentCooldown;
    }

    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "game_save.json";
    [SerializeField] private bool useEncryption = true;
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        try
        {
            GameSaveData saveData = CreateSaveData();
            string json = JsonUtility.ToJson(saveData, true);

            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved successfully to {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving game: {e.Message}");
        }
    }

    public bool LoadGame()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("No save file found.");
                return false;
            }

            string json = File.ReadAllText(SavePath);
            
            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            LoadSaveData(saveData);
            Debug.Log("Game loaded successfully");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading game: {e.Message}");
            return false;
        }
    }

    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData
        {
            saveDate = DateTime.Now.ToString(),
            turnNumber = GameRules.Instance.GetCurrentTurn(),
            isPlayer1Turn = GameManager.Instance.IsPlayer1Turn,
            player1Score = GameManager.Instance.Player1Score,
            player2Score = GameManager.Instance.Player2Score,
            player1Animals = new List<AnimalSaveData>(),
            player2Animals = new List<AnimalSaveData>()
        };

        // Save Player 1 animals
        foreach (Animal animal in GameManager.Instance.GetPlayer1Animals())
        {
            saveData.player1Animals.Add(CreateAnimalSaveData(animal));
        }

        // Save Player 2 animals
        foreach (Animal animal in GameManager.Instance.GetPlayer2Animals())
        {
            saveData.player2Animals.Add(CreateAnimalSaveData(animal));
        }

        return saveData;
    }

    private AnimalSaveData CreateAnimalSaveData(Animal animal)
    {
        Vector2Int position = GameBoard.Instance.GetAnimalPosition(animal);
        
        AnimalSaveData animalData = new AnimalSaveData
        {
            type = animal.type,
            position = new Vector2IntData(position),
            health = animal.health,
            skills = new List<SkillSaveData>()
        };

        // Save skill cooldowns
        foreach (SkillData skill in animal.skills)
        {
            animalData.skills.Add(new SkillSaveData
            {
                skillName = skill.skillName,
                currentCooldown = skill.GetCooldownProgress()
            });
        }

        return animalData;
    }

    private void LoadSaveData(GameSaveData saveData)
    {
        // Clear current game state
        GameManager.Instance.ClearBoard();

        // Restore game state
        GameManager.Instance.SetTurn(saveData.isPlayer1Turn);
        GameManager.Instance.SetScores(saveData.player1Score, saveData.player2Score);

        // Spawn and setup Player 1 animals
        foreach (AnimalSaveData animalData in saveData.player1Animals)
        {
            SpawnAndSetupAnimal(animalData, true);
        }

        // Spawn and setup Player 2 animals
        foreach (AnimalSaveData animalData in saveData.player2Animals)
        {
            SpawnAndSetupAnimal(animalData, false);
        }
    }

    private void SpawnAndSetupAnimal(AnimalSaveData animalData, bool isPlayer1)
    {
        // Spawn animal
        Animal animal = GameManager.Instance.SpawnAnimal(
            animalData.type,
            animalData.position.ToVector2Int(),
            isPlayer1
        );

        if (animal != null)
        {
            // Set health
            animal.health = animalData.health;

            // Restore skill cooldowns
            for (int i = 0; i < animal.skills.Length && i < animalData.skills.Count; i++)
            {
                SkillData skill = animal.skills[i];
                float cooldown = animalData.skills[i].currentCooldown;
                skill.SetCooldown(cooldown);
            }
        }
    }

    private string EncryptDecrypt(string data)
    {
        // Simple XOR encryption (you might want to use more secure encryption in production)
        string key = "CoThuVietNam2024";
        char[] result = new char[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (char)(data[i] ^ key[i % key.Length]);
        }

        return new string(result);
    }

    public bool HasSaveGame()
    {
        return File.Exists(SavePath);
    }

    public void DeleteSaveGame()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted successfully");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting save file: {e.Message}");
        }
    }

    public DateTime? GetLastSaveTime()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                return File.GetLastWriteTime(SavePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting save time: {e.Message}");
        }
        return null;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame(); // Auto-save when application is paused
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame(); // Auto-save when application is quit
    }
}
