using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * A list-like structure that sorts trees based on their location. Allows
 * for O(approxItemsPerEntry) get calls, at the cost of memory (determined by
 * the the approxItemsPerEntry and the total number of trees).
 */
public class ProximityList
{
    private List<GlobalTree>[,] list;

    private float squareEdgeLength;
    private int count;
    private int xCellsCount;
    private int yCellsCount;
    private float xCellLen;
    private float yCellLen;

    private float xMin = float.MaxValue;
    private float yMin = float.MaxValue;
    private float xMax = float.MinValue;
    private float yMax = float.MinValue;


    /*
     * @param trees A list of tree instances
     * @param approxItemsPerEntry the approxiamte number of trees per cell for the proximity list,
     *        based on the average number per cell that you want
     */
    public ProximityList(List<GlobalTree> trees, float approxItemsPerEntry)
    {
        // Finds the rectangle boundaries for trees
        foreach (GlobalTree tree in trees)
        {
            if (tree.GetPosition().x < xMin)
                xMin = tree.GetPosition().x;
            if (tree.GetPosition().x > xMax)
                xMax = tree.GetPosition().x;
            if (tree.GetPosition().z < yMin)
                yMin = tree.GetPosition().z;
            if (tree.GetPosition().z > yMax)
                yMax = tree.GetPosition().z;
        }

        // Prevents trees from being on the max edge. Makes sorting easier.
        xMax += 1;
        yMax += 1;

        count = trees.Count;

        // Number of cells in the ProximityList
        int numSpots = (int)Mathf.Ceil(count / approxItemsPerEntry);

        float xRange = xMax - xMin;
        float yRange = yMax - yMin;

        // Gets the size of length in the world space that each cell covers if the cells were a square
        squareEdgeLength = Mathf.Sqrt(xRange * yRange / numSpots);

        // Number of x and y cells
        xCellsCount = (int)Mathf.Ceil(xRange / squareEdgeLength);
        yCellsCount = (int)Mathf.Ceil(yRange / squareEdgeLength);

        // Length in the world space that each cell covers
        xCellLen = (xMax - xMin) / xCellsCount;
        yCellLen = (yMax - yMin) / yCellsCount;

        list = new List<GlobalTree>[xCellsCount, yCellsCount];

        for (int x = 0; x < xCellsCount; x++)
        {
            for (int y = 0; y < yCellsCount; y++)
            {
                list[x, y] = new List<GlobalTree>();
            }
        }

        // Sorts each tree into the list.
        foreach (GlobalTree tree in trees)
        {
            float x = tree.GetPosition().x;
            float y = tree.GetPosition().z;
            int xInd = (int)((float)(x - xMin) / xCellLen);
            int yInd = (int)((float)(y - yMin) / yCellLen);
            list[xInd, yInd].Add(tree);
        }
    }


    /*
     * @param position Position of the player in the world space
     * @param reach The reach of the player
     * Returns a list of trees that are within the players reach in O(approxItemsPerEntry) time
     */
    public List<GlobalTree> GetTreesInReach(Vector3 position, float reach)
    {
        List<GlobalTree> returnList = new List<GlobalTree>();

        Vector2 playerPosition = new Vector2(position.x, position.z);

        // gets the location of the trees in the ProximityList
        int xInd = (int)((playerPosition.x - xMin) / xCellLen);
        int yInd = (int)((playerPosition.y - yMin) / yCellLen);

        // sets the boundaries in case the player is at the edge of the ProximityList cell
        int xStart = xInd - 1, xEnd = xInd + 2;
        int yStart = yInd - 1, yEnd = yInd + 2;

        // Ensures the start and finishes are in range;
        if (xStart < 0)
            xStart = 0;
        else if (xStart > xCellsCount)
            xStart = xCellsCount;
        
        if (xEnd > xCellsCount)
            xEnd = xCellsCount;

        if (yStart < 0)
            yStart = 0;
        else if (yStart > yCellsCount)
            yStart = yCellsCount;
        
        if (yEnd > yCellsCount)
            yEnd = yCellsCount;
        
        // Checks cells immediately around the players position.
        for (int x = xStart; x < xEnd; x++)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                foreach (GlobalTree tree in list[x, y])
                {
                    // If the tree is within the player's reach
                    if (Vector2.Distance(playerPosition, new Vector2(tree.GetPosition().x, tree.GetPosition().z)) < reach &&
                        tree.GetNumberItemsRemaining() > 0)
                    {
                        returnList.Add(tree);
                    }
                }
            }
        }

        return returnList;
    }
}
