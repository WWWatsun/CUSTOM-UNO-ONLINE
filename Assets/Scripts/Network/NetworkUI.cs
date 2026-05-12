using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host (Server + Client)"))
                NetworkManager.Singleton.StartHost();

            if (GUILayout.Button("Start Client (Join Game)"))
                NetworkManager.Singleton.StartClient();
        }
        GUILayout.EndArea();
    }
}