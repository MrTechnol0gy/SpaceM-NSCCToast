using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabAmount
{
    public GameObject prefab;
    public int amount;
}

public class RandomTerrainGenerator : MonoBehaviour
{
    public static RandomTerrainGenerator get;
    public int terrainRadius = 10;
    public List<PrefabAmount> prefabList;
    public float minHeight = 0.0f;
    public float maxHeight = 5.0f;
    public float minDistance = 1.0f;
    public float minScale = 10f;
    public float maxScale = 50f;

    private List<GameObject> spawnedPrefabs = new List<GameObject>();

    void Awake()
    {
        get = this;
    }

    public void GenerateTerrain()
    {
        int totalPrefabs = 0;
        foreach (PrefabAmount prefabAmount in prefabList)
        {
            totalPrefabs += prefabAmount.amount;
        }

        for (int i = 0; i < totalPrefabs; i++)
        {
            // Get a random prefab from the list, based on the amount set in the inspector
            GameObject prefab = null;
            int randomPrefabIndex = Random.Range(0, totalPrefabs);
            foreach (PrefabAmount prefabAmount in prefabList)
            {
                randomPrefabIndex -= prefabAmount.amount;
                if (randomPrefabIndex < 0)
                {
                    prefab = prefabAmount.prefab;
                    prefabAmount.amount--;
                    totalPrefabs--;
                    break;
                }
            }

            // Generate a random position within the radius
            Vector3 position = new Vector3(Random.Range(-terrainRadius, terrainRadius), 0, Random.Range(-terrainRadius, terrainRadius));

            // Check if there are any existing prefabs too close to the new position
            bool tooClose = false;
            foreach (GameObject existingPrefab in spawnedPrefabs)
            {
                float distance = Vector3.Distance(existingPrefab.transform.position, position);
                if (distance < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            // If the new position is too close to an existing prefab, try again with a new position
            if (tooClose)
            {
                i--;
                continue;
            }

            // Create the new prefab and add it to the list of spawned prefabs
            GameObject newPrefab = Instantiate(prefab, position, Quaternion.identity);
            newPrefab.transform.parent = transform;

            // Randomly scale the prefab
            float scale = Random.Range(minScale, maxScale);
            newPrefab.transform.localScale = new Vector3(scale, scale, scale);

            // Randomly rotate the prefab
            newPrefab.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            // Randomly adjust the y position of the prefab
            Vector3 prefabPosition = newPrefab.transform.position;
            prefabPosition.y = Random.Range(minHeight, maxHeight);
            newPrefab.transform.position = prefabPosition;

            // Add the new prefab to the list of spawned prefabs
            spawnedPrefabs.Add(newPrefab);
        }
    }
}
