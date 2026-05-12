using UnityEngine;
using Unity.Netcode;

public class LocalPlayerCameraTarget : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        
        TopDownCamera cameraFollow = Camera.main.GetComponent<TopDownCamera>();
        if (cameraFollow != null) { cameraFollow.SetTarget(transform); }
    }
}
