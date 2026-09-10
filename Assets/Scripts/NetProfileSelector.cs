using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;

public class NetProfileSelector : MonoBehaviour
{
    // Ces assets sont créés via Create > Multiplayer > NetworkSimulatorPresetAsset.
    [SerializeField] private NetworkSimulatorPresetAsset[] presets;
    [SerializeField] private NetworkSimulator simulator;

    private void Awake()
    {
        if (simulator != null) return;

        // Le simulateur est en général sur le même objet que le transport.
        var transport = FindAnyObjectByType<UnityTransport>();
        if (transport != null)
            simulator = transport.GetComponent<NetworkSimulator>();

        if (simulator == null)
            simulator = FindAnyObjectByType<NetworkSimulator>();
    }

    // Branchée sur le OnValueChanged du dropdown.
    public void ApplyPreset(int index)
    {
        if (simulator == null)
        {
            Debug.LogError("[NET] NetworkSimulator introuvable dans la scène.");
            return;
        }

        if (presets == null || index < 0 || index >= presets.Length || presets[index] == null)
        {
            Debug.LogWarning($"[NET] Preset {index} invalide ou non assigné.");
            return;
        }

        simulator.ChangeConnectionPreset(presets[index]);
        Debug.Log($"[NET] Profil actif : {presets[index].Name}");
    }
}