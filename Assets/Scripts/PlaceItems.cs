using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceItems : MonoBehaviour
{
    public GameObject[] itemTypes;
    public float[] numItemsPerType;
    public float[] yOffsetsPerType;
    public float ySpawnMin, ySpawnMax;

    private Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = GameObject.FindGameObjectWithTag("PickupParent").transform;

        //// Places the items for each type around the map.
        for (int i = 0; i < itemTypes.Length; i++)
        {
            for (int j = 0; j < numItemsPerType[i]; j++)
            {
                Vector3 worldPos = new Vector3(0, 0, 0);
                float height = 0;
                // Ensures the height is appropriate so its not on top the mountain or in the water.
                while (!(height >= ySpawnMin && height <= ySpawnMax))
                {
                    int terrIdx = Random.Range(0, Terrain.activeTerrains.Length);
                    Terrain terrain = Terrain.activeTerrains[terrIdx];
                    Vector3 terrPos = new Vector3(Random.Range(0f, 1f), 0, Random.Range(0f, 1f));
                    worldPos = Vector3.Scale(terrPos, terrain.terrainData.size) + terrain.GetPosition();
                    height = GetHeight(terrain, worldPos);
                    worldPos.y = height;
                }

                GameObject pickup = Instantiate(itemTypes[i], worldPos + Vector3.up * yOffsetsPerType[i], Quaternion.Euler(0, 0, 0));
                pickup.transform.parent = parent;
            }
        }
    }

    private float GetHeight(Terrain terrain, Vector3 worldPos)
    {
        return terrain.SampleHeight(worldPos) + terrain.transform.position.y;
    }
}
