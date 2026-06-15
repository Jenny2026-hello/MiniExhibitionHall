using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject startPanel;
    public GameObject infoPanel;
    public Text infoTitleText;
    public Text infoDescText;
    public Text info3IText;
    public Text interactionHint;
    public GameObject pausePanel;

    [Header("Settings")]
    public float mouseSensitivity = 2f;
    public float moveSpeed = 5f;
    public bool invertY = false;

    private bool isPaused;
    private bool isGameStarted;
    private bool isInfoOpen;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        BindRuntimeButtons();

        if (startPanel != null) startPanel.SetActive(true);
        if (infoPanel != null) infoPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        isPaused = false;
        isInfoOpen = false;
        LockCursor(false);
    }

    void Update()
    {
        if (!isGameStarted && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            StartGame();
            return;
        }

        if (isInfoOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            HideExhibitInfo();
            return;
        }

        if (isGameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void BindRuntimeButtons()
    {
        BindButton("StartButton", StartGame);
        BindButton("QuitButton", QuitGame);
        BindButton("CloseInfoButton", HideExhibitInfo);
        BindButton("ResumeButton", TogglePause);
        BindButton("PauseQuitButton", QuitGame);
    }

    void BindButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        Button button = FindButtonIncludingInactive(objectName);
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    Button FindButtonIncludingInactive(string objectName)
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button button in buttons)
        {
            if (button != null && button.name == objectName)
            {
                return button;
            }
        }

        return null;
    }

    public void StartGame()
    {
        isGameStarted = true;
        isPaused = false;
        Time.timeScale = 1f;

        HideAllStartPanels();
        RemoveLegacySceneOverlays();
        if (pausePanel != null) pausePanel.SetActive(false);

        LockCursor(true);
        AudioManager.Instance?.PlayBGM();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowExhibitInfo(string title, string description, string immersionNote = "", string interactionNote = "", string imaginationNote = "")
    {
        if (infoPanel == null) return;

        infoPanel.SetActive(true);
        isInfoOpen = true;

        if (infoTitleText != null) infoTitleText.text = title;
        if (infoDescText != null) infoDescText.text = description;

        if (info3IText != null)
        {
            string threeI = "";
            if (!string.IsNullOrEmpty(immersionNote)) threeI += "\u6c89\u6d78: " + immersionNote + "\n";
            if (!string.IsNullOrEmpty(interactionNote)) threeI += "\u4ea4\u4e92: " + interactionNote + "\n";
            if (!string.IsNullOrEmpty(imaginationNote)) threeI += "\u6784\u60f3: " + imaginationNote;
            info3IText.text = threeI;
        }

        LockCursor(false);
    }

    public void HideExhibitInfo()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
        isInfoOpen = false;

        if (isGameStarted && !isPaused)
        {
            LockCursor(true);
        }
    }

    public void ShowHint(string hint)
    {
        if (interactionHint == null) return;

        interactionHint.text = hint;
        GameObject hintRoot = interactionHint.transform.parent != null && interactionHint.transform.parent.name == "HintPanel"
            ? interactionHint.transform.parent.gameObject
            : interactionHint.gameObject;
        hintRoot.SetActive(true);
    }

    public void HideHint()
    {
        if (interactionHint == null) return;

        GameObject hintRoot = interactionHint.transform.parent != null && interactionHint.transform.parent.name == "HintPanel"
            ? interactionHint.transform.parent.gameObject
            : interactionHint.gameObject;
        hintRoot.SetActive(false);
    }

    public void TogglePause()
    {
        if (isInfoOpen)
        {
            HideExhibitInfo();
            return;
        }

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel != null) pausePanel.SetActive(isPaused);
        LockCursor(!isPaused);
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HideAllStartPanels()
    {
        if (startPanel != null) startPanel.SetActive(false);

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (obj == null || !obj.scene.IsValid()) continue;
            if (obj.name == "StartPanel")
            {
                obj.SetActive(false);
            }
        }
    }

    private void RemoveLegacySceneOverlays()
    {
        foreach (TextMesh textMesh in FindObjectsOfType<TextMesh>())
        {
            if (textMesh != null && IsLegacyFloatingText(textMesh.gameObject))
            {
                Destroy(textMesh.gameObject);
            }
        }

        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && IsLegacyFloatingText(canvas.gameObject))
            {
                Destroy(canvas.gameObject);
            }
        }

        foreach (Light light in FindObjectsOfType<Light>())
        {
            if (light != null)
            {
                Destroy(light.gameObject);
            }
        }

        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>())
        {
            if (audioSource != null && audioSource.GetComponent<AudioManager>() == null)
            {
                Destroy(audioSource.gameObject);
            }
        }
    }

    private bool IsLegacyFloatingText(GameObject obj)
    {
        if (obj == null) return false;
        return obj.name.Contains("WelcomeText") || obj.name.Contains("WorldText") || obj.name.Contains("StartTitle") || obj.name.Contains("Legacy");
    }
}
