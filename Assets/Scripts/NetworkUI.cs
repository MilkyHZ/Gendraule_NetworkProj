using UnityEngine;
using TMPro;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public GameObject gameUIPanel;
    public TextMeshProUGUI playerCountText;

    private int playerCount;
    
    public void Update()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
            playerCountText.text = $"{playerCount}";
        }
        else 
        {
            playerCountText.text = "";
        }
    }

    public void ShowGameUI() => Show(gameUIPanel);
    private void Show(GameObject obj)
    {
        obj.SetActive(true);
    }
    private void Hide(GameObject obj) 
    {
        obj.SetActive(false);
    }
}
