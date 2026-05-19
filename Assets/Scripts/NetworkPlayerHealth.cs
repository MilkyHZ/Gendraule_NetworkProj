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
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private void OnHealthChange(int oldValue, int newValue) 
    {
        maxHealth = oldValue;    
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value -= amount;
        currentHealth.Value = Mathf.Max(currentHealth.Value,0,maxHealth);

        if (currentHealth.Value <= 0) 
        {
            currentHealth.Value = 0;
        }
    }
    void Start()
    {
        OnNetworkSpawn();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
