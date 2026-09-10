using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleInput : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    // vitesse convergence visuel entre startp et targetp
    [SerializeField, Min(0f)] private float displayRate = 12f;

    private NetworkVariable<float> yPos = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // dernière intention envoyer par client

    private float m_lastSentDirection;

    // intention retenu par serv

    private float m_serverDirection;

    private void SendMyIntention()
    {
        if (!IsOwner || !IsClient) return;

        float direction = ReadVerticalIntent();

        if (Mathf.Approximately(direction, m_lastSentDirection)) return;

        m_lastSentDirection = direction;

        SubmitInputServerRpc(direction);
    }

    private void Update()
    {
        SendMyIntention();

        if (IsServer)
        {
            SimulateServer();
        }

        SmoothDisplay();
    }

    private void SmoothDisplay()
    {
        if (IsServer) return;

        float convergence = 1f - Mathf.Exp(-displayRate * Time.deltaTime);

        Vector3 pos = transform.position;

        float startPos = pos.y;
        float targetPos = yPos.Value;

        pos.y = Mathf.Lerp(startPos, targetPos, convergence);
        transform.position = pos;


    }

    private void SimulateServer()
    {
        if (Mathf.Approximately(m_serverDirection, 0f)) return;

        Vector3 pos = transform.position;
        pos.y += m_serverDirection * speed * Time.deltaTime;
        transform.position = pos;

        yPos.Value = pos.y;
    }

    [ServerRpc]
    private void SubmitInputServerRpc(float direction)
    {
        if (float.IsNaN(direction) || float.IsInfinity(direction)) return;

        m_serverDirection = Mathf.Clamp(direction, -1.0f, 1.0f);
    }

    private float ReadVerticalIntent()
    {
        // mettre input

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return 0f;

        float direction = 0f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            direction += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            direction -= 1f;
        }


        return Mathf.Clamp(direction, -1f, 1f);
    }
}
