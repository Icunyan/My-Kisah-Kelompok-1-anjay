using UnityEngine;
using System.Collections;

/// <summary>
/// AudioManager singleton – mengelola BGM dan SFX di seluruh game.
/// DontDestroyOnLoad agar musik tidak reset saat pindah scene.
/// 
/// SETUP DI UNITY:
/// 1. Buat GameObject kosong bernama "AudioManager" di scene MainMenu.
/// 2. Tambahkan script ini ke GameObject tersebut.
/// 3. Assign AudioClip BGM & SFX di Inspector.
/// 4. AudioManager akan otomatis DontDestroyOnLoad.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("=== BGM Settings ===")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip gameplayBGM;
    [SerializeField] private AudioClip tensionBGM;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("=== SFX Settings ===")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;

    [Header("=== SFX Clips ===")]
    [SerializeField] private AudioClip sfxButtonClick;
    [SerializeField] private AudioClip sfxSwordStrike;
    [SerializeField] private AudioClip sfxExplosion;
    [SerializeField] private AudioClip sfxMagicAura;
    [SerializeField] private AudioClip sfxMagicPotion;
    [SerializeField] private AudioClip sfxSwoosh;
    [SerializeField] private AudioClip sfxThunder;
    [SerializeField] private AudioClip sfxWind;
    [SerializeField] private AudioClip sfxEnergyFlow;
    [SerializeField] private AudioClip sfxImpact;
    [SerializeField] private AudioClip sfxExplosionHit;
    [SerializeField] private AudioClip sfxExplosionBattle;
    [SerializeField] private AudioClip sfxFairySwoosh;

    // Audio Sources
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    // Track current BGM to avoid replaying same clip
    private AudioClip currentBGMClip;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup BGM AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        // Setup SFX AudioSource
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    // =========================================================================
    // BGM CONTROLS
    // =========================================================================

    /// <summary>
    /// Mainkan BGM menu utama.
    /// </summary>
    public void PlayMainMenuBGM()
    {
        PlayBGM(mainMenuBGM);
    }

    /// <summary>
    /// Mainkan BGM gameplay.
    /// </summary>
    public void PlayGameplayBGM()
    {
        PlayBGM(gameplayBGM);
    }

    /// <summary>
    /// Mainkan BGM tension/suspense.
    /// </summary>
    public void PlayTensionBGM()
    {
        PlayBGM(tensionBGM);
    }

    /// <summary>
    /// Mainkan BGM tertentu. Jika sudah sedang diputar, skip.
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (currentBGMClip == clip && bgmSource.isPlaying) return;

        StartCoroutine(CrossfadeBGM(clip));
    }

    /// <summary>
    /// Stop BGM dengan fade out.
    /// </summary>
    public void StopBGM()
    {
        StartCoroutine(FadeOutBGM());
    }

    /// <summary>
    /// Set volume BGM (0-1).
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    /// <summary>
    /// Set volume SFX (0-1).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    // =========================================================================
    // SFX CONTROLS
    // =========================================================================

    /// <summary>
    /// Mainkan SFX sekali (one-shot).
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // --- Shortcut methods untuk SFX yang sering dipakai ---

    public void PlayButtonClick()    => PlaySFX(sfxButtonClick);
    public void PlaySwordStrike()    => PlaySFX(sfxSwordStrike);
    public void PlayExplosion()      => PlaySFX(sfxExplosion);
    public void PlayMagicAura()      => PlaySFX(sfxMagicAura);
    public void PlayMagicPotion()    => PlaySFX(sfxMagicPotion);
    public void PlaySwoosh()         => PlaySFX(sfxSwoosh);
    public void PlayThunder()        => PlaySFX(sfxThunder);
    public void PlayWind()           => PlaySFX(sfxWind);
    public void PlayEnergyFlow()     => PlaySFX(sfxEnergyFlow);
    public void PlayImpact()         => PlaySFX(sfxImpact);
    public void PlayExplosionHit()   => PlaySFX(sfxExplosionHit);
    public void PlayExplosionBattle()=> PlaySFX(sfxExplosionBattle);
    public void PlayFairySwoosh()    => PlaySFX(sfxFairySwoosh);

    // =========================================================================
    // FADE / CROSSFADE COROUTINES
    // =========================================================================

    private IEnumerator CrossfadeBGM(AudioClip newClip)
    {
        // Fade out current BGM
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }

            bgmSource.Stop();
        }

        // Switch to new clip and fade in
        bgmSource.clip = newClip;
        currentBGMClip = newClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, fadeElapsed / fadeDuration);
            yield return null;
        }

        bgmSource.volume = bgmVolume;
    }

    private IEnumerator FadeOutBGM()
    {
        if (!bgmSource.isPlaying) yield break;

        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        currentBGMClip = null;
    }
}
