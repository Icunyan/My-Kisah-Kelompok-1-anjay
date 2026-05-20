using UnityEngine;
using UnityEngine.EventSystems;
using FantasyLifeVN.Core;

namespace FantasyLifeVN.Navigation
{
    [RequireComponent(typeof(Collider2D))]
    public class NPCInteraction : MonoBehaviour, IPointerClickHandler
    {
        [Header("NPC Settings")]
        [Tooltip("NPC ID in lowercase (e.g., lara, lucia, marco)")]
        [SerializeField] private string npcId;

        public void TriggerDialogue()
        {
            if (GameManager.Instance != null && !string.IsNullOrEmpty(npcId))
            {
                GameManager.Instance.TriggerDialogue(npcId);
            }
            else
            {
                Debug.LogWarning($"NPCInteraction on {gameObject.name} lacks GameManager instance or npcId is empty.");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerDialogue();
        }

        private void OnMouseDown()
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                TriggerDialogue();
            }
        }
    }
}
