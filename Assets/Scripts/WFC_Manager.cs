using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFC_Manager : MonoBehaviour
{
    public int mapSize;
    public Node[] nodeObj;
    public List<Unit> units;
    public Unit unitObj;

    int iter = 0;
    int nodeSize = 5;

    private void Awake()
    {
        units = new List<Unit>();
        MapGeneration();
    }

    private void MapGeneration()
    {
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                Unit unit = Instantiate(unitObj, new Vector3(x * nodeSize, 0, y * nodeSize), Quaternion.identity);
                unit.AddUnit(false, nodeObj);
                units.Add(unit);
            }
        }

        StartCoroutine(InitializeMap());
    }

    IEnumerator InitializeMap()
    {
        List<Unit> tempUnit = new List<Unit>(units);

        tempUnit.RemoveAll(u => u.collap);
        tempUnit.Sort((a, b) => { return a.nodeOptions.Length - b.nodeOptions.Length; });

        int length = tempUnit[0].nodeOptions.Length;
        int index = default;

        for (int i = 0; i < tempUnit.Count; i++)
        {
            if (tempUnit[i].nodeOptions.Length > length)
            {
                index = i;
                break;
            }
        }

        if (index > 0)
        {
            tempUnit.RemoveRange(index, tempUnit.Count - index);
        }

        yield return new WaitForSeconds(0.01f);

        CollapsedUnit(tempUnit);
    }

    private void CollapsedUnit(List<Unit> tempUnits)
    {
        int ranIndex = UnityEngine.Random.Range(0, tempUnits.Count);
        Unit unitToCollap = tempUnits[ranIndex];

        if (unitToCollap.nodeOptions.Length == 0)
        {
            Debug.LogWarning("No available node options for unit at position: " + unitToCollap.transform.position);
            return;
        }

        unitToCollap.collap = true;

        Node currentNode = unitToCollap.nodeOptions[UnityEngine.Random.Range(0, unitToCollap.nodeOptions.Length)];
        unitToCollap.nodeOptions = new Node[] { currentNode };

        Node pickedNode = unitToCollap.nodeOptions[0];
        Instantiate(pickedNode, unitToCollap.transform.position, Quaternion.identity);

        Generate();
    }

    private void Generate()
    {
        List<Unit> generateUnit = new List<Unit>(units);

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                var index = x + y * mapSize;
                if (units[index].collap)
                    generateUnit[index] = units[index];
                else
                {
                    List<Node> opt = new List<Node>();
                    foreach (Node n in nodeObj)
                    { opt.Add(n); }

                    if (y > 0)
                    {
                        Unit top = units[x + (y - 1) * mapSize];
                        List<Node> validOpt = new List<Node>();

                        foreach (Node possibleOpt in top.nodeOptions)
                        {
                            var valOpt = Array.FindIndex(nodeObj, obj => obj == possibleOpt);
                            var valid = nodeObj[valOpt].topNode;

                            validOpt = validOpt.Concat(valid).ToList();
                        }

                        ValidCheck(opt, validOpt);
                    }

                    if (x < mapSize - 1)
                    {
                        Unit right = units[x + 1 + y * mapSize].GetComponent<Unit>();
                        List<Node> validOpt = new List<Node>();

                        foreach (Node possibleOpt in right.nodeOptions)
                        {
                            var valOpt = Array.FindIndex(nodeObj, obj => obj == possibleOpt);
                            var valid = nodeObj[valOpt].leftNode;

                            validOpt = validOpt.Concat(valid).ToList();
                        }

                        ValidCheck(opt, validOpt);
                    }

                    if (y < mapSize - 1)
                    {
                        Unit bottom = units[x + (y + 1) * mapSize].GetComponent<Unit>();
                        List<Node> validOpt = new List<Node>();

                        foreach (Node possibleOpt in bottom.nodeOptions)
                        {
                            var valOpt = Array.FindIndex(nodeObj, obj => obj == possibleOpt);
                            var valid = nodeObj[valOpt].bottomNode;

                            validOpt = validOpt.Concat(valid).ToList();
                        }

                        ValidCheck(opt, validOpt);
                    }

                    if (x > 0)
                    {
                        Unit left = units[x - 1 + y * mapSize].GetComponent<Unit>();
                        List<Node> validOpt = new List<Node>();

                        foreach (Node possibleOpt in left.nodeOptions)
                        {
                            var valOpt = Array.FindIndex(nodeObj, obj => obj == possibleOpt);
                            var valid = nodeObj[valOpt].rightNode;

                            validOpt = validOpt.Concat(valid).ToList();
                        }

                        ValidCheck(opt, validOpt);
                    }

                    Node[] newNodes = new Node[opt.Count];

                    for (int i = 0; i < opt.Count; i++)
                    {
                        newNodes[i] = opt[i];
                    }

                    generateUnit[index].RemakeUnit(newNodes);
                }
            }
        }

        units = generateUnit;
        iter++;

        if (iter < mapSize * mapSize)
            StartCoroutine(InitializeMap());
    }

    private void ValidCheck(List<Node> optList, List<Node> validOpt)
    {
        for (int i = optList.Count - 1; i >= 0; i--)
        {
            var elm = optList[i];
            if (validOpt.Contains(elm))
                optList.RemoveAt(i);
        }
    }
}
