using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TAREFA 1: Projétil disparado pelo jogador.
/// Desativa inimigos permanentemente ao colidir.
/// Configurável via Inspector com cor, velocidade e dano próprios.
/// </summary>
public class PlayerBullet : MonoBehaviour
{
    [Header("Configurações do Projétil")]
    [Tooltip("Velocidade do projétil em unidades por segundo")]
    public float speed = 15f;
    
    [Tooltip("Tempo de vida máximo do projétil em segundos")]
    public float lifetime = 3f;
    
    [Tooltip("Cor do projétil (diferente dos efeitos existentes)")]
    public Color bulletColor = new Color(1f, 0.3f, 0f, 1f); // Laranja por padrão
    
    [Header("Efeitos Visuais")]
    [Tooltip("Trail Renderer para rastro do projétil")]
    public TrailRenderer trailRenderer;
    
    [Tooltip("Light component para iluminação do projétil")]
    public Light bulletLight;
    
    // Timer interno para controle de lifetime
    private float currentTime = 0f;
    
    // Flag para evitar múltiplas colisões
    private bool hasHit = false;

    void Start()
    {
        // Aplica a cor configurada aos componentes visuais
        ApplyBulletColor();
        
        // Auto-destruir após lifetime para evitar objetos órfãos
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move o projétil para frente
        transform.position += transform.forward * speed * Time.deltaTime;
        
        // Controle de tempo de vida (redundante com Destroy, mas seguro)
        currentTime += Time.deltaTime;
        if (currentTime > lifetime)
        {
            DestroyBullet();
        }
    }

    /// <summary>
    /// Aplica a cor configurada aos componentes visuais do projétil
    /// </summary>
    void ApplyBulletColor()
    {
        // Aplica cor ao TrailRenderer se existir
        if (trailRenderer != null)
        {
            trailRenderer.startColor = bulletColor;
            trailRenderer.endColor = new Color(bulletColor.r, bulletColor.g, bulletColor.b, 0f);
        }
        
        // Aplica cor à luz se existir
        if (bulletLight != null)
        {
            bulletLight.color = bulletColor;
        }
        
        // Aplica cor ao renderer do objeto se existir
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = bulletColor;
            rend.material.SetColor("_EmissionColor", bulletColor * 2f);
        }
    }

    /// <summary>
    /// Chamado quando o projétil colide com algo físico
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        // Destrói ao colidir com qualquer objeto físico
        DestroyBullet();
    }

    /// <summary>
    /// Chamado quando o projétil entra em um trigger
    /// Detecta inimigos e os desativa permanentemente
    /// Detecta Boss e aplica dano (requer múltiplos tiros)
    /// </summary>
    void OnTriggerEnter(Collider col)
    {
        if (hasHit) return;
        
        // DESAFIO: Verifica se é um Boss primeiro
        BossAI bossAI = col.GetComponent<BossAI>();
        if (bossAI != null)
        {
            hasHit = true;
            
            // Aplica dano ao Boss (não desativa imediatamente)
            bossAI.TakeDamage(1);
            
            // Cria efeito visual de impacto
            CreateHitEffect();
            
            DestroyBullet();
            return;
        }
        
        // Tenta obter o componente AIAgent do objeto colidido (inimigo normal)
        AIAgent enemyAgent = col.GetComponent<AIAgent>();
        
        if (enemyAgent != null)
        {
            hasHit = true;
            
            // REQUISITO: Desativa inimigo permanentemente
            DisableEnemyPermanently(enemyAgent);
            
            // Cria efeito visual de impacto
            CreateHitEffect();
            
            DestroyBullet();
        }
    }

    /// <summary>
    /// Desativa o inimigo permanentemente.
    /// Integra com o sistema de IA existente para garantir que o inimigo
    /// não volte a agir.
    /// </summary>
    void DisableEnemyPermanently(AIAgent enemy)
    {
        // Desativa a IA do inimigo
        enemy.aiEnabled = false;
        
        // Marca o personagem como morto para evitar reativação
        if (enemy.character != null)
        {
            enemy.character.dead = true;
        }
        
        // Para qualquer movimento
        if (enemy.navAgent != null)
        {
            enemy.navAgent.Stop();
        }
        
        // Desativa a visão do inimigo
        if (enemy.aiSight != null)
        {
            enemy.aiSight.enabled = false;
        }
        
        // Desativa a luz de busca
        if (enemy.searchLight != null)
        {
            enemy.searchLight.enabled = false;
        }
        
        // Toca partículas de stun (feedback visual)
        if (enemy.stunParticles != null)
        {
            enemy.stunParticles.Play();
        }
        
        // Remove do contador de perseguidores se estava perseguindo
        if (enemy.chasing)
        {
            enemy.chasing = false;
            GameLogic.instance?.RemoveChaser();
        }
        
        // Escurece o material do inimigo para indicar desativação
        if (enemy.bodyRenderer != null)
        {
            enemy.bodyRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }

    /// <summary>
    /// Cria efeito visual no ponto de impacto
    /// </summary>
    void CreateHitEffect()
    {
        // Usa o sistema de efeitos existente se disponível
        if (EffectsManager.getInstance() != null && EffectsManager.getInstance().damageEffect != null)
        {
            GameObject effect = Instantiate(EffectsManager.getInstance().damageEffect);
            effect.transform.position = transform.position;
        }
    }

    /// <summary>
    /// Destrói o projétil de forma segura
    /// </summary>
    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
