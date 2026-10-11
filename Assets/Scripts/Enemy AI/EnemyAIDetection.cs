using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EnemyAIDetection : MonoBehaviour
{
    [SerializeField] private Collider detectionCollider;
    [SerializeField] private float targetDetection = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] bool playerDetected;

    [Header("PathFinding")]
    [SerializeField] private UnityEngine.AI.NavMeshPath path;
    [SerializeField] private float updatePathRate = 0.2f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Vector3 lastTargetPosition;
    [SerializeField] private float pathupdateTimer;


    public float debugTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new UnityEngine.AI.NavMeshPath();


    }

    private void Update()
    {
        DebugAgentMovement();
        Detection();

        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (playerDetected && playerTransform != null)
        {
            agent.isStopped = false;

            pathupdateTimer += Time.deltaTime;

            if (pathupdateTimer >= updatePathRate)
            {
                CalculatePath();
                pathupdateTimer = 0f;
            }
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
        }


    }

    private void DebugAgentMovement()
    {
        debugTimer += Time.deltaTime;

        if (debugTimer < 1f)
            return;

        debugTimer = 0f;

        Debug.Log(
            $"Agent on NavMesh: {agent.isOnNavMesh} | " +
            $"Stopped: {agent.isStopped} | " +
            $"Speed setting: {agent.speed} | " +
            $"Actual velocity: {agent.velocity.magnitude:F2} | " +
            $"Remaining distance: {agent.remainingDistance:F2} | " +
            $"Path pending: {agent.pathPending} | " +
            $"Update Position: {agent.updatePosition}"
        );
    }

    private void Detection()
    {
        playerDetected = Physics.CheckSphere(transform.position, targetDetection, playerLayer, QueryTriggerInteraction.Collide);

        if (!playerDetected)
        {
            playerTransform = null;
            return;
        }

        // Find the player if we don't currently have a target.
        if (playerTransform == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                playerTransform = playerObject.transform;
        }
    }

    private void CalculatePath()
    {
        if (playerTransform == null)
        {
            return;
        }

        lastTargetPosition = playerTransform.position;

        bool pathFound = NavMesh.CalculatePath(
            transform.position,
            lastTargetPosition,
            NavMesh.AllAreas,
            path
        );

        if (!pathFound)
        {
            Debug.LogWarning("NavMesh path calculation failed.", this);
            return;
        }

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            if (path.corners.Length > 1)
            {
                bool pathSet = agent.SetPath(path);

                Debug.Log(
                    $"Path set: {pathSet} | " +
                    $"Corners: {path.corners.Length} | " +
                    $"Agent position: {agent.nextPosition} | " +
                    $"Enemy position: {transform.position} | " +
                    $"Destination: {path.corners[path.corners.Length - 1]}"
                );

                Debug.Log("following path");
            }
        }
        else
        {
            Debug.LogWarning(
                "Path is not complete. Status: " + path.status,
                this
            );
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetDetection);

        if (path != null && path.corners.Length > 1)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(
                    path.corners[i],
                    path.corners[i + 1]
                );
            }
        }
        Gizmos.DrawWireSphere(transform.position, targetDetection);
    }
}
