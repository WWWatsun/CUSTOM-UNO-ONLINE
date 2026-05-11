using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

namespace PlayerScripts
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float maxLookAngle = 85.0f;

        private PlayerInput playerInput;

        private Vector2 lookInput;
        private float verticalRotation = 0f;
        private bool isPlaying = true; // Flag to track if the game has started

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        // Update is called once per frame
        void Update()
        {
            HandleLook();
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
            transform.Rotate(Vector3.up * mouseX);
        }

        public void SetPlayerTurnControls(bool isCurrentTurn)
        {
            isPlaying = isCurrentTurn; // Update the flag based on whether it's the player's turn
            playerCamera.Priority = isCurrentTurn ? 20 : 10; // Set camera priority to switch between players
        }
    }
}

