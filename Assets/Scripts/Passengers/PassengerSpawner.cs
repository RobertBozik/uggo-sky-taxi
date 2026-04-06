using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject passengerPrefab;
    [SerializeField] private Platform[] platforms;

    public void SpawnPassenger()
    {
        if (platforms == null || platforms.Length < 2) return;

        int fromIndex = Random.Range(0, platforms.Length);
        int toIndex;
        do { toIndex = Random.Range(0, platforms.Length); } while (toIndex == fromIndex);

        Platform fromPlatform = platforms[fromIndex];
        Vector3 spawnPos = fromPlatform.transform.position + Vector3.up * 0.5f;

        GameObject paxObj = Instantiate(passengerPrefab, spawnPos, Quaternion.identity);
        Passenger pax = paxObj.GetComponent<Passenger>();
        pax.SetTarget(platforms[toIndex].PlatformId);
    }
}