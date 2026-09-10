using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class UIManager : MonoBehaviour
{
    [SerializeField] private string adress = "127.0.0.1";
    [SerializeField] private ushort port = 7777;
    [SerializeField] private GameObject connectPanel;
    private NetworkManager m_networkManager;

    private NetworkManager Manager => m_networkManager != null ? m_networkManager : NetworkManager.Singleton != null ? NetworkManager.Singleton : FindAnyObjectByType<NetworkManager>();

    private UnityTransport Transport => (UnityTransport)Manager.NetworkConfig.NetworkTransport;


    private void Start()
    {
        m_networkManager = Manager;
        if (m_networkManager == null)
        {
            Debug.LogError("[NET] NetworkManager not found in the scene.");
            return;
        }

        m_networkManager.OnServerStarted += HandleServerStarting;
        m_networkManager.OnClientConnectedCallback += HandleClientConnected;
        m_networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    public void StartHost()
    {
        Transport.SetConnectionData("0.0.0.0", port);
        var manager = Manager;

        if (manager == null) return;

        bool ok = manager.StartHost();
        Debug.Log($"[NET] Host sur 0.0.0.0: {port} started: {ok}");
    }


    public void StartClient()
    {
        Transport.SetConnectionData(adress, port);
        var manager = Manager;

        if (manager == null) return;

        bool ok = manager.StartClient();
        Debug.Log($"[NET] Host sur {adress}: {port} started: {ok}");
    }

    private void HandleServerStarting() => ShowConnectPanel(false);

    private void HandleClientConnected(ulong clientId)
    {
        if (Manager != null && clientId == Manager.LocalClientId)
        {
            ShowConnectPanel(false);
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (Manager != null && clientId == Manager.LocalClientId)
        {
            ShowConnectPanel(true);
        }
    }

    private void ShowConnectPanel(bool visible)
    {
        if (connectPanel != null)
        {
            connectPanel.SetActive(visible);
        }
    }

    private void OnDestroy()
    {
        if (m_networkManager == null) return;

        m_networkManager.OnClientStarted -= HandleServerStarting;
        m_networkManager.OnClientConnectedCallback -= HandleClientConnected;
        m_networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
    }
}
