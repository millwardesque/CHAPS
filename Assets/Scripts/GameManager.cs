using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    GameObject m_levelContainer;

    public Canvas mainCanvas;
    public GameObject gameOverPrefab;

    private int m_totalIntelCollected = 0;
    public int TotalIntelCollected
    {
        get { return m_totalIntelCollected; }
        set
        {
            m_totalIntelCollected = value;
            ES2.Save<int>(m_totalIntelCollected, "player0.txt?tag=totalIntelCollected");
            Messenger.SendMessage(this, "TotalIntelCollectedChange", m_totalIntelCollected);
        }
    }
    MessageManager m_messenger = null;
    public MessageManager Messenger {
        get { return m_messenger; }
    }

    PlatformerMotor m_player = null;
    public PlatformerMotor Player {
        get { return m_player; }
    }

    AudioSource m_audioManager;
    public AudioSource AudioManager {
        get { return m_audioManager; }
    }

    LevelManager m_levelManager;
    public LevelManager Level {
        get { return m_levelManager; }
    }

    public static GameManager Instance = null;

    void Awake() {
        if (null == Instance) {
            Instance = this;
            m_messenger = new MessageManager ();
            m_audioManager = GetComponent<AudioSource> ();
            m_levelManager = GetComponent<LevelManager> ();

            Debug.Assert(mainCanvas != null, "Game Manager: No main canvas has been assigned.");
        } else {
            Destroy (gameObject);
        }
    }

    void Start() {
        Messenger.AddListener("GameOver", OnGameOver);
        m_player = GameObject.FindGameObjectWithTag ("Player").GetComponent <PlatformerMotor>();
        Time.timeScale = 1f;

        if (ES2.Exists("player0.txt?tag=totalIntelCollected")) {
            TotalIntelCollected = ES2.Load<int>("player0.txt?tag=totalIntelCollected");
        }            
        else {
            TotalIntelCollected = 0;
        }

        m_levelContainer = Level.GenerateRooms (12, "Level", new Vector2 (0f, 0f));
    }

    void Update() {
        // Debug controls
        if (Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("Resetting total intel collected.");
            TotalIntelCollected = 0;
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnGameOver(Message message) {
        GameObject gameOverWindow = Instantiate<GameObject>(gameOverPrefab);
        gameOverWindow.transform.SetParent(mainCanvas.transform, false);
        Time.timeScale = 0f;
    }
}
