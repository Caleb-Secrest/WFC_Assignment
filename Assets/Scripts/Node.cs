using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Node", menuName = "Node")]
[System.Serializable]
public class Node : ScriptableObject
{
    [Header("Node Details")]
    public string Name;
    public GameObject Prefab;

    [Header("Connections")]
    public Connector up;
    public Connector down;
    public Connector left;
    public Connector right;

    public void ValidateNode()
    {
        if (up == null || down == null || left == null || right == null)
        {
            Debug.LogWarning($"Node {Name} has unassigned connectors.");
        }
    }
}

[System.Serializable]
public class Connector
{
    [Tooltip("List of nodes that can connect through this connector")]
    public List<Node> usableNodes = new List<Node>();
}
