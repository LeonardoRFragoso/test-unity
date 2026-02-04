using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DESAFIO: Boss Enemy
/// Adapta o sistema de IA existente (AIAgent) para criar um inimigo Boss.
/// 
/// Características do Boss:
/// - Maior resistência (requer múltiplos tiros para derrotar)
/// - Área de ataque maior
/// - Pode ser derrotado OU contornado
/// - Exige uso das novas habilidades (tiro e/ou hover)
/// 
/// Design Decision: Extende o AIAgent existente em vez de criar sistema novo,
/// reutilizando estados, comportamentos e navegação já implementados.
/// </summary>
[RequireComponent(typeof(AIAgent))]
public class BossAI : MonoBehaviour
{
    [Header("Configurações do Boss")]
    [Tooltip("Vida total do Boss (número de tiros necessários para derrotar)")]
    public int maxHealth = 5;
    
    [Tooltip("Vida atual do Boss")]
    public int currentHealth;
    
    [Tooltip("Tempo de invulnerabilidade após levar dano")]
    public float invulnerabilityTime = 0.5f;
    
    [Tooltip("Boss está invulnerável?")]
    private bool isInvulnerable = false;
    
    [Header("Comportamento de Combate")]
    [Tooltip("Multiplicador de velocidade de perseguição")]
    public float chaseSpeedMultiplier = 1.5f;
    
    [Tooltip("Multiplicador de alcance de visão")]
    public float sightRangeMultiplier = 1.5f;
    
    [Tooltip("Intervalo entre disparos (segundos)")]
    public float fireInterval = 1.5f;
    
    [Tooltip("Número de tiros em rajada")]
    public int burstCount = 3;
    
    [Tooltip("Intervalo entre tiros da rajada")]
    public float burstInterval = 0.2f;
    
    [Header("Área de Perigo (requer Hover para atravessar)")]
    [Tooltip("Ativa área de perigo ao redor do Boss")]
    public bool enableDangerZone = true;
    
    [Tooltip("Raio da área de perigo")]
    public float dangerZoneRadius = 3f;
    
    [Tooltip("Dano por segundo na área de perigo")]
    public float dangerZoneDamage = 5f;
    
    [Tooltip("Hover protege da área de perigo?")]
    public bool hoverProtectsFromDangerZone = true;
    
    [Header("Efeitos Visuais")]
    [Tooltip("Cor de emissão do Boss")]
    public Color bossEmissionColor = Color.red;
    
    [Tooltip("Partículas de dano do Boss")]
    public ParticleSystem damageParticles;
    
    [Tooltip("Partículas de morte do Boss")]
    public ParticleSystem deathParticles;
    
    [Tooltip("Escala do Boss (maior que inimigos normais)")]
    public float bossScale = 1.5f;
    
    // Referências internas
    private AIAgent aiAgent;
    private Character character;
    private StealthPlayerController player;
    
    // Estado interno
    private bool isDead = false;
    private float lastDamageTime = 0f;
    
    // Cache do ChaseState original para modificar
    private float originalChaseSpeed;
    private float originalShootingTimer;

    void Awake()
    {
        aiAgent = GetComponent<AIAgent>();
        character = GetComponent<Character>();
        
        // Aplica escala do Boss
        transform.localScale = Vector3.one * bossScale;
    }

    void Start()
    {
        player = StealthPlayerController.getInstance();
        currentHealth = maxHealth;
        
        // Modifica parâmetros do AIAgent para comportamento de Boss
        ConfigureBossAI();
        
        // Aplica cor de emissão do Boss
        if (aiAgent.bodyRenderer != null)
        {
            aiAgent.bodyRenderer.material.SetColor("_EmissionColor", bossEmissionColor);
        }
        
        // Aumenta a luz de busca para o Boss
        if (aiAgent.searchLight != null)
        {
            aiAgent.searchLight.color = bossEmissionColor;
            aiAgent.searchLight.intensity *= 1.5f;
            aiAgent.searchLight.range *= sightRangeMultiplier;
        }
    }

