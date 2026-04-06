using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Platform : MonoBehaviour
{
    [SerializeField] private int platformId;
    public int PlatformId => platformId;
}