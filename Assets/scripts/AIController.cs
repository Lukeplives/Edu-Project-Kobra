using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AStarPathfindingOpt pathfinding;
    [SerializeField] private Camera mainCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float nodeReachDistance = 0.05f;

    [Header("References")]
    [SerializeField] private AStarPathfindingOpt pathfindingOpt;
    [SerializeField] private Transform target;

    private List<AStarNodeOpt> grid;
    private Coroutine pathRequestRoutine;
    private Coroutine movementRoutine;
    private AStarNodeOpt currentOriginNode;
    private AStarNodeOpt currentTargetNode;
    private AStarNodeOpt playerNode;
    private int requestVersion;

    private void Awake()
    {
        if (pathfinding == null)
        {
            pathfinding = FindFirstObjectByType<AStarPathfindingOpt>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (pathfinding == null)
        {
            Debug.LogError("AIController requires an AStarPathfindingOpt in the scene.", this);
            enabled = false;
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("AIController requires a camera reference to read mouse clicks.", this);
            enabled = false;
            return;
        }

        grid = pathfinding.grid != null && pathfinding.grid.Count > 0
            ? pathfinding.grid
            : new List<AStarNodeOpt>(FindObjectsByType<AStarNodeOpt>(FindObjectsSortMode.None));

        currentOriginNode = FindClosestNode(transform.position);
        currentTargetNode = currentOriginNode;
    }

    private void Update()
    {
        if (target == null)
            return;

        AStarNodeOpt targetNode = FindClosestNode(target.position);

        if (targetNode == null)
            return;

        if (targetNode != playerNode)
        {
            playerNode = targetNode;
            RequestPath(targetNode);
        }
    }

    private void RequestPath(AStarNodeOpt destinationNode)
    {
        requestVersion++;

        if (pathRequestRoutine != null)
        {
            StopCoroutine(pathRequestRoutine);
        }

        if (movementRoutine != null)
        {
            StopCoroutine(movementRoutine);
        }

        currentOriginNode = FindClosestNode(transform.position);
        currentTargetNode = currentOriginNode;

        pathRequestRoutine = StartCoroutine(
            RequestPathRoutine(destinationNode, requestVersion));
    }

    private IEnumerator RequestPathRoutine(AStarNodeOpt destinationNode, int version)
    {
        pathfinding.CancelInvoke();

        pathfinding.startNode = currentOriginNode;
        pathfinding.endNode = destinationNode;
        pathfinding.gameStatus = AStarPathfindingOpt.GameStatus.Ready;
        pathfinding.StartAStar();

        yield return null;

        while (version == requestVersion && pathfinding.gameStatus != AStarPathfindingOpt.GameStatus.None)
        {
            yield return null;
        }

        if (version != requestVersion)
        {
            yield break;
        }

        if (pathfinding.path == null || pathfinding.path.Count == 0)
        {
            currentTargetNode = currentOriginNode;
            yield break;
        }

        movementRoutine = StartCoroutine(FollowPathRoutine(new List<AStarNodeOpt>(pathfinding.path), version));
    }

    private IEnumerator FollowPathRoutine(List<AStarNodeOpt> path, int version)
    {
        if (path.Count == 0)
        {
            yield break;
        }

        currentOriginNode = path[0];
        currentTargetNode = currentOriginNode;

        for (int i = 1; i < path.Count; i++)
        {
            AStarNodeOpt nextNode = path[i];
            currentTargetNode = nextNode;

            Vector3 targetPosition = new Vector3(
                nextNode.transform.position.x,
                transform.position.y,
                nextNode.transform.position.z);

            while (version == requestVersion && Vector3.Distance(transform.position, targetPosition) > nodeReachDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }

            if (version != requestVersion)
            {
                yield break;
            }

            transform.position = targetPosition;
            currentOriginNode = nextNode;
        }

        currentTargetNode = currentOriginNode;
    }

    private AStarNodeOpt ResolveOriginNode()
    {
        if (movementRoutine != null && currentTargetNode != null)
        {
            return currentTargetNode;
        }

        if (currentOriginNode != null)
        {
            return currentOriginNode;
        }

        return FindClosestNode(transform.position);
    }

    private AStarNodeOpt FindClosestNode(Vector3 position)
    {
        if (grid == null || grid.Count == 0)
        {
            grid = new List<AStarNodeOpt>(FindObjectsByType<AStarNodeOpt>(FindObjectsSortMode.None));
        }

        AStarNodeOpt closestNode = null;
        float closestDistance = float.MaxValue;

        foreach (AStarNodeOpt node in grid)
        {
            if (node == null || node.status == NodeStatus.Obstacle)
            {
                continue;
            }

            Vector3 nodePosition = node.transform.position;
            float sqrDistance = (new Vector3(nodePosition.x, position.y, nodePosition.z) - position).sqrMagnitude;

            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestNode = node;
            }
        }

        return closestNode;
    }
}
