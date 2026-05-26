using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerUI : NetworkBehaviour
{
    public TextMeshPro healthUI;

    public NetworkPlayerHealth playerHealth;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Player");


        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        }
    }

    private void Update()
    {
        if (!IsOwner || playerHealth == null) return;

        healthUI.text = "HP: " + playerHealth.currentHealth.Value.ToString();
    }
}
