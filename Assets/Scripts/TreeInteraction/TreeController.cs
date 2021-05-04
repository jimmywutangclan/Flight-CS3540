using System;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    public GameObject treeColliderPrefab;

    public float makeColliderAngle = 10f;

    private List<GlobalTree> treesWithCollider;

    private float reach;

    private GameObject colliderParent;

    private float deltaTime;

    private ItemCollector itemCollector;
    private ProximityList closeTrees;

    private List<Tuple<int, int>> treePrototypeMapping;

    // Start is called before the first frame update
    void Start()
    {
        // Initializes basic variables
        List<GlobalTree> trees = new List<GlobalTree>();
        reach = GetComponent<PlayerController>().GetReach();
        colliderParent = GameObject.FindGameObjectWithTag("TreeColliderParent");
        treesWithCollider = new List<GlobalTree>();
        itemCollector = GetComponent<ItemCollector>();

        // Initializing mapping of tree type to the respective stripped tree type.
        treePrototypeMapping = new List<Tuple<int, int>>();
        // Maps broadleaf tree to stripped broadleaf.
        treePrototypeMapping.Add(new Tuple<int, int>(0, 1));
        treePrototypeMapping.Add(new Tuple<int, int>(2, 3));
        treePrototypeMapping.Add(new Tuple<int, int>(4, 5));

        // Gets number of sticks available for the trees
        int initialSticksAmount = treeColliderPrefab.GetComponent<ItemSource>().totalItems;

        // Does initialization work for all the trees in the map.
        // Loops through all terrain pieces.
        for (int terrainIdx = 0; terrainIdx < Terrain.activeTerrains.Length; terrainIdx++)
        {
            Terrain terrain = Terrain.activeTerrains[terrainIdx];

            // Loops through all trees on terrain piece.
            for (int treeIdx = 0; treeIdx < terrain.terrainData.treeInstanceCount; treeIdx++)
            {
                bool hasSticks = true;

                // Determines if the tree has sticks based on whether it is stripped or not.
                foreach (Tuple<int, int> tup in treePrototypeMapping)
                {
                    if (tup.Item2 == terrain.terrainData.treeInstances[treeIdx].prototypeIndex)
                    {
                        hasSticks = false;
                    }
                }

                // Creates trees, and determines whether they have sticks based on type.
                trees.Add(new GlobalTree(terrain.terrainData.treeInstances[treeIdx], terrain.GetPosition(),
                    terrain.terrainData.size, terrainIdx, treeIdx, hasSticks ? initialSticksAmount : 0));
            }
        }

        // Initializes proximity list. More optimized tree searching.
        if (trees.Count > 0)
            closeTrees = new ProximityList(trees, 10);
    }

    // Update is called once per frame
    void Update()
    {
        // Uncomment to show fps information.
        //deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //float fps = 1.0f / deltaTime;
        //Debug.Log(fps);

        GetTreeColliders();

        RemoveTreeColliders();
    }

    public void MineTree(Collider collider)
    {
        // Add item to inventory if possible
        bool stickMined = itemCollector.AddItemFromSource(collider.gameObject.GetComponent<IItemSource>());
        if (!stickMined)
        {
            // not possible; exit
            return;
        }

        // Gets the tree that was hit
        GlobalTree hitTree = collider.GetComponent<GlobalTreeInstance>().GetGlobalTree();
        hitTree.RemoveItem();

        // Change texture when there are no sticks available to be mined.
        if (hitTree.GetNumberItemsRemaining() == 0)
        {
            int newProtoIdx = -1;
            foreach (Tuple<int, int> tup in treePrototypeMapping)
            {
                if (tup.Item1 == hitTree.GetTreeInstance().prototypeIndex)
                {
                    newProtoIdx = tup.Item2;
                }
            }

            if (newProtoIdx != -1)
            {
                // Sets current terrainData to improve readability.
                TerrainData terrain = Terrain.activeTerrains[hitTree.GetTerrainIndex()].terrainData;

                // Copies current trees into a new array.
                TreeInstance[] currentTreeList = new TreeInstance[terrain.treeInstances.Length];
                System.Array.Copy(terrain.treeInstances,
                    currentTreeList, terrain.treeInstances.Length);

                // Modifies the hit tree.
                currentTreeList[hitTree.GetTreeIndex()].prototypeIndex = newProtoIdx;

                // Overwrites terrain data. NOTE: this is permenant. To redo, you will have to
                // delete the tree and replace it, or create a method that changes all trees of
                // the new type back to the old type.
                terrain.treeInstances = currentTreeList;
            }
            else
                Debug.Log("Did not find a prototype indx");

            Destroy(hitTree.GetCollider());
        }
    }

    private void GetTreeColliders()
    {
        if (closeTrees != null)
        {
            // Gets trees within reach.
            foreach (GlobalTree tree in closeTrees.GetTreesInReach(transform.position, reach))
            {
                // Checks if player is basically looking at the tree
                if (AngleBetweenObjects(tree.GetPosition()) <= makeColliderAngle * (1 / (Vector3.Distance(tree.GetPosition(), transform.position)))
                    * Mathf.Deg2Rad && !tree.HasCollider())
                {
                    // Creates collider for the tree
                    GameObject collider = Instantiate(treeColliderPrefab, tree.GetPosition(), Quaternion.Euler(0, 0, 0));

                    // Sets collider position and size. Multiplies height by 3 to match tree height.
                    // Adjust 3 if there are issues with different types of 3, and it can be made a variable
                    // for the prefab.
                    collider.transform.localScale = new Vector3(tree.GetTreeInstance().widthScale * 1.5f,
                        tree.GetTreeInstance().heightScale * 3, tree.GetTreeInstance().widthScale * 1.5f);
                    Vector3 temp = collider.transform.position;
                    temp.y += collider.transform.localScale.y;
                    collider.transform.position = temp;

                    collider.GetComponent<GlobalTreeInstance>().SetGlobalTree(tree);

                    // Sets collider parent.
                    collider.transform.parent = colliderParent.transform;

                    // Attaches the collider to the GlobalTree instance and list of currently
                    // marked trees.
                    treesWithCollider.Add(tree);
                    tree.SetHasCollider(true);
                    tree.SetCollider(collider);
                }
            }
        }
    }

    private void RemoveTreeColliders()
    {
        // Checks if the colliders should be removed.
        for (int i = treesWithCollider.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(transform.position, treesWithCollider[i].GetPosition()) > reach ||
                AngleBetweenObjects(treesWithCollider[i].GetPosition()) > makeColliderAngle * Mathf.Deg2Rad)
            {
                Destroy(treesWithCollider[i].GetCollider());
                treesWithCollider[i].SetHasCollider(false);
                treesWithCollider.RemoveAt(i);
            }
        }
    }

    private float AngleBetweenObjects(Vector3 tree)
    {
        Vector3 displacment = tree - transform.position;
        displacment.y = 0;

        Vector3 playerLook = transform.forward;
        playerLook.y = 0;

        return Quaternion.Angle(Quaternion.Euler(displacment.normalized), Quaternion.Euler(playerLook.normalized));
    }

    public void ReinitializeTrees()
    {
        for (int terrainIdx = 0; terrainIdx < Terrain.activeTerrains.Length; terrainIdx++)
        {
            Terrain terrain = Terrain.activeTerrains[terrainIdx];

            TreeInstance[] currentTreeList = new TreeInstance[terrain.terrainData.treeInstances.Length]; ;
            System.Array.Copy(terrain.terrainData.treeInstances,
                currentTreeList, terrain.terrainData.treeInstances.Length);

            // Loops through all trees on terrain piece.
            for (int treeIdx = 0; treeIdx < terrain.terrainData.treeInstanceCount; treeIdx++)
            {
                // Determines if the tree has sticks based on whether it is stripped or not.
                foreach (Tuple<int, int> tup in treePrototypeMapping)
                {
                    if (tup.Item2 == terrain.terrainData.treeInstances[treeIdx].prototypeIndex)
                    {
                        currentTreeList[treeIdx].prototypeIndex = tup.Item1;
                    }
                }
            }

            terrain.terrainData.treeInstances = currentTreeList;
        }
    }
}
