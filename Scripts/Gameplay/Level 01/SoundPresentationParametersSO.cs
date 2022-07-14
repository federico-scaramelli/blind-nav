using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "SoundPresentation", fileName = "SoundPresentation", order = 1)]
public class SoundPresentationParametersSO : ScriptableObject
{
    [SerializeField] private AudioClip voiceAudioClip;

    public AudioClip VoiceAudioClip => voiceAudioClip;

    public int MSDelayBetweenVoiceAnd3DSound => msDelayBetweenVoiceAnd3DSound;

    public AudioClip Sound3DAudioClip => sound3DAudioClip;
        
    public AudioClip EnvironmentAudioClip => environmentAudioClip;

    public bool RequestForInput => requestForInput;

    public int MSDelayToRequestInput => msDelayToRequestInput;

    public int MSDelayToNextPresentation => msDelayToNextPresentation;

    [SerializeField] [Range(0, 1000)] private int msDelayBetweenVoiceAnd3DSound;
    [SerializeField] private AudioClip sound3DAudioClip;
    [SerializeField] private AudioClip environmentAudioClip;
    [SerializeField] private bool requestForInput;
    [SerializeField] [Range(0, 2000)] private int msDelayToRequestInput;
    [SerializeField] [Range(0, 4000)] private int msDelayToNextPresentation;
}
