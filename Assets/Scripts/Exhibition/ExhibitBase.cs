using UnityEngine;

/// <summary>
/// 展品基类 - 所有展品继承此类
/// 挂载到每个展品GameObject上
/// </summary>
public class ExhibitBase : MonoBehaviour
{
    [Header("展品信息")]
    public string exhibitName = "未命名展品";
    [TextArea(3, 10)]
    public string exhibitDescription = "这是一件展品的描述。";

    [Header("展示设置")]
    public float interactionDistance = 3f;       // 交互距离
    public string hintText = "按 E 查看详情";     // 提示文字
    public float autoRotateSpeed = 10f;          // 自动旋转速度（0=不旋转）

    [Header("音效")]
    public AudioClip exhibitSound;               // 展品专属音效

    [Header("3I 标签（用于报告中分析）")]
    public string immersionNote = "";            // 沉浸性说明
    public string interactionNote = "";          // 交互性说明
    public string imaginationNote = "";          // 构想性说明

    private bool isPlayerNearby = false;

    void Update()
    {
        // 自动旋转（展示效果）
        if (autoRotateSpeed > 0)
        {
            transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 玩家进入交互范围
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            GameManager.Instance?.ShowHint(hintText);
        }
    }

    /// <summary>
    /// 玩家在交互范围内
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 按E键交互（仅在未暂停且鼠标锁定时响应）
        if (Input.GetKeyDown(KeyCode.E) && Cursor.lockState == CursorLockMode.Locked)
        {
            OnInteract();
        }
    }

    /// <summary>
    /// 玩家离开交互范围
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            GameManager.Instance?.HideHint();
        }
    }

    /// <summary>
    /// 交互逻辑（子类可重写）
    /// </summary>
    public virtual void OnInteract()
    {
        // 显示信息面板（含3I标签）
        GameManager.Instance?.ShowExhibitInfo(
            exhibitName,
            exhibitDescription,
            immersionNote,
            interactionNote,
            imaginationNote
        );

        // 播放音效
        if (exhibitSound != null)
        {
            AudioManager.Instance?.PlaySFX(exhibitSound);
        }
    }

    /// <summary>
    /// 关闭交互
    /// </summary>
    public virtual void OnCloseInteract()
    {
        GameManager.Instance?.HideExhibitInfo();
    }

    void OnDrawGizmosSelected()
    {
        // 在Scene视图中可视化交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
