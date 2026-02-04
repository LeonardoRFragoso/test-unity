using UnityEngine;

/// <summary>
/// TAREFA 2: Camera Shake - Adiciona "juice" ao gameplay através de screen shake.
/// Singleton para fácil acesso de qualquer script.
/// 
/// Uso: CameraShake.Instance.Shake(intensity, duration);
/// 
/// DECISÃO TÉCNICA: Usa Perlin Noise para movimento mais orgânico
/// em vez de valores aleatórios puros.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Configurações Padrão")]
    [Tooltip("Intensidade padrão do shake")]
    public float defaultIntensity = 0.1f;
    
    [Tooltip("Duração padrão do shake em segundos")]
    public float defaultDuration = 0.15f;
    
    [Tooltip("Velocidade do ruído Perlin")]
    public float noiseSpeed = 25f;
    
    [Header("Limites")]
    [Tooltip("Intensidade máxima permitida")]
    public float maxIntensity = 0.5f;
    
    // Singleton com auto-criação defensiva
    private static CameraShake _instance;
    private static bool _searchedForInstance = false;
    
    /// <summary>
    /// TAREFA 2: Singleton com fallback automático.
    /// Cria automaticamente se não existir, eliminando necessidade de setup manual.
    /// </summary>
    public static CameraShake Instance
    {
        get
        {
            if (_instance == null && !_searchedForInstance)
            {
                _searchedForInstance = true;
                _instance = FindObjectOfType<CameraShake>();
                
                // Auto-criar se não existir (plug-and-play)
                if (_instance == null)
                {
                    Camera mainCam = Camera.main;
                    if (mainCam != null)
                    {
                        _instance = mainCam.gameObject.AddComponent<CameraShake>();
                        Debug.Log("[CameraShake] Criado automaticamente na câmera principal.");
                    }
                }
            }
            return _instance;
        }
    }
    
    // Estado interno
    private Vector3 originalPosition;
    private float currentIntensity = 0f;
    private float currentDuration = 0f;
    private float shakeTimer = 0f;
    private bool isShaking = false;
    private float noiseSeed;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Seed aleatório para Perlin Noise
        noiseSeed = Random.Range(0f, 1000f);
    }

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (!isShaking) return;
        
        shakeTimer += Time.deltaTime;
        
        if (shakeTimer >= currentDuration)
        {
            // Terminou o shake
            StopShake();
            return;
        }
        
        // Calcula intensidade com fade out suave
        float progress = shakeTimer / currentDuration;
        float fadeMultiplier = 1f - (progress * progress); // Ease out quadrático
        float intensity = currentIntensity * fadeMultiplier;
        
        // Usa Perlin Noise para movimento mais orgânico
        float time = Time.time * noiseSpeed;
        float offsetX = (Mathf.PerlinNoise(noiseSeed, time) - 0.5f) * 2f * intensity;
        float offsetY = (Mathf.PerlinNoise(noiseSeed + 100, time) - 0.5f) * 2f * intensity;
        
        // Aplica offset
        transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
    }

    /// <summary>
    /// Inicia um screen shake com parâmetros customizados.
    /// </summary>
    /// <param name="intensity">Intensidade do shake (distância máxima)</param>
    /// <param name="duration">Duração em segundos</param>
    public void Shake(float intensity, float duration)
    {
        // Limita intensidade
        intensity = Mathf.Min(intensity, maxIntensity);
        
        // Se já está shaking, usa o maior valor
        if (isShaking)
        {
            currentIntensity = Mathf.Max(currentIntensity, intensity);
            currentDuration = Mathf.Max(currentDuration - shakeTimer, duration);
            shakeTimer = 0f;
        }
        else
        {
            currentIntensity = intensity;
            currentDuration = duration;
            shakeTimer = 0f;
            isShaking = true;
            originalPosition = transform.localPosition;
        }
        
        // Novo seed para variação
        noiseSeed = Random.Range(0f, 1000f);
    }

    /// <summary>
    /// Inicia um screen shake com valores padrão.
    /// </summary>
    public void Shake()
    {
        Shake(defaultIntensity, defaultDuration);
    }

    /// <summary>
    /// Shake específico para tiro do jogador.
    /// </summary>
    public void ShakeOnShoot()
    {
        Shake(0.05f, 0.1f);
    }

    /// <summary>
    /// Shake para quando o jogador leva dano.
    /// </summary>
    public void ShakeOnDamage()
    {
        Shake(0.15f, 0.2f);
    }

    /// <summary>
    /// Shake forte para eventos importantes (Boss, explosões).
    /// </summary>
    public void ShakeHeavy()
    {
        Shake(0.3f, 0.3f);
    }

    /// <summary>
    /// Para o shake imediatamente.
    /// </summary>
    public void StopShake()
    {
        isShaking = false;
        transform.localPosition = originalPosition;
    }
}
