using UnityEngine;
using UnityEngine.EventSystems;

namespace FantasyLifeVN.Navigation
{
    public class RoomTrigger : MonoBehaviour, IPointerClickHandler
    {
        [Header("Transition Settings")]
        [Tooltip("The room name to transition to (e.g., Bedroom, Kitchen, LivingRoom, Yard, Dungeon)")]
        [SerializeField] private string targetRoom;

        /// <summary>
        /// Public method that can be assigned directly to standard Unity UI Buttons
        /// </summary>
        public void TriggerTransition()
        {
            if (RoomManager.Instance != null && !string.IsNullOrEmpty(targetRoom))
            {
                RoomManager.Instance.TransitionToRoom(targetRoom);
            }
            else
            {
                Debug.LogWarning($"RoomTrigger on {gameObject.name} lacks RoomManager instance or targetRoom is empty.");
            }
        }

        /// <summary>
        /// Used when clicking on a 2D collider sprite representing a doorway or path (requires Physics2DRaycaster on Camera)
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerTransition();
        }

        /// <summary>
        /// Fallback for basic collider clicks if EventSystem is not utilized
        /// </summary>
        private void OnMouseDown()
        {
            // Only trigger if not clicking UI elements to avoid double inputs
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                TriggerTransition();
            }
        }
    }
}
