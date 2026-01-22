using UnityEngine;

public class MovementIllusionHandler : MonoBehaviour
{
    [SerializeField] private Transform[] prefabs;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private float lifetime = 20f;
}