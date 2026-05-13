using UnityEngine;
using Unity.Netcode;
using Managers;

public class Card : NetworkBehaviour
{
    [SerializeField] SpriteRenderer cardFront;

    public void SetCard(CardScriptables cardData)
    {
        cardFront.sprite = cardData.cardSprite;
    }
}
