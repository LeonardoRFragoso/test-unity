using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TAREFA 2: Habilidade de Hover (Flutuação)
/// Permite ao jogador flutuar levemente acima do chão enquanto segura um botão.
/// Consome energia continuamente (similar ao Cloak).
/// Desacoplado e configurável via Inspector.
/// </summary>
public class HoverAbility : MonoBehaviour
{
    [Header("Configurações de Hover")]
    [Tooltip("Altura de flutuação acima do chão")]
    public float hoverHeight = 0.5f;
    
    [Tooltip("Velocidade de subida/descida durante hover")]
    public float hoverTransitionSpeed = 3f;
    
    [Tooltip("Multiplicador de consumo de energia durante hover")]
    public float hoverEnergyMultiplier = 2.5f;
    
    [Tooltip("Força de sustentação aplicada ao Rigidbody")]
    public float hoverForce = 15f;
    
    [Header("Detecção de Chão")]
    [Tooltip("Distância máxima para detectar o chão")]
    public float groundCheckDistance = 1.5f;
    
    [Tooltip("Layers consideradas como chão")]
    public LayerMask groundLayer = ~0; // Todas as layers por padrão
    
    [Tooltip("Offset do raycast de detecção de chão")]
    public Vector3 groundCheckOffset = Vector3.zero;
    
    [Header("Efeitos Visuais")]
    [Tooltip("Sistema de partículas para efeito de hover")]
    public ParticleSystem hoverParticles;
    
    [Tooltip("Cor das partículas de hover")]
    public Color hoverParticleColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    
    [Header("Efeitos Sonoros")]
    [Tooltip("AudioSource para som de hover")]
    public AudioSource hoverAudioSource;
    
    [Tooltip("AudioClip do som de hover (loop)")]
    public AudioClip hoverSound;
    
    [Tooltip("Volume do som de hover")]
    [Range(0f, 1f)]
    public float hoverSoundVolume = 0.5f;
    
    // Referências internas
    private StealthPlayerController playerController;
    private Rigidbody playerRb;
    
    // Estado do hover
    private bool isHovering = false;
    private bool wasGrounded = true;
    private float currentHoverHeight = 0f;
    private float targetHoverHeight = 0f;
    
    // Cache para otimização
    private RaycastHit groundHit;
    
    /// <summary>
    /// Propriedade pública para verificar se está em hover
    /// </summary>
    public bool IsHovering => isHovering;

    void Start()
    {
        // Obtém referências necessárias
        playerController = GetComponent<StealthPlayerController>();
        if (playerController != null)
        {
            playerRb = playerController.rb;
        }
        else
        {
            playerRb = GetComponent<Rigidbody>();
        }
        
        // Configura partículas se existirem
        if (hoverParticles != null)
        {
            var main = hoverParticles.main;
            main.startColor = hoverParticleColor;
            hoverParticles.Stop();
        }
        
        // Configura áudio se existir
        if (hoverAudioSource != null && hoverSound != null)
        {
            hoverAudioSource.clip = hoverSound;
            hoverAudioSource.loop = true;
            hoverAudioSource.volume = hoverSoundVolume;
        }
    }

    void Update()
    {
        // Não processa se o jogo não estiver em gameplay
        if (GameLogic.instance.gameState != GameLogic.GameStates.gameplay)
        {
            if (isHovering)
            {
                StopHover();
            }
            return;
        }
        
        // Verifica input de hover (botão Space ou outro configurável)
        bool hoverInput = Input.GetButton("Jump");
        
        if (hoverInput && CanStartHover())
        {
            if (!isHovering)
            {
                StartHover();
            }
            MaintainHover();
        }
        else if (isHovering)
        {
            StopHover();
        }
        
        // Atualiza estado de grounded
        wasGrounded = IsGrounded();
    }

    void FixedUpdate()
    {
        if (isHovering && playerRb != null)
        {
            ApplyHoverPhysics();
        }
    }

