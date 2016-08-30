using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;

public class CameraController : MonoBehaviour {
    [Header ("Enemy Proximity Zoom")]
    [Range (0f, 2f)]
    public float timeScale = 0.5f;

    [Range (0f, 2f)]
    public float zoomAmount = 1f;

    [Range (0f, 1f)]
    public float zoomDuration = 0.2f;

    [Header ("Safe Room Zoom")]
    [Range (0f, 4f)]
    public float safeRoomZoomAmount = 3f;

    [Range (0f, 1f)]
    public float safeRoomZoomDuration = 0.2f;

    int m_invokedCount = 0;
    ProCamera2D m_camera;

    void Awake() {
        m_camera = GetComponent<ProCamera2D> ();
    }

	// Use this for initialization
	void Start () {
        GameManager.Instance.Messenger.AddListener ("PlayerCloseToEnemy", OnPlayerCloseToEnemy);
        GameManager.Instance.Messenger.AddListener ("PlayerFarFromEnemy", OnPlayerFarFromEnemy);

        GameManager.Instance.Messenger.AddListener ("SafeRoomEnter", OnSafeRoomEnter);
        GameManager.Instance.Messenger.AddListener ("SafeRoomExit", OnSafeRoomExit);
	}
	
    void OnPlayerCloseToEnemy(Message message) {
        if (m_invokedCount == 0) {
            m_camera.Zoom (-zoomAmount, zoomDuration);
            m_invokedCount++;
            Time.timeScale *= timeScale;
        }
    }

    void OnPlayerFarFromEnemy(Message message) {
        if (m_invokedCount > 0) {
            m_camera.Zoom (zoomAmount, zoomDuration);
            m_invokedCount--;
            Time.timeScale /= timeScale;
        }
    }

    void OnSafeRoomEnter(Message mssage) {
        m_camera.Zoom (-safeRoomZoomAmount, safeRoomZoomDuration);
        GetComponent<ProCamera2DForwardFocus> ().RightFocus = 0.001f;
    }

    void OnSafeRoomExit(Message mssage) {
        m_camera.Zoom (safeRoomZoomAmount, safeRoomZoomDuration);
        GetComponent<ProCamera2DForwardFocus> ().RightFocus = GetComponent<ProCamera2DForwardFocus> ().LeftFocus;
    }
}
