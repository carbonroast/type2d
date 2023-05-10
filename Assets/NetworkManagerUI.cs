using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        serverButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
    }

    
}
