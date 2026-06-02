using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerShooter : NetworkBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireCooldown = 0.2f;
    [SerializeField] KeyCode fireButton = KeyCode.Mouse0;

    private float nextFireTime;

    void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetKeyDown(fireButton) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            RequestShootServerRPC(firePoint.position, firePoint.forward);
        }
    }

    [ServerRpc]
    private void RequestShootServerRPC(Vector3 spawnPosition, Vector3 shootDirection, ServerRpcParams rpcParams = default)
    {
        GameObject projectileInstantiate = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection));

        projectileInstantiate.SetActive(false);

        NetworkObject networkObject = projectileInstantiate.GetComponent<NetworkObject>();
        ulong shooterClientId = rpcParams.Receive.SenderClientId;

        // 3. Safely initialize network state and ownership
        networkObject.SpawnWithOwnership(shooterClientId);

        // 4. Turn it on! It is now fully spawned and ready for physics collisions
        projectileInstantiate.SetActive(true);
    }
}