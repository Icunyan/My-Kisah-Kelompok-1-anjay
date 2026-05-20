using UnityEngine;
using UnityEngine.UI;
using FantasyLifeVN.Core;

namespace FantasyLifeVN.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Main HUD Components")]
        [SerializeField] private Text dayHUDText;
        [SerializeField] private Text timeHUDText;
        [SerializeField] private Text energyHUDText;
        [SerializeField] private Slider energyHUDSlider;
        [SerializeField] private Text storyHUDText;
        [SerializeField] private Button sleepHUDButton; // Tidur button in Bedroom
        [SerializeField] private Image timePhaseBadge;

        [Header("Status Panel Pop-up - Party Attributes")]
        [SerializeField] private GameObject statusPanel;
        
        [Header("Ren (Mage) UI Stats")]
        [SerializeField] private Text renStatsText; // Combined display e.g. "Ren (Mage) HP: 120/120 MP: 80/80 ATK: 22 DEF: 6"
        [SerializeField] private Text renHPText;
        [SerializeField] private Text renMPText;

        [Header("Marco (Knight) UI Stats")]
        [SerializeField] private Text marcoStatsText;
        [SerializeField] private Text marcoHPText;

        [Header("Lucia (Priestess) UI Stats")]
        [SerializeField] private Text luciaStatsText;
        [SerializeField] private Text luciaHPText;

        [Header("Heroine Affection Elements")]
        [SerializeField] private Text statusLaraFriendshipText;
        [SerializeField] private Slider statusLaraFriendshipSlider;
        [SerializeField] private Text statusLuciaAffectionText;
        [SerializeField] private Slider statusLuciaAffectionSlider;

        [Header("Save/Load Panel Pop-up")]
        [SerializeField] private GameObject saveLoadPanel;
        [SerializeField] private Text[] slotDetailTexts = new Text[3];
        [SerializeField] private Button[] slotLoadButtons = new Button[3];

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (statusPanel != null) statusPanel.SetActive(false);
            if (saveLoadPanel != null) saveLoadPanel.SetActive(false);

            GameManager.OnGameStateChanged += UpdateHUD;
            UpdateHUD();
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= UpdateHUD;
        }

        #region HUD Rendering
        public void UpdateHUD()
        {
            if (GameManager.Instance == null) return;

            if (dayHUDText != null) dayHUDText.text = $"Hari {GameManager.Instance.Day} / {GameManager.Instance.MaxDays}";
            if (timeHUDText != null) timeHUDText.text = GameManager.Instance.TimePhase;
            if (storyHUDText != null) 
            {
                storyHUDText.text = $"Progress: Level {GameManager.Instance.StoryLevel} / 30";
            }
            if (energyHUDText != null) energyHUDText.text = $"{GameManager.Instance.CurrentEnergy} / {GameManager.Instance.MaxEnergy}";

            if (energyHUDSlider != null)
            {
                energyHUDSlider.maxValue = GameManager.Instance.MaxEnergy;
                energyHUDSlider.value = GameManager.Instance.CurrentEnergy;
            }

            if (timePhaseBadge != null)
            {
                switch (GameManager.Instance.TimePhase)
                {
                    case "Pagi":
                        timePhaseBadge.color = new Color(1f, 0.76f, 0.2f); // Golden Morning
                        break;
                    case "Siang":
                        timePhaseBadge.color = new Color(0.2f, 0.6f, 1f);  // Sunny Afternoon
                        break;
                    case "Malam":
                        timePhaseBadge.color = new Color(0.38f, 0.2f, 0.8f); // Cosmic Evening
                        break;
                }
            }

            if (sleepHUDButton != null)
            {
                bool isNight = GameManager.Instance.TimePhase == "Malam";
                bool inBedroom = GameManager.Instance.CurrentRoom.Equals("Bedroom", System.StringComparison.OrdinalIgnoreCase);
                
                sleepHUDButton.gameObject.SetActive(inBedroom);
                sleepHUDButton.interactable = isNight;
            }
        }
        #endregion

        #region Status Window
        public void ToggleStatusPanel(bool active)
        {
            if (statusPanel != null)
            {
                statusPanel.SetActive(active);
                if (active) UpdateStatusDetails();
            }
        }

        private void UpdateStatusDetails()
        {
            if (GameManager.Instance == null) return;

            // 1. Render Ren stats
            if (renStatsText != null)
            {
                var r = GameManager.Instance.renStats;
                renStatsText.text = $"{r.charName} ({r.charClass})\nHP: {r.hp}/{r.maxHP}  MP: {r.mp}/{r.maxMP}\nATK: {r.atk}  DEF: {r.def}";
            }
            if (renHPText != null) renHPText.text = $"HP: {GameManager.Instance.renStats.hp} / {GameManager.Instance.renStats.maxHP}";
            if (renMPText != null) renMPText.text = $"MP: {GameManager.Instance.renStats.mp} / {GameManager.Instance.renStats.maxMP}";

            // 2. Render Marco stats
            if (marcoStatsText != null)
            {
                var m = GameManager.Instance.marcoStats;
                marcoStatsText.text = $"{m.charName} ({m.charClass})\nHP: {m.hp}/{m.maxHP}  MP: {m.mp}/{m.maxMP}\nATK: {m.atk}  DEF: {m.def}";
            }
            if (marcoHPText != null) marcoHPText.text = $"HP: {GameManager.Instance.marcoStats.hp} / {GameManager.Instance.marcoStats.maxHP}";

            // 3. Render Lucia stats
            if (luciaStatsText != null)
            {
                var l = GameManager.Instance.luciaStats;
                luciaStatsText.text = $"{l.charName} ({l.charClass})\nHP: {l.hp}/{l.maxHP}  MP: {l.mp}/{l.maxMP}\nATK: {l.atk}  DEF: {l.def}";
            }
            if (luciaHPText != null) luciaHPText.text = $"HP: {GameManager.Instance.luciaStats.hp} / {GameManager.Instance.luciaStats.maxHP}";

            // 4. Render Lara Friendship
            if (statusLaraFriendshipText != null) 
            {
                string suffix = GameManager.Instance.StoryLevel < 30 ? " [Sakit]" : "";
                statusLaraFriendshipText.text = $"Lara Friendship: {GameManager.Instance.LaraFriendship} / 100{suffix}";
            }
            if (statusLaraFriendshipSlider != null)
            {
                statusLaraFriendshipSlider.maxValue = 100;
                statusLaraFriendshipSlider.value = GameManager.Instance.LaraFriendship;
            }

            // 5. Render Lucia Affection
            if (statusLuciaAffectionText != null) 
            {
                statusLuciaAffectionText.text = $"Lucia Affection: {GameManager.Instance.AffectionLucia} / 100";
            }
            if (statusLuciaAffectionSlider != null)
            {
                statusLuciaAffectionSlider.maxValue = 100;
                statusLuciaAffectionSlider.value = GameManager.Instance.AffectionLucia;
            }
        }
        #endregion

        #region Save / Load System
        public void ToggleSaveLoadPanel(bool active)
        {
            if (saveLoadPanel != null)
            {
                saveLoadPanel.SetActive(active);
                if (active) RenderSaveSlots();
            }
        }

        private void RenderSaveSlots()
        {
            if (SaveSystem.Instance == null) return;

            for (int i = 0; i < 3; i++)
            {
                int slotNumber = i + 1;
                if (SaveSystem.Instance.HasSaveFile(slotNumber))
                {
                    SaveSystem.SaveData data = SaveSystem.Instance.GetSaveSummary(slotNumber);
                    if (data != null)
                    {
                        slotDetailTexts[i].text = $"Slot {slotNumber}\nHari {data.day}/30 - {data.timePhase} (Story Lvl {data.storyLevel})\nLara Friendship: {data.laraFriendship}\nDisimpan: {data.saveTime}";
                        slotLoadButtons[i].interactable = true;
                    }
                    else
                    {
                        slotDetailTexts[i].text = $"Slot {slotNumber}\n[Data Rusak]";
                        slotLoadButtons[i].interactable = false;
                    }
                }
                else
                {
                    slotDetailTexts[i].text = $"Slot {slotNumber}\n[Slot Kosong]";
                    slotLoadButtons[i].interactable = false;
                }
            }
        }

        public void SaveGameSlot(int slotNumber)
        {
            if (SaveSystem.Instance == null) return;
            
            bool success = SaveSystem.Instance.SaveGame(slotNumber);
            if (success)
            {
                Debug.Log($"Game successfully saved on Slot {slotNumber}");
                RenderSaveSlots();
            }
        }

        public void LoadGameSlot(int slotNumber)
        {
            if (SaveSystem.Instance == null) return;

            bool success = SaveSystem.Instance.LoadGame(slotNumber);
            if (success)
            {
                Debug.Log($"Game successfully loaded from Slot {slotNumber}");
                ToggleSaveLoadPanel(false);
            }
        }
        #endregion

        #region Actions
        public void OnSleepButtonClicked()
        {
            if (GameManager.Instance == null) return;

            bool slept = GameManager.Instance.SleepAndResetDay();
            if (slept)
            {
                Debug.Log("Slept successfully! Energy restored, advanced day.");
                UpdateHUD();
            }
        }
        #endregion
    }
}
