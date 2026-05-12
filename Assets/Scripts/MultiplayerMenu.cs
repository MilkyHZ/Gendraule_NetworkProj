using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;


public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    private NetworkUI ui;
    [Header("UI References")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text statusText;

    [Header("Relay Settings")]
    [SerializeField] private int maxConnections = 4;

    private const string WebGLConnectionType = "wss";

    public async void Start()
    {
        ui = GetComponent<NetworkUI>();
        await InitializeUnityServices();
    }

    private async System.Threading.Tasks.Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            SetStatus("Unity Services ready.");
        }
        catch (Exception exception)
        {
            SetStatus("Unity Services failed to initialize.");
            Debug.LogError(exception);
        }
    }

    public async void StartHost()
    {
        try
        {
            SetStatus("Creating host session...");

            await InitializeUnityServices();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.UseWebSockets = true;

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, WebGLConnectionType)
            );

            bool started = NetworkManager.Singleton.StartHost();

            if (started)
            {
                if (joinCodeText != null)
                {
                    joinCodeText.text = "Join Code: " + joinCode;
                }

                SetStatus("Host started. Join Code: " + joinCode);
                HideUI();
            }
            else
            {
                SetStatus("Failed to start Host.");
            }
        }
        catch (Exception exception)
        {
            SetStatus("Host failed. Check Console.");
            Debug.LogError(exception);
        }
    }

    public async void StartClient()
    {
        try
        {
            SetStatus("Joining session...");

            await InitializeUnityServices();

            if (joinCodeInput == null)
            {
                SetStatus("Join Code Input is missing.");
                return;
            }

            string joinCode = joinCodeInput.text.Trim().ToUpper();

            if (string.IsNullOrEmpty(joinCode))
            {
                SetStatus("Please enter a join code.");
                return;
            }

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.UseWebSockets = true;

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(joinAllocation, WebGLConnectionType)
            );

            bool started = NetworkManager.Singleton.StartClient();

            if (started)
            {
                SetStatus("Client started.");
                HideUI();
            }
            else
            {
                SetStatus("Failed to start Client.");
            }
        }
        catch (Exception exception)
        {
            SetStatus("Client failed. Check join code and Console.");
            Debug.LogError(exception);
        }

    }

    public void StartServer()
    {
        SetStatus("Dedicated Server is not recommended for Unity Play WebGL.");
        Debug.LogWarning("StartServer is disabled for Unity Play WebGL. Use StartHost or StartClient instead.");
    }


    //public void StartHost()
    //{
    //    if (NetworkManager.Singleton.StartHost())
    //    {
    //        HideUI();
    //    }
    //}

    //public void StartClient()
    //{
    //    if (NetworkManager.Singleton.StartClient())
    //    {
    //        HideUI();
    //    } 
    //}

    //public void StartServer()
    //{
    //    if (NetworkManager.Singleton.StartServer())
    //    {
    //        HideUI();
    //    }
    //}

    private void HideUI()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
        ui.ShowGameUI();
    }

    private void SetStatus(string message)
    {
        Debug.Log(message);

        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}