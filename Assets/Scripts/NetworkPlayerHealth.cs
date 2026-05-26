using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public NetworkVariable<int> currentHealth = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        currentHealth.OnValueChanged += OnHealthChange;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChange;
    }

    private void OnHealthChange(int oldValue, int newValue)
    {
        Debug.Log($"{gameObject.name} health change from {oldValue} -> {newValue}");
    }

    private void Respawn()
    {
        currentHealth.Value = maxHealth;
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        int randomIndex = Random.Range(0, spawns.Length);
        Transform selectedSpawn = spawns[randomIndex].transform;
        CharacterController controller = GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        transform.position = selectedSpawn.position;
        transform.rotation = selectedSpawn.rotation;

        if (controller != null)
            controller.enabled = true;
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value -= amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value <= 0)
        {
            Respawn();
        }
    }
}
