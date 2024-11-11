using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WFC_Manager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int nodeSize = 5;
    [SerializeField] private List<Node> nodes;

    public Slider gridSlider;
    public TMP_Text gridText;

    private Node[,] map;
    private Queue<Vector2Int> collap = new Queue<Vector2Int>();

    private List<GameObject> createdObjects = new List<GameObject>();

    private static readonly Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)
    };

    public void GenerateMap()
    {
        foreach (GameObject obj in createdObjects)
        {
            Destroy(obj);
        }
        createdObjects.Clear();

        map = new Node[width, height];
        collap.Clear();

        CreateMap();
    }

    private void Update()
    {
        width = (int)gridSlider.value;
        height = (int)gridSlider.value;
        gridText.text = gridSlider.value.ToString();
    }

    private void CreateMap()
    {
        collap.Clear();
        collap.Enqueue(new Vector2Int(width / 2, height / 2));

        while (collap.Count > 0)
        {
            Vector2Int current = collap.Dequeue();
            int x = current.x;
            int z = current.y;

            List<Node> posibleNodes = nodes.ToList();

            foreach (Vector2Int offset in offsets)
            {
                Vector2Int buddy = current + offset;

                if (WithinMap(buddy))
                {
                    Node buddyNode = map[buddy.x, buddy.y];

                    if (buddyNode != null)
                    {
                        EliminateNodes(posibleNodes, GetUsableNodesForDirection(buddyNode, offset));
                    }
                    else if (!collap.Contains(buddy))
                    {
                        collap.Enqueue(buddy);
                    }
                }
            }

            if (posibleNodes.Count == 0)
            {
                map[x, z] = nodes[0];
                Debug.Log($"Non-compatible Node at ({x}, {z})");
            }
            else
            {
                Node pickedNode = posibleNodes[Random.Range(0, posibleNodes.Count)];
                map[x, z] = pickedNode;

                Quaternion rot = GetNodeRotation(pickedNode, x, z);
                InstantiateNodePrefab(map[x, z], x, z, rot);
            }
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

    private void InstantiateNodePrefab(Node node, int x, int z, Quaternion rot)
    {
        GameObject newNode = Instantiate(node.Prefab, new Vector3(x * nodeSize, 0f, z * nodeSize), rot);
        createdObjects.Add(newNode);
    }

    private Quaternion GetNodeRotation(Node node, int x, int z)
    {
        if (node.Name == "Straight")
        {
            if (HorizontalPath(x, z))
            {
                return Quaternion.Euler(0, 90, 0);
            }
            else
            {
                return Quaternion.identity;
            }
        }
        else if (node.Name == "Turn")
        {
            bool upNodeExists = WithinMap(new Vector2Int(x, z + 1)) && map[x, z + 1] != null;
            bool downNodeExists = WithinMap(new Vector2Int(x, z - 1)) && map[x, z - 1] != null;
            bool leftNodeExists = WithinMap(new Vector2Int(x - 1, z)) && map[x - 1, z] != null;
            bool rightNodeExists = WithinMap(new Vector2Int(x + 1, z)) && map[x + 1, z] != null;

            if (upNodeExists && rightNodeExists)
            {
                return Quaternion.Euler(0, 90, 0);
            }
            else if (upNodeExists && leftNodeExists)
            {
                return Quaternion.Euler(0, 0, 0);
            }
            else if (downNodeExists && rightNodeExists)
            {
                return Quaternion.Euler(0, 180, 0);
            }
            else if (downNodeExists && leftNodeExists)
            {
                return Quaternion.Euler(0, 270, 0);
            }
        }

        return Quaternion.identity;
    }

    private bool HorizontalPath(int x, int z)
    {
        return (WithinMap(new Vector2Int(x - 1, z)) && map[x - 1, z] != null) ||
               (WithinMap(new Vector2Int(x + 1, z)) && map[x + 1, z] != null);
    }
}
