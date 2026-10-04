using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class AStarNodeOpt : MonoBehaviour
{
    public AStarNodeOpt parent;
    public float gCost, hCost, fCost;
    public NodeStatus status;

    public void CalculateCost(AStarNodeOpt startNode, AStarNodeOpt endNode, float parentCost)
    {
        gCost = parentCost + 1;
        Vector3 dir = endNode.transform.position - transform.position;
        hCost = dir.sqrMagnitude;        
        fCost = gCost + hCost;

    }

    public void SetMaterial(Material material)
    {
        GetComponent<Renderer>().material = material;
    }

    public void ResetNode()
    {
        parent = null;
        gCost = hCost = fCost = 0f;

    }
}
