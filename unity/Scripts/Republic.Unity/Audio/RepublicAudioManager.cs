namespace Republic.Unity.Audio;

using UnityEngine;
using Republic.Core.Workspace.Models;
using Republic.Unity.Bridge;

/// <summary>
/// Unity MonoBehavior managing office desk sound effects, disaster sirens, and dynamic background score.
/// </summary>
public sealed class RepublicAudioManager : MonoBehavior
{
    public static RepublicAudioManager Instance { get; private set; } = null!;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource = null!;
    [SerializeField] private AudioSource musicAudioSource = null!;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip phoneRingingClip = null!;
    [SerializeField] private AudioClip emailReceivedClip = null!;
    [SerializeField] private AudioClip newsFlashClip = null!;
    [SerializeField] private AudioClip disasterSirenClip = null!;
    [SerializeField] private AudioClip decreeEnactedClip = null!;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.PhoneRinging += OnPhoneRinging;
            bridge.EmailReceived += OnEmailReceived;
            bridge.NewsPublished += OnNewsPublished;
            bridge.CrisisTriggered += OnCrisisTriggered;
            bridge.DecreeEnacted += OnDecreeEnacted;
        }
    }

    public void PlaySoundEffect(RepublicAudioClip clipType)
    {
        var clip = clipType switch
        {
            RepublicAudioClip.PhoneRinging => phoneRingingClip,
            RepublicAudioClip.EmailReceived => emailReceivedClip,
            RepublicAudioClip.NewsFlashChime => newsFlashClip,
            RepublicAudioClip.DisasterSiren => disasterSirenClip,
            RepublicAudioClip.DecreeEnactedFanfare => decreeEnactedClip,
            _ => null
        };

        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.Log($"[Audio Manager] Sound Effect triggered: {clipType}");
        }
    }

    private void OnPhoneRinging(PhoneCall call)
    {
        PlaySoundEffect(RepublicAudioClip.PhoneRinging);
    }

    private void OnEmailReceived(EmailMessage email)
    {
        PlaySoundEffect(RepublicAudioClip.EmailReceived);
    }

    private void OnNewsPublished(NewsArticle article)
    {
        PlaySoundEffect(RepublicAudioClip.NewsFlashChime);
    }

    private void OnCrisisTriggered(string title, string category, string severity)
    {
        PlaySoundEffect(RepublicAudioClip.DisasterSiren);
    }

    private void OnDecreeEnacted(string id, string title)
    {
        PlaySoundEffect(RepublicAudioClip.DecreeEnactedFanfare);
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.PhoneRinging -= OnPhoneRinging;
            bridge.EmailReceived -= OnEmailReceived;
            bridge.NewsPublished -= OnNewsPublished;
            bridge.CrisisTriggered -= OnCrisisTriggered;
            bridge.DecreeEnacted -= OnDecreeEnacted;
        }
    }
}
