using UnityEngine;
using System.Collections;

// Object will chase player on sight.
// If object also patrols will go back to patrol after getting to player.
public class ObstacleChase : MonoBehaviour
{
    
    public enum ChaseAxis { X, Y, Both }

    public string playerTag = "Player";
    public float sightRange = 5f;
    public ChaseAxis chaseAxis = ChaseAxis.X;
    public float chaseSpeed = 3f;
    public bool trackPlayerWhileChasing = false;
    public float chaseEndPause = 0.5f;
    public float chaseCooldown = 1f;

    private Transform player;
    private ObstaclePatrol patrol;
    private bool patrolSuspended;

    private bool isChasing;
    private bool isPausing;
    private Vector2 chaseTargetPos;
    private float nextChaseAllowedTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag).transform;
        patrol = GetComponent<ObstaclePatrol>();
    }

    void Update()
    {
        if (isPausing) return;

        if (!isChasing)
        {
            if (Time.time < nextChaseAllowedTime)
            {
                return;
            }
            if (Vector2.Distance(player.position, transform.position) > sightRange)
            {
                return;
            }

            BeginChase();
        }

        if (trackPlayerWhileChasing)
        {
            chaseTargetPos = player.position;
        }

        Vector2 pos = transform.position;
        Vector2 goal = pos;

        switch (chaseAxis)
        {
            case ChaseAxis.X:
                goal = new Vector2(chaseTargetPos.x, pos.y);
                break;
            case ChaseAxis.Y:
                goal = new Vector2(pos.x, chaseTargetPos.y);
                break;
            case ChaseAxis.Both:
                goal = chaseTargetPos;
                break;
        }

        transform.position = Vector2.MoveTowards(pos, goal, chaseSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, goal) < 0.05f)
        {
            StartCoroutine(EndChase());
        }
    }

    void BeginChase()
    {
        isChasing = true;
        chaseTargetPos = player.position;

        if (patrol != null && patrol.enabled)
        {
            patrol.enabled = false;
            patrolSuspended = true;
        }
    }

    IEnumerator EndChase()
    {
        isChasing = false;
        isPausing = true;
        nextChaseAllowedTime = Time.time + chaseEndPause + chaseCooldown;

        yield return new WaitForSeconds(chaseEndPause);

        isPausing = false;
        ResumePatrol();
    }

    void ResumePatrol()
    {
        if (patrolSuspended && patrol != null)
        {
            patrol.enabled = true;
        }
        patrolSuspended = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isChasing = false;
        isPausing = false;
        ResumePatrol();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