    /// <summary>
    /// Configura os parâmetros do AIAgent para comportamento de Boss.
    /// Reutiliza o sistema existente com valores modificados.
    /// </summary>
    void ConfigureBossAI()
    {
        // Aumenta alcance de visão
        if (aiAgent.aiSight != null)
        {
            aiAgent.aiSight.viewDistance *= sightRangeMultiplier;
            aiAgent.aiSight.viewAngle *= 1.2f; // Ângulo de visão ligeiramente maior
        }
        
        // Modifica o ChaseState para comportamento mais agressivo
        if (aiAgent.chasingState != null)
        {
            originalChaseSpeed = aiAgent.chasingState.chaseSpeed;
            originalShootingTimer = aiAgent.chasingState.shootingTimer;
            
            aiAgent.chasingState.chaseSpeed *= chaseSpeedMultiplier;
            aiAgent.chasingState.shootingTimer = fireInterval;
        }
        
        // Boss não pode ser drenado facilmente
        if (character != null)
        {
            character.maxDrainEnergy *= 3f; // Requer mais tempo para drenar
            character.energyLeft = character.maxDrainEnergy;
        }
    }

    void Update()
    {
        if (isDead || GameLogic.instance == null || GameLogic.instance.gameState != GameLogic.GameStates.gameplay)
        {
            return;
        }
        
        // Área de perigo ao redor do Boss
        if (enableDangerZone)
        {
            CheckDangerZone();
        }
        
        // Atualiza invulnerabilidade
        if (isInvulnerable && Time.time - lastDamageTime > invulnerabilityTime)
        {
            isInvulnerable = false;
        }
    }