    /// <summary>
    /// Verifica se o jogador pode iniciar o hover.
    /// Requisitos: estar no chão, ter energia, não estar em estado inválido.
    /// </summary>
    bool CanStartHover()
    {
        // Deve estar no chão para iniciar (mas pode manter se já estiver hovering)
        if (!isHovering && !IsGrounded())
        {
            return false;
        }
        
        // Verifica se tem energia suficiente
        if (playerController != null && playerController.energy <= 0)
        {
            return false;
        }
        
        // Verifica se não está em estado que impede hover
        if (playerController != null)
        {
            if (playerController.state == Character.States.attacking ||
                playerController.state == Character.States.shocking ||
                playerController.state == Character.States.hitStun)
            {
                return false;
            }
            
            // Não pode hover enquanto cloaked
            if (playerController.cloaked)
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Verifica se o jogador está no chão usando raycast.
    /// </summary>
    bool IsGrounded()
    {
        Vector3 rayOrigin = transform.position + groundCheckOffset;
        return Physics.Raycast(rayOrigin, Vector3.down, out groundHit, groundCheckDistance, groundLayer);
    }

    /// <summary>
    /// Inicia o estado de hover.
    /// </summary>
    void StartHover()
    {
        isHovering = true;
        targetHoverHeight = hoverHeight;
        
        // Inicia partículas
        if (hoverParticles != null && !hoverParticles.isPlaying)
        {
            hoverParticles.Play();
        }
        
        // Inicia som
        if (hoverAudioSource != null && hoverSound != null && !hoverAudioSource.isPlaying)
        {
            hoverAudioSource.Play();
        }
        
        Debug.Log("[Hover] Hover iniciado.");
    }

    /// <summary>
    /// Mantém o estado de hover, consumindo energia.
    /// </summary>
    void MaintainHover()
    {
        if (playerController == null) return;
        
        // Consome energia continuamente (integrado com sistema existente)
        // O consumo é baseado no energyDrainSpeed do player multiplicado pelo hoverEnergyMultiplier
        float energyDrain = playerController.energyDrainSpeed * hoverEnergyMultiplier * Time.deltaTime;
        
        // Verifica se tem energia suficiente
        if (playerController.energy - energyDrain <= 0)
        {
            StopHover();
            return;
        }
        
        // O consumo de energia já é feito pelo sistema principal do StealthPlayerController
        // Aqui apenas marcamos que estamos em hover para o multiplicador ser aplicado
    }

    /// <summary>
    /// Para o estado de hover.
    /// </summary>
    void StopHover()
    {
        isHovering = false;
        targetHoverHeight = 0f;
        
        // Para partículas
        if (hoverParticles != null && hoverParticles.isPlaying)
        {
            hoverParticles.Stop();
        }
        
        // Para som
        if (hoverAudioSource != null && hoverAudioSource.isPlaying)
        {
            hoverAudioSource.Stop();
        }
        
        Debug.Log("[Hover] Hover encerrado.");
    }

    /// <summary>
    /// Aplica física de hover ao Rigidbody.
    /// Mantém o jogador flutuando a uma altura constante.
    /// </summary>
    void ApplyHoverPhysics()
    {
        if (playerRb == null) return;
        
        // Detecta distância até o chão
        Vector3 rayOrigin = transform.position + groundCheckOffset;
        if (Physics.Raycast(rayOrigin, Vector3.down, out groundHit, groundCheckDistance + hoverHeight, groundLayer))
        {
            float currentDistanceToGround = groundHit.distance;
            float targetDistance = hoverHeight;
            
            // Calcula força necessária para manter a altura
            float heightError = targetDistance - currentDistanceToGround;
            
            // Aplica força proporcional ao erro de altura
            // Também cancela a gravidade
            float verticalForce = (heightError * hoverForce) + Mathf.Abs(Physics.gravity.y);
            
            // Limita a força para evitar movimentos bruscos
            verticalForce = Mathf.Clamp(verticalForce, -hoverForce, hoverForce * 2);
            
            // Aplica a força
            playerRb.AddForce(Vector3.up * verticalForce, ForceMode.Acceleration);
            
            // Amortece velocidade vertical para hover suave
            Vector3 vel = playerRb.velocity;
            vel.y *= 0.9f;
            playerRb.velocity = vel;
        }
    }

    /// <summary>
    /// Retorna o multiplicador de energia para ser usado pelo StealthPlayerController.
    /// </summary>
    public float GetEnergyMultiplier()
    {
        return isHovering ? hoverEnergyMultiplier : 1f;
    }

    void OnDrawGizmosSelected()
    {
        // Visualiza o raycast de detecção de chão
        Gizmos.color = Color.green;
        Vector3 rayOrigin = transform.position + groundCheckOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        
        // Visualiza altura de hover
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * (groundCheckDistance - hoverHeight));
    }
}
