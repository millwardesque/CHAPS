using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public Canvas mainCanvas;
    public GameObject gameOverPrefab;
    

    MessageManager m_messenger = null;
    public MessageManager Messenger {
        get { return m_messenger; }
    }

    PlatformerMotor m_player = null;
    public PlatformerMotor Player {
        get { return m_player; }
    }

    public static GameManager Instance = null;

    void Awake() {
        if (null == Instance) {
            Instance = this;
            m_messenger = new MessageManager ();

            Debug.Assert(mainCanvas != null, "Game Manager: No main canvas has been assigned.");
        } else {
            Destroy (gameObject);
        }
    }

    void Start() {
        Messenger.AddListener("GameOver", OnGameOver);
        m_player = GameObject.FindGameObjectWithTag ("Player").GetComponent <PlatformerMotor>();
        Time.timeScale = 1f;
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
