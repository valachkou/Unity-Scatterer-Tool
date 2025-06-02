using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AnalogScattererInstrumentForUnity : MonoBehaviour
{
    // Определение пула источников аудио, микшера и бас-шины.
    [Header("AudioClips Settings")]
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup audioGroup;
    [Range(0, 500)] [SerializeField] private int SpawnRate = 100;

    // Определяем диапазон задержки воспроизведения аудиклипов и диапазон рандомизации громкости и частоты.
    [Header("Audio Settings")]
    [Range(0.1f, 300)] [SerializeField] private float minDelay = 5f;
    [Range(0.1f, 300)] [SerializeField] private float maxDelay = 20f;
    [Range(0, 1)] [SerializeField] private float minVolume = 0.8f;
    [Range(0, 1)] [SerializeField] private float maxVolume = 1f;
    [Range(0.99f, 1.01f)] [SerializeField] private float minPitch = 0.995f;
    [Range(0.99f, 1.01f)] [SerializeField] private float maxPitch = 1.005f;

    // Определяем диапазон расстояний, в пределах которого будет происходить случайный разброс (scatter) аудиоисточников вокруг текущей позиции объекта и
    //тип кривой аттенюации.
    [Header("Spatial Settings")]
    [SerializeField] private bool enable3D = true;
    [Range(0, 500)] [SerializeField] private float minScatterDistance = 20f;
    [Range(0, 500)] [SerializeField] private float maxScatterDistance = 50f;
    [SerializeField] private AudioRolloffMode audioRolloff;
    private float dopplerLevel = 0f;




    //Определение массива клипов,который будет использоваться для воспроизведения.
    private List<AudioSource> audioSourcesPool = new List<AudioSource>();
    private int lastClipIndex = -1; // Индекс последнего воспроизведенного аудиоклипа для предотвращения повтора одного и того же клипа подряд.


    //Создаем переменную, которая хранит ссылку на корутину, которая используется для управления воспроизведением звуков с задержкой.
    //Корутина управляет временными интервалами между воспроизведениями.
    private Coroutine playAudioCoroutine;

    //Инициализация аудиосорсов, проверка на корректность аудиоклипов и задержек, а также запуск корутины для воспроизведения клипов с задержкой.
    //Проверка валидности аудиоклипов и задержек перед началом воспроизведения.
    void Start()
    {
        ValidateAudioSource();

        InitializeAudioSources(poolSize);

        if (!ValidateAudioClips() || !ValidateDelayValues() || !ValidateDistanceValue()) return;

        playAudioCoroutine = StartCoroutine(PlayAudioClipsWithDelay());
    }

    //Метод для получения случайного индекса аудиоклипа, отличного от предыдущего воспроизведенного,
    //если количество клипов меньше двух, метод возвращает 0, чтобы избежать бесконечного цикла.
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



    //Метод инициализации пула аудиосорсов,источники добавляются в пул по мере необходимости.
    private void InitializeAudioSources(int numberOfSources)
    {
        for (int i = 0; i < numberOfSources; i++)
        {
            audioSourcesPool.Add(CreateNewAudioSource());

        }


    }


    //Метод проверки на наличие компонента AudioSourse
    private void ValidateAudioSource()
    {

        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = audioGroup;
        }

    }
    //Метод для проверки наличия аудиоклипов.
    private bool ValidateAudioClips()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogError("Audio clips array is empty or not assigned!");
            return false;
        }
        return true;
    }
    //Метод проверки корректности значений задержки.
    private bool ValidateDelayValues()
    {
        if (minDelay > maxDelay)
        {
            Debug.LogError("minDelay should not be greater than maxDelay!");
            return false;
        }
        return true;
    }
    // Метод проверки корректности значений дистанции разброса.
    private bool ValidateDistanceValue()
    {
        if(minScatterDistance > maxScatterDistance)
        {
            Debug.LogError("minScatterDistance should not be greater than maxScatterDistance!");
            return false;
        }
        return true;
    }
    
    //Функция для создания нового аудиосорса.
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
        newSource.maxDistance = maxScatterDistance; // используется для установки расстояния, на котором звук полностью затухает.


        return newSource;
    }

    //Метод для получения доступного аудиосорса из пула. Если все источники заняты и их количество меньше poolSize, создается новый источник.
    //Если все источники заняты и достигнуто максимальное количество, возвращается первый источник из пула.
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

    //Метод для получения случайной позиции в заданном диапазоне расстояний от текущей позиции объекта.
    //Создает эффект случайного разброса источников звука в 3D-пространстве.
    private Vector3 GetRandomPosition(float minDistance, float maxDistance)
    {
        Vector3 randomDirection = Random.onUnitSphere;
        float randomDistance = Random.Range(minDistance, maxDistance);
        return transform.position + randomDirection * randomDistance;
    }

    
    //Корутина для плавного перемещения аудиосорса к целевой позиции."float duration" контролирует время перемещения, позиция источника изменяется каждый кадр.
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

  

    // Корутина для удаления аудиосорса из пула после того, как клип завершил воспроизведение. Он проверяет, завершился ли клип, и удаляет источник только после этого.
    private IEnumerator DestroyAudioSourceAfterClip(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            float clipLength = source.clip.length;

            // Использовать флаг для отслеживания завершения клипа
            bool clipEnded = false;

            // Проверить, завершился ли клип
            while (clipLength > 0)
            {
                yield return null; // Ожидание одного кадра
                clipLength -= Time.deltaTime; // Уменьшить оставшееся продолжительность клипа
                if (clipLength <= 0)
                {
                    clipEnded = true;
                }
            }

            // Подождать небольшую задержку после завершения
            yield return new WaitForSeconds(0.1f);

            // Проверить, что аудиосорс не был изменен и клип действительно завершился
            if (source != null && !source.isPlaying && clipEnded)
            {
                source.Stop();
                source.clip = null;
                audioSourcesPool.Remove(source);
                Destroy(source.gameObject);
            }
        }
    }

    // Метод,  который определяет, сколько раз аудиоклип должен воспроизвестись на основе заданной вероятности (SpawnRate).
    private void PlayAudioSourceMultipleTimes(float probability)
    {
        
        float probabilityNormalized = Mathf.Clamp(probability / 100f, 0f, 5f);

        
        int maxRepetitions = Mathf.Clamp(Mathf.CeilToInt(probability / 100f), 1, 5);

        
        for (int i = 0; i < maxRepetitions; i++)
        {
            float effectiveProbability = Mathf.Max(probabilityNormalized - i, 0f); // Убеждаемся, что вероятность не отрицательна
            
            float randomValue = Random.value;

            if (randomValue <= effectiveProbability)
            {
                PlayAudioClip(); 
            }
        }
    }


    //Метод отвечает за настройку аудиосорса (позиция, громкость, высота звука) и воспроизведение клипа.
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
                StartCoroutine(MoveAudioSourceSmoothly(source, newStartPosition, 10f)); // 10f выбрано как оптимальное решение между скоростью перемещения и естественностью звучания.
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

    // Корутина отвечает за воспроизведение с случайными временными интервалами между запусками.
    private IEnumerator PlayAudioClipsWithDelay()
    {
        
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            PlayAudioSourceMultipleTimes(SpawnRate);

        }
    }

    //Метод останавливает текущую корутину воспроизведения, а также все источники в пуле. Все корутины в объекте также останавливаются.
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
