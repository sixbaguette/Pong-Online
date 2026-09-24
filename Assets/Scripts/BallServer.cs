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

    [Header("Rebond")]
    [SerializeField] private float spinStrenght = 0.75f;
    [SerializeField] private float minBounceX = 0.45f;

    private float lastHitTime;

    private Vector2 direction = Vector2.right;

    public NetworkVariable<Vector2> NetPos = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<Vector2> NetDirection = new NetworkVariable<Vector2>(
        Vector2.right, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    public float Speed => speed;
    public float YMax => yMax;
    public float YMin => yMin;
    public float XMax => xMax;
    public float XMin => xMin;

    public Vector2 BallHalfSize => ballHalf;
    public Vector2 PaddleHalfSize => paddleHalfSize;

    public float SpinStrenght => spinStrenght;
    public float MinBounceX => minBounceX;

    public override void OnNetworkSpawn()
    {
        MesureBall();
        MesurePaddle();

        if (!IsServer) return;

        transform.position = Vector3.zero;
        NetPos.Value = transform.position;
        NetDirection.Value = direction;
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
        CollideWithPaddle(ref p, RightPaddle, 1);

        transform.position = p;
        NetPos.Value = p;
        NetDirection.Value = direction;
    }

    private void CollideWithPaddle(ref Vector2 p, Transform pad, int side)
    {
        if (pad == null) return;

        if (Time.time - lastHitTime < 0.03f) return;

        if (Mathf.Sign(direction.x) != side) return;

        var paddleView = pad.GetComponent<PaddleView>();
        Transform padVis = paddleView != null ? paddleView.GetVisual() : null;

        Vector2 padCenter = pad.position;
        Vector2 padHalf = paddleHalfSize;

        var paddleSpriteRenderer = padVis != null ? padVis.GetComponent<SpriteRenderer>() : null;

        if (paddleSpriteRenderer != null)
        {
            padHalf = (Vector2)paddleSpriteRenderer.bounds.extents;
        }

        Vector2 ballCenter = p;

        float ballMinX = ballCenter.x - ballHalf.x;
        float ballMaxX = ballCenter.x + ballHalf.x;
        float ballMinY = ballCenter.y - ballHalf.y;
        float ballMaxY = ballCenter.y + ballHalf.y;

        float padMinX = padCenter.x - padHalf.x;
        float padMaxX = padCenter.x + padHalf.x;
        float padMinY = padCenter.y - padHalf.y;
        float padMaxY = padCenter.y + padHalf.y;

        bool overlap = ballMinY < padMaxY 
            && ballMinX < padMaxX
            && ballMaxY > padMinY
            && ballMaxX > padMinX;

        if (!overlap) return;

        // pen test
        float penLeft = Mathf.Abs(ballMaxX - padMinX);
        float penRight = Mathf.Abs(ballMinX - padMaxX);
        float penBottom = Mathf.Abs(ballMaxY - padMinY);
        float penTop = Mathf.Abs(ballMinY - padMaxY);

        float minPen = Mathf.Min(penLeft, penRight, penBottom, penTop);

        if (minPen == penLeft) ballCenter.x = padMinX - ballHalf.x;
        else if (minPen == penRight) ballCenter.x = padMaxX + ballHalf.x;
        else if (minPen == penBottom) ballCenter.y = padMinY - ballHalf.y;
        else ballCenter.y = padMaxY + ballHalf.y;

        direction.x = -direction.x;
        float centralDelta = Mathf.Clamp((ballCenter.y - padCenter.y) / padHalf.x, -1f, 1f);

        direction.y += centralDelta * spinStrenght;
        direction = direction.normalized;

        if (Mathf.Abs(direction.x) < MinBounceX)
        {
            float signX = Mathf.Sign(direction.x);
            if (signX == 0) signX = side > 0 ? 1f : -1f;

            float x = signX * minBounceX;
            float y = Mathf.Sign(direction.y) * Mathf.Sqrt(Mathf.Max(0, 1f - Mathf.Pow(minBounceX, 2)));
            direction = new Vector2(x, y);
            direction = direction.normalized;
        }

        p = ballCenter;
        lastHitTime = Time.time;
    }
}
