using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    private VisualElement mainMenuContainer;
    private VisualElement scoreContainer;
    private VisualElement gameOverContainer;

    private Button startButton;
    private Button exitButton;
    private Button playAgainButton;

    private Label gameOverLabel;
    private Label gameOverScoreLabel;
    private Label playerScoreLabel;
    private Label enemyScoreLabel;
    private Label powerLabel;
    private Label angleLabel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        PanelRenderer renderer = GetComponent<PanelRenderer>();
        renderer.RegisterUIReloadCallback(ActivateUI);

        GameManager.Instance.ChangePhase(GameManager.Phase.Menu);
    }

    private void ActivateUI(PanelRenderer renderer, VisualElement root)
    {
        mainMenuContainer = root.Q<VisualElement>("main-menu-container");
        scoreContainer = root.Q<VisualElement>("score-container");
        gameOverContainer = root.Q<VisualElement>("game-over-container");

        startButton = root.Q<Button>("play-button");
        exitButton = root.Q<Button>("exit-button");
        playAgainButton = root.Q<Button>("play-again-button");

        gameOverLabel = root.Q<Label>("game-over-label");
        gameOverScoreLabel = root.Q<Label>("game-over-score");
        playerScoreLabel = root.Q<Label>("player-score-label");
        enemyScoreLabel = root.Q<Label>("enemy-score-label");
        powerLabel = root.Q<Label>("power-label");
        angleLabel = root.Q<Label>("angle-label");

        mainMenuContainer.style.display = DisplayStyle.Flex;
        scoreContainer.style.display = DisplayStyle.None;
        gameOverContainer.style.display = DisplayStyle.None;

        startButton.clicked += OnStartClicked;
        exitButton.clicked += OnExitClicked;
        playAgainButton.clicked += OnPlayAgainClicked;

        playerScoreLabel.style.color = Color.darkGreen;
    }

    public void OnStartClicked()
    {
        mainMenuContainer.style.display = DisplayStyle.None;
        scoreContainer.style.display = DisplayStyle.Flex;

        GameManager.Instance.ChangePhase(GameManager.Phase.GameStart);
        GameManager.Instance.SpawnTanks();
    }

    public void OnExitClicked()
    {
        Application.Quit();
        Debug.Log("Player exited the game");
    }

    public void OnPlayAgainClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        OnStartClicked();
    }

    public void Power(float power)
    {
        powerLabel.text = $"{power}";
    }

    public void Angle(float angle)
    {
        angleLabel.text = $"{angle}";
    }

    public void PlayerScore(int scoreIncrease)
    {
        playerScoreLabel.text = $"{scoreIncrease}";
    }

    public void EnemyScore(int scoreIncrease)
    {
        enemyScoreLabel.text = $"{scoreIncrease}";
    }

    public void PlayerTurn()
    {
        if (playerScoreLabel != null && enemyScoreLabel != null)
        {
            playerScoreLabel.style.color = Color.darkGreen;
            enemyScoreLabel.style.color = Color.white;
        }
        
    }

    public void EnemyTurn()
    {
        if (playerScoreLabel != null && enemyScoreLabel != null)
        {
            enemyScoreLabel.style.color = Color.darkGreen;
            playerScoreLabel.style.color = Color.white;
        }
    }

    public void GameOver()
    {
        GameManager.Instance.ChangePhase(GameManager.Phase.GameOver);

        scoreContainer.style.display = DisplayStyle.None;
        gameOverContainer.style.display = DisplayStyle.Flex;

        if (GameManager.Instance.enemyAIWin == true) gameOverLabel.text = "You Lose!";
        else gameOverLabel.text = "You Win!";
        gameOverScoreLabel.text = $"The end results are: {playerScoreLabel.text} / {enemyScoreLabel.text}";
    }
}