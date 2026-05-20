using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text dialogueBodyText;
        [SerializeField] private Image portraitImage;

        [Header("Choice Settings")]
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Transform choicesContainer;

        [Header("Text Effects")]
        [SerializeField] private float typingSpeed = 0.02f;

        [Header("Expression Portraits")]
        [SerializeField] private List<NPCExpressionMap> npcPortraits = new List<NPCExpressionMap>();

        [System.Serializable]
        public struct NPCExpressionMap
        {
            public string npcId; // lara, lucia, marco
            public string expression; // Normal, Senang, Tsundere, Malu, Serius, Cemas, Lemah, Terkejut
            public Sprite sprite;
        }

        private DialogueSequence currentSequence;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private string activeNPCId;
        private bool isCampaignActive = false;
        private int activeCampaignSection = 1;

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
            
            // Play initial opening chapter automatically
            StartCoroutine(TriggerInitialOpening());
        }

        private void OnDestroy()
        {
            GameManager.OnDialogueTriggered -= StartDailyDialogue;
            GameManager.OnStoryEventTriggered -= StartStoryCampaign;
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

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            currentLineIndex = 0;
            ClearChoices();
            ShowNextLine();
        }

        public void OnDialogueBoxClicked()
        {
            if (isTyping)
            {
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
                    if (currentSequence.choices.Count > 0 && choicesContainer.childCount == 0)
                    {
                        DisplayChoices();
                    }
                    else if (currentSequence.choices.Count == 0)
                    {
                        EndDialogue();
                    }
                }
            }
        }

        private void ShowNextLine()
        {
            DialogueLine currentLine = currentSequence.lines[currentLineIndex];
            speakerNameText.text = currentLine.speakerName;
            
            Sprite expressionSprite = GetPortrait(isCampaignActive ? currentLine.speakerName : activeNPCId, currentLine.expression);
            if (portraitImage != null)
            {
                if (expressionSprite != null)
                {
                    portraitImage.sprite = expressionSprite;
                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
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
                Text btnText = choiceBtnObj.GetComponentInChildren<Text>();
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
                    GameManager.Instance.CurrentRoom = "Yard";
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
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            ClearChoices();
            GameManager.Instance.NotifyStateChanged();
            Debug.Log("Dialogue Ended.");
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
    }
}
