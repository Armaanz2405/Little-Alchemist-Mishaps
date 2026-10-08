using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RoomCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer[] rooms;
    [SerializeField] private Collider2D baseFloor;
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float smoothTime = 0.12f;

    private Camera view;
    private Collider2D targetCollider;
    private readonly Collider2D[] touching = new Collider2D[8];
    private bool onBaseFloor = true;
    private float baseY;
    private Vector2 velocity;

    private void Awake()
    {
        view = GetComponent<Camera>();
        baseY = transform.position.y;
        Array.Sort(rooms, (a, b) => a.bounds.center.x.CompareTo(b.bounds.center.x));
        if (target != null)
        {
            targetCollider = target.GetComponent<Collider2D>();
        }
    }

    private void Start()
    {
        if (target == null || rooms.Length == 0)
        {
            return;
        }
        FitToRooms();
        SetPosition(Goal());
    }

    private void LateUpdate()
    {
        if (target == null || rooms.Length == 0)
        {
            return;
        }

        FitToRooms();
        UpdateOnBaseFloor();
        Vector2 goal = Goal();
        Vector2 next = Vector2.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        if ((goal - next).sqrMagnitude < 0.000001f)
        {
            next = goal;
            velocity = Vector2.zero;
        }
        SetPosition(next);
    }

    // zoom in just enough that the view never goes past the edge of a background
    private void FitToRooms()
    {
        float size = float.MaxValue;
        foreach (SpriteRenderer room in rooms)
        {
            size = Mathf.Min(size, room.bounds.extents.y, room.bounds.extents.x / view.aspect);
        }
        view.orthographicSize = size;
    }

    // only slide between rooms while standing on the base floor, not on a shelf
    private void UpdateOnBaseFloor()
    {
        if (baseFloor == null || targetCollider == null)
        {
            onBaseFloor = true;
            return;
        }

        int count = targetCollider.GetContacts(touching);
        for (int i = 0; i < count; i++)
        {
            Collider2D other = touching[i];
            if (other != null && targetCollider.bounds.min.y >= other.bounds.max.y - 0.05f && (other == baseFloor || other.CompareTag("Ground")))
            {
                onBaseFloor = other == baseFloor;
                return;
            }
        }
    }

    private Vector2 Goal()
    {
        float targetX = target.position.x;

        for (int i = 0; i < rooms.Length - 1 && onBaseFloor; i++)
        {
            float line = (rooms[i].bounds.max.x + rooms[i + 1].bounds.min.x) / 2f;
            if (Mathf.Abs(targetX - line) < slideDistance)
            {
                float t = Mathf.SmoothStep(0f, 1f, (targetX - (line - slideDistance)) / (2f * slideDistance));
                return Vector2.Lerp(RoomView(rooms[i]), RoomView(rooms[i + 1]), t);
            }
        }

        SpriteRenderer current = rooms[0];
        foreach (SpriteRenderer room in rooms)
        {
            if (Mathf.Abs(room.bounds.center.x - targetX) < Mathf.Abs(current.bounds.center.x - targetX))
            {
                current = room;
            }
        }
        return RoomView(current);
    }

    private Vector2 RoomView(SpriteRenderer room)
    {
        Bounds bounds = room.bounds;
        float size = view.orthographicSize;
        return new Vector2(bounds.center.x, Mathf.Clamp(baseY, bounds.min.y + size, bounds.max.y - size));
    }

    private void SetPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}
