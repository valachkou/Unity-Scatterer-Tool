using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AnalogScattererInstrumentForUnity : MonoBehaviour
{
    // Define the pool of audio sources, mixer, and bus.
    [Header("AudioClips Settings")]
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup audioGroup;
    [Range(0, 500)] [SerializeField] private int SpawnRate = 100;

    // Define the range for playback delay and randomization of volume and pitch.
    [Header("Audio Settings")]
    [Range(0.1f, 300)] [SerializeField] private float minDelay = 5f;
    [Range(0.1f, 300)] [SerializeField] private float maxDelay = 20f;
    [Range(0, 1)] [SerializeField] private float minVolume = 0.8f;
    [Range(0, 1)] [SerializeField] private float maxVolume = 1f;
    [Range(0.99f, 1.01f)] [SerializeField] private float minPitch = 0.995f;
    [Range(0.99f, 1.01f)] [SerializeField] private float maxPitch = 1.005f;

    // Define the distance range for random scattering of audio sources around the object's position and the attenuation curve type.
    [Header("Spatial Settings")]
    [SerializeField] private bool enable3D = true;
    [Range(0, 500)] [SerializeField] private float minScatterDistance = 20f;
    [Range(0, 500)] [SerializeField] private float maxScatterDistance = 50f;
    [SerializeField] private AudioRolloffMode audioRolloff;
    private float dopplerLevel = 0f;




    // Define the array of clips to be used for playback.
    private List<AudioSource> audioSourcesPool = new List<AudioSource>();
    private int lastClipIndex = -1; // Index of the last played audio clip to prevent consecutive repeats.

    // Create a variable to store the coroutine used for managing audio playback with delays.
    // The coroutine controls the time intervals between playbacks.
    private Coroutine playAudioCoroutine;

    // Initialize audio sources, validate audio clips and delays, and start the coroutine for delayed clip playback.
    // Validate audio clips and delays before starting playback.
    void Start()
    {
        ValidateAudioSource();

        InitializeAudioSources(poolSize);

        if (!ValidateAudioClips() || !ValidateDelayValues() || !ValidateDistanceValue()) return;

        playAudioCoroutine = StartCoroutine(PlayAudioClipsWithDelay());
    }

    // Method to get a random audio clip index different from the previously played one.
    // Returns 0 if the number of clips is less than two to avoid an infinite loop.
    private int GetRandomIndex(int clipLength)
    {
        if (clipLength <= 1) return 0;
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, clipLength);
        } while (randomIndex == lastClipIndex);

        lastClipIndex = randomIndex;
        return randomIndex;
    }



    // Method to initialize the audio source pool, adding sources as needed.
    private void InitializeAudioSources(int numberOfSources)
    {
        for (int i = 0; i < numberOfSources; i++)
        {
            audioSourcesPool.Add(CreateNewAudioSource());

        }


    }


    // Method to check for the presence of an AudioSource component.
    private void ValidateAudioSource()
    {

        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = audioGroup;
        }

    }
    // Method to validate the presence of audio clips.
    private bool ValidateAudioClips()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogError("Audio clips array is empty or not assigned!");
            return false;
        }
        return true;
    }
    // Method to validate delay values.
    private bool ValidateDelayValues()
    {
        if (minDelay > maxDelay)
        {
            Debug.LogError("minDelay should not be greater than maxDelay!");
            return false;
        }
        return true;
    }
   // Method to validate scatter distance values.
    private bool ValidateDistanceValue()
    {
        if(minScatterDistance > maxScatterDistance)
        {
            Debug.LogError("minScatterDistance should not be greater than maxScatterDistance!");
            return false;
        }
        return true;
    }
    
    // Function to create a new audio source.
    private AudioSource CreateNewAudioSource()
    {
        GameObject newSourceObject = new GameObject("AudioSource_" + audioSourcesPool.Count);
        newSourceObject.transform.SetParent(transform);
        newSourceObject.transform.position = GetRandomPosition(minScatterDistance, maxScatterDistance);
        AudioSource newSource = newSourceObject.AddComponent<AudioSource>();

        newSource.outputAudioMixerGroup = audioGroup;
        newSource.dopplerLevel = dopplerLevel;
        newSource.playOnAwake = false;
        newSource.rolloffMode = audioRolloff;
        newSource.maxDistance = maxScatterDistance; 


        return newSource;
    }

    // Method to get an available audio source from the pool. If all sources are busy and the pool size is not exceeded, a new source is created.
    // If all sources are busy and the maximum pool size is reached, the first source in the pool is returned.
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcesPool)
        {
            if (source != null && !source.isPlaying) return source;
        }

        if (audioSourcesPool.Count < poolSize)
        {
            AudioSource newSource = CreateNewAudioSource();
            audioSourcesPool.Add(newSource);
            return newSource;
        }

        return audioSourcesPool[0];
    }

    // Method to get a random position within the specified distance range from the object's position.
    // Creates a random scattering effect for audio sources in 3D space.
    private Vector3 GetRandomPosition(float minDistance, float maxDistance)
    {
        Vector3 randomDirection = Random.onUnitSphere;
        float randomDistance = Random.Range(minDistance, maxDistance);
        return transform.position + randomDirection * randomDistance;
    }

    
    // Coroutine for smoothly moving an audio source to a target position. "float duration" controls the movement duration, updating the source position each frame.
    private IEnumerator MoveAudioSourceSmoothly(AudioSource source, Vector3 targetPosition, float duration)
    {

        Vector3 startPosition = source.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (source == null) yield break;
            float progress = elapsedTime / duration;
            source.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (source != null)
        {
            source.transform.position = targetPosition;
        }

       
    }

  

    // Coroutine to remove an audio source from the pool after the clip finishes playing. It checks if the clip has ended and removes the source only after completion.
    private IEnumerator DestroyAudioSourceAfterClip(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            float clipLength = source.clip.length;

            // Use a flag to track clip completion.
            bool clipEnded = false;

            // Check if the clip has finished.
            while (clipLength > 0)
            {
                yield return null; // Wait for one frame.
                clipLength -= Time.deltaTime; // Decrease the remaining clip duration.
                if (clipLength <= 0)
                {
                    clipEnded = true;
                }
            }

            // Wait for a short delay after completion.
            yield return new WaitForSeconds(0.1f);

            // Ensure the audio source hasn't been modified and the clip has truly ended.
            if (source != null && !source.isPlaying && clipEnded)
            {
                source.Stop();
                source.clip = null;
                audioSourcesPool.Remove(source);
                Destroy(source.gameObject);
            }
        }
    }

    // Method to determine how many times an audio clip should play based on the specified probability (SpawnRate).
    private void PlayAudioSourceMultipleTimes(float probability)
    {
        
        float probabilityNormalized = Mathf.Clamp(probability / 100f, 0f, 5f);

        
        int maxRepetitions = Mathf.Clamp(Mathf.CeilToInt(probability / 100f), 1, 5);

        
        for (int i = 0; i < maxRepetitions; i++)
        {
            float effectiveProbability = Mathf.Max(probabilityNormalized - i, 0f); // Ensure probability is not negative.
            
            float randomValue = Random.value;

            if (randomValue <= effectiveProbability)
            {
                PlayAudioClip(); 
            }
        }
    }


    // Method to configure the audio source (position, volume, pitch) and play the clip.
    private void PlayAudioClip()
    {
        Vector3 previousPosition = transform.position;
        AudioSource source = GetAvailableAudioSource();
        if (source == null) return;
        if (enable3D)
        {
            source.spatialBlend = 1f;
            Vector3 newStartPosition = GetRandomPosition(minScatterDistance, maxScatterDistance);

            if (Vector3.Distance(previousPosition, newStartPosition) > maxScatterDistance / 2)
            {
                StartCoroutine(MoveAudioSourceSmoothly(source, newStartPosition, 10f)); // 10f is chosen as an optimal balance between movement speed and natural sound.
            }
            else
            {
                source.transform.position = newStartPosition;
            }
        }
        else
        {
            source.spatialBlend = 0f;
        }

        int randomIndex = GetRandomIndex(audioClips.Length);
        source.clip = audioClips[randomIndex];
        source.volume = Random.Range(minVolume, maxVolume);
        source.pitch = Random.Range(minPitch, maxPitch);
        source.Play();
        StartCoroutine(DestroyAudioSourceAfterClip(source));

        Debug.Log($"Playing clip: {source.clip.name} from position: {source.transform.position}");
    }

     // Coroutine to handle playback with random time intervals between plays.
    private IEnumerator PlayAudioClipsWithDelay()
    {
        
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            PlayAudioSourceMultipleTimes(SpawnRate);

        }
    }

   // Method to stop the current playback coroutine and all sources in the pool. All coroutines in the object are also stopped.
    public void StopPlaying()
    {
        if (playAudioCoroutine != null)
        {
            StopCoroutine(playAudioCoroutine);
            playAudioCoroutine = null;
        }

        foreach (AudioSource source in audioSourcesPool)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
            }
        }

        StopAllCoroutines();
    }
}
