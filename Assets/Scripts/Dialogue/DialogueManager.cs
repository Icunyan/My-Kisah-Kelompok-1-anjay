using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using FantasyLifeVN.Core;

namespace FantasyLifeVN.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public enum CombatState { Start, PlayerTurn, EnemyTurn, Victory, Defeat }

        [Header("UI Components")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueBodyText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button btnNextDialogue;

        [Header("Choice Settings")]
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Transform choicesContainer;

        [Header("Text Effects")]
        [SerializeField] private float typingSpeed = 0.02f;

        [Header("Expression Portraits")]
        [SerializeField] private List<NPCPortraitSet> npcPortraitSets = new List<NPCPortraitSet>();
        // Keeping the old list for backward compatibility (campaign mode)
        [SerializeField] private List<NPCExpressionMap> npcPortraits = new List<NPCExpressionMap>();

        [System.Serializable]
        public class NPCPortraitSet
        {
            public string npcId;          // lara, lucia, marco
            public Sprite defaultSprite;  // Gambar diam / idle (muncul saat dialog selesai)
            public Sprite talkingSprite;  // Gambar ngomong (muncul saat dialog berjalan)
        }

        [System.Serializable]
        public struct NPCExpressionMap
        {
            public string npcId;
            public string expression;
            public Sprite sprite;
        }

        private DialogueSequence currentSequence;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private string activeNPCId;
        private bool isCampaignActive = false;
        private int activeCampaignSection = 1;

        // State tracking to close dialogue on external actions (room change, train, rest, etc.)
        private string lastRoom = "";
        private int lastEnergy = -1;
        private int lastDay = -1;
        private string lastPhase = "";
        private bool isClosing = false;

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
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            
            // Subscribe to triggers
            GameManager.OnDialogueTriggered += StartDailyDialogue;
            GameManager.OnStoryEventTriggered += StartStoryCampaign;
            GameManager.OnGameStateChanged += HandleGameStateChanged;

            if (btnNextDialogue != null)
            {
                btnNextDialogue.onClick.AddListener(OnDialogueBoxClicked);
            }
            
            // Play initial opening chapter automatically
            StartCoroutine(TriggerInitialOpening());
        }

        private void OnDestroy()
        {
            GameManager.OnDialogueTriggered -= StartDailyDialogue;
            GameManager.OnStoryEventTriggered -= StartStoryCampaign;
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private IEnumerator TriggerInitialOpening()
        {
            yield return new WaitForSeconds(1.5f);
            if (GameManager.Instance != null && GameManager.Instance.StoryLevel == 1)
            {
                StartStoryCampaign(1);
            }
        }

        /// <summary>
        /// Starts the story campaign based on GIM.pdf sections.
        /// </summary>
        public void StartStoryCampaign(int sectionNumber)
        {
            isCampaignActive = true;
            activeCampaignSection = sectionNumber;
            
            currentSequence = DialogueDatabase.GetCampaignDialogue(sectionNumber);

            if (currentSequence == null || currentSequence.lines.Count == 0)
            {
                Debug.LogWarning($"Story Campaign Section {sectionNumber} not found.");
                return;
            }

            if (GameManager.Instance != null)
            {
                lastRoom = GameManager.Instance.CurrentRoom;
                lastEnergy = GameManager.Instance.CurrentEnergy;
                lastDay = GameManager.Instance.Day;
                lastPhase = GameManager.Instance.TimePhase;
            }

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            currentLineIndex = 0;
            ClearChoices();
            ShowNextLine();
        }

        /// <summary>
        /// Starts normal daily dialogue interactively with room characters.
        /// </summary>
        public void StartDailyDialogue(string npcId)
        {
            if (GameManager.Instance == null) return;
            
            isCampaignActive = false;
            activeNPCId = npcId.ToLower();
            
            int affectionValue = activeNPCId == "lara" ? GameManager.Instance.LaraFriendship : 
                                 (activeNPCId == "lucia" ? GameManager.Instance.AffectionLucia : 0);

            // Lara is permanently sick/bedridden in her bedroom from Day 1 due to the Demon Lord's curse
            currentSequence = DialogueDatabase.GetDailyDialogue(
                activeNPCId,
                GameManager.Instance.TimePhase,
                affectionValue,
                true // isLaraSick is always true during the campaign until cured
            );

            if (currentSequence == null || currentSequence.lines.Count == 0)
            {
                Debug.LogWarning($"No daily dialogue lines found for NPC: {npcId}");
                return;
            }

            // Save state to detect external changes
            lastRoom = GameManager.Instance.CurrentRoom;
            lastEnergy = GameManager.Instance.CurrentEnergy;
            lastDay = GameManager.Instance.Day;
            lastPhase = GameManager.Instance.TimePhase;

            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            // Hide choices container - daily dialogue has no choices, click-through only
            if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

            currentLineIndex = 0;
            ClearChoices();
            ShowNextLine();
        }

        public void OnDialogueBoxClicked()
        {
            if (isTyping)
            {
                // Skip typewriter animation: immediately show full text
                StopCoroutine(typingCoroutine);
                dialogueBodyText.text = currentSequence.lines[currentLineIndex].text;
                isTyping = false;
            }
            else
            {
                currentLineIndex++;
                if (currentLineIndex < currentSequence.lines.Count)
                {
                    ShowNextLine();
                }
                else
                {
                    if (isCampaignActive && currentSequence.choices.Count > 0)
                    {
                        // In campaign mode, auto-select the choice when clicking Next
                        DialogueChoice firstChoice = currentSequence.choices[0];
                        OnChoiceSelected(firstChoice);
                    }
                    else
                    {
                        // Loop Lara's daily dialogue or room dialogue
                        if (!isCampaignActive && activeNPCId == "lara")
                        {
                            if (GameManager.Instance != null && GameManager.Instance.CurrentRoom.ToLower() == "kamar lara")
                            {
                                if (currentSequence.sequenceID == "kamar_lara_menu")
                                {
                                    // Keep menu line visible
                                    currentLineIndex = 0;
                                    ShowNextLine();
                                }
                                else
                                {
                                    // Loop to another random Lara dialogue
                                    int randomIndex = UnityEngine.Random.Range(1, 6);
                                    currentSequence = DialogueDatabase.GetRandomLaraDialogue(randomIndex);
                                    currentLineIndex = 0;
                                    ShowNextLine();
                                }
                            }
                            else
                            {
                                currentLineIndex = 0;
                                ShowNextLine();
                            }
                        }
                        else
                        {
                            EndDialogue();
                        }
                    }
                }
            }
        }

        private void ShowNextLine()
        {
            DialogueLine currentLine = currentSequence.lines[currentLineIndex];
            speakerNameText.text = currentLine.speakerName;

            if (portraitImage != null)
            {
                // Coba cari sprite berdasarkan ekspresi spesifik (NPC Portraits list)
                Sprite expressionSprite = GetPortrait(isCampaignActive ? currentLine.speakerName : activeNPCId, currentLine.expression);

                if (expressionSprite != null)
                {
                    // Gunakan sprite ekspresi spesifik jika ditemukan
                    portraitImage.sprite = expressionSprite;
                }
                else
                {
                    // Fallback: gunakan Talking Sprite dari NPC Portrait Set
                    Sprite talkingSprite = GetTalkingPortrait(isCampaignActive ? currentLine.speakerName : activeNPCId);
                    if (talkingSprite != null)
                        portraitImage.sprite = talkingSprite;
                }
                // Selalu tampilkan portrait selama dialog berlangsung
                portraitImage.gameObject.SetActive(true);
            }

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeWriter(currentLine.text));
        }

        private IEnumerator TypeWriter(string line)
        {
            isTyping = true;
            dialogueBodyText.text = "";
            foreach (char letter in line.ToCharArray())
            {
                dialogueBodyText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            isTyping = false;
        }

        private void DisplayChoices()
        {
            ClearChoices();

            foreach (DialogueChoice choice in currentSequence.choices)
            {
                GameObject choiceBtnObj = Instantiate(choiceButtonPrefab, choicesContainer);
                TextMeshProUGUI btnText = choiceBtnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText == null)
                {
                    // Fallback to legacy Text if TMP not found on prefab
                    Text legacyText = choiceBtnObj.GetComponentInChildren<Text>();
                    if (legacyText != null) legacyText.text = choice.choiceText;
                }
                Button btn = choiceBtnObj.GetComponent<Button>();

                if (choice.energyCost > 0)
                {
                    btnText.text = $"{choice.choiceText} [⚡ -{choice.energyCost}]";
                }
                else
                {
                    btnText.text = choice.choiceText;
                }

                bool hasEnoughEnergy = isCampaignActive || (GameManager.Instance.CurrentEnergy >= choice.energyCost);
                btn.interactable = hasEnoughEnergy;

                if (!hasEnoughEnergy)
                {
                    btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, 0.5f);
                }

                DialogueChoice localChoice = choice;
                btn.onClick.AddListener(() => OnChoiceSelected(localChoice));
            }
        }

        private void OnChoiceSelected(DialogueChoice choice)
        {
            if (GameManager.Instance == null) return;

            // Custom Lara Room Choices Interruption
            if (choice.targetNodeID == "lara_talk")
            {
                int randomIndex = UnityEngine.Random.Range(1, 6);
                currentSequence = DialogueDatabase.GetRandomLaraDialogue(randomIndex);
                currentLineIndex = 0;
                ShowNextLine();
                return;
            }
            else if (choice.targetNodeID == "lara_upgrade")
            {
                if (GameManager.Instance.CurrentEnergy >= 15)
                {
                    GameManager.Instance.ConsumeEnergy(15);
                    GameManager.Instance.LaraFriendship += 5;
                    // Boost party statistics as upgrade effect
                    GameManager.Instance.renStats.TrainBoost(3, 2, 1, 1);
                    GameManager.Instance.marcoStats.TrainBoost(5, 0, 1, 2);
                    GameManager.Instance.luciaStats.TrainBoost(3, 4, 0, 1);

                    DialogueSequence upgradeSeq = new DialogueSequence();
                    upgradeSeq.sequenceID = "lara_upgrade_success";
                    upgradeSeq.lines.Add(new DialogueLine { speakerName = "Lara", text = "Terima kasih, Ren! Kamar ini terasa jauh lebih nyaman dan aku merasa energi kita semua meningkat.", expression = "Pose1_Tutupmata_senyum" });
                    currentSequence = upgradeSeq;
                    currentLineIndex = 0;
                    ShowNextLine();
                }
                else
                {
                    DialogueSequence noEnergySeq = new DialogueSequence();
                    noEnergySeq.sequenceID = "lara_no_energy";
                    noEnergySeq.lines.Add(new DialogueLine { speakerName = "Lara", text = "Ren, kamu terlihat lelah sekali... Istirahatlah dulu sebelum membantu meningkatkan skill.", expression = "Pose2_TutupMulut" });
                    currentSequence = noEnergySeq;
                    currentLineIndex = 0;
                    ShowNextLine();
                }
                return;
            }
            else if (choice.targetNodeID == "lara_return")
            {
                GameManager.Instance.CurrentRoom = "Kamar Ren";
                EndDialogue();
                return;
            }

            if (isCampaignActive)
            {
                ClearChoices();

                if (choice.targetNodeID.StartsWith("trigger_section_"))
                {
                    int nextSection = int.Parse(choice.targetNodeID.Replace("trigger_section_", ""));
                    GameManager.Instance.AdvanceStoryLevel();
                    StartStoryCampaign(nextSection);
                    return;
                }
                else if (choice.targetNodeID == "trigger_dungeon_prep")
                {
                    GameManager.Instance.AdvanceStoryLevel(); // Set story to Level 5
                    GameManager.Instance.CurrentRoom = "Kamar Ren"; // Go to Kamar Ren!
                    EndDialogue();
                    return;
                }
            }

            if (GameManager.Instance.CurrentEnergy >= choice.energyCost)
            {
                GameManager.Instance.ConsumeEnergy(choice.energyCost);

                if (activeNPCId == "lara")
                {
                    GameManager.Instance.LaraFriendship += choice.affectionGain;
                }
                else if (activeNPCId == "lucia")
                {
                    GameManager.Instance.AffectionLucia += choice.affectionGain;
                }

                if (choice.targetNodeID.Contains("sleep"))
                {
                    GameManager.Instance.SleepAndResetDay();
                }

                ClearChoices();
                speakerNameText.text = "Sistem";
                if (portraitImage != null) portraitImage.gameObject.SetActive(false);
                
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                typingCoroutine = StartCoroutine(ShowConsequence(choice.consequenceText));
            }
        }

        private IEnumerator ShowConsequence(string text)
        {
            isTyping = true;
            dialogueBodyText.text = "";
            foreach (char letter in text.ToCharArray())
            {
                dialogueBodyText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            isTyping = false;

            while (!Input.GetMouseButtonDown(0))
            {
                yield return null;
            }
            EndDialogue();
        }

        private void EndDialogue()
        {
            if (isClosing) return;
            isClosing = true;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            ClearChoices();

            // Kembalikan gambar Lara/karakter ke pose diam (default) setelah dialog selesai
            if (portraitImage != null)
            {
                Sprite defaultSprite = GetDefaultPortrait(activeNPCId);
                if (defaultSprite != null)
                {
                    portraitImage.sprite = defaultSprite;
                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
            }

            if (GameManager.Instance != null)
            {
                // Sync state so we don't double-trigger
                lastRoom = GameManager.Instance.CurrentRoom;
                lastEnergy = GameManager.Instance.CurrentEnergy;
                lastDay = GameManager.Instance.Day;
                lastPhase = GameManager.Instance.TimePhase;

                GameManager.Instance.NotifyStateChanged();
            }
            Debug.Log("Dialogue Ended.");
            isClosing = false;
        }

        private void HandleGameStateChanged()
        {
            if (GameManager.Instance != null)
            {
                string currentRoom = GameManager.Instance.CurrentRoom;

                if (currentRoom != lastRoom)
                {
                    if (currentRoom.ToLower() == "kamar lara")
                    {
                        StartKamarLaraMenu();
                        return;
                    }
                    else
                    {
                        if (dialoguePanel != null && dialoguePanel.activeSelf)
                        {
                            EndDialogue();
                        }
                    }
                }

                if (dialoguePanel != null && dialoguePanel.activeSelf)
                {
                    if (GameManager.Instance.CurrentEnergy != lastEnergy ||
                        GameManager.Instance.Day != lastDay ||
                        GameManager.Instance.TimePhase != lastPhase)
                    {
                        EndDialogue();
                    }
                }
            }
        }

        public void StartKamarLaraMenu()
        {
            isCampaignActive = false;
            activeNPCId = "lara";
            currentSequence = DialogueDatabase.GetKamarLaraMenu();

            if (GameManager.Instance != null)
            {
                lastRoom = GameManager.Instance.CurrentRoom;
                lastEnergy = GameManager.Instance.CurrentEnergy;
                lastDay = GameManager.Instance.Day;
                lastPhase = GameManager.Instance.TimePhase;
            }

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (choicesContainer != null) choicesContainer.gameObject.SetActive(true);

            currentLineIndex = 0;
            ClearChoices();
            ShowNextLine();
            DisplayChoices();
        }

        private void ClearChoices()
        {
            if (choicesContainer == null) return;
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private Sprite GetPortrait(string speaker, string expression)
        {
            string speakerKey = speaker.ToLower();
            if (speakerKey.Contains("ren")) speakerKey = "ren";
            else if (speakerKey.Contains("lara")) speakerKey = "lara";
            else if (speakerKey.Contains("lucia")) speakerKey = "lucia";
            else if (speakerKey.Contains("marco")) speakerKey = "marco";

            NPCExpressionMap map = npcPortraits.Find(p => 
                p.npcId.Equals(speakerKey, System.StringComparison.OrdinalIgnoreCase) && 
                p.expression.Equals(expression, System.StringComparison.OrdinalIgnoreCase));
            return map.sprite;
        }

        private Sprite GetTalkingPortrait(string npcId)
        {
            string key = npcId.ToLower();
            NPCPortraitSet set = npcPortraitSets.Find(p => p.npcId.Equals(key, System.StringComparison.OrdinalIgnoreCase));
            return set?.talkingSprite;
        }

        private Sprite GetDefaultPortrait(string npcId)
        {
            string key = npcId.ToLower();
            NPCPortraitSet set = npcPortraitSets.Find(p => p.npcId.Equals(key, System.StringComparison.OrdinalIgnoreCase));
            return set?.defaultSprite;
        }
    }
}
