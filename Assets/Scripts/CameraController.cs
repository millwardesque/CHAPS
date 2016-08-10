using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;

public class CameraController : MonoBehaviour {
    [Range (0f, 2f)]
    public float timeScale = 0.5f;

    [Range (0f, 2f)]
    public float zoomAmount = 1f;

    [Range (0f, 1f)]
    public float zoomDuration = 0.2f;

    int m_invokedCount = 0;

	// Use this for initialization
	void Start () {
        GameManager.Instance.Messenger.AddListener ("PlayerCloseToEnemy", OnPlayerCloseToEnemy);
        GameManager.Instance.Messenger.AddListener ("PlayerFarFromEnemy", OnPlayerFarFromEnemy);
	}
	
    void OnPlayerCloseToEnemy(Message message) {
        if (m_invokedCount == 0) {
            GetComponent<ProCamera2D> ().Zoom (-zoomAmount, zoomDuration);
            m_invokedCount++;
            Time.timeScale *= timeScale;
        }
    }

    void OnPlayerFarFromEnemy(Message message) {
        if (m_invokedCount > 0) {
            GetComponent<ProCamera2D> ().Zoom (zoomAmount, zoomDuration);
            m_invokedCount--;
            Time.timeScale /= timeScale;
        }
    }
}
