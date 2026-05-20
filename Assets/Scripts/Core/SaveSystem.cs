using UnityEngine;
using System.IO;
using System;

namespace FantasyLifeVN.Core
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Serializable]
        public class SaveData
        {
            public int day;
            public string timePhase;
            public int currentEnergy;
            public int storyLevel;
            public int laraFriendship;
            public int luciaAffection;

            // Ren Stats
            public int renHP;
            public int renMaxHP;
            public int renMP;
            public int renMaxMP;
            public int renATK;
            public int renDEF;

            // Marco Stats
            public int marcoHP;
            public int marcoMaxHP;
            public int marcoMP;
            public int marcoMaxMP;
            public int marcoATK;
            public int marcoDEF;

            // Lucia Stats
            public int luciaHP;
            public int luciaMaxHP;
            public int luciaMP;
            public int luciaMaxMP;
            public int luciaATK;
            public int luciaDEF;

            public string currentRoom;
            public string saveTime;
        }

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

        private string GetSaveFilePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"fantasy_life_save_slot_{slot}.json");
        }

        public bool SaveGame(int slot)
        {
            if (GameManager.Instance == null) return false;

            try
            {
                SaveData data = new SaveData
                {
                    day = GameManager.Instance.Day,
                    timePhase = GameManager.Instance.TimePhase,
                    currentEnergy = GameManager.Instance.CurrentEnergy,
                    storyLevel = GameManager.Instance.StoryLevel,
                    laraFriendship = GameManager.Instance.LaraFriendship,
                    luciaAffection = GameManager.Instance.AffectionLucia,

                    renHP = GameManager.Instance.renStats.hp,
                    renMaxHP = GameManager.Instance.renStats.maxHP,
                    renMP = GameManager.Instance.renStats.mp,
                    renMaxMP = GameManager.Instance.renStats.maxMP,
                    renATK = GameManager.Instance.renStats.atk,
                    renDEF = GameManager.Instance.renStats.def,

                    marcoHP = GameManager.Instance.marcoStats.hp,
                    marcoMaxHP = GameManager.Instance.marcoStats.maxHP,
                    marcoMP = GameManager.Instance.marcoStats.mp,
                    marcoMaxMP = GameManager.Instance.marcoStats.maxMP,
                    marcoATK = GameManager.Instance.marcoStats.atk,
                    marcoDEF = GameManager.Instance.marcoStats.def,

                    luciaHP = GameManager.Instance.luciaStats.hp,
                    luciaMaxHP = GameManager.Instance.luciaStats.maxHP,
                    luciaMP = GameManager.Instance.luciaStats.mp,
                    luciaMaxMP = GameManager.Instance.luciaStats.maxMP,
                    luciaATK = GameManager.Instance.luciaStats.atk,
                    luciaDEF = GameManager.Instance.luciaStats.def,

                    currentRoom = GameManager.Instance.CurrentRoom,
                    saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(GetSaveFilePath(slot), json);
                
                Debug.Log($"Game successfully saved to Slot {slot} at: {GetSaveFilePath(slot)}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game to slot {slot}: {e.Message}");
                return false;
            }
        }

        public bool LoadGame(int slot)
        {
            if (GameManager.Instance == null) return false;

            string filePath = GetSaveFilePath(slot);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Save file not found for slot {slot}");
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                GameManager.Instance.SetState(
                    data.day,
                    data.timePhase,
                    data.currentEnergy,
                    data.storyLevel,
                    
                    data.renHP,
                    data.renMP,
                    data.renMaxHP,
                    data.renMaxMP,
                    data.renATK,
                    data.renDEF,

                    data.marcoHP,
                    data.marcoMP,
                    data.marcoMaxHP,
                    data.marcoMaxMP,
                    data.marcoATK,
                    data.marcoDEF,

                    data.luciaHP,
                    data.luciaMP,
                    data.luciaMaxHP,
                    data.luciaMaxMP,
                    data.luciaATK,
                    data.luciaDEF,

                    data.laraFriendship,
                    data.luciaAffection,
                    data.currentRoom
                );

                Debug.Log($"Game successfully loaded from Slot {slot}. Current Room: {data.currentRoom}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game from slot {slot}: {e.Message}");
                return false;
            }
        }

        public bool HasSaveFile(int slot)
        {
            return File.Exists(GetSaveFilePath(slot));
        }

        public SaveData GetSaveSummary(int slot)
        {
            string filePath = GetSaveFilePath(slot);
            if (!File.Exists(filePath)) return null;

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool DeleteSave(int slot)
        {
            string filePath = GetSaveFilePath(slot);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to delete save file for slot {slot}: {e.Message}");
                    return false;
                }
            }
            return false;
        }
    }
}
