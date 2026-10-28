using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTank : MonoBehaviour
{
    public GameObject barrel;
    public GameObject missle;
    public Transform missleSpawnPoint;
    private float missleSpeed;

    public float speed = 6f;
    public float rotationBound = 120f;

    private Rigidbody2D rb;
    private float currentAngle = 0f;
    private float powerCount = 0f;
    private float powerMultiplier = 0.02f;
    private bool hasShot = false;
    private bool grounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (barrel != null)
        {
            currentAngle = barrel.GetComponent<Transform>().localEulerAngles.z;
            if (currentAngle > 180) currentAngle -= 360f; 
        }
    }

    void Update()
    {
        float rotationLimit = Mathf.Clamp(currentAngle, -rotationBound, rotationBound);
        PanelManager.Instance.Power(Mathf.Round(powerCount));
        PanelManager.Instance.Angle(Mathf.Round(rotationLimit));

        if (!GameManager.Instance.isPlayerTurn) return;

        float inputX = 0f;
        float inputY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputX = 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputY = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputY = -1f;
            if (Keyboard.current.spaceKey.wasPressedThisFrame && !hasShot && GameManager.Instance.isPlayerTurn) MissileShoot();
        }

        currentAngle -= inputX * speed * Time.deltaTime;

        barrel.GetComponent<Transform>().localRotation = Quaternion.Euler(0, 0, rotationLimit);

        if (inputY > 0 && powerCount < 1000)
        {
            powerCount += 125f * Time.deltaTime;
        }
        else if (inputY < 0 && powerCount > 0)
        {
            powerCount -= 125f * Time.deltaTime;
        }

        powerCount = Mathf.Clamp(powerCount, 0 , 999);
    }

    public void MissileShoot()
    {
        if (!grounded) return;
        hasShot = true;
        float actualSpeed = powerCount * powerMultiplier;

        GameObject missleShoot = Instantiate(missle, missleSpawnPoint.position, missleSpawnPoint.rotation);

        Missle missleScript = missleShoot.GetComponent<Missle>();
        missleScript.isShotByPlayer = true;

        Collider2D missleCollider = missleShoot.GetComponent<Collider2D>();
        Collider2D[] playerCollider = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D collider in playerCollider)
        {
            Physics2D.IgnoreCollision(missleCollider, collider);
        }

        Rigidbody2D missleRb = missleShoot.GetComponent<Rigidbody2D>();
        missleRb.linearVelocity = missleSpawnPoint.up * actualSpeed;
    }

    public void PlayerTurn()
    {
        hasShot = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground")) grounded = false;
        else grounded = true;
    }

}
