using UnityEngine;

/// <summary>
/// 音频管理器 - 管理背景音乐和音效播放
/// 挂载到一个专用GameObject上
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频源")]
    private AudioSource bgmSource;           // 背景音乐
    private AudioSource sfxSource;           // 音效（可叠加）

    [Header("背景音乐")]
    public AudioClip bgmClip;
    [Range(0f, 1f)]
    public float bgmVolume = 0.3f;

    [Header("环境音效")]
    public AudioClip ambientClip;            // 环境音（如展览馆回声）
    [Range(0f, 1f)]
    public float ambientVolume = 0.15f;

    private AudioSource ambientSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 设置BGM音源
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.playOnAwake = false;

        // 设置SFX音源
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // 设置环境音源
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.playOnAwake = false;

        if (ambientClip != null)
        {
            ambientSource.clip = ambientClip;
            ambientSource.Play();
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBGM()
    {
        if (bgmClip == null) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 播放一次性音效
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }
}
