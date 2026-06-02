using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject damagePopupPrefab;

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

        if (spawns.Length == 0)
        {
            Debug.LogWarning("No SpawnPoints found in the scene! Add objects tagged 'SpawnPoint'.");
            return;
        }

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

    public void TakeDamage(int damageAmount, ulong attackerClientId)
    {
        if (!IsServer) return;

        currentHealth.Value -= damageAmount;

        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { attackerClientId } }
        };

        SpawnPopupClientRpc(damageAmount, transform.position, rpcParams);

        if (currentHealth.Value <= 0)
        {
            Respawn();
        }
    }

    [ClientRpc]
    private void SpawnPopupClientRpc(int damageAmount, Vector3 spawnPosition, ClientRpcParams rpcParams = default)
    {
        if (damagePopupPrefab == null) return;

        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, Random.Range(-0.5f, 0.5f));
        GameObject popupGo = Instantiate(damagePopupPrefab, spawnPosition + offset, Quaternion.identity);
        DamagePopup popup = popupGo.GetComponent<DamagePopup>();

        if (popup != null)
        {
            popup.Setup(damageAmount, Color.red);
        }
    }
}