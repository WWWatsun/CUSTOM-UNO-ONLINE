using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Managers;

namespace PlayerScripts
{
    public class PlayerHand : NetworkBehaviour
    {
        [SerializeField] List<CardScriptables> handList;
        [SerializeField] GameObject cardPrefab;
        [SerializeField] float cardSpacing = 1f;

        List<GameObject> cardObjects = new List<GameObject>();

        public void DrawCard(ulong playerID)
        {
            CardScriptables card = DeckManager.Instance.DrawCard();
            handList.Add(card);

            GameObject cardObject = Instantiate(cardPrefab, transform.position, transform.rotation);
            cardObjects.Add(cardObject);

            NetworkObject cardNetworkObject = cardObject.GetComponent<NetworkObject>();
            cardNetworkObject.SpawnWithOwnership(playerID);
            cardNetworkObject.TrySetParent(transform);

            cardObject.GetComponent<Card>().SetCard(card.cardID);

            UpdateHandLayout();

            Debug.Log($"Draw Card: {card.CardName()}");
        }

        public void DiscardCard(ulong cardNetworkId)
        {
            int index = cardObjects.FindIndex(c => c.GetComponent<NetworkObject>().NetworkObjectId == cardNetworkId);
            if (index < 0 || index >= handList.Count) return;

            CardScriptables cardToDiscard = handList[index];

            handList.RemoveAt(index);
            GameObject cardObject = cardObjects[index];
            cardObjects.RemoveAt(index);
            Destroy(cardObject);
            UpdateHandLayout();

            Debug.Log($"Discard Card: {cardToDiscard.CardName()}");
        }

        public int GetCardCount()
        {
            return handList.Count;
        }

        public List<CardScriptables> HandList
        {
            get { return handList; }
            set { handList = value; }
        }
        public CardScriptables GetCardAtIndex(int index)
        {
            if (index < 0 || index >= handList.Count) return null;
            return handList[index];
        }

        public CardScriptables GetCardByNetworkID(ulong networkID)
        {
            int index = cardObjects.FindIndex(c => c.GetComponent<NetworkObject>().NetworkObjectId == networkID);
            if (index < 0 || index >= handList.Count) return null;
            return handList[index];
        }

        public void UpdateSwappedHand(ulong ownerId)
        {
            foreach (var obj in cardObjects) Destroy(obj);
            cardObjects.Clear();

            foreach(CardScriptables card in handList)
            {
                GameObject cardObject = Instantiate(cardPrefab, transform.position, transform.rotation);
                cardObjects.Add(cardObject);

                NetworkObject cardNetworkObject = cardObject.GetComponent<NetworkObject>();
                cardNetworkObject.SpawnWithOwnership(ownerId);
                cardNetworkObject.TrySetParent(transform);

                cardObject.GetComponent<Card>().SetCard(card.cardID);
            }
            UpdateHandLayout();
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
