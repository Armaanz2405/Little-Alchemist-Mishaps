using UnityEngine;
using Unity.Cinemachine;

public class CameraZoneTransition : MonoBehaviour
{
    public CinemachineConfiner2D confiner;
    public PolygonCollider2D newBoundingPolygon;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            confiner.BoundingShape2D = newBoundingPolygon;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}