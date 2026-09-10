using Unity.Netcode;
using UnityEngine;

public class PaddleSpawnPosition : NetworkBehaviour
{
    [SerializeField] private float xOffset = 7f;

    public override void OnNetworkSpawn()
    {
        // seul le serv peut def la pos
        if (!IsServer) return;

        // pong a 2j le host gauche et le client a droite
        bool isHostPlayer = OwnerClientId == NetworkManager.ServerClientId;
        float xPosition = isHostPlayer ? -xOffset : xOffset;

        transform.position = new Vector3(xPosition, 0f, 0f);
    }
}
