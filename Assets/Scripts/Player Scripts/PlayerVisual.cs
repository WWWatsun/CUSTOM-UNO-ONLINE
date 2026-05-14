using UnityEngine;

namespace PlayerScripts
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer playerFace; // Reference to the player's sprite renderer

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetPlayerTurnVisual(bool isCurrentTurn)
        {
            // Implement visual changes to indicate the player's turn
            if (isCurrentTurn)
            {
                transform.localScale = Vector3.one * 1.5f; // Enlarge the player to indicate it's their turn
            }
            else
            {
                transform.localScale = Vector3.one; // Reset to normal size
            }
        }
    }
}
