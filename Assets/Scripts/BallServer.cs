using Unity.Netcode;
using UnityEngine;

public class BallServer : NetworkBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private Transform visual;

    [Header("Arène")]
    [SerializeField] private float yMax = 4.5f;
    [SerializeField] private float yMin = -4.5f;
    [SerializeField] private float xMax = 8f;
    [SerializeField] private float xMin = -8f;

    private Vector2 ballHalf;

    [Header("Paddle")]
    public Transform LeftPaddle;
    public Transform RightPaddle;
    [SerializeField] private Vector2 paddleHalfSize;

    private Vector2 direction = Vector2.right;

    public NetworkVariable<Vector2> NetPos = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        transform.position = Vector3.zero;
        NetPos.Value = transform.position;

        MesureBall();
        MesurePaddle();
    }

    private void MesureBall()
    {
        var sr = transform.GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            ballHalf = (Vector2)sr.bounds.extents;
            return;
        }

        ballHalf = new Vector2(0.125f, 0.125f);
    }

    private void MesurePaddle()
    {

    }

    private void Update()
    {
        if (!IsServer) return;

        GetWall();
    }

    private void GetWall()
    {
        Vector2 p = transform.position;
        p += direction * speed * Time.deltaTime;

        float top = p.y + ballHalf.y;
        float bottom = p.y - ballHalf.y;


        if (top > yMax)
        {
            p.y = yMax - ballHalf.y;
            direction.y = -Mathf.Abs(direction.y);
        }
        else if (bottom < yMin)
        {
            p.y = yMin + ballHalf.y;
            direction.y = Mathf.Abs(direction.y);
        }
        
        float right = p.x + ballHalf.x;    
        float left = p.x - ballHalf.x;

        if (right > xMax)
        {
            p.x = xMax - ballHalf.x;
            direction.x = -Mathf.Abs(direction.x);
        }
        else if (left < xMin)
        {
            p.x = xMin + ballHalf.x;
            direction.x = Mathf.Abs(direction.x);
        }

        // paddle
        CollideWithPaddle(ref p, LeftPaddle, -1);
        CollideWithPaddle(ref p, RightPaddle, -1);

        transform.position = p;
        NetPos.Value = p;
    }

    private void CollideWithPaddle(ref Vector2 p, Transform pad, int side)
    {
        if (pad == null) return;

        if (Mathf.Sign(direction.x) != side) return;

        var paddleView = pad.GetComponent<PaddleView>();
        Transform padVis = paddleView != null ? paddleView.GetVisual() : null;

        Vector2 pCenter = pad.position;
        Vector2 pHalf = paddleHalfSize;

        var paddleSpriteRenderer = padVis != null ? padVis.GetComponent<SpriteRenderer>() : null;
    }
}
