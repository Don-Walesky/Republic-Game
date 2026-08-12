namespace Republic.Unity.Audio;

using UnityEngine;
using Republic.Core.Military.Models;
using Republic.Unity.Bridge;

/// <summary>
/// Unity AudioManager executing audio clips for executive stamp slams, phone rings, press camera shutters, and DEFCON sirens.
/// </summary>
public sealed class RepublicAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource = null!;
    [SerializeField] private AudioSource ambienceSource = null!;
    [SerializeField] private AudioSource phoneRingSource = null!;

    [Header("Sound Effects Clips")]
    [SerializeField] private AudioClip stampSlamClip = null!;
    [SerializeField] private AudioClip phoneRingClip = null!;
    [SerializeField] private AudioClip dossierFlipClip = null!;
    [SerializeField] private AudioClip pressCameraShutterClip = null!;
    [SerializeField] private AudioClip defconSirenClip = null!;
    [SerializeField] private AudioClip officeAmbienceClip = null!;

    private static RepublicAudioManager instance = null!;
    public static RepublicAudioManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (ambienceSource != null && officeAmbienceClip != null)
        {
            ambienceSource.clip = officeAmbienceClip;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }

        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.PhoneRinging += OnPhoneRinging;
            bridge.DecreeEnacted += OnDecreeEnacted;
            bridge.DefconLevelChanged += OnDefconChanged;
            bridge.PressConferenceConducted += OnPressConferenceConducted;
            bridge.EmailReceived += OnEmailReceived;
            bridge.NewsPublished += OnNewsPublished;
        }
    }

    public void PlayStampSlam()
    {
        if (sfxSource != null && stampSlamClip != null)
        {
            sfxSource.PlayOneShot(stampSlamClip);
        }
    }

    public void PlayDossierFlip()
    {
        if (sfxSource != null && dossierFlipClip != null)
        {
            sfxSource.PlayOneShot(dossierFlipClip);
        }
    }

    private void OnPhoneRinging(Republic.Core.Workspace.Models.PhoneCall call)
    {
        if (phoneRingSource != null && phoneRingClip != null)
        {
            phoneRingSource.clip = phoneRingClip;
            phoneRingSource.loop = true;
            phoneRingSource.Play();
        }
    }

    private void OnEmailReceived(Republic.Core.Workspace.Models.EmailMessage email)
    {
        PlayDossierFlip();
    }

    private void OnNewsPublished(Republic.Core.Workspace.Models.NewsArticle article)
    {
        if (sfxSource != null && pressCameraShutterClip != null)
        {
            sfxSource.PlayOneShot(pressCameraShutterClip);
        }
    }

    private void OnDecreeEnacted(string decreeId, string title)
    {
        PlayStampSlam();
    }

    private void OnDefconChanged(DefconLevel prev, DefconLevel next)
    {
        if (next == DefconLevel.Defcon1_MaximumReadiness || next == DefconLevel.Defcon2_ArmedForcesArmed)
        {
            if (sfxSource != null && defconSirenClip != null)
            {
                sfxSource.PlayOneShot(defconSirenClip);
            }
        }
    }

    private void OnPressConferenceConducted(string topic, double delta, string summary)
    {
        if (sfxSource != null && pressCameraShutterClip != null)
        {
            sfxSource.PlayOneShot(pressCameraShutterClip);
        }
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.PhoneRinging -= OnPhoneRinging;
            bridge.DecreeEnacted -= OnDecreeEnacted;
            bridge.DefconLevelChanged -= OnDefconChanged;
            bridge.PressConferenceConducted -= OnPressConferenceConducted;
            bridge.EmailReceived -= OnEmailReceived;
            bridge.NewsPublished -= OnNewsPublished;
        }
    }
}
