using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class AStarNodeOpt : MonoBehaviour
{
    public AStarNodeOpt parent;
    public List<AStarNodeOpt> neighbors;
    public float gCost, hCost, fCost;
    public NodeStatus status;
    AStarPathfindingOpt pathfinding;
    TextMeshPro text;
    void Start()
    {
        pathfinding = FindFirstObjectByType<AStarPathfindingOpt>();
        text = GetComponentInChildren<TextMeshPro>();
    }

    // O custo é calculado para todo o nó da lista aberta que possui o menor custo f
    public void CalculateCost(AStarNodeOpt startNode, AStarNodeOpt endNode, float parentCost)
    {
        gCost = parentCost + 1;
        Vector3 dir = endNode.transform.position - transform.position;
        hCost = dir.sqrMagnitude;        
        fCost = gCost + hCost;
        // gCost:F2 é o mesmo que escrever gCost.ToString("F2")
        text.text = $"G: {gCost:F2}\nH: {hCost:F2}\nF: {fCost:F2}";
    }

    public void SetMaterial(Material material)
    {
        GetComponent<Renderer>().material = material;
    }

    public void ResetNode()
    {
        parent = null;
        gCost = hCost = fCost = 0f;
        text.text = string.Empty;
        neighbors.Clear();
    }

    private void OnMouseDown()
    {
        if (status == NodeStatus.Obstacle) return;
        if (pathfinding.gameStatus == AStarPathfindingOpt.GameStatus.SelectStart)
        {
            pathfinding.SetStartNode(this);
        }
        else if(pathfinding.gameStatus == AStarPathfindingOpt.GameStatus.SelectEnd)
        {
            pathfinding.SetEndNode(this);
        }
        else { return; }
    }
}
