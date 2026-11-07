using System;
using UnityEngine;

public class moveLadder : MonoBehaviour
{
    [SerializeField] private float slideSpeed;
    [SerializeField] private int direction;
    [SerializeField] private Vector2 offSet;
    private bool beingPushed = false;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private Transform transform;
    private Rigidbody2D playerRb;
    private Transform playerTrans;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        rb = GetComponentInParent<Rigidbody2D>();
        transform = this.GetComponent<Transform>();
        playerTrans = playerObj.GetComponent<Transform>();
        if (playerObj != null)
        {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }
    }
    void FixedUpdate()
    {
        if (!beingPushed) return;
        
        int xinput = (int)playerMovement.getMoveInput().x;
        if (xinput == direction)
        {
            rb.linearVelocity = new Vector2(direction * slideSpeed, 0);
            playerRb.linearVelocity = new Vector2(direction * slideSpeed, 0);
        }
        else if (xinput == 0)
        {
            rb.linearVelocity = Vector2.zero;
            playerRb.linearVelocity = Vector2.zero;
        }
    }

    public void StartPush()
    {
        Debug.Log("Starting push direction: " + direction);
        beingPushed = true;
        playerMovement.SetCurrentSpeed(slideSpeed);
        playerMovement.isPushing = true;
        playerTrans.position = (Vector2)transform.position + offSet;
    }

    public void StopPush()
    {
        Debug.Log("Stopping push");
        beingPushed = false;
        playerMovement.isPushing = false;
        playerMovement.SetCurrentSpeed(playerMovement.walkSpeed);
    }
    
    
}
