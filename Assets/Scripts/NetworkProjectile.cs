using Unity.Netcode;
using UnityEngine;

public class NetworkProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 12.5f;
    [SerializeField] private float lifetime = 10.0f;
    [SerializeField] private int damage = 25;
    [SerializeField] private LayerMask playerLayer;

    private float projectileRadius = 1.0f;
    private float despawnTime;

    private void Awake()
    {
        // Calculate physical size automatically
        if (TryGetComponent<SphereCollider>(out SphereCollider sphereCollider))
        {
            projectileRadius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }
        else
        {
            projectileRadius = transform.localScale.x * 0.5f;
        }

        // Set lifetime immediately on awake as a safety fallback
        despawnTime = Time.time + lifetime;
    }

    void FixedUpdate()
    {
        // FIX: Allow BOTH Server and Clients to move the projectile forward locally.
        // This gives you lag-free projectile movement without needing a NetworkTransform!
        transform.position += transform.forward * speed * Time.fixedDeltaTime;

        // ONLY the server handles destruction and damage logic
        if (!IsServer) { return; }

        if (Time.time >= despawnTime)
        {
            SafeDespawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only evaluate hits on the server
        if (!IsServer) { return; }

        if (other.CompareTag("Player"))
        {
            // Optional: Prevent shooting yourself
            if (other.TryGetComponent<NetworkObject>(out NetworkObject hitNetworkObject))
            {
                if (hitNetworkObject.OwnerClientId == OwnerClientId) return;
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, projectileRadius, playerLayer);
            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<NetworkPlayerHealth>(out NetworkPlayerHealth targetHealth))
                {
                    targetHealth.TakeDamage(damage, OwnerClientId);
                }
            }

            SafeDespawn();
        }
    }

    private void SafeDespawn()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
        else
        {
            Destroy(gameObject); // Fallback if Netcode hasn't initialized it
        }
    }
}