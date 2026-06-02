using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerAttack : NetworkBehaviour
{
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(attackKey))
        {
            RequestAttackServerRPC();
        }
    }

    [ServerRpc]
    private void RequestAttackServerRPC(ServerRpcParams rpcParams = default)
    {
        Vector3 attackCenter = transform.position + transform.forward;

        Damage(attackCenter, attackRange, playerLayer, rpcParams.Receive.SenderClientId);
    }

    public void Damage(Vector3 position, float radius, LayerMask layerMask, ulong attackerId)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius, layerMask);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) { continue; }

            if (hit.TryGetComponent<NetworkPlayerHealth>(out NetworkPlayerHealth targetHealth))
            {
                targetHealth.TakeDamage(attackDamage, attackerId);
                break;
            }
        }
    }
}