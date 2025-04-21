using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Section Settings")]
    public GameObject[] sectionPrefabs;
    public float sectionLength = 50f;
    public int sectionsAhead = 2;
    public int sectionsBehind = 1;
    public int totalSections = -1; // -1 for endless

    [Header("Obstacle Settings")]
    public ObstacleSet[] obstacleSets;
    public float minObstacleDistance = 15f;
    public float maxObstacleDistance = 25f;
    public float obstacleProbability = 0.7f;

    [Header("Collectible Settings")]
    public CollectibleSet[] collectibleSets;
    public float minCollectibleDistance = 5f;
    public float maxCollectibleDistance = 15f;
    public float collectibleProbability = 0.5f;

    [Header("Enemy Settings")]
    public EnemySet[] enemySets;
    public float minEnemyDistance = 20f;
    public float maxEnemyDistance = 40f;
    public float enemyProbability = 0.3f;

    private List<GameObject> activeSections = new List<GameObject>();
    private int currentSectionIndex = 0;
    private Transform playerTransform;
    private float spawnZ = 0f;
    private float lastObstacleZ = 0f;
    private float lastCollectibleZ = 0f;
    private float lastEnemyZ = 0f;
    private bool isGenerating = false;

    [System.Serializable]
    public class ObstacleSet
    {
        public string name;
        public GameObject[] obstacles;
        public float probability = 1f;
        public float minDistance = 0f;
    }

    [System.Serializable]
    public class CollectibleSet
    {
        public string name;
        public GameObject[] collectibles;
        public float probability = 1f;
        public float minDistance = 0f;
    }

    [System.Serializable]
    public class EnemySet
    {
        public string name;
        public GameObject[] enemies;
        public float probability = 1f;
        public float minDistance = 0f;
    }

    void Start()
    {
        // Find player
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!isGenerating) return;
        
        // Check if we need to spawn new sections or remove old ones
        if (playerTransform != null)
        {
            // Player position in terms of sections
            int playerSectionIndex = Mathf.FloorToInt(playerTransform.position.z / sectionLength);
            
            // Spawn sections ahead if needed
            while (currentSectionIndex < playerSectionIndex + sectionsAhead)
            {
                SpawnSection();
                SpawnObstacles();
                SpawnCollectibles();
                SpawnEnemies();
                currentSectionIndex++;
            }
            
            // Remove sections behind if needed
            for (int i = activeSections.Count - 1; i >= 0; i--)
            {
                GameObject section = activeSections[i];
                int sectionIndex = Mathf.FloorToInt(section.transform.position.z / sectionLength);
                if (sectionIndex < playerSectionIndex - sectionsBehind)
                {
                    Destroy(section);
                    activeSections.RemoveAt(i);
                }
            }
        }
    }

    public void StartGenerator()
    {
        isGenerating = true;
        currentSectionIndex = 0;
        spawnZ = 0f;
        
        // Clear any existing sections
        foreach (GameObject section in activeSections)
        {
            Destroy(section);
        }
        activeSections.Clear();
        
        // Spawn initial sections
        for (int i = 0; i < sectionsAhead; i++)
        {
            SpawnSection();
            SpawnObstacles();
            SpawnCollectibles();
            SpawnEnemies();
            currentSectionIndex++;
        }
    }

    void SpawnSection()
    {
        // Check if we've reached the total number of sections
        if (totalSections > 0 && currentSectionIndex >= totalSections)
        {
            return;
        }
        
        // Select a random section prefab
        int randomIndex = Random.Range(0, sectionPrefabs.Length);
        GameObject sectionPrefab = sectionPrefabs[randomIndex];
        
        // Instantiate section
        GameObject section = Instantiate(sectionPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);
        section.transform.parent = transform;
        
        // Add to active sections
        activeSections.Add(section);
        
        // Update spawn position for next section
        spawnZ += sectionLength;
    }

    void SpawnObstacles()
    {
        if (obstacleSets.Length == 0) return;
        
        float sectionStartZ = (currentSectionIndex * sectionLength);
        float sectionEndZ = sectionStartZ + sectionLength;
        
        // Ensure minimum distance from last obstacle
        float startZ = Mathf.Max(sectionStartZ, lastObstacleZ + minObstacleDistance);
        
        while (startZ < sectionEndZ - 5f) // Leave some space at the end
        {
            // Decide whether to spawn an obstacle
            if (Random.value < obstacleProbability)
            {
                // Select obstacle set based on probability
                ObstacleSet selectedSet = GetRandomObstacleSet();
                if (selectedSet != null && selectedSet.obstacles.Length > 0)
                {
                    // Select random obstacle from set
                    int randomIndex = Random.Range(0, selectedSet.obstacles.Length);
                    GameObject obstaclePrefab = selectedSet.obstacles[randomIndex];
                    
                    // Determine lane position (left, center, right)
                    int lane = Random.Range(-1, 2);
                    
                    // Instantiate obstacle
                    Vector3 position = new Vector3(lane * 3f, 0.01f, startZ);
                    GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
                    obstacle.transform.parent = activeSections[activeSections.Count - 1].transform;
                    
                    // Update last obstacle position
                    lastObstacleZ = startZ;
                }
            }
            
            // Move to next potential obstacle position
            startZ += Random.Range(minObstacleDistance, maxObstacleDistance);
        }
    }

    void SpawnCollectibles()
    {
        if (collectibleSets.Length == 0) return;
        
        float sectionStartZ = (currentSectionIndex * sectionLength);
        float sectionEndZ = sectionStartZ + sectionLength;
        
        // Ensure minimum distance from last collectible
        float startZ = Mathf.Max(sectionStartZ, lastCollectibleZ + minCollectibleDistance);
        
        while (startZ < sectionEndZ - 5f) // Leave some space at the end
        {
            // Decide whether to spawn a collectible
            if (Random.value < collectibleProbability)
            {
                // Select collectible set based on probability
                CollectibleSet selectedSet = GetRandomCollectibleSet();
                if (selectedSet != null && selectedSet.collectibles.Length > 0)
                {
                    // Select random collectible from set
                    int randomIndex = Random.Range(0, selectedSet.collectibles.Length);
                    GameObject collectiblePrefab = selectedSet.collectibles[randomIndex];
                    
                    // Determine lane position (left, center, right)
                    int lane = Random.Range(-1, 2);
                    
                    // Instantiate collectible
                    Vector3 position = new Vector3(lane * 3f, 1f, startZ);
                    GameObject collectible = Instantiate(collectiblePrefab, position, Quaternion.identity);
                    collectible.transform.parent = activeSections[activeSections.Count - 1].transform;
                    
                    // Update last collectible position
                    lastCollectibleZ = startZ;
                }
            }
            
            // Move to next potential collectible position
            startZ += Random.Range(minCollectibleDistance, maxCollectibleDistance);
        }
    }

    void SpawnEnemies()
    {
        if (enemySets.Length == 0) return;
        
        float sectionStartZ = (currentSectionIndex * sectionLength);
        float sectionEndZ = sectionStartZ + sectionLength;
        
        // Ensure minimum distance from last enemy
        float startZ = Mathf.Max(sectionStartZ, lastEnemyZ + minEnemyDistance);
        
        while (startZ < sectionEndZ - 10f) // Leave more space at the end for enemies
        {
            // Decide whether to spawn an enemy
            if (Random.value < enemyProbability)
            {
                // Select enemy set based on probability
                EnemySet selectedSet = GetRandomEnemySet();
                if (selectedSet != null && selectedSet.enemies.Length > 0)
                {
                    // Select random enemy from set
                    int randomIndex = Random.Range(0, selectedSet.enemies.Length);
                    GameObject enemyPrefab = selectedSet.enemies[randomIndex];
                    
                    // Determine lane position (left, center, right)
                    int lane = Random.Range(-1, 2);
                    
                    // Instantiate enemy
                    Vector3 position = new Vector3(lane * 3f, 0.01f, startZ);
                    GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
                    enemy.transform.parent = activeSections[activeSections.Count - 1].transform;
                    
                    // Update last enemy position
                    lastEnemyZ = startZ;
                }
            }
            
            // Move to next potential enemy position
            startZ += Random.Range(minEnemyDistance, maxEnemyDistance);
        }
    }

    ObstacleSet GetRandomObstacleSet()
    {
        float totalProbability = 0f;
        foreach (ObstacleSet set in obstacleSets)
        {
            totalProbability += set.probability;
        }
        
        float random = Random.Range(0f, totalProbability);
        float currentProbability = 0f;
        
        foreach (ObstacleSet set in obstacleSets)
        {
            currentProbability += set.probability;
            if (random <= currentProbability)
            {
                return set;
            }
        }
        
        return obstacleSets[0];
    }

    CollectibleSet GetRandomCollectibleSet()
    {
        float totalProbability = 0f;
        foreach (CollectibleSet set in collectibleSets)
        {
            totalProbability += set.probability;
        }
        
        float random = Random.Range(0f, totalProbability);
        float currentProbability = 0f;
        
        foreach (CollectibleSet set in collectibleSets)
        {
            currentProbability += set.probability;
            if (random <= currentProbability)
            {
                return set;
            }
        }
        
        return collectibleSets[0];
    }

    EnemySet GetRandomEnemySet()
    {
        float totalProbability = 0f;
        foreach (EnemySet set in enemySets)
        {
            totalProbability += set.probability;
        }
        
        float random = Random.Range(0f, totalProbability);
        float currentProbability = 0f;
        
        foreach (EnemySet set in enemySets)
        {
            currentProbability += set.probability;
            if (random <= currentProbability)
            {
                return set;
            }
        }
        
        return enemySets[0];
    }
}