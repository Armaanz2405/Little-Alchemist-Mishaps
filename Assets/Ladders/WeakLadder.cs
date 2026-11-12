using UnityEngine;

public class WeakLadder : MonoBehaviour
{
    [SerializeField] private int durabilaty = 1;
    [SerializeField] private bool beingClimbed = false;

    // Update is called once per frame
    void Update()
    {
        if (!beingClimbed && durabilaty <= 0)
        {
            Destroy(this.gameObject);
        }
    }
    public void LossDurabilty(){
        durabilaty -= 1;
    }

    public void BeingClimbed()
    {
        beingClimbed = true;
    }

    public void StopClimbed()
    {
        beingClimbed = false;
    }
    
}
