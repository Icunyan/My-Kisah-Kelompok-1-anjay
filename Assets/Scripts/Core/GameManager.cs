using UnityEngine;
using System;

namespace FantasyLifeVN.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Survival Settings (30 Days Limit)")]
        [SerializeField] private int day = 1;
        private const int MAX_DAYS = 30;
        [SerializeField] private string timePhase = "Pagi"; // Pagi, Siang, Malam
        [SerializeField] private int currentEnergy = 100;
        [SerializeField] private int maxEnergy = 100;

        [Header("Story Campaign Progres (1-30)")]
        [SerializeField] private int storyLevel = 1;
        private const int MAX_STORY_LEVEL = 30;

        [Header("Lara Friendship (0 - 100)")]
        [SerializeField] private int laraFriendship = 15; // Starts at 15 base friendship

        [Header("Lucia Affection (0 - 100)")]
        [SerializeField] private int luciaAffection = 10; // Starts at 10 base affection

        [Header("Room Settings")]
        [SerializeField] private string currentRoom = "Yard";

        [System.Serializable]
        public class CharacterStats
        {
            public string charName;
            public string charClass;
            public int hp;
            public int maxHP;
            public int mp;
            public int maxMP;
            public int atk;
            public int def;

            public CharacterStats(string name, string className, int maxHp, int maxMp, int attack, int defense)
            {
                charName = name;
                charClass = className;
                maxHP = maxHp;
                hp = maxHp;
                maxMP = maxMp;
                mp = maxMp;
                atk = attack;
                def = defense;
            }

            public void LevelUp(int hpGain, int mpGain, int atkGain, int defGain)
            {
                maxHP += hpGain;
                hp = maxHP;
                maxMP += mpGain;
                mp = maxMP;
                atk += atkGain;
                def += defGain;
            }

            public void TrainBoost(int hpGain, int mpGain, int atkGain, int defGain)
            {
                maxHP += hpGain;
                hp = Mathf.Min(hp + hpGain, maxHP);
                maxMP += mpGain;
                mp = Mathf.Min(mp + mpGain, maxMP);
                atk += atkGain;
                def += defGain;
            }
        }

        [Header("Party Stats SO-like Structures")]
        public CharacterStats renStats = new CharacterStats("Ren", "Mage", 120, 80, 22, 6);
        public CharacterStats marcoStats = new CharacterStats("Marco", "Knight", 200, 30, 14, 18);
        public CharacterStats luciaStats = new CharacterStats("Lucia", "Mage", 100, 100, 10, 8);

        // Action events for UI updates
        public static event Action OnGameStateChanged;
        public static event Action<int> OnStoryEventTriggered; 
        public static event Action<string> OnDialogueTriggered;
        public static event Action<string> OnEndingTriggered; // Triggers True, Good, or Bad Ending

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeParty();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            NotifyStateChanged();
        }

        private void InitializeParty()
        {
            renStats.hp = renStats.maxHP;
            renStats.mp = renStats.maxMP;
            
            marcoStats.hp = marcoStats.maxHP;
            marcoStats.mp = marcoStats.maxMP;
            
            luciaStats.hp = luciaStats.maxHP;
            luciaStats.mp = luciaStats.maxMP;
        }

        #region Getters & Setters
        public int Day => day;
        public int MaxDays => MAX_DAYS;
        public string TimePhase => timePhase;
        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => maxEnergy;
        public int StoryLevel => storyLevel;
        public int LaraFriendship { get => laraFriendship; set => laraFriendship = Mathf.Clamp(value, 0, 100); }
        public int AffectionLucia { get => luciaAffection; set => luciaAffection = Mathf.Clamp(value, 0, 100); }
        public string CurrentRoom
        {
            get => currentRoom;
            set
            {
                if (currentRoom != value)
                {
                    currentRoom = value;
                    NotifyStateChanged();
                }
            }
        }

        public bool IsBossStage()
        {
            return storyLevel == MAX_STORY_LEVEL;
        }
        #endregion

        #region Cycles & Time Systems
        /// <summary>
        /// Consumes energy. Returns true if successful, false otherwise.
        /// </summary>
        public bool ConsumeEnergy(int amount)
        {
            if (currentEnergy >= amount)
            {
                currentEnergy -= amount;
                NotifyStateChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Light Rest action: increases small energy (+20) and advances time phase from Pagi -> Siang.
        /// </summary>
        public void LightRest()
        {
            currentEnergy = Mathf.Min(currentEnergy + 20, maxEnergy);
            
            if (timePhase == "Pagi")
            {
                timePhase = "Siang";
            }
            
            NotifyStateChanged();
        }

        /// <summary>
        /// Training action: boosts party attributes at the expense of 25 energy.
        /// </summary>
        public void TrainTeam()
        {
            if (currentEnergy >= 25)
            {
                currentEnergy -= 25;

                // Random stat increments
                renStats.TrainBoost(UnityEngine.Random.Range(5, 10), UnityEngine.Random.Range(3, 7), UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(1, 2));
                marcoStats.TrainBoost(UnityEngine.Random.Range(10, 15), UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(2, 4));
                luciaStats.TrainBoost(UnityEngine.Random.Range(4, 8), UnityEngine.Random.Range(5, 10), UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(1, 2));

                // Training advances phase Pagi -> Siang
                if (timePhase == "Pagi")
                {
                    timePhase = "Siang";
                }

                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Visit Lara action: transitions to Kamar Lara.
        /// </summary>
        public void VisitLara()
        {
            CurrentRoom = "Kamar Lara";
        }

        /// <summary>
        /// Sleep Rest action: resets energy, advances day, sets time to Morning. Checks 30 day limit.
        /// </summary>
        public bool SleepAndResetDay()
        {
            if (timePhase == "Malam")
            {
                day++;
                
                // Check 30 Days Limit
                if (day > MAX_DAYS)
                {
                    TriggerEnding("BadEnding_TimeLimit");
                    return false;
                }

                timePhase = "Pagi";
                currentEnergy = maxEnergy;

                // Overnight recovery
                renStats.hp = Mathf.Min(renStats.hp + (int)(renStats.maxHP * 0.4f), renStats.maxHP);
                renStats.mp = Mathf.Min(renStats.mp + (int)(renStats.maxMP * 0.4f), renStats.maxMP);

                marcoStats.hp = Mathf.Min(marcoStats.hp + (int)(marcoStats.maxHP * 0.4f), marcoStats.maxHP);
                marcoStats.mp = Mathf.Min(marcoStats.mp + (int)(marcoStats.maxMP * 0.4f), marcoStats.maxMP);

                luciaStats.hp = Mathf.Min(luciaStats.hp + (int)(luciaStats.maxHP * 0.4f), luciaStats.maxHP);
                luciaStats.mp = Mathf.Min(luciaStats.mp + (int)(luciaStats.maxMP * 0.4f), luciaStats.maxMP);

                NotifyStateChanged();
                return true;
            }
            return false;
        }
        #endregion

        #region Progression & Endings
        public void AdvanceStoryLevel()
        {
            if (storyLevel < MAX_STORY_LEVEL)
            {
                storyLevel++;
                
                // Story Campaign triggers based on level changes
                if (storyLevel == 2)
                {
                    TriggerStoryEvent(3);
                }
                else if (storyLevel == 3)
                {
                    TriggerStoryEvent(4);
                }
                else if (storyLevel == 4)
                {
                    TriggerStoryEvent(5);
                }

                LevelUpParty();

                // Successful combat in Siang automatically advances phase to Malam
                timePhase = "Malam";

                NotifyStateChanged();
            }
        }

        private void LevelUpParty()
        {
            renStats.LevelUp(15, 10, 4, 1);
            marcoStats.LevelUp(25, 4, 2, 4);
            luciaStats.LevelUp(12, 15, 1, 2);
        }

        public void EvaluateFinalEnding()
        {
            if (day > MAX_DAYS)
            {
                TriggerEnding("BadEnding_TimeLimit");
            }
            else if (laraFriendship >= 80)
            {
                TriggerEnding("TrueEnding");
            }
            else if (laraFriendship >= 40)
            {
                TriggerEnding("GoodEnding");
            }
            else
            {
                TriggerEnding("BadEnding_LowAffection");
            }
        }

        public void TriggerEnding(string endingId)
        {
            OnEndingTriggered?.Invoke(endingId);
        }
        #endregion

        #region Helpers
        public void NotifyStateChanged()
        {
            OnGameStateChanged?.Invoke();
        }

        public void TriggerDialogue(string npcId)
        {
            OnDialogueTriggered?.Invoke(npcId);
        }

        public void TriggerStoryEvent(int sectionNumber)
        {
            OnStoryEventTriggered?.Invoke(sectionNumber);
        }

        public void SetState(int loadedDay, string loadedPhase, int loadedEnergy, int loadedStory, 
                             int renHP, int renMP, int renMaxHP, int renMaxMP, int renAtk, int renDef,
                             int marcoHP, int marcoMP, int marcoMaxHP, int marcoMaxMP, int marcoAtk, int marcoDef,
                             int luciaHP, int luciaMP, int luciaMaxHP, int luciaMaxMP, int luciaAtk, int luciaDef,
                             int friendshipLara, int affectionLucia, string room)
        {
            day = loadedDay;
            timePhase = loadedPhase;
            currentEnergy = loadedEnergy;
            storyLevel = loadedStory;
            
            renStats.hp = renHP;
            renStats.mp = renMP;
            renStats.maxHP = renMaxHP;
            renStats.maxMP = renMaxMP;
            renStats.atk = renAtk;
            renStats.def = renDef;

            marcoStats.hp = marcoHP;
            marcoStats.mp = marcoMP;
            marcoStats.maxHP = marcoMaxHP;
            marcoStats.maxMP = marcoMaxMP;
            marcoStats.atk = marcoAtk;
            marcoStats.def = marcoDef;

            luciaStats.hp = luciaHP;
            luciaStats.mp = luciaMP;
            luciaStats.maxHP = luciaMaxHP;
            luciaStats.maxMP = luciaMaxMP;
            luciaStats.atk = luciaAtk;
            luciaStats.def = luciaDef;

            laraFriendship = friendshipLara;
            this.luciaAffection = affectionLucia;
            currentRoom = room;

            NotifyStateChanged();
        }
        #endregion
    }
}
