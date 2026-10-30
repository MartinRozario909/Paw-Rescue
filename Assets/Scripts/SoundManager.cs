using UnityEngine;

public enum Sound
{
    Jump,
    Coin,
    Heart,
    Hurt,
    Stomp,
    Spring,
    Rescue,
    Locked,
    Gate,
    Wave,
    Win,
    Lose
}

// A small pool of AudioSources, modelled on Kenney's 3D platformer starter kit (audio.gd):
// each sound plays on the next free source with a little random pitch so repeats don't sound the same.
// The project has no audio files, so the short retro sound effects are generated when the game starts.
// Play a sound from anywhere with SoundManager.Play(Sound.Coin).
public class SoundManager : MonoBehaviour
{
    private const int SampleRate = 44100;

    private enum Wave { Sine, Square, Triangle, Noise }

    private static SoundManager instance;

    [SerializeField] private int voices = 8;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private float pitchVariance = 0.06f; // Random pitch up or down by this much

    private AudioSource[] sources;
    private AudioClip[] clips;
    private int nextSource;

    void Awake()
    {
        instance = this;

        sources = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].spatialBlend = 0f; // 2D: the camera is always near the pet
        }

        clips = new AudioClip[System.Enum.GetValues(typeof(Sound)).Length];
        clips[(int)Sound.Jump] = Sweep("Jump", 0.14f, 320f, 640f, Wave.Square, 0.25f);
        clips[(int)Sound.Coin] = Notes("Coin", 0.07f, Wave.Square, 0.25f, 988f, 1319f, 1319f);
        clips[(int)Sound.Heart] = Notes("Heart", 0.07f, Wave.Triangle, 0.5f, 523f, 659f, 784f, 1047f);
        clips[(int)Sound.Hurt] = Sweep("Hurt", 0.28f, 420f, 110f, Wave.Square, 0.3f, 0.4f);
        clips[(int)Sound.Stomp] = Sweep("Stomp", 0.16f, 220f, 55f, Wave.Sine, 0.9f, 0.35f);
        clips[(int)Sound.Spring] = Sweep("Spring", 0.35f, 180f, 900f, Wave.Triangle, 0.5f);
        clips[(int)Sound.Rescue] = Notes("Rescue", 0.1f, Wave.Square, 0.22f, 523f, 659f, 784f, 1047f, 1319f);
        clips[(int)Sound.Locked] = Notes("Locked", 0.09f, Wave.Square, 0.2f, 150f, 120f);
        clips[(int)Sound.Gate] = Sweep("Gate", 0.7f, 90f, 60f, Wave.Triangle, 0.6f, 0.5f);
        clips[(int)Sound.Wave] = Notes("Wave", 0.12f, Wave.Square, 0.2f, 440f, 660f, 440f, 660f);
        clips[(int)Sound.Win] = Notes("Win", 0.14f, Wave.Square, 0.22f, 523f, 659f, 784f, 1047f, 784f, 1047f);
        clips[(int)Sound.Lose] = Notes("Lose", 0.2f, Wave.Triangle, 0.5f, 392f, 330f, 262f, 196f);
    }

    public static void Play(Sound sound)
    {
        if (instance != null) instance.PlaySound(sound);
    }

    private void PlaySound(Sound sound)
    {
        AudioSource source = sources[nextSource];
        nextSource = (nextSource + 1) % sources.Length;

        source.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        source.PlayOneShot(clips[(int)sound], volume);
    }

    // A tone that slides from one pitch to another, with an optional mix of noise
    private static AudioClip Sweep(string name, float duration, float startHz, float endHz, Wave wave, float loudness, float noise = 0f)
    {
        int count = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            phase += Mathf.Lerp(startHz, endHz, t) / SampleRate;
            float tone = Sample(wave, phase) * (1f - noise) + Sample(Wave.Noise, phase) * noise;
            samples[i] = tone * Envelope(t) * loudness;
        }
        return MakeClip(name, samples);
    }

    // A quick run of notes, one after the other
    private static AudioClip Notes(string name, float noteLength, Wave wave, float loudness, params float[] notesHz)
    {
        int perNote = Mathf.CeilToInt(noteLength * SampleRate);
        float[] samples = new float[perNote * notesHz.Length];
        float phase = 0f;

        for (int n = 0; n < notesHz.Length; n++)
        {
            for (int i = 0; i < perNote; i++)
            {
                phase += notesHz[n] / SampleRate;
                // The last note rings out a little longer than the others
                float fade = n == notesHz.Length - 1 ? 1f - (float)i / perNote : Mathf.Min(1f, (perNote - i) / (perNote * 0.2f));
                samples[n * perNote + i] = Sample(wave, phase) * fade * loudness;
            }
        }
        return MakeClip(name, samples);
    }

    private static float Sample(Wave wave, float phase)
    {
        float p = phase - Mathf.Floor(phase);
        switch (wave)
        {
            case Wave.Square: return p < 0.5f ? 1f : -1f;
            case Wave.Triangle: return 1f - 4f * Mathf.Abs(p - 0.5f);
            case Wave.Noise: return Random.value * 2f - 1f;
            default: return Mathf.Sin(p * Mathf.PI * 2f);
        }
    }

    // Fast fade in so there is no click, then fade out to silence
    private static float Envelope(float t)
    {
        return Mathf.Min(1f, t * 40f) * (1f - t);
    }

    private static AudioClip MakeClip(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
