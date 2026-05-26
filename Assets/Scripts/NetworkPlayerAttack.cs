using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerAttack : NetworkBehaviour
{
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    [SerializeField] private GameObject damagePopupPrefab;

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
        ulong attackerId = rpcParams.Receive.SenderClientId;
        
        Vector3 attackCenter = transform.position + transform.forward;
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRange, playerLayer);

        foreach (Collider hit in hits) 
        {
            if (hit.gameObject == gameObject) { continue; }

            NetworkPlayerHealth targetHealth = hit.GetComponent<NetworkPlayerHealth>();

            if (targetHealth != null) 
            {
                targetHealth.TakeDamage(attackDamage);
                SpawnPopupClientRpc(attackDamage, hit.transform.position);
                break;
            }
        }
    }


    [ClientRpc]
    private void SpawnPopupClientRpc(int damageAmount, Vector3 spawnPosition)
    {
        if (!IsOwner || damagePopupPrefab == null) return;

        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, Random.Range(-0.5f, 0.5f));
        GameObject popupGo = Instantiate(damagePopupPrefab, spawnPosition + offset, Quaternion.identity);
        DamagePopup popup = popupGo.GetComponent<DamagePopup>();

        if (popup != null)
        {
            popup.Setup(damageAmount, Color.red);
        }
    }


}
