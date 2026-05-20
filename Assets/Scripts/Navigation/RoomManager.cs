using UnityEngine;
using System.Collections.Generic;
using FantasyLifeVN.Core;

namespace FantasyLifeVN.Navigation
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [System.Serializable]
        public struct TimeSprites
        {
            public Sprite pagiSprite;
            public Sprite siangSprite;
            public Sprite malamSprite;
        }

        [System.Serializable]
        public struct RoomConfig
        {
            public string roomName;
            public SpriteRenderer backgroundRenderer;
            public TimeSprites timeSprites;
        }

        [Header("Room Setup")]
        [SerializeField] private List<RoomConfig> rooms = new List<RoomConfig>();
        [SerializeField] private SpriteRenderer mainBackgroundRenderer;

        [Header("GIM Character Spawners")]
        [SerializeField] private GameObject laraNPC;
        [SerializeField] private GameObject luciaNPC;
        [SerializeField] private GameObject marcoNPC;
        
        [Header("NPC Location Pivots")]
        [SerializeField] private Transform bedroomPivot;
        [SerializeField] private Transform kitchenPivot;
        [SerializeField] private Transform livingRoomPivot;
        [SerializeField] private Transform yardPivot;

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
            GameManager.OnGameStateChanged += UpdateRoomVisuals;
            UpdateRoomVisuals();
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= UpdateRoomVisuals;
        }

        public void TransitionToRoom(string roomName)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentRoom = roomName;
                GameManager.Instance.NotifyStateChanged();
                Debug.Log($"Transitioned to room: {roomName}");
            }
        }

        public void UpdateRoomVisuals()
        {
            if (GameManager.Instance == null) return;

            string currentRoom = GameManager.Instance.CurrentRoom;
            string timePhase = GameManager.Instance.TimePhase;

            // 1. Update Background Sprite
            RoomConfig? currentConfig = rooms.Find(r => r.roomName.Equals(currentRoom, System.StringComparison.OrdinalIgnoreCase));
            if (currentConfig.HasValue && mainBackgroundRenderer != null)
            {
                Sprite targetSprite = null;
                switch (timePhase)
                {
                    case "Pagi":
                        targetSprite = currentConfig.Value.timeSprites.pagiSprite;
                        break;
                    case "Siang":
                        targetSprite = currentConfig.Value.timeSprites.siangSprite;
                        break;
                    case "Malam":
                        targetSprite = currentConfig.Value.timeSprites.malamSprite;
                        break;
                }

                if (targetSprite != null)
                {
                    mainBackgroundRenderer.sprite = targetSprite;
                }
            }

            // 2. Position NPCs based on GIM.pdf Schedule
            // Lara is permanently bedridden sick in her room until the Demon Lord is defeated
            UpdateNPCScheduling(currentRoom, timePhase, GameManager.Instance.StoryLevel, true);
        }

        private void UpdateNPCScheduling(string room, string phase, int storyLevel, bool isSick)
        {
            if (laraNPC == null || luciaNPC == null || marcoNPC == null) return;

            laraNPC.SetActive(false);
            luciaNPC.SetActive(false);
            marcoNPC.SetActive(false);

            string currentRoomLower = room.ToLower();

            if (storyLevel <= 4)
            {
                return;
            }

            // Lara is permanently sick in the Bedroom
            if (isSick)
            {
                if (currentRoomLower == "bedroom")
                {
                    laraNPC.transform.position = bedroomPivot != null ? bedroomPivot.position : Vector3.zero;
                    laraNPC.SetActive(true);
                }
            }

            // Lucia sitting near Lara's bed or planning
            if (currentRoomLower == "bedroom" && (phase == "Pagi" || phase == "Malam"))
            {
                luciaNPC.transform.position = bedroomPivot != null ? bedroomPivot.position + new Vector3(1.5f, 0, 0) : Vector3.zero;
                luciaNPC.SetActive(true);
            }
            else if (currentRoomLower == "livingroom" && phase == "Siang")
            {
                luciaNPC.transform.position = livingRoomPivot != null ? livingRoomPivot.position : Vector3.zero;
                luciaNPC.SetActive(true);
            }

            // Marco patrols/trains
            if (currentRoomLower == "yard" && (phase == "Pagi" || phase == "Siang"))
            {
                marcoNPC.transform.position = yardPivot != null ? yardPivot.position : Vector3.zero;
                marcoNPC.SetActive(true);
            }
            else if (currentRoomLower == "livingroom" && phase == "Malam")
            {
                marcoNPC.transform.position = livingRoomPivot != null ? livingRoomPivot.position : Vector3.zero;
                marcoNPC.SetActive(true);
            }
        }
    }
}
