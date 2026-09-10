using Unity.Netcode;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject ballPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkObject ball = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);

        // spawn la balle sur tout les client
        ball.Spawn();
    }
}
