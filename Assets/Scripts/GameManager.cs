using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum Phase { Menu, GameStart, GameOver }
    public Phase CurrentPhase;

    public GameObject playerTank;
    public GameObject enemyTank;

    public bool isPlayerTurn;
    public bool activateAI;
    public bool missileRefill;
    public bool enemyAIWin;

    private GameObject activePlayerTank;
    private GameObject activeEnemyTank;

    private int playerScore;
    private int enemyScore;


    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void ChangePhase(Phase newPhase)
    {
        CurrentPhase = newPhase;

        switch (newPhase)
        {
            case Phase.Menu:
                Time.timeScale = 0f;
                break;  

            case Phase.GameStart:
                Time.timeScale = 1f;
                break;

            case Phase.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    void Start()
    {
        isPlayerTurn = true;
    }

    public void SpawnTanks()
    {
        while (true)
        {
            float randomX1 = Random.Range(-8.5f, 8.5f);
            float randomX2 = Random.Range(-8.5f, 8.5f);


            if (Mathf.Abs(randomX1 - randomX2) >= 8f)
            {
                Vector3 spawnPositionPlayer = new Vector3(randomX1, 8f, 1.5f);
                Vector3 spawnPositionEnemy = new Vector3(randomX2, 8f, 1.5f);

                activePlayerTank = GameObject.Instantiate(playerTank, spawnPositionPlayer, Quaternion.identity);
                activeEnemyTank = GameObject.Instantiate(enemyTank, spawnPositionEnemy, Quaternion.identity);
                break;
            }
            else
            {
                Debug.Log("Spawn was to close to eachother");
                continue;
            }
        }
    }

    public void PlayerHit()
    {
        playerScore++;
        PanelManager.Instance.PlayerScore(playerScore);

        if (playerScore >= 10)
        {
            enemyAIWin = false;
            PanelManager.Instance.GameOver();
        }

        GameObjectRespawn();
    }

    public void EnemyHit()
    {
        enemyScore++;
        PanelManager.Instance.EnemyScore(enemyScore);

        if (enemyScore >= 10)
        {
            enemyAIWin = true;
            PanelManager.Instance.GameOver();
        }

        GameObjectRespawn();
    }

    public void IsPlayerTurn()
    {
        isPlayerTurn = !isPlayerTurn;

        if (isPlayerTurn)
        {
            activePlayerTank.GetComponent<PlayerTank>().PlayerTurn();
            activeEnemyTank.GetComponent<EnemyTank>().PlayerTurn();

            PanelManager.Instance.PlayerTurn();
            activateAI = false;
        }
        else
        {
            activateAI = true;
            PanelManager.Instance.EnemyTurn();
        }
    }

    public void GameObjectRespawn()
    {
        Destroy(activePlayerTank.gameObject);
        Destroy(activeEnemyTank.gameObject);
        SpawnTanks();

        isPlayerTurn = true;
        activateAI = false;
        PanelManager.Instance.PlayerTurn();
    }
}
