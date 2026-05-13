using System.Collections.Generic;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField] List<CardScriptables> handList;

        public void DrawCard()
        {
            CardScriptables card = DeckManager.Instance.DrawCard();
            handList.Add(card);
            Debug.Log($"Draw Card: {card.CardName()}");
        }
    }
}
