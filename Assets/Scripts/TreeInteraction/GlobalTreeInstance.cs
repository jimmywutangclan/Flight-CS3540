using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTreeInstance : MonoBehaviour
{
    private GlobalTree tree;

    public GlobalTree GetGlobalTree()
    {
        return tree;
    }

    public void SetGlobalTree(GlobalTree tree)
    {
        this.tree = tree;
    }
}
