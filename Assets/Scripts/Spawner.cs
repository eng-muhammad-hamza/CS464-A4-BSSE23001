using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Where")]
    [SerializeField] Transform player;
    [SerializeField] Transform[] zones;
    [SerializeField] float minDistanceFromPlayer = 3f;
    [SerializeField] float hazardClearance = 1f;
    [SerializeField] float minDistanceBetweenPickups = 2f;

    [Header("Coins")]
    [SerializeField] GameObject coinPrefab;
    [SerializeField] float interval = 1.5f;
    [SerializeField] int maxCoins = 8;

    [Header("Power-ups (the type is chosen at random)")]
    [SerializeField] GameObject[] powerUpPrefabs;
    [SerializeField] float powerUpInterval = 8f;
    [SerializeField] int maxPowerUps = 2;
    [SerializeField] float powerUpLifetimeSeconds = 15f;

    const int maxPlacementAttempts = 40;
    const float pickupHeight = 0.8f;

    Transform coinContainer;
    Transform powerUpContainer;
    SpawnBlocker[] spawnBlockers;

    void Start()
    {
        if (player == null || coinPrefab == null || zones == null || zones.Length == 0)
        {
            Debug.LogError("Spawner needs a Player, a Coin Prefab and at least one Zone.", this);
            enabled = false;
            return;
        }

        coinContainer = createContainer("SpawnedCoins");
        powerUpContainer = createContainer("SpawnedPowerUps");
        spawnBlockers = FindObjectsByType<SpawnBlocker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        InvokeRepeating(nameof(spawnCoin), 1f, Mathf.Max(0.1f, interval));
        if (hasAnyPowerUpPrefab())
            InvokeRepeating(nameof(spawnPowerUp), powerUpInterval, Mathf.Max(0.1f, powerUpInterval));
    }

    Transform createContainer(string containerName)
    {
        Transform container = new GameObject(containerName).transform;
        container.SetParent(transform, false);
        return container;
    }

    bool hasAnyPowerUpPrefab()
    {
        if (powerUpPrefabs == null) return false;
        foreach (GameObject prefab in powerUpPrefabs)
            if (prefab != null) return true;
        return false;
    }

    void spawnCoin()
    {
        if (coinContainer.childCount >= maxCoins) return;
        if (!tryFindFreePosition(out Vector3 spawnPosition)) return;
        Instantiate(coinPrefab, spawnPosition, Quaternion.identity, coinContainer);
    }

    void spawnPowerUp()
    {
        if (powerUpContainer.childCount >= maxPowerUps) return;
        GameObject chosenPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        if (chosenPrefab == null) return;
        if (!tryFindFreePosition(out Vector3 spawnPosition)) return;
        GameObject spawned = Instantiate(chosenPrefab, spawnPosition, Quaternion.identity, powerUpContainer);
        if (powerUpLifetimeSeconds > 0f) Destroy(spawned, powerUpLifetimeSeconds);
    }

    bool tryFindFreePosition(out Vector3 freePosition)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Transform zone = pickZone();
            Vector3 candidate = zone.TransformPoint(new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)));
            candidate.y = zone.position.y + pickupHeight;
            if (!isFreeSpot(candidate)) continue;
            freePosition = candidate;
            return true;
        }
        freePosition = default;
        return false;
    }

    Transform pickZone()
    {
        float total = 0f;
        foreach (Transform zone in zones) total += area(zone);
        float roll = Random.Range(0f, total);
        foreach (Transform zone in zones)
        {
            roll -= area(zone);
            if (roll <= 0f) return zone;
        }
        return zones[zones.Length - 1];
    }

    static float area(Transform zone) => Mathf.Abs(zone.lossyScale.x * zone.lossyScale.z);

    bool isFreeSpot(Vector3 candidate)
    {
        if (flatDistance(candidate, player.position) < minDistanceFromPlayer) return false;
        foreach (SpawnBlocker blocker in spawnBlockers)
            if (blocker != null && blocker.covers(candidate, hazardClearance)) return false;
        return !isTooCloseToAnyChild(coinContainer, candidate) && !isTooCloseToAnyChild(powerUpContainer, candidate);
    }

    bool isTooCloseToAnyChild(Transform container, Vector3 candidate)
    {
        foreach (Transform child in container)
            if (flatDistance(candidate, child.position) < minDistanceBetweenPickups) return true;
        return false;
    }

    static float flatDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    void OnDrawGizmos()
    {
        if (zones == null) return;
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
        foreach (Transform zone in zones)
        {
            if (zone == null) continue;
            Gizmos.matrix = Matrix4x4.TRS(zone.position, zone.rotation, new Vector3(zone.lossyScale.x, 0.05f, zone.lossyScale.z));
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
