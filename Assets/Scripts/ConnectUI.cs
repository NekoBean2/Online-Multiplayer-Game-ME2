using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;

public class ConnectUI : MonoBehaviour
{
    public Button hostButton;
    public Button connectButton;
    public GameObject joinMenu;
    public TMP_InputField joinCodeInput;  
    public TMP_Text joinCodeLabel;         

    private const int MaxConnections = 4;  

    async void Start()
    {
        if (joinMenu == null)
            joinMenu = GameObject.Find("JoinMenu");

        joinCodeLabel.gameObject.SetActive(false);
        hostButton.interactable = false;
        connectButton.interactable = false;

        hostButton.onClick.AddListener(OnHostButtonClicked);
        connectButton.onClick.AddListener(OnConnectButtonClicked);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            hostButton.interactable = true;
            connectButton.interactable = true;
        }
        catch (Exception e)
        {
            Debug.Log($"Failed{e}");
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
    }

    private async void OnHostButtonClicked()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();

            joinCodeLabel.text = $"Join code: {joinCode}";
            joinCodeLabel.gameObject.SetActive(true);
            joinMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed{e}");
        }
    }

    private async void OnConnectButtonClicked()
    {
        string joinCode = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(joinCode))
        {
            return;
        }

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            joinMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed{e}");
        }
    }

    private void OnDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost)
            joinMenu.SetActive(true);
    }
}
