using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

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
    [SerializeField] private VariableStorageBehaviour variableStorage;
    private float facing = 1f;
    
    private Rigidbody2D heldItem;
    private PlayerMovement playerMovement;

    public Transform HoldPoint => holdPoint;
    public float GrabRadius => grabRadius;
    public LayerMask GrabbableLayer => grabbableLayer;
    public Rigidbody2D HeldItem => heldItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        characterSprite = GetComponent<SpriteRenderer>();
        if (variableStorage == null) variableStorage = FindAnyObjectByType<VariableStorageBehaviour>();
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
            heldItem.transform.SetParent(transform);
            heldItem.freezeRotation = true;
            float half = hit.bounds.extents.y;
            heldItem.transform.localPosition = new Vector2(0f, 0.445f + half);

            if (heldItem.GetComponent<Potion>() != null) SetHoldingPotion(true);
        }
    }

    public void InteractInput(InputAction.CallbackContext context) {
        if (context.started) {
            // E talks to the customer instead of dropping
            if (heldItem != null && CustomerTalk.AnyCanTalk()) return;

            if (heldItem == null)
            {
                TryGrab();
            }
            else if (heldItem != null)
            {
                Release();
            }
            
            Collider2D hit = Physics2D.OverlapCircle(transform.position, grabRadius, interactableLayer);

            if (hit != null)
            {
                //in theory we would want to access this object's interact method for different uses
                //right now the book ui is hard coded bc that's the only interactable so far
                var interactableObj = hit.gameObject;
                bookUI.SetActive(true);
                InputManager.instanceInput.EnableUI();
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

            if (heldItem.GetComponent<Potion>() != null) SetHoldingPotion(false);
            heldItem = null;
        }
    }

    private void Release()
    {
        heldItem.transform.SetParent(null);

        heldItem.simulated = true;                
        heldItem.linearVelocity = Vector2.down;

        if (heldItem.GetComponent<Potion>() != null) SetHoldingPotion(false);
        heldItem = null;
    }

    public void ConsumeHeldItem()
    {
        if (heldItem == null) return;
        if (heldItem.GetComponent<Potion>() != null) SetHoldingPotion(false);
        Destroy(heldItem.gameObject);
        heldItem = null;
    }

    private void SetHoldingPotion(bool holding)
    {
        if (variableStorage != null) variableStorage.SetValue("$isHoldingPotion", holding);
    }
    
    public bool hasHeldItem()
    {
        if (heldItem != null)
        {
            return true;
        }
        else return false;
    }

    public float getFacing()
    {
        return facing;
    }
}
