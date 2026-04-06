using UnityEngine;

public class Passenger : MonoBehaviour
{
    [SerializeField] private int targetPlatformId;
    public int TargetPlatformId => targetPlatformId;
    public bool IsWaiting { get; private set; } = true;

    public void SetTarget(int platformId)
    {
        targetPlatformId = platformId;
    }

    public void PickUp()
    {
        IsWaiting = false;
        gameObject.SetActive(false);
    }
}