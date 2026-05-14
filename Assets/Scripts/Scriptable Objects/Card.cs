using UnityEngine;
using Unity.Netcode;
using Managers;

public class Card : NetworkBehaviour
{
    [SerializeField] SpriteRenderer cardFront;

    private NetworkVariable<int> m_CardID = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Subscribe to changes so the sprite updates when the ID is set
        m_CardID.OnValueChanged += OnCardIDChanged;

        // If the ID was already set before we spawned (late join/spawn), apply it now
        if (m_CardID.Value != -1)
        {
            ApplySprite(m_CardID.Value);
        }
    }

    public void SetCard(int cardID)
    {
        if (!IsServer) return;
        m_CardID.Value = cardID; // Changing this on server triggers the sync
    }

    private void OnCardIDChanged(int previousValue, int newValue)
    {
        ApplySprite(newValue);
    }

    private void ApplySprite(int id)
    {
        CardScriptables cardData = DeckManager.Instance.GetCardData(id);
        if (cardData != null)
        {
            cardFront.sprite = cardData.cardSprite;
        }
    }

    public override void OnNetworkDespawn()
    {
        m_CardID.OnValueChanged -= OnCardIDChanged;
    }

    public CardScriptables GetCardData()
    {
        return DeckManager.Instance.GetCardData(m_CardID.Value);
    }
}
