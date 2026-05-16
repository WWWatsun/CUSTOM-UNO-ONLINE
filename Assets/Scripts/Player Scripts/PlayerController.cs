using Managers;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float maxLookAngle = 85.0f;

        [Header("References")]
        [SerializeField] private LayerMask cardLayer;

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

                Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
                Cursor.visible = false; // Hide the cursor
            }

            base.OnNetworkSpawn();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Safety check: We only want the local player's machine to mess with its own cursor
            if (!IsOwner) return;

            if (hasFocus)
            {
                // The game window just regained focus, so re-lock the mouse!
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OnInteract(InputValue context)
        {
            // Block interaction if it's not your turn OR if you are currently using the mouse for UI
            if (!isPlaying || Cursor.lockState != CursorLockMode.Locked) return;

            if (context.isPressed)
            {
                FireRaycast();
            }
        }

        public void OnLook(InputValue context)
        {
            // Block looking if it's not your turn OR if you are currently using the mouse for UI
            if (!isPlaying || Cursor.lockState != CursorLockMode.Locked)
            {
                lookInput = Vector2.zero; // Stop camera drift immediately
                return;
            }

            lookInput = context.Get<Vector2>();
        }

        public void OnShowCursor(InputValue context)
        {
            if (!IsOwner) return;

            if (context.isPressed)
            {
                Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                Cursor.visible = true; // Show the cursor
                lookInput = Vector2.zero; // Instantly kill any current camera movement
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
                Cursor.visible = false; // Hide the cursor
            }
        }

        private void HandleLook()
        {
            // Safety net: Do not apply any rotations if the cursor is currently unlocked
            if (Cursor.lockState != CursorLockMode.Locked) return;

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

        private void FireRaycast()
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 100f, Color.red, 1f);

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, cardLayer))
            {
                NetworkObject hitObject = hit.collider.GetComponent<NetworkObject>();

                if (hitObject != null)
                {
                    // Check if THIS player owns the card they clicked
                    if (DeckManager.Instance.IsDrawPile(hitObject.NetworkObjectId))
                    {
                        PlayersManager.Instance.RequestCardDrawRpc(NetworkManager.Singleton.LocalClientId);
                    }
                    else if (hitObject.IsOwner)
                    {
                        Debug.Log("Hit my card! Requesting deletion...");
                        PlayersManager.Instance.RequestCardActionRpc(NetworkManager.Singleton.LocalClientId, hitObject.NetworkObjectId);
                    }
                    else
                    {
                        Debug.Log("That's not your card!");
                    }
                }
            }
        }
    }
}

