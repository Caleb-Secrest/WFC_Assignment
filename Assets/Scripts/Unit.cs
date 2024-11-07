using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool collap;
    public Node[] nodeOptions;

    public void AddUnit(bool collapsed, Node[] nodes)
    {
        collap = collapsed;
        nodeOptions = nodes;
    }

    public void RemakeUnit(Node[] nodes)
    {
        nodeOptions = nodes;
    }
}
