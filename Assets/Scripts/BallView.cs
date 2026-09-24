using Unity.Netcode;
using UnityEngine;

public class BallView : NetworkBehaviour
{
    [SerializeField] private BallServer source;
    [SerializeField] private Transform target;

    [SerializeField] private float smoothTime = 0.08f;

    [SerializeField] private bool predictOnClient = true;
    [SerializeField] private float reconciliationRate = 18f;
    [SerializeField] private float hardCorrectionDistance = 1.5f;

    private Vector3 m_vel;
    private Vector2 m_predictedPosition;
    private Vector2 m_predictedDirection;
    private Vector2 m_lastDirection;
    private bool m_predictionInitialized;
    private PaddleInput[] m_paddles;

    public Transform GetVisual() => target;

    public override void OnNetworkSpawn()
    {
        if (source == null) source = GetComponent<BallServer>();
        if (target == null) target = transform.Find("Visual");

        target.SetParent(null);
    }

    protected override void OnNetworkPostSpawn()
    {
        if (target != null)
        {
            target.position = transform.position;
        }

        if (source != null)
        {
            m_predictedPosition = source.NetPos.Value;
            m_predictedDirection = source.NetDirection.Value.normalized;
            m_lastDirection = m_predictedDirection;
            m_predictionInitialized = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (source == null) Destroy(target.gameObject);
    }

    public void Update()
    {
        if (source == null && target == null) return;

        Vector3 sourcePos = source.NetPos.Value;
        target.position = Vector3.SmoothDamp(target.position, sourcePos, ref m_vel, smoothTime);

        if (!m_predictionInitialized)
        {
            m_predictedPosition = source.NetPos.Value;
            m_predictedDirection = source.NetDirection.Value.normalized;
            m_lastDirection = m_predictedDirection;
            m_predictionInitialized = true;
        }

        Vector2 authoritativePosition = source.NetPos.Value;
        Vector2 authoritativeDirection = source.NetDirection.Value.normalized;
        Vector2 error = authoritativePosition - m_predictedPosition;

        if (error.sqrMagnitude > Mathf.Pow(hardCorrectionDistance, 2))
        {
            m_predictedPosition = authoritativePosition;
        }
        else
        {
            m_predictedPosition += error * (1f - Mathf.Exp(-reconciliationRate * Time.deltaTime));
        }

        if ((m_lastDirection - authoritativeDirection).sqrMagnitude > 0.0001f)
        {
            m_predictedDirection = authoritativeDirection;
        }

        m_lastDirection = authoritativeDirection;

        StepPrediction(Time.deltaTime);
        target.position = new Vector3(m_predictedPosition.x, m_predictedPosition.y, target.position.z);
    }

    private void StepPrediction(float dt)
    {
        m_predictedPosition += m_predictedDirection * source.Speed * dt;
        Vector2 ballHalf = source.BallHalfSize;

        if (m_predictedPosition.y + ballHalf.y > source.YMax)
        {
            m_predictedPosition.y = source.YMax - ballHalf.y;
            m_predictedDirection.y = -Mathf.Abs(m_predictedDirection.y);
        }
        else if (m_predictedPosition.y - ballHalf.y < source.YMin)
        {
            m_predictedPosition.y = source.YMin + ballHalf.y;
            m_predictedDirection.y = Mathf.Abs(m_predictedDirection.y);
        }

        if (m_predictedPosition.x + ballHalf.x > source.XMax)
        {
            m_predictedPosition.x = source.XMax - ballHalf.x;
            m_predictedDirection.x = -Mathf.Abs(m_predictedDirection.x);
        }
        else if (m_predictedPosition.x - ballHalf.x < source.XMin)
        {
            m_predictedPosition.x = source.XMin + ballHalf.x;
            m_predictedDirection.x = Mathf.Abs(m_predictedDirection.x);
        }

        EnsurePaddle();
        if (m_paddles == null) return;

        foreach (PaddleInput paddle in m_paddles)
        {
            PredictedPaddleContact(paddle, ballHalf);
        }
    }

    private void EnsurePaddle()
    {
        if (m_paddles == null || m_paddles.Length == 0)
        {
            m_paddles = FindObjectsByType<PaddleInput>(FindObjectsSortMode.None);
        }
    }

    private void PredictedPaddleContact(PaddleInput paddle, Vector2 ballHalf)
    {
        if (paddle == null) return;

        float side = Mathf.Sign(paddle.transform.position.x);
        if (Mathf.Sign(m_predictedDirection.x) != side) return;

        Vector2 paddleCenter = new Vector2(
            paddle.transform.position.x, 
            paddle.transform.position.y);

        Vector2 paddleHalf = source.PaddleHalfSize;

        PaddleView paddleView = paddle.GetComponent<PaddleView>();

        SpriteRenderer paddleSprite = paddleView != null && paddleView.GetVisual() != null 
            ? paddleView.GetVisual().GetComponent<SpriteRenderer>() : null;

        if (paddleSprite != null) paddleHalf = paddleSprite.bounds.extents;

        Vector2 min = m_predictedPosition - ballHalf;
        Vector2 max = m_predictedPosition + ballHalf;
        Vector2 paddleMin = paddleCenter - paddleHalf;
        Vector2 paddleMax = paddleCenter + paddleHalf;

        if (min.x >= paddleMax.x
            || max.x <= paddleMin.x
            || min.y >= paddleMax.y
            || max.y <= paddleMin.y)
        {
            return;
        }

        if (side < 0)
        {
            m_predictedPosition.x = paddleMax.x + ballHalf.x;
        }
        else
        {
            m_predictedPosition.x = paddleMin.x - ballHalf.x;
        }

        m_predictedDirection.x = -m_predictedDirection.x;

        float centerDelta = Mathf.Clamp((m_predictedPosition.y - paddleCenter.y) / paddleHalf.y, -1, 1);

        m_predictedDirection.y += centerDelta * source.SpinStrenght;
        m_predictedDirection = m_predictedDirection.normalized;

        if (Mathf.Abs(m_predictedDirection.x) < source.MinBounceX)
        {
            float x = Mathf.Sign(m_predictedDirection.x) * source.MinBounceX;
            float y = Mathf.Sign(m_predictedDirection.y) * 
                Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Pow(source.MinBounceX, 2)));

            m_predictedDirection = new Vector2(x, y);
        }
    }
}
