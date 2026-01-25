#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugManager : MonoBehaviour
{
    private bool isFpsLimited = false;

    [SerializeField] private int defaultFPS = 160;
    [SerializeField] private int targetFps = 30;
    [SerializeField] private float speedUpFactor = 10f;

    private void Awake()
    {
        Application.targetFrameRate = defaultFPS;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ReloadScene();
        if (Input.GetKeyDown(KeyCode.L)) ToggleFpsLimit();
        if (Input.GetKeyDown(KeyCode.Space)) SpeedUp(true);
        if (Input.GetKeyUp(KeyCode.Space)) SpeedUp(false);
    }

    public void SpeedUp(bool speedUp)
    {
        Time.timeScale = speedUp ? speedUpFactor : 1f;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleFpsLimit()
    {
        isFpsLimited = !isFpsLimited;
        Application.targetFrameRate = isFpsLimited ? targetFps : defaultFPS;
        Debug.Log(isFpsLimited ? $"FPS limited to {targetFps}" : "FPS unlimited");
    }
}
#endif