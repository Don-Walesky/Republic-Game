namespace Republic.Unity.Audio;

using UnityEngine;
using Republic.Core.Workspace.Models;
using Republic.Unity.Bridge;

/// <summary>
/// Unity Monobehavior managing office desk sound effects, disaster sirens, and dynamic background score.
/// </summary>
public sealed class RepublicAudioManager : Monobehavior
{
    public static RepublicAudioManager Instance { get; private set; } = null!;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource = null!;
    [SerializeField] private AudioSource musicAudioSource = null!;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip phoneRingingClip = null!;
    [SerializeField] private AudioClip phonePickUpClip = null!;
    [SerializeField] private AudioClip emailReceivedClip = null!;
    [SerializeField] private AudioClip newsFlashClip = null!;
    [SerializeField] private AudioClip paperShuffleClip = null!;
    [SerializeField] private AudioClip disasterSirenClip = null!;
    [SerializeField] private AudioClip decreeEnactedClip = null!;
    [SerializeField] private AudioClip backgroundAmbienceClip = null!;
    [SerializeField] private AudioClip crisisMusicClip = null!;

    private bool _isCrisisMusicActive;

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
            bridge.AppointmentReminded += OnAppointmentReminded;
            bridge.DecisionPrompted += OnDecisionPrompted;
        }

        PlayBackgroundMusic(isCrisis: false);
    }

    public void PlaySoundEffect(RepublicAudioClip clipType)
    {
        var clip = clipType switch
        {
            RepublicAudioClip.PhoneRinging => phoneRingingClip,
            RepublicAudioClip.PhonePickUp => phonePickUpClip,
            RepublicAudioClip.EmailReceived => emailReceivedClip,
            RepublicAudioClip.NewsFlashChime => newsFlashClip,
            RepublicAudioClip.PaperShuffle => paperShuffleClip,
            RepublicAudioClip.DisasterSiren => disasterSirenClip,
            RepublicAudioClip.DecreeEnactedFanfare => decreeEnactedClip,
            RepublicAudioClip.BackgroundAmbience => backgroundAmbienceClip,
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

    public void PlayBackgroundMusic(bool isCrisis)
    {
        if (musicAudioSource == null)
        {
            Debug.Log($"[Audio Manager] Background music toggled (Crisis: {isCrisis})");
            return;
        }

        var targetClip = isCrisis ? crisisMusicClip : backgroundAmbienceClip;
        if (targetClip != null && (musicAudioSource.clip != targetClip || !_isCrisisMusicActive != !isCrisis))
        {
            _isCrisisMusicActive = isCrisis;
            musicAudioSource.Stop();
            musicAudioSource.clip = targetClip;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
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
        PlayBackgroundMusic(isCrisis: true);
    }

    private void OnDecreeEnacted(string id, string title)
    {
        PlaySoundEffect(RepublicAudioClip.DecreeEnactedFanfare);
    }

    private void OnAppointmentReminded(CalendarAppointment appointment)
    {
        PlaySoundEffect(RepublicAudioClip.PaperShuffle);
    }

    private void OnDecisionPrompted(Republic.Core.Decisions.Models.DecisionContext decision)
    {
        PlaySoundEffect(RepublicAudioClip.PaperShuffle);
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
            bridge.AppointmentReminded -= OnAppointmentReminded;
            bridge.DecisionPrompted -= OnDecisionPrompted;
        }
    }
}