    /// <summary>
    /// Verifica se o jogador está na área de perigo do Boss.
    /// Hover protege o jogador da área de perigo.
    /// </summary>
    void CheckDangerZone()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        if (distanceToPlayer < dangerZoneRadius)
        {
            // Verifica se o jogador está usando Hover (protegido)
            HoverAbility playerHover = player.hoverAbility;
            bool isProtected = hoverProtectsFromDangerZone && playerHover != null && playerHover.IsHovering;
            
            if (!isProtected && !player.cloaked)
            {
                // Aplica dano contínuo ao jogador
                float damage = dangerZoneDamage * Time.deltaTime;
                player.DealDamage(damage);
            }
        }
    }

    /// <summary>
    /// Chamado quando o Boss é atingido por um projétil do jogador.
    /// Integra com o sistema PlayerBullet.
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        if (isDead || isInvulnerable) return;
        
        currentHealth -= damage;
        lastDamageTime = Time.time;
        isInvulnerable = true;
        
        // Feedback visual de dano
        StartCoroutine(DamageFlashRoutine());
        
        // Partículas de dano
        if (damageParticles != null)
        {
            damageParticles.Play();
        }
        
        // Toca som de dano se disponível
        if (aiAgent.audioSource != null)
        {
            // Pode adicionar som específico de dano do Boss aqui
        }
        
        Debug.Log($"[Boss] Recebeu dano! Vida: {currentHealth}/{maxHealth}");
        
        // Verifica se morreu
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Boss fica mais agressivo quando ferido
            OnDamaged();
        }
    }

    /// <summary>
    /// Chamado quando o Boss leva dano mas não morre.
    /// Aumenta agressividade.
    /// TAREFA 2: Adiciona feedback visual/sonoro quando Boss fica agressivo.
    /// </summary>
    void OnDamaged()
    {
        // Aumenta velocidade de perseguição temporariamente
        if (aiAgent.chasingState != null)
        {
            aiAgent.chasingState.chaseSpeed = originalChaseSpeed * chaseSpeedMultiplier * 1.2f;
            aiAgent.chasingState.shootingTimer = fireInterval * 0.7f;
        }
        
        // Se não estava perseguindo, começa a perseguir
        if (aiAgent.currentStateType != AIAgent.StateType.chasing && player != null)
        {
            aiAgent.target = player.transform;
            aiAgent.setState(aiAgent.chasingState);
            
            // TAREFA 2: Feedback visual/sonoro quando Boss entra em modo agressivo
            OnBossEnraged();
        }
    }
    
    /// <summary>
    /// TAREFA 2: Feedback quando o Boss entra em estado agressivo.
    /// Alerta visual e sonoro para o jogador.
    /// </summary>
    void OnBossEnraged()
    {
        // Mensagem de alerta
        ConsoleText console = ConsoleText.getInstance();
        if (console != null)
        {
            console.ShowMessage("BOSS ENFURECIDO!");
        }
        
        // Camera shake forte
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeHeavy();
        }
        
        // Som de alerta (usa o audioSource do AIAgent)
        if (aiAgent.audioSource != null && AudioManager.getInstance() != null)
        {
            aiAgent.audioSource.PlayOneShot(AudioManager.getInstance().enemyAlert);
        }
        
        // Flash visual intenso no Boss
        StartCoroutine(EnrageFlashRoutine());
    }
    
    /// <summary>
    /// Coroutine para flash visual de rage.
    /// </summary>
    IEnumerator EnrageFlashRoutine()
    {
        if (aiAgent.bodyRenderer == null) yield break;
        
        // Flash rápido vermelho-branco
        for (int i = 0; i < 3; i++)
        {
            aiAgent.bodyRenderer.material.SetColor("_EmissionColor", Color.white * 3f);
            yield return new WaitForSeconds(0.05f);
            aiAgent.bodyRenderer.material.SetColor("_EmissionColor", Color.red * 2f);
            yield return new WaitForSeconds(0.05f);
        }
        
        // Volta para cor de dano
        // AUDITORIA: Proteção contra divisão por zero
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        Color damagedColor = Color.Lerp(Color.red, bossEmissionColor, healthPercent);
        aiAgent.bodyRenderer.material.SetColor("_EmissionColor", damagedColor);
    }

    /// <summary>
    /// Coroutine para flash visual de dano.
    /// </summary>
    IEnumerator DamageFlashRoutine()
    {
        if (aiAgent.bodyRenderer != null)
        {
            Color originalColor = bossEmissionColor;
            
            // Flash branco
            aiAgent.bodyRenderer.material.SetColor("_EmissionColor", Color.white * 2f);
            yield return new WaitForSeconds(0.1f);
            
            // Volta para cor do Boss (mais intensa conforme perde vida)
            // AUDITORIA: Proteção contra divisão por zero
            float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            Color damagedColor = Color.Lerp(Color.red, bossEmissionColor, healthPercent);
            aiAgent.bodyRenderer.material.SetColor("_EmissionColor", damagedColor);
        }
    }

    /// <summary>
    /// Chamado quando o Boss é derrotado.
    /// </summary>
    void Die()
    {
        isDead = true;
        
        Debug.Log("[Boss] Boss derrotado!");
        
        // Desativa a IA
        aiAgent.aiEnabled = false;
        character.dead = true;
        
        // Para movimento
        if (aiAgent.navAgent != null)
        {
            aiAgent.navAgent.Stop();
        }
        
        // Desativa colisões de perigo
        enableDangerZone = false;
        
        // Partículas de morte
        if (deathParticles != null)
        {
            deathParticles.Play();
        }
        
        // Desativa luz
        if (aiAgent.searchLight != null)
        {
            aiAgent.searchLight.enabled = false;
        }
        
        // Escurece o material
        if (aiAgent.bodyRenderer != null)
        {
            aiAgent.bodyRenderer.material.SetColor("_EmissionColor", Color.black);
        }
        
        // Remove do contador de perseguidores
        if (aiAgent.chasing)
        {
            aiAgent.chasing = false;
            GameLogic.instance.RemoveChaser();
        }
        
        // Mostra mensagem de vitória
        ConsoleText.getInstance()?.ShowMessage("BOSS DERROTADO!");
    }

    /// <summary>
    /// Verifica se o Boss foi derrotado (para uso externo).
    /// </summary>
    public bool IsDefeated()
    {
        return isDead;
    }

    /// <summary>
    /// Retorna a porcentagem de vida do Boss.
    /// </summary>
    public float GetHealthPercent()
    {
        // AUDITORIA: Proteção contra divisão por zero
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }

    void OnDrawGizmosSelected()
    {
        // Visualiza área de perigo
        if (enableDangerZone)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, dangerZoneRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, dangerZoneRadius);
        }
    }
}
