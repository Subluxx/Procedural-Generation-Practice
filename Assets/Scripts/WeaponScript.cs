using UnityEngine;


public class WeaponScript : MonoBehaviour
{
    private Rigidbody2D Rb { get; set; }
    
    public float acceleration;
    public float velocity;
    public Vector2 MousePosition; 
    
    // Start is called before the first frame update
    void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
    }

    public void FixedUpdate()
    {
        Vector2 aimDirection = MousePosition - Rb.position;
    }

    // Update is called once per frame
    void Update()
    {
        MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void ApplyForce(float force)
    {
        acceleration = Rb.AddForce(force);
    }
    
}
