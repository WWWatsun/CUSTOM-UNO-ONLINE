using Managers;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

namespace PlayerScripts
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float maxLookAngle = 85.0f;

        private PlayerInput playerInput;

        private Vector2 lookInput;
        private float verticalRotation = 0f;
        private bool isPlaying = true; // Flag to track if the game has started

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = false;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            HandleLook();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                playerInput.enabled = true; // Enable input for the owning player
                playerCamera.Priority = 20; // Set camera priority to ensure the owning player's camera is active
            }

            base.OnNetworkSpawn();
        }

        public void OnLook(InputValue context)
        {
            if (!isPlaying) return; // Ignore input if the game hasn't started
            lookInput = context.Get<Vector2>();
        }

        private void HandleLook()
        {
            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            PlayersManager.Instance.UpdatePlayerLookInputRpc(NetworkManager.Singleton.LocalClientId, mouseX);
        }

        public void SetPlayerTurnControls(bool isCurrentTurn)
        {
            isPlaying = isCurrentTurn; // Update the flag based on whether it's the player's turn
            //playerCamera.Priority = isCurrentTurn ? 20 : 10; // Set camera priority to switch between players
        }
    }
}

