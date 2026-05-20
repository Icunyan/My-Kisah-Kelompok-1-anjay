using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FantasyLifeVN.Core;

namespace FantasyLifeVN.Combat
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        public enum CombatState { Start, RenTurn, MarcoTurn, LuciaTurn, EnemyTurn, Victory, Defeat }

        [Header("Combat UI Panel")]
        [SerializeField] private GameObject combatPanel;
        [SerializeField] private Text enemyNameText;
        [SerializeField] private Text enemyHPText;
        [SerializeField] private Slider enemyHPSlider;
        [SerializeField] private Text combatLogText;

        [Header("Party UI Bars")]
        [SerializeField] private Text renHPText;
        [SerializeField] private Text renMPText;
        [SerializeField] private Slider renHPSlider;
        [SerializeField] private Slider renMPSlider;

        [SerializeField] private Text marcoHPText;
        [SerializeField] private Text marcoMPText;
        [SerializeField] private Slider marcoHPSlider;
        [SerializeField] private Slider marcoMPSlider;

        [SerializeField] private Text luciaHPText;
        [SerializeField] private Text luciaMPText;
        [SerializeField] private Slider luciaHPSlider;
        [SerializeField] private Slider luciaMPSlider;

        [Header("Action Buttons HUD")]
        [SerializeField] private Text currentTurnIndicatorText; // e.g. "Giliran: Ren (Mage)"
        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;
        [SerializeField] private Button runButton;
        
        [Header("Skill Button Sub-labels")]
        [SerializeField] private Button skill1Button; // Slash (Ren) / Shield Slam (Marco) / Heal (Lucia)
        [SerializeField] private Button skill2Button; // Ultimate Burst (Ren) / Provoke (Marco) / Shield Party (Lucia)
        [SerializeField] private Text skill1Label;
        [SerializeField] private Text skill2Label;

        [Header("Ending Panel HUD overlays")]
        [SerializeField] private GameObject trueEndingPanel;
        [SerializeField] private GameObject goodEndingPanel;
        [SerializeField] private GameObject badEndingPanel;

        private CombatState currentState;

        // Enemy attributes
        private string enemyName;
        private int enemyMaxHP;
        private int enemyHP;
        private int enemyATK;
        private int enemyDEF;

        // Turn guards & taunts
        private bool isRenGuarding = false;
        private bool isMarcoGuarding = false;
        private bool isLuciaGuarding = false;
        private bool isMarcoTaunting = false;

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
            if (combatPanel != null) combatPanel.SetActive(false);
            if (trueEndingPanel != null) trueEndingPanel.SetActive(false);
            if (goodEndingPanel != null) goodEndingPanel.SetActive(false);
            if (badEndingPanel != null) badEndingPanel.SetActive(false);

            GameManager.OnEndingTriggered += ShowEndingScreen;
        }

        private void OnDestroy()
        {
            GameManager.OnEndingTriggered -= ShowEndingScreen;
        }

        public void TryEnterDungeon()
        {
            if (GameManager.Instance == null) return;

            // Adventure consumes 30 energy as per updated training structure
            if (GameManager.Instance.CurrentEnergy >= 30)
            {
                GameManager.Instance.ConsumeEnergy(30);
                StartCombat();
            }
            else
            {
                Debug.LogWarning("Energi tidak cukup untuk bertualang! Butuh 30 Energi.");
                UpdateLog("Energi tidak cukup! Kamu butuh 30 Energi untuk masuk Benua Iblis.");
            }
        }

        private void StartCombat()
        {
            if (combatPanel != null) combatPanel.SetActive(true);
            currentState = CombatState.Start;
            
            isRenGuarding = false;
            isMarcoGuarding = false;
            isLuciaGuarding = false;
            isMarcoTaunting = false;

            SetupEnemy();
            UpdateUI();
            
            UpdateLog($"Dungeon Level {GameManager.Instance.StoryLevel}: Menghadapi {enemyName}!");
            StartCoroutine(TransitionToRenTurn());
        }

        private void SetupEnemy()
        {
            int storyLevel = GameManager.Instance.StoryLevel;

            if (GameManager.Instance.IsBossStage())
            {
                enemyName = "Lord Malakor (DEMON KING)";
                enemyMaxHP = 750;
                enemyHP = enemyMaxHP;
                enemyATK = 55;
                enemyDEF = 22;
            }
            else
            {
                string[] prefixes = { "Demon Beast", "Fallen Imp", "Gargoyle Raider", "Hell Hound", "Abyss Skeleton", "Demon General" };
                string prefix = prefixes[Mathf.Min(storyLevel / 5, prefixes.Length - 1)];
                
                enemyName = $"{prefix} Benua Iblis";
                enemyMaxHP = 50 + storyLevel * 15;
                enemyHP = enemyMaxHP;
                enemyATK = 10 + storyLevel * 3;
                enemyDEF = 3 + storyLevel * 1;
            }
        }

        #region State Transitions
        private IEnumerator TransitionToRenTurn()
        {
            yield return new WaitForSeconds(1.0f);
            currentState = CombatState.RenTurn;
            isRenGuarding = false;
            
            if (currentTurnIndicatorText != null) currentTurnIndicatorText.text = "Giliran: Ren (Mage)";
            if (skill1Label != null) skill1Label.text = "Slash [⚡10 MP]";
            if (skill2Label != null) skill2Label.text = "Ultimate Burst [⚡25 MP]";
            
            SetActionButtonsInteractable(true);
        }

        private void TransitionToMarcoTurn()
        {
            currentState = CombatState.MarcoTurn;
            isMarcoGuarding = false;

            if (currentTurnIndicatorText != null) currentTurnIndicatorText.text = "Giliran: Marco (Knight)";
            if (skill1Label != null) skill1Label.text = "Shield Slam [⚡8 MP]";
            if (skill2Label != null) skill2Label.text = "Taunt/Provoke [⚡10 MP]";

            SetActionButtonsInteractable(true);
        }

        private void TransitionToLuciaTurn()
        {
            currentState = CombatState.LuciaTurn;
            isLuciaGuarding = false;

            if (currentTurnIndicatorText != null) currentTurnIndicatorText.text = "Giliran: Lucia (Mage)";
            if (skill1Label != null) skill1Label.text = "Divine Heal [⚡15 MP]";
            if (skill2Label != null) skill2Label.text = "Holy Protect [⚡12 MP]";

            SetActionButtonsInteractable(true);
        }

        private IEnumerator TransitionToEnemyTurn()
        {
            currentState = CombatState.EnemyTurn;
            SetActionButtonsInteractable(false);
            if (currentTurnIndicatorText != null) currentTurnIndicatorText.text = "Giliran: Musuh...";

            yield return new WaitForSeconds(1.2f);
            EnemyAction();
        }
        #endregion

        #region Actions Selection
        public void OnAttackSelected()
        {
            SetActionButtonsInteractable(false);
            int damage = 0;

            switch (currentState)
            {
                case CombatState.RenTurn:
                    damage = Mathf.Max(GameManager.Instance.renStats.atk - enemyDEF, 5);
                    enemyHP = Mathf.Max(enemyHP - damage, 0);
                    UpdateLog($"Ren merapalkan bola api! Menghantam {enemyName} sebesar {damage} damage.");
                    UpdateUI();
                    CheckEnemyHealth(() => TransitionToMarcoTurn());
                    break;

                case CombatState.MarcoTurn:
                    damage = Mathf.Max(GameManager.Instance.marcoStats.atk - enemyDEF, 3);
                    enemyHP = Mathf.Max(enemyHP - damage, 0);
                    UpdateLog($"Marco menghantamkan pedangnya! Memberikan {damage} damage ke {enemyName}.");
                    UpdateUI();
                    CheckEnemyHealth(() => TransitionToLuciaTurn());
                    break;

                case CombatState.LuciaTurn:
                    damage = Mathf.Max(GameManager.Instance.luciaStats.atk - enemyDEF, 1);
                    enemyHP = Mathf.Max(enemyHP - damage, 0);
                    UpdateLog($"Lucia melesatkan sihir suci! Memberikan {damage} damage ke {enemyName}.");
                    UpdateUI();
                    CheckEnemyHealth(() => StartCoroutine(TransitionToEnemyTurn()));
                    break;
            }
        }

        public void OnSkill1Selected()
        {
            var gm = GameManager.Instance;
            SetActionButtonsInteractable(false);
            int damage = 0;

            switch (currentState)
            {
                case CombatState.RenTurn:
                    // Slash: costs 10 MP
                    gm.renStats.mp -= 10;
                    damage = Mathf.Max((int)(gm.renStats.atk * 2.5f) - enemyDEF, 10);
                    enemyHP = Mathf.Max(enemyHP - damage, 0);
                    UpdateLog($"Ren melepas [Slash] sihir tajam! Mengoyak {enemyName} sebesar {damage} damage!");
                    UpdateUI();
                    CheckEnemyHealth(() => TransitionToMarcoTurn());
                    break;

                case CombatState.MarcoTurn:
                    // Shield Slam: costs 8 MP
                    gm.marcoStats.mp -= 8;
                    damage = Mathf.Max((int)(gm.marcoStats.atk * 2.0f) - enemyDEF, 8);
                    enemyHP = Mathf.Max(enemyHP - damage, 0);
                    UpdateLog($"Marco membenturkan perisainya [Shield Slam]! {enemyName} terhuyung sebesar {damage} damage!");
                    UpdateUI();
                    CheckEnemyHealth(() => TransitionToLuciaTurn());
                    break;

                case CombatState.LuciaTurn:
                    // Divine Heal: costs 15 MP. Heals all party members by 40 HP!
                    gm.luciaStats.mp -= 15;
                    gm.renStats.hp = Mathf.Min(gm.renStats.hp + 40, gm.renStats.maxHP);
                    gm.marcoStats.hp = Mathf.Min(gm.marcoStats.hp + 40, gm.marcoStats.maxHP);
                    gm.luciaStats.hp = Mathf.Min(gm.luciaStats.hp + 40, gm.luciaStats.maxHP);
                    UpdateLog("Lucia melantunkan [Divine Heal]! Seluruh anggota tim memulihkan +40 HP.");
                    UpdateUI();
                    StartCoroutine(TransitionToEnemyTurn());
                    break;
            }
        }

        public void OnSkill2Selected()
        {
            var gm = GameManager.Instance;
            SetActionButtonsInteractable(false);
            int damage = 0;

            switch (currentState)
            {
                case CombatState.RenTurn:
                    // Ultimate Burst: costs 25 MP
                    gm.renStats.mp -= 25;
                    damage = Mathf.Max((int)(gm.renStats.atk * 4.5f) - enemyDEF, 25);
                    enemyHP = Mathf.Max(enemyHP - damage, 0);
                    UpdateLog($"Ren melepaskan [Ultimate Burst]! Badai sihir raksasa menghancurkan {enemyName} sebesar {damage} damage!!");
                    UpdateUI();
                    CheckEnemyHealth(() => TransitionToMarcoTurn());
                    break;

                case CombatState.MarcoTurn:
                    // Provoke: costs 10 MP. Forces enemy to attack Marco and reduces all party damage by 30%
                    gm.marcoStats.mp -= 10;
                    isMarcoTaunting = true;
                    UpdateLog("Marco memprovokasi musuh dengan [Provoke]! Musuh dipaksa menyerang Marco giliran berikutnya.");
                    UpdateUI();
                    TransitionToLuciaTurn();
                    break;

                case CombatState.LuciaTurn:
                    // Holy Protect: costs 12 MP. Restores 40 MP to Ren! (Support Mage)
                    gm.luciaStats.mp -= 12;
                    gm.renStats.mp = Mathf.Min(gm.renStats.mp + 40, gm.renStats.maxMP);
                    UpdateLog("Lucia merapalkan [Holy Protect] di sekitar Ren! Memulihkan +40 MP Ren.");
                    UpdateUI();
                    StartCoroutine(TransitionToEnemyTurn());
                    break;
            }
        }

        public void OnGuardSelected()
        {
            SetActionButtonsInteractable(false);

            switch (currentState)
            {
                case CombatState.RenTurn:
                    isRenGuarding = true;
                    UpdateLog("Ren bersiap menangkis serangan (Guard).");
                    TransitionToMarcoTurn();
                    break;

                case CombatState.MarcoTurn:
                    isMarcoGuarding = true;
                    UpdateLog("Marco mengangkat perisai besarnya dalam posisi Guard.");
                    TransitionToLuciaTurn();
                    break;

                case CombatState.LuciaTurn:
                    isLuciaGuarding = true;
                    UpdateLog("Lucia merapalkan pelindung cahaya tipis untuk dirinya sendiri.");
                    StartCoroutine(TransitionToEnemyTurn());
                    break;
            }
        }

        public void OnRunSelected()
        {
            SetActionButtonsInteractable(false);
            if (Random.value > 0.5f)
            {
                UpdateLog("Tim berhasil melarikan diri kembali ke rumah!");
                StartCoroutine(EndCombatDelay());
            }
            else
            {
                UpdateLog("Gagal melarikan diri! Musuh mengepung barisan belakang.");
                switch (currentState)
                {
                    case CombatState.RenTurn: TransitionToMarcoTurn(); break;
                    case CombatState.MarcoTurn: TransitionToLuciaTurn(); break;
                    case CombatState.LuciaTurn: StartCoroutine(TransitionToEnemyTurn()); break;
                }
            }
        }
        #endregion

        #region Logic Turn Updates
        private void CheckEnemyHealth(System.Action onAlive)
        {
            if (enemyHP <= 0)
            {
                currentState = CombatState.Victory;
                UpdateLog($"Kemenangan! {enemyName} berhasil ditakhlukkan.");
                StartCoroutine(ProcessVictory());
            }
            else
            {
                onAlive?.Invoke();
            }
        }

        private void EnemyAction()
        {
            var gm = GameManager.Instance;
            
            // Determine target: If Marco taunted, target is Marco. Else, target random alive member
            int targetIndex = isMarcoTaunting ? 1 : Random.Range(0, 3);
            string targetName = targetIndex == 0 ? "Ren" : (targetIndex == 1 ? "Marco" : "Lucia");

            int damage = enemyATK;
            bool guarded = false;

            if (targetIndex == 0)
            {
                damage = Mathf.Max(damage - gm.renStats.def, 1);
                if (isRenGuarding) { damage = Mathf.Max(damage / 2, 1); guarded = true; }
                gm.renStats.hp = Mathf.Max(gm.renStats.hp - damage, 0);
            }
            else if (targetIndex == 1)
            {
                damage = Mathf.Max(damage - gm.marcoStats.def, 1);
                if (isMarcoGuarding) { damage = Mathf.Max(damage / 2, 1); guarded = true; }
                gm.marcoStats.hp = Mathf.Max(gm.marcoStats.hp - damage, 0);
            }
            else
            {
                damage = Mathf.Max(damage - gm.luciaStats.def, 1);
                if (isLuciaGuarding) { damage = Mathf.Max(damage / 2, 1); guarded = true; }
                gm.luciaStats.hp = Mathf.Max(gm.luciaStats.hp - damage, 0);
            }

            string guardText = guarded ? " (Berhasil ditahan!)" : "";
            UpdateLog($"{enemyName} menerjang kuat menyerang {targetName}! Menghasilkan {damage} damage{guardText}.");
            UpdateUI();

            isMarcoTaunting = false; // Reset taunt after turn

            StartCoroutine(CheckPartyStatus());
        }

        private IEnumerator CheckPartyStatus()
        {
            yield return new WaitForSeconds(1.0f);
            var gm = GameManager.Instance;

            // Check if all party members have fallen
            if (gm.renStats.hp <= 0 && gm.marcoStats.hp <= 0 && gm.luciaStats.hp <= 0)
            {
                currentState = CombatState.Defeat;
                UpdateLog("Kekalahan telak... Seluruh anggota tim jatuh tidak sadarkan diri.");
                StartCoroutine(ProcessDefeat());
            }
            else
            {
                StartCoroutine(TransitionToRenTurn());
            }
        }

        private IEnumerator ProcessVictory()
        {
            yield return new WaitForSeconds(1.5f);

            if (GameManager.Instance.IsBossStage())
            {
                // Defeated Level 30 boss! Evaluate ending branching
                GameManager.Instance.EvaluateFinalEnding();
            }
            else
            {
                GameManager.Instance.AdvanceStoryLevel();
                StartCoroutine(EndCombatDelay());
            }
        }

        private IEnumerator ProcessDefeat()
        {
            yield return new WaitForSeconds(1.5f);
            
            // Defeat triggers Bad Ending directly as per updated PRD Section 3D!
            GameManager.Instance.TriggerEnding("BadEnding_Defeat");
        }

        private IEnumerator EndCombatDelay()
        {
            yield return new WaitForSeconds(1.5f);
            combatPanel.SetActive(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyStateChanged();
            }
        }
        #endregion

        #region Endings View Overlay
        private void ShowEndingScreen(string endingId)
        {
            SetActionButtonsInteractable(false);
            if (combatPanel != null) combatPanel.SetActive(false);

            if (endingId == "TrueEnding")
            {
                if (trueEndingPanel != null) trueEndingPanel.SetActive(true);
                UpdateLog("TRUE ENDING: Penawar berhasil diracik! Hubunganmu dengan Lara yang erat menyembuhkannya secara total.");
            }
            else if (endingId == "GoodEnding")
            {
                if (goodEndingPanel != null) goodEndingPanel.SetActive(true);
                UpdateLog("GOOD ENDING: Penawar berhasil diserahkan ke Lara. Lara sembuh, meski sirkuit mananya menyisakan bekas luka.");
            }
            else
            {
                if (badEndingPanel != null) badEndingPanel.SetActive(true);
                if (endingId.Contains("TimeLimit"))
                {
                    UpdateLog("BAD ENDING: Waktu 30 hari telah habis! Lara tidak dapat bertahan dari sisa kutukan Raja Iblis.");
                }
                else if (endingId.Contains("Defeat"))
                {
                    UpdateLog("BAD ENDING: Tim kalah bertarung di Benua Iblis! Penawar gagal didapatkan, Lara tidak tertolong.");
                }
                else
                {
                    UpdateLog("BAD ENDING: Penawar tidak bekerja optimal karena ikatan perasaan Lara terlalu rendah.");
                }
            }
        }

        public void RestartGame()
        {
            if (trueEndingPanel != null) trueEndingPanel.SetActive(false);
            if (goodEndingPanel != null) goodEndingPanel.SetActive(false);
            if (badEndingPanel != null) badEndingPanel.SetActive(false);
            if (combatPanel != null) combatPanel.SetActive(false);

            // Resets to Day 1 Morning
            GameManager.Instance.SetState(1, "Pagi", 100, 1, 
                120, 80, 120, 80, 22, 6,
                200, 30, 200, 30, 14, 18,
                100, 100, 100, 100, 10, 8,
                15, 10, "Yard");
        }
        #endregion

        #region Helpers & UI updates
        private void SetActionButtonsInteractable(bool state)
        {
            attackButton.interactable = state;
            guardButton.interactable = state;
            runButton.interactable = state;
            
            var gm = GameManager.Instance;
            if (currentState == CombatState.RenTurn)
            {
                skill1Button.interactable = state && (gm.renStats.mp >= 10) && (gm.renStats.hp > 0);
                skill2Button.interactable = state && (gm.renStats.mp >= 25) && (gm.renStats.hp > 0);
                
                attackButton.interactable = state && (gm.renStats.hp > 0);
                guardButton.interactable = state && (gm.renStats.hp > 0);
            }
            else if (currentState == CombatState.MarcoTurn)
            {
                skill1Button.interactable = state && (gm.marcoStats.mp >= 8) && (gm.marcoStats.hp > 0);
                skill2Button.interactable = state && (gm.marcoStats.mp >= 10) && (gm.marcoStats.hp > 0);
                
                attackButton.interactable = state && (gm.marcoStats.hp > 0);
                guardButton.interactable = state && (gm.marcoStats.hp > 0);
            }
            else if (currentState == CombatState.LuciaTurn)
            {
                skill1Button.interactable = state && (gm.luciaStats.mp >= 15) && (gm.luciaStats.hp > 0);
                skill2Button.interactable = state && (gm.luciaStats.mp >= 12) && (gm.luciaStats.hp > 0);
                
                attackButton.interactable = state && (gm.luciaStats.hp > 0);
                guardButton.interactable = state && (gm.luciaStats.hp > 0);
            }
        }

        private void UpdateUI()
        {
            if (GameManager.Instance == null) return;

            var gm = GameManager.Instance;

            // Ren Mage
            if (renHPText != null) renHPText.text = $"Ren HP: {gm.renStats.hp}/{gm.renStats.maxHP}";
            if (renMPText != null) renMPText.text = $"MP: {gm.renStats.mp}/{gm.renStats.maxMP}";
            if (renHPSlider != null) { renHPSlider.maxValue = gm.renStats.maxHP; renHPSlider.value = gm.renStats.hp; }
            if (renMPSlider != null) { renMPSlider.maxValue = gm.renStats.maxMP; renMPSlider.value = gm.renStats.mp; }

            // Marco Knight
            if (marcoHPText != null) marcoHPText.text = $"Marco HP: {gm.marcoStats.hp}/{gm.marcoStats.maxHP}";
            if (marcoMPText != null) marcoMPText.text = $"MP: {gm.marcoStats.mp}/{gm.marcoStats.maxMP}";
            if (marcoHPSlider != null) { marcoHPSlider.maxValue = gm.marcoStats.maxHP; marcoHPSlider.value = gm.marcoStats.hp; }
            if (marcoMPSlider != null) { marcoMPSlider.maxValue = gm.marcoStats.maxMP; marcoMPSlider.value = gm.marcoStats.mp; }

            // Lucia Priestess
            if (luciaHPText != null) luciaHPText.text = $"Lucia HP: {gm.luciaStats.hp}/{gm.luciaStats.maxHP}";
            if (luciaMPText != null) luciaMPText.text = $"MP: {gm.luciaStats.mp}/{gm.luciaStats.maxMP}";
            if (luciaHPSlider != null) { luciaHPSlider.maxValue = gm.luciaStats.maxHP; luciaHPSlider.value = gm.luciaStats.hp; }
            if (luciaMPSlider != null) { luciaMPSlider.maxValue = gm.luciaStats.maxMP; luciaMPSlider.value = gm.luciaStats.mp; }

            // Enemy
            if (enemyNameText != null) enemyNameText.text = enemyName;
            if (enemyHPText != null) enemyHPText.text = $"{enemyHP} / {enemyMaxHP}";
            if (enemyHPSlider != null) { enemyHPSlider.maxValue = enemyMaxHP; enemyHPSlider.value = enemyHP; }
        }

        private void UpdateLog(string logText)
        {
            if (combatLogText != null)
            {
                combatLogText.text = logText;
            }
        }
        #endregion
    }
}
