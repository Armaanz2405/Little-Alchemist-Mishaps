using UnityEngine;
using UnityEngine.InputSystem;

public class GrabAndThrow : MonoBehaviour
{
    private Rigidbody2D rb;
    
    [SerializeField] private Transform holdPoint; 
    private SpriteRenderer characterSprite;

    [SerializeField] private float grabRadius = 0.6f;
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwHieght = 4f;

    [SerializeField] private GameObject bookUI;
    private float facing = 1f;
    
    private Rigidbody2D heldItem;
    private PlayerMovement playerMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        characterSprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(playerMovement.getMoveInput().x) > 0.1f)
        {
            facing = Mathf.Sign(playerMovement.getMoveInput().x);
            Vector3 p = holdPoint.localPosition;
            p.x = Mathf.Abs(p.x) * facing;
            characterSprite.flipX = (facing > 0) ? true : false;
            holdPoint.localPosition = p;
        }
        
    }
    
    public void GrabInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (heldItem == null)
            {
                TryGrab();
            }
            else if (heldItem != null)
            {
                Release();
            }
        }
    }
    private void TryGrab()
    {
        Collider2D hit = Physics2D.OverlapCircle(holdPoint.position, grabRadius, grabbableLayer);

        if (hit != null && hit.attachedRigidbody != null)
        {
            Debug.Log("Grabbed: " + hit.name);
            Debug.Log("HoldPoint world pos: " + holdPoint.position + " | parent: " + (holdPoint.parent ? holdPoint.parent.name : "none"));
            heldItem = hit.attachedRigidbody;
            
            heldItem.linearVelocity = Vector2.zero;
            heldItem.angularVelocity = 0f;
            heldItem.simulated = false;          
            heldItem.transform.SetParent(holdPoint);
            heldItem.freezeRotation = true;
            heldItem.transform.localPosition = Vector2.zero;
        }
    }

    public void InteractInput(InputAction.CallbackContext context) {
        if (context.started) {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, grabRadius, interactableLayer);

            if (hit != null)
            {
                //in theory we would want to access this object's interact method for different uses
                //right now the book ui is hard coded bc that's the only interactable so far
                var interactableObj = hit.gameObject;
                bookUI.SetActive(true);
            }
        }
    }

    public void Throw(InputAction.CallbackContext context)
    {
        if (heldItem != null && context.started)
        {
            heldItem.transform.SetParent(null);

            heldItem.simulated = true;
            Vector2 throwVector = new Vector2(facing,throwHieght);
            heldItem.linearVelocity =  throwVector * throwForce;

            heldItem = null;
        }
    }

    private void Release()
    {
        heldItem.transform.SetParent(null);

        heldItem.simulated = true;                
        heldItem.linearVelocity = Vector2.down;

        heldItem = null;
    }
    
    public bool hasHeldItem()
    {
        if (heldItem != null)
        {
            return true;
        }
        else return false;
    }
}
