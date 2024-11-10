using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFC_Manager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int nodeSize = 5;
    [SerializeField] private List<Node> nodes;

    private Node[,] map;
    private Queue<Vector2Int> toCollapse = new Queue<Vector2Int>();

    private static readonly Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)
    };

    private void Start()
    {
        map = new Node[width, height];
        CreateMap();
    }

    private void CreateMap()
    {
        toCollapse.Clear();
        toCollapse.Enqueue(new Vector2Int(width / 2, height / 2));

        while (toCollapse.Count > 0)
        {
            Vector2Int current = toCollapse.Dequeue();
            int x = current.x;
            int z = current.y;

            List<Node> possibleNodes = nodes.ToList();

            foreach (Vector2Int offset in offsets)
            {
                Vector2Int neighbor = current + offset;

                if (WithinMap(neighbor))
                {
                    Node neighborNode = map[neighbor.x, neighbor.y];

                    if (neighborNode != null)
                    {
                        EliminateNodes(possibleNodes, GetUsableNodesForDirection(neighborNode, offset));
                    }
                    else if (!toCollapse.Contains(neighbor))
                    {
                        toCollapse.Enqueue(neighbor);
                    }
                }
            }

            if (possibleNodes.Count == 0)
            {
                map[x, z] = nodes[0];
                Debug.Log($"Non-compatible Node at ({x}, {z})");
            }
            else
            {
                map[x, z] = possibleNodes[Random.Range(0, possibleNodes.Count)];
            }

            InstantiateNodePrefab(map[x, z], x, z);
        }
    }

    private bool WithinMap(Vector2Int coord) => coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;

    private void EliminateNodes(List<Node> possibleNodes, List<Node> usableNodes)
    {
        possibleNodes.RemoveAll(node => !usableNodes.Contains(node));
    }

    private List<Node> GetUsableNodesForDirection(Node node, Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return node.down.usableNodes;
        if (direction == new Vector2Int(0, -1)) return node.up.usableNodes;
        if (direction == new Vector2Int(1, 0)) return node.left.usableNodes;
        if (direction == new Vector2Int(-1, 0)) return node.right.usableNodes;
        return new List<Node>();
    }

    private void InstantiateNodePrefab(Node node, int x, int z)
    {
        Instantiate(node.Prefab, new Vector3(x * nodeSize, 0f, z * nodeSize), Quaternion.identity);
    }
}
