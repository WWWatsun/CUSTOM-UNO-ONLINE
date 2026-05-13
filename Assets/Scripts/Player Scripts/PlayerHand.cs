using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Managers;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace PlayerScripts
{
    public class PlayerHand : NetworkBehaviour
    {
        [SerializeField] List<CardScriptables> handList;
        [SerializeField] GameObject cardPrefab;
        [SerializeField] float cardSpacing = 1f;

        List<GameObject> cardObjects = new List<GameObject>();


        public void DrawCard()
        {
            CardScriptables card = DeckManager.Instance.DrawCard();
            handList.Add(card);

            GameObject cardObject = Instantiate(cardPrefab, transform.position, transform.rotation);
            cardObjects.Add(cardObject);

            NetworkObject cardNetworkObject = cardObject.GetComponent<NetworkObject>();
            cardNetworkObject.Spawn();
            cardNetworkObject.TrySetParent(transform);

            cardObject.GetComponent<Card>().SetCard(card.cardID);

            UpdateHandLayout();

            Debug.Log($"Draw Card: {card.CardName()}");
        }

        private void UpdateHandLayout()
        {
            int cardCount = cardObjects.Count;
            if (cardCount == 0) return;

            float totalWidth = (cardCount - 1) * cardSpacing;

            if (transform.position.x == 0)
            {
                float startX = transform.position.x - totalWidth / 2f;

                for (int i = 0; i < cardCount; i++)
                {
                    float target = startX + i * cardSpacing;

                    cardObjects[i].transform.position = new Vector3(
                        target,
                        transform.position.y,
                        transform.position.z
                    );
                }
            }
            else if (transform.position.z == 0)
            {
                float startZ = transform.position.z - totalWidth / 2f;

                for (int i = 0; i < cardCount; i++)
                {
                    float target = startZ + i * cardSpacing;

                    cardObjects[i].transform.position = new Vector3(
                        transform.position.x,
                        transform.position.y,
                        target
                    );
                }
            }
        }
    }
}
