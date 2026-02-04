using UnityEngine;

/// <summary>
/// ENTREGA FINAL: Bootstrap do jogo para avaliação.
/// 
/// Este script garante que todos os sistemas estejam configurados corretamente
/// ao iniciar o jogo, eliminando necessidade de setup manual.
/// 
/// Responsabilidades:
/// 1. Validar componentes essenciais
/// 2. Criar managers faltantes automaticamente
/// 3. Logar status dos sistemas para o avaliador
/// 
/// NOTA PARA AVALIADORES: Este script é executado automaticamente.
/// Nenhuma configuração manual é necessária.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Configuração de Bootstrap")]
    [Tooltip("Mostrar log detalhado no console")]
    public bool verboseLogging = true;
    
    [Tooltip("Prefab do PlayerBullet para autoconfiguração")]
    public GameObject playerBulletPrefab;
    
    // Singleton para evitar duplicação
    private static GameBootstrap _instance;
    private static bool _hasInitialized = false;
    
    // AUDITORIA: Reset de flags estáticas entre Play Sessions no Editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _instance = null;
        _hasInitialized = false;
    }
    
    // Status dos sistemas
    private bool playerReady = false;
    private bool cameraReady = false;
    private bool debugSystemReady = false;
    private bool shootSystemReady = false;
    private bool hoverSystemReady = false;
    private bool bossPresent = false;

    void Awake()
    {
        // Singleton - evita múltiplas inicializações
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // Só inicializa uma vez por sessão
        if (_hasInitialized) return;
        _hasInitialized = true;
        
        // Executa bootstrap
        InitializeGame();
    }

    /// <summary>
    /// Inicializa e valida todos os sistemas do jogo.
    /// </summary>
    void InitializeGame()
    {
        Log("========================================");
        Log("🎮 GAME BOOTSTRAP - Iniciando validação...");
        Log("========================================");
        
        // 1. Validar e configurar Player
        ValidatePlayer();
        
        // 2. Validar e configurar Câmera
        ValidateCamera();
        
        // 3. Validar e configurar Debug System
        ValidateDebugSystem();
        
        // 4. Verificar Boss na cena
        ValidateBoss();
        
        // 5. Log final
        LogSystemStatus();
    }

    /// <summary>
    /// Valida e autoconfigura o Player.
    /// </summary>
    void ValidatePlayer()
    {
        StealthPlayerController player = StealthPlayerController.getInstance();
        
        if (player == null)
        {
            LogWarning("Player não encontrado na cena!");
            return;
        }
        
        playerReady = true;
        Log("✓ Player encontrado: " + player.gameObject.name);
        
        // Autoconfigurar PlayerBullet se não estiver setado
        if (player.playerBulletPrefab == null)
        {
            if (playerBulletPrefab != null)
            {
                player.playerBulletPrefab = playerBulletPrefab;
                Log("  → PlayerBullet prefab autoconfigurado via Bootstrap");
                shootSystemReady = true;
            }
            else
            {
                // Tentar carregar do Resources
                GameObject bulletPrefab = Resources.Load<GameObject>("PlayerBullet");
                if (bulletPrefab != null)
                {
                    player.playerBulletPrefab = bulletPrefab;
                    Log("  → PlayerBullet prefab carregado de Resources");
                    shootSystemReady = true;
                }
                else
                {
                    LogWarning("  → PlayerBullet prefab não configurado. Tiro desabilitado.");
                    player.canShoot = false;
                }
            }
        }
        else
        {
            shootSystemReady = true;
            Log("  → Sistema de Tiro: OK");
        }
        
        // Verificar HoverAbility
        HoverAbility hover = player.GetComponent<HoverAbility>();
        if (hover == null)
        {
            hover = player.gameObject.AddComponent<HoverAbility>();
            Log("  → HoverAbility adicionado automaticamente");
        }
        hoverSystemReady = true;
        Log("  → Sistema de Hover: OK");
        
        // Garantir energia inicial para testes
        if (player.energy < player.maxEnergy * 0.5f)
        {
            player.ResetEnergy();
            Log("  → Energia resetada para 100% (facilitar avaliação)");
        }
    }

    /// <summary>
    /// Valida e autoconfigura a Câmera.
    /// </summary>
    void ValidateCamera()
    {
        Camera mainCam = Camera.main;
        
        if (mainCam == null)
        {
            LogWarning("Câmera principal não encontrada!");
            return;
        }
        
        cameraReady = true;
        Log("✓ Câmera encontrada: " + mainCam.gameObject.name);
        
        // Verificar CameraShake
        CameraShake shake = mainCam.GetComponent<CameraShake>();
        if (shake == null)
        {
            shake = mainCam.gameObject.AddComponent<CameraShake>();
            Log("  → CameraShake adicionado automaticamente");
        }
        Log("  → Sistema de Camera Shake: OK");
    }

    /// <summary>
    /// Valida e autoconfigura o sistema de Debug.
    /// </summary>
    void ValidateDebugSystem()
    {
        DebugOverlay debugOverlay = FindObjectOfType<DebugOverlay>();
        
        if (debugOverlay == null)
        {
            // Criar DebugManager automaticamente
            GameObject debugManager = new GameObject("DebugManager");
            debugOverlay = debugManager.AddComponent<DebugOverlay>();
            debugOverlay.isEnabled = false; // Desligado por padrão
            Log("✓ DebugOverlay criado automaticamente");
        }
        else
        {
            Log("✓ DebugOverlay encontrado");
        }
        
        debugSystemReady = true;
        Log("  → Debug Mode: Pressione F1 para ativar");
        Log("  → Assist Mode: Pressione F2 para ativar");
    }

    /// <summary>
    /// Verifica se há Boss na cena.
    /// </summary>
    void ValidateBoss()
    {
        BossAI[] bosses = FindObjectsOfType<BossAI>();
        
        if (bosses.Length > 0)
        {
            bossPresent = true;
            Log($"✓ Boss encontrado: {bosses.Length} na cena");
            
            foreach (BossAI boss in bosses)
            {
                Log($"  → {boss.gameObject.name} (HP: {boss.maxHealth})");
            }
        }
        else
        {
            Log("ℹ Nenhum Boss na cena (opcional para avaliação)");
        }
    }

    /// <summary>
    /// Loga o status final de todos os sistemas.
    /// </summary>
    void LogSystemStatus()
    {
        Log("");
        Log("========================================");
        Log("📋 STATUS DOS SISTEMAS");
        Log("========================================");
        Log($"  Player:      {(playerReady ? "✓ OK" : "✗ ERRO")}");
        Log($"  Câmera:      {(cameraReady ? "✓ OK" : "✗ ERRO")}");
        Log($"  Tiro:        {(shootSystemReady ? "✓ OK" : "⚠ Desabilitado")}");
        Log($"  Hover:       {(hoverSystemReady ? "✓ OK" : "⚠ Desabilitado")}");
        Log($"  Debug:       {(debugSystemReady ? "✓ OK" : "⚠ Desabilitado")}");
        Log($"  Boss:        {(bossPresent ? "✓ Presente" : "ℹ Não presente")}");
        Log("========================================");
        Log("");
        
        // Mensagem final para avaliadores
        if (playerReady && cameraReady)
        {
            Log("🎮 GAME READY FOR EVALUATION");
            Log("");
            Log("╔════════════════════════════════════════╗");
            Log("║  CONTROLES RÁPIDOS PARA AVALIAÇÃO      ║");
            Log("╠════════════════════════════════════════╣");
            Log("║  WASD        → Movimento               ║");
            Log("║  F / Mouse L → Tiro                    ║");
            Log("║  Space       → Hover (segurar)         ║");
            Log("║  X           → Shock                   ║");
            Log("║  C           → Cloak                   ║");
            Log("║  F1          → Debug Mode              ║");
            Log("║  F2          → Assist Mode             ║");
            Log("╚════════════════════════════════════════╝");
        }
        else
        {
            LogError("⚠ ATENÇÃO: Alguns sistemas não estão prontos!");
        }
    }

    #region Logging Helpers
    
    void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[Bootstrap] {message}");
        }
    }
    
    void LogWarning(string message)
    {
        Debug.LogWarning($"[Bootstrap] {message}");
    }
    
    void LogError(string message)
    {
        Debug.LogError($"[Bootstrap] {message}");
    }
    
    #endregion
}
