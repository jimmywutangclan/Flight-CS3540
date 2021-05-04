using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTree
{
    private TreeInstance tree;
    private Vector3 position;
    private bool hasCollider = false;
    private GameObject collider;

    private int terrainIdx, treeIdx;

    private int itemsRemaining;
    public GlobalTree(TreeInstance tree, Vector3 terrainGlobalPosition, Vector3 terrainSize,
        int terrainIdx, int treeIdx, int initialItemsAmount)
    {
        this.tree = tree;
        position = Vector3.Scale(tree.position, terrainSize) + terrainGlobalPosition;

        this.terrainIdx = terrainIdx;
        this.treeIdx = treeIdx;

        itemsRemaining = initialItemsAmount;
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public TreeInstance GetTreeInstance()
    {
        return tree;
    }

    public void SetHasCollider(bool hasCollider)
    {
        this.hasCollider = hasCollider;
    }

    public bool HasCollider()
    {
        return hasCollider;
    }

    public void SetCollider(GameObject collider)
    {
        this.collider = collider;
    }

    public GameObject GetCollider()
    {
        return collider;
    }

    public int GetTerrainIndex()
    {
        return terrainIdx;
    }

    public int GetTreeIndex()
    {
        return treeIdx;
    }

    public void RemoveItem()
    {
        itemsRemaining--;
    }

    public int GetNumberItemsRemaining()
    {
        return itemsRemaining;
    }
}