using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AStarPathfindingOpt : MonoBehaviour
{
    public List<AStarNodeOpt> openList;
    public List<AStarNodeOpt> closedList;
    public List<AStarNodeOpt> path;
    public List<AStarNodeOpt> grid;
    public AStarNodeOpt startNode;
    public AStarNodeOpt endNode;
    public AStarNodeOpt currentNode;
    public Material openMaterial, closedMaterial, pathMaterial, gridMaterial, startMaterial, endMaterial;
    public enum GameStatus { None, SelectStart, SelectEnd, Ready };
    public GameStatus gameStatus = GameStatus.None;
    public Toggle toggle;
    public TMPro.TextMeshProUGUI text;

    void Start()
    {
        grid = new List<AStarNodeOpt>(FindObjectsByType<AStarNodeOpt>(FindObjectsSortMode.None));
        text.text = "Selecione o nó inicial e o nó final entre os nós brancos. Se desenar visualizar passo a passo, marque a referida caixa.";
    }

    public void SelectStartNode()
    {
        if (gameStatus != GameStatus.None) return;

        gameStatus = GameStatus.SelectStart;
        text.text = "Clique no nó inicial (branco) para iniciar o caminho.";
    }

    public void SetStartNode(AStarNodeOpt node)
    {
        if (startNode != null) startNode.SetMaterial(gridMaterial);

        startNode = node;
        node.SetMaterial(startMaterial);
        gameStatus = GameStatus.SelectEnd;
        text.text = "Clique no nó final (branco) para finalizar o caminho.";
    }

    public void SetEndNode(AStarNodeOpt node)
    {
        if (endNode != null) endNode.SetMaterial(gridMaterial);
        endNode = node;
        node.SetMaterial(endMaterial);
        gameStatus = GameStatus.Ready;
        text.text = "Clique calcular rota para iniciar o caminho.";
    }

    public void StartAStar()
    {
        foreach (AStarNodeOpt node in grid)
        {
            if (node.status != NodeStatus.Obstacle)
            {
                node.SetMaterial(gridMaterial);
                node.ResetNode();
            }
        }

        if (startNode == null || endNode == null || gameStatus != GameStatus.Ready)
        {
            text.text = "Os nós inicial e final não foram definidos.";
            return;
        }

        currentNode = startNode;
        openList = new List<AStarNodeOpt>();
        closedList = new List<AStarNodeOpt>();
        path = new List<AStarNodeOpt>();

        openList.Add(currentNode);
        currentNode.CalculateCost(startNode, endNode, -1);

        if (toggle.isOn)
        {
            InvokeRepeating("CalculatePath", 0f, 0.5f);
        }
        else
        {
            CalculatePath();
        }
    }

    void CalculatePath()
    {
        foreach (AStarNodeOpt node in grid)
        {
            float deltaX = Mathf.Abs(node.transform.position.x - currentNode.transform.position.x);
            float deltaZ = Mathf.Abs(node.transform.position.z - currentNode.transform.position.z);

            if (Mathf.Approximately(deltaX + deltaZ, 1f))
            {
                if (closedList.Contains(node) || node.status == NodeStatus.Obstacle || node == startNode)
                {
                    continue;
                }

                node.CalculateCost(startNode, endNode, currentNode.gCost);
                node.parent = currentNode;
                openList.Add(node);
                node.SetMaterial(openMaterial);
            }
        }
        openList.Remove(currentNode);
        closedList.Add(currentNode);
        currentNode.SetMaterial(closedMaterial);

        if (openList.Count == 0)
        {
            text.text = "Não foi possível encontrar um caminho.";
            CancelInvoke("CalculatePath");
            gameStatus = GameStatus.None;
            return;
        }

        openList.Sort((node1, node2) => node1.fCost.CompareTo(node2.fCost));

        if (currentNode == endNode)
        {
            if (toggle.isOn)
            {
                CancelInvoke("CalculatePath");
            }
            SetPath(currentNode);
        }
        else
        {
            currentNode = openList[0];
            if (!toggle.isOn)
            {
                CalculatePath();
            }
        }
    }

    void SetPath(AStarNodeOpt lastNode)
    {
        foreach (AStarNodeOpt node in grid)
        {
            if (node.status != NodeStatus.Obstacle && node != startNode && node != endNode)
            {
                node.SetMaterial(gridMaterial);
            }
        }

        path.Add(lastNode);
        while (lastNode != startNode)
        {
            lastNode = lastNode.parent;
            path.Add(lastNode);
        }

        path.Reverse();

        foreach (AStarNodeOpt node in path)
        {
            if (node != startNode && node != endNode)
            {
                node.SetMaterial(pathMaterial);
            }
        }
        gameStatus = GameStatus.None;
    }
}
