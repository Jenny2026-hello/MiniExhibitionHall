using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏全局管理器 - 控制游戏状态、场景切换、全局UI
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject startPanel;          // 开始面板
    public GameObject infoPanel;           // 展品信息面板
    public Text infoTitleText;             // 展品标题
    public Text infoDescText;              // 展品描述
    public Text info3IText;                // 3I标签文字（新增）
    public Text interactionHint;           // 交互提示文字
    public GameObject pausePanel;          // 暂停菜单

    [Header("Settings")]
    public float mouseSensitivity = 2f;
    public float moveSpeed = 5f;
    public bool invertY = false;

    private bool isPaused = false;
    private bool isGameStarted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 初始显示开始面板
        if (startPanel != null)
            startPanel.SetActive(true);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        LockCursor(false);
    }

    void Update()
    {
        // ESC 暂停/继续（仅在游戏开始后生效）
        if (isGameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 开始游戏 - 关闭开始面板，锁定鼠标
    /// </summary>
    public void StartGame()
    {
        isGameStarted = true;

        if (startPanel != null)
            startPanel.SetActive(false);

        LockCursor(true);
        AudioManager.Instance?.PlayBGM();
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 显示展品信息面板（含3I标签）
    /// </summary>
    public void ShowExhibitInfo(string title, string description,
        string immersionNote = "", string interactionNote = "", string imaginationNote = "")
    {
        if (infoPanel == null) return;

        infoPanel.SetActive(true);
        if (infoTitleText != null) infoTitleText.text = title;
        if (infoDescText != null) infoDescText.text = description;

        // 显示3I标签信息
        if (info3IText != null && !string.IsNullOrEmpty(immersionNote))
        {
            string threeI = "";
            if (!string.IsNullOrEmpty(immersionNote))
                threeI += "【沉浸性】" + immersionNote + "\n";
            if (!string.IsNullOrEmpty(interactionNote))
                threeI += "【交互性】" + interactionNote + "\n";
            if (!string.IsNullOrEmpty(imaginationNote))
                threeI += "【构想性】" + imaginationNote;
            info3IText.text = threeI;
        }

        LockCursor(false);
    }

    /// <summary>
    /// 隐藏展品信息面板
    /// </summary>
    public void HideExhibitInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        LockCursor(true);
    }

    /// <summary>
    /// 显示交互提示
    /// </summary>
    public void ShowHint(string hint)
    {
        if (interactionHint != null)
        {
            interactionHint.text = hint;
            interactionHint.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    public void HideHint()
    {
        if (interactionHint != null)
            interactionHint.gameObject.SetActive(false);
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        LockCursor(!isPaused);
    }

    /// <summary>
    /// 锁定/解锁鼠标光标
    /// </summary>
    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
