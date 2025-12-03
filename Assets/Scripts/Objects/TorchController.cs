using UnityEngine;

/// <summary>
/// Имитация мерцания огня в факеле.
/// Управляет интенсивностью источника света и, опционально, эмиссией материала.
/// </summary>
[DisallowMultipleComponent]
public class TorchController : MonoBehaviour
{
    [Header("Light")]
    [Tooltip("Источник света факела. Если не задан, будет взят первый Light на объекте или детях.")]
    [SerializeField] private Light targetLight;

    [Tooltip("Базовая интенсивность света (среднее значение). Если 0 или меньше — будет взято текущее значение Light.intensity в Start.")]
    [SerializeField] private float baseIntensity = 0f;

    [Tooltip("Максимальное отклонение яркости от базового значения.")]
    [SerializeField] private float intensityAmplitude = 0.5f;

    [Tooltip("Скорость изменения яркости (частота мерцания).")]
    [SerializeField] private float flickerSpeed = 2f;

    [Tooltip("Дополнительная вариативность через шум (Perlin noise).")]
    [SerializeField] private float noiseScale = 1f;

    [Tooltip("Начальное смещение по времени, чтобы разные факелы не мерцали синхронно.")]
    [SerializeField] private float timeOffset = 0f;

    [Header("Emission (опционально)")]
    [Tooltip("Рендерер, материал которого будет менять эмиссию. Если не задан, будет взят Renderer на объекте.")]
    [SerializeField] private Renderer emissionRenderer;

    [Tooltip("Имя свойства эмиссии в шейдере.")]
    [SerializeField] private string emissionProperty = "_EmissionColor";

    [Tooltip("Базовый цвет эмиссии. Если alpha <= 0 — будет взят текущий цвет эмиссии из материала.")]
    [ColorUsage(true, true)]
    [SerializeField] private Color baseEmissionColor = new Color(0f, 0f, 0f, 0f);

    [Tooltip("Множитель влияния мерцания на эмиссию (0 = не трогаем).")]
    [SerializeField, Range(0f, 5f)] private float emissionFlickerStrength = 1f;

    private Material _emissionMaterialInstance;
    private float _baseIntensityInternal;

    private void Reset()
    {
        // Автоподстановка компонентов при добавлении скрипта
        if (targetLight == null)
            targetLight = GetComponentInChildren<Light>();

        if (emissionRenderer == null)
            emissionRenderer = GetComponent<Renderer>();

        // Небольшой рандом для десинхронизации при добавлении
        timeOffset = Random.Range(0f, 100f);
    }

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponentInChildren<Light>();

        if (targetLight == null)
        {
            Debug.LogWarning($"{nameof(TorchController)}: Light не найден, мерцание яркости работать не будет.", this);
        }
        else
        {
            _baseIntensityInternal = baseIntensity > 0f ? baseIntensity : targetLight.intensity;
        }

        if (emissionRenderer != null)
        {
            // Создаём инстанс материала, чтобы не трогать sharedMaterial
            _emissionMaterialInstance = emissionRenderer.material;

            if (baseEmissionColor.a <= 0.0001f)
            {
                if (_emissionMaterialInstance.HasProperty(emissionProperty))
                {
                    baseEmissionColor = _emissionMaterialInstance.GetColor(emissionProperty);
                }
            }

            // Включаем эмиссию, если нужно
            _emissionMaterialInstance.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        float t = (Time.time + timeOffset) * flickerSpeed;

        // Perlin noise в [0..1] -> [-1..1]
        float noise = Mathf.PerlinNoise(t * noiseScale, 0.0f) * 2f - 1f;

        // Дополнительно можно добавить немного быстрых колебаний через синус
        float fastOscillation = Mathf.Sin(t * 2.37f) * 0.3f;

        float flickerValue = noise + fastOscillation;

        // Нормируем и применяем амплитуду
        float intensityOffset = flickerValue * intensityAmplitude;

        if (targetLight != null)
        {
            float newIntensity = _baseIntensityInternal + intensityOffset;
            targetLight.intensity = Mathf.Max(0f, newIntensity);
        }

        if (_emissionMaterialInstance != null && emissionFlickerStrength > 0f)
        {
            // Коэффициент изменения для эмиссии
            float emissionFactor = 1f + flickerValue * emissionFlickerStrength;
            emissionFactor = Mathf.Max(0f, emissionFactor);

            Color emissionColor = baseEmissionColor * emissionFactor;
            _emissionMaterialInstance.SetColor(emissionProperty, emissionColor);
        }
    }

    /// <summary>
    /// Позволяет в рантайме сменить базовую интенсивность (например, усилить факел бафом).
    /// </summary>
    public void SetBaseIntensity(float newBaseIntensity)
    {
        _baseIntensityInternal = Mathf.Max(0f, newBaseIntensity);
        baseIntensity = _baseIntensityInternal;
    }

    /// <summary>
    /// Позволяет задать новый базовый цвет эмиссии в рантайме.
    /// </summary>
    public void SetBaseEmissionColor(Color color)
    {
        baseEmissionColor = color;
    }
}
