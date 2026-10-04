using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float nodeReachDistance = 0.05f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform target;
    public Transform Target => target;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private AStarPathfindingOpt pathfinding;
    [SerializeField] private AStarNodeOpt[] patrolPoints;
    private Health health;

    [Header("Combat")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private Transform cannon;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float cannonRotationSpeed = 2f;

    [Header("Lost Target")]
    [SerializeField] private float lostTargetCheckInterval = 1f;


    private float nextLostTargetCheck;
    private Vector3 lastKnownPlayerPosition;
    private bool lostTargetShot;

    private float nextFireTime;

    private List<AStarNodeOpt> grid;
    private Coroutine pathRequestRoutine;
    private Coroutine movementRoutine;
    private AStarNodeOpt currentOriginNode;
    private AStarNodeOpt currentPatrolPoint;
    private AStarNodeOpt playerNode;
    private int requestVersion;

    private int currentPatrolIndex;
    private bool patrolActive;
    private bool searchFinished;
    

    private void Awake()
    {
        if (pathfinding == null)
        {
            pathfinding = FindFirstObjectByType<AStarPathfindingOpt>();
        }


        if(enemyData != null)
        {
            health = GetComponent<Health>();

            if(health != null)
            {
                health.maxHealth = enemyData.Heatlh;
            }
            
            fireRate = enemyData.fireRate;
            moveSpeed = enemyData.moveSpeed;
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

        grid = pathfinding.grid != null && pathfinding.grid.Count > 0
            ? pathfinding.grid
            : new List<AStarNodeOpt>(FindObjectsByType<AStarNodeOpt>(FindObjectsSortMode.None));

        currentOriginNode = FindClosestNode(transform.position);
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

        for (int i = 1; i < path.Count; i++)
        {
            AStarNodeOpt nextNode = path[i];

            Vector3 targetPosition = new Vector3(
                nextNode.transform.position.x,
                transform.position.y,
                nextNode.transform.position.z);

            while (version == requestVersion && Vector3.Distance(transform.position, targetPosition) > nodeReachDistance)
            {
                Vector3 direction = targetPosition - transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
                }

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

    public void StartPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        patrolActive = true;

        currentPatrolIndex = 0;
        currentPatrolPoint = patrolPoints[currentPatrolIndex];

        RequestPath(currentPatrolPoint);
    }

    public void NextPatrolPoint()
    {
        if (!patrolActive || patrolPoints == null || patrolPoints.Length == 0)
            return;

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;

        currentPatrolPoint = patrolPoints[currentPatrolIndex];

        RequestPath(currentPatrolPoint);
    }

    public void StopMovement()
    {
        patrolActive = false;

        requestVersion++;

        if (pathRequestRoutine != null)
            StopCoroutine(pathRequestRoutine);

        if (movementRoutine != null)
            StopCoroutine(movementRoutine);

        pathRequestRoutine = null;
        movementRoutine = null;
    }
    public bool HasReachedPatrolPoint()
    {
        if (currentPatrolPoint == null)
            return false;

        Vector3 targetPosition = currentPatrolPoint.transform.position;
        targetPosition.y = transform.position.y;

        return Vector3.Distance(
            transform.position,
            targetPosition) <= nodeReachDistance;
    }
    public void StartPursue()
    {
        if (target == null)
            return;

        playerNode = null;
        lostTargetShot = false;
        nextLostTargetCheck = 0f;

        lastKnownPlayerPosition = target.position;
    }

    public void UpdatePursue()
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

    public void StartAttack()
    {
        nextFireTime = 0f;
    }

    public void UpdateAttack()
    {
        if(target == null)
            return;
        
        RotateTowardsPlayer();
        RotateCannon();

        if(Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void RotateCannon()
    {
        float? angle = CalculateAngle(target.position, true);

        if (angle == null)
            return;

        Quaternion targetRotation = Quaternion.Euler(
            360f - angle.Value,
            0f,
            0f);

        cannon.localRotation = Quaternion.Slerp(
            cannon.localRotation,
            targetRotation,
            cannonRotationSpeed * Time.deltaTime);
    }
    public float? CalculateAngle(Vector3 targetPosition, bool low)
    {
        Vector3 targetDir = targetPosition - cannon.position;

        float y = targetDir.y;

        targetDir.y = 0f;

        float x = targetDir.magnitude - 1f;

        float gravity = 9.81f;
        float speedSqr = bulletSpeed * bulletSpeed;

        float underTheRoot =
            speedSqr * speedSqr -
            gravity * (gravity * x * x + 2f * y * speedSqr);

        if (underTheRoot < 0f)
            return null;

        float root = Mathf.Sqrt(underTheRoot);

        float highAngle = speedSqr + root;
        float lowAngle = speedSqr - root;

        float angle;

        if (low)
            angle = Mathf.Atan2(lowAngle, gravity * x) * Mathf.Rad2Deg;
        else
            angle = Mathf.Atan2(highAngle, gravity * x) * Mathf.Rad2Deg;

        return angle;
    }
    private void Fire()
    {
        Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);
    }
    private void RotateTowardsPlayer()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    public void UpdateLastKnownPosition()
    {
        if (target == null)
            return;

        lastKnownPlayerPosition = target.position;
    }

    public bool ShouldSearchForPlayer()
    {
        if (lostTargetShot)
            return false;

        if (Time.time < nextLostTargetCheck)
            return false;

        nextLostTargetCheck = Time.time + lostTargetCheckInterval;

        return true;
    }


    public void StartSearch()
    {
        searchFinished = false;

        FireAtPosition(lastKnownPlayerPosition);

        searchFinished = true;
    }

    public bool HasFinishedSearch()
    {
        return searchFinished;
    }
    private void FireAtPosition(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        float? angle = CalculateAngle(position, true);

        if (angle == null)
            return;

        cannon.localRotation = Quaternion.Euler(
            360f - angle.Value,
            0f,
            0f);

        GameObject shell = Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        Rigidbody rb = shell.GetComponent<Rigidbody>();

        rb.linearVelocity = bulletSpeed * cannon.forward;
    }

    

}
