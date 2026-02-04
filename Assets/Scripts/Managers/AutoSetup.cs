using UnityEngine;

/// <summary>
/// ENTREGA FINAL: Script de auto-setup que roda antes de qualquer outro script.
/// Garante que todos os componentes necessários existam na cena.
/// 
/// Este script usa [DefaultExecutionOrder(-100)] para executar primeiro.
/// </summary>
[DefaultExecutionOrder(-100)]
public class AutoSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        // Registrar para executar quando a cena carregar
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
        // Auto-setup após a cena carregar
        SetupGameSystems();
    }
    
    /// <summary>
    /// Configura todos os sistemas do jogo automaticamente.
    /// Chamado uma vez quando a cena carrega.
    /// </summary>
    static void SetupGameSystems()
    {
        // 1. Garantir que GameBootstrap existe
        if (FindObjectOfType<GameBootstrap>() == null)
        {
            GameObject bootstrapObj = new GameObject("GameBootstrap (Auto)");
            GameBootstrap bootstrap = bootstrapObj.AddComponent<GameBootstrap>();
            
            // Tentar carregar o prefab do PlayerBullet
            bootstrap.playerBulletPrefab = Resources.Load<GameObject>("PlayerBullet");
        }
        
        // 2. Garantir que DebugOverlay existe (será criado pelo singleton se necessário)
        var _ = DebugOverlay.Instance;
        
        // 3. Garantir que CameraShake existe (será criado pelo singleton se necessário)
        var __ = CameraShake.Instance;
        
        // 4. Garantir que o Player tem HoverAbility
        StealthPlayerController player = FindObjectOfType<StealthPlayerController>();
        if (player != null)
        {
            HoverAbility hover = player.GetComponent<HoverAbility>();
            if (hover == null)
            {
                player.gameObject.AddComponent<HoverAbility>();
            }
        }
    }
}
