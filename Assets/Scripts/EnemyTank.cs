using UnityEngine;

public class EnemyTank : MonoBehaviour
{
public GameObject barrel;
    public GameObject missle;
    public Transform missleSpawnPoint;
    private float missleSpeed;

    public float speed = 6f;
    public float rotationBound = 120f;
    public float barrelMovement = 150f;
    public float thinkingDelay = 2.5f;
    public float waitingToShoot = 2f;

    private Rigidbody2D rb;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private float powerCount = 0f;
    private float actualSpeed;
    private float powerMultiplier = 0.02f;
    private float currentThinkingDelay = 0f;
    private float currentShootDelay = 0f;
    private bool hasShot = false;
    private bool grounded = false;
    private bool isAiming = false;
    private bool isThinking = false;
    private bool isWaitingToShoot = false;
    private bool activeAI;
    private bool stageOne;
    private bool stageTwo;
    private bool stageThree;
    private int missStreak = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (barrel != null)
        {
            currentAngle = barrel.GetComponent<Transform>().localEulerAngles.z;
            if (currentAngle > 180) currentAngle -= 360f; 
        }

        stageOne = true;
    }

    void Update()
    {
        if (isThinking)
        {
            currentThinkingDelay += Time.deltaTime;
            if (currentThinkingDelay >= thinkingDelay)
            {
                isThinking = false;
                AITurn();
            }
            return;
        }

        if (isAiming)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, barrelMovement * Time.deltaTime);
            barrel.GetComponent<Transform>().localRotation = Quaternion.Euler(0, 0, currentAngle);

            if (Mathf.Approximately(currentAngle, targetAngle))
            {
                isAiming = false;
                isWaitingToShoot = true;
                currentShootDelay = 0f;
            }
            return;
        }

        if (isWaitingToShoot)
        {
            currentShootDelay += Time.deltaTime;
            if (currentShootDelay >= waitingToShoot)
            {
                isWaitingToShoot = false;
                MissileShoot();
            }
            return;
        }

        if (activeAI) return;

        if (GameManager.Instance.activateAI)
        {
            isThinking = true;
            currentThinkingDelay = 0f;
            activeAI = true;
        }
    }

    public void AITurn()
    {
        if (GameManager.Instance.isPlayerTurn) return;
        
        if (stageOne)
        {
            int randomStageOne;
            if (missStreak >= 3) randomStageOne = Random.Range(1, 4);
            else randomStageOne = Random.Range(1, 7);
            
            if (randomStageOne > 1)
            {
                missStreak++;
                BarrelMovement();
                Debug.Log("Enemy AI is in stage one");
                return;
            }
            else
            {
                stageOne = false;
                stageTwo = true;
                missStreak = 0;
            }
        }
        
        if (stageTwo)
        {
            int randomStageTwo = Random.Range(1, 4);

            if (randomStageTwo > 1)
            {
                BarrelMovement();
                Debug.Log("Enemy AI is in stage two");
                return;
            }
            else
            {
                stageTwo = false;
                stageThree = true;
            }
        }
        
        if (stageThree)
        {
            BarrelMovement();
            Debug.Log("Enemy AI is in stage three");
            return;
        }
    }

    public void BarrelMovement()
    {
        if (!grounded) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector2 playerLocation = player.transform.position;
        Vector2 AILocation = transform.position;
        float distanceX = playerLocation.x - AILocation.x;
        
        if (stageOne)
        {
            targetAngle = Random.Range(-30f, 30f);
            powerCount = Random.Range(300f, 700f);
        }
        else if (stageTwo && player != null)
        {
            if (distanceX > 0) targetAngle = Random.Range(-40f, -10f);
            else targetAngle = Random.Range(10f, 40f);

            powerCount = Mathf.Clamp(Mathf.Abs(distanceX) * 25f + Random.Range(350f, 500f), 250f, 850f);
        }
        else if (stageThree && player != null)
        {
            if (distanceX > 0) targetAngle = Random.Range(-20f, -10f);
            else targetAngle = Random.Range(10f, 20f);
            
            powerCount = Mathf.Clamp(Mathf.Abs(distanceX) * 30f + Random.Range(400f, 500f), 350f, 850f);
        }
        else
        {
            targetAngle = 0f;
            powerCount = 500f;
        }

        bool wallAhead = true;
        int counter = 0;

        while(wallAhead && counter < 15)
        {
            barrel.GetComponent<Transform>().localRotation = Quaternion.Euler(0, 0, targetAngle);
            RaycastHit2D raycastHit = Physics2D.Raycast(missleSpawnPoint.position, missleSpawnPoint.up, 3f);

            if (raycastHit.collider != null && !raycastHit.collider.CompareTag("Player") && !raycastHit.collider.CompareTag("Enemy"))
            {
                if (distanceX < 0)
                {
                    targetAngle -= 4f;
                }
                else
                {
                    targetAngle += 4f;
                }
                counter++;
            }
            else wallAhead = false;
        }
        
        actualSpeed = powerCount * powerMultiplier;
        isAiming = true;
    }
    public void MissileShoot()
    {
        hasShot = true;
        GameObject missleShoot = Instantiate(missle, missleSpawnPoint.position, missleSpawnPoint.rotation);

        Missle missleScript = missleShoot.GetComponent<Missle>();
        missleScript.isShotByPlayer = false;

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
        activeAI = false;
        isAiming = false;
        isThinking = false;
        isWaitingToShoot = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground")) grounded = false;
        else grounded = true;
    }

}
