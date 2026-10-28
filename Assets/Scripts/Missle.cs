using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Missle : MonoBehaviour
{
    public ParticleSystem groundExplosion = default;
    public ParticleSystem tankExplosion = default;

    public bool isShotByPlayer = true;
    public float explosionRadius = 2f;
    private bool explosionTriggered = false;
    private SpriteRenderer renderer;


    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.enabled = true;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (explosionTriggered == true) return;

        bool playerCollision = collision.gameObject.CompareTag("Player");
        bool groundCollision = collision.gameObject.CompareTag("Ground");
        bool enemyCollision = collision.gameObject.CompareTag("Enemy");
        
        if (groundCollision || enemyCollision || playerCollision)
        {
            explosionTriggered = true;
            Explode();
        }
    }

    void Explode()
    {
        bool tankExploded = false;

        Vector2 tankLocation = transform.position;
        
        Collider2D[] colliderCheck = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D check in colliderCheck)
        {
            if (check.gameObject == gameObject || check.transform.IsChildOf(transform)) continue;

            if (check.CompareTag("Player"))
            {
                tankExploded = true;
                tankLocation = check.transform.position;
                renderer.enabled = false;
                GameManager.Instance.EnemyHit();
                break;
            }
            else if (check.CompareTag("Enemy"))
            {
                tankExploded = true;
                tankLocation = check.transform.position;
                renderer.enabled = false;
                GameManager.Instance.PlayerHit();
                break;
            }
        }

        if (tankExploded) Instantiate(tankExplosion, tankLocation, Quaternion.identity);
        else Instantiate(groundExplosion, transform.position, Quaternion.identity);

        if (!tankExploded) GameManager.Instance.IsPlayerTurn();
        Destroy(gameObject);
    }
}
