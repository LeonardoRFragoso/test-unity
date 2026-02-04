using UnityEngine;

/// <summary>
/// TAREFA 3: Classe utilitária com métodos auxiliares para validação de gameplay.
/// Centraliza verificações comuns para evitar duplicação de código e facilitar manutenção.
/// 
/// PONTO DE EXTENSÃO: Adicionar novos métodos de validação conforme necessário.
/// Exemplo: ValidateAmmo(), ValidateCooldown(), etc.
/// </summary>
public static class GameplayUtilities
{
    #region Energy Validation
    
    /// <summary>
    /// Verifica se o jogador tem energia suficiente para uma ação.
    /// </summary>
    /// <param name="player">Referência ao player controller</param>
    /// <param name="requiredEnergy">Quantidade de energia necessária</param>
    /// <param name="showFeedback">Se deve mostrar feedback ao jogador quando falhar</param>
    /// <returns>True se tem energia suficiente</returns>
    public static bool HasEnoughEnergy(StealthPlayerController player, float requiredEnergy, bool showFeedback = true)
    {
        if (player == null) return false;
        
        bool hasEnergy = player.energy >= requiredEnergy;
        
        if (!hasEnergy && showFeedback)
        {
            OnInsufficientEnergy(player);
        }
        
        return hasEnergy;
    }
    
    /// <summary>
    /// Callback chamado quando energia é insuficiente.
    /// Centraliza feedback visual/sonoro para falta de energia.
    /// </summary>
    private static void OnInsufficientEnergy(StealthPlayerController player)
    {
        // Feedback visual - flash vermelho na barra de energia
        if (player.energyBar != null)
        {
            player.energyBar.Flash(Color.red, 0.2f);
        }
        
        // Mensagem no console do jogo
        ConsoleText console = ConsoleText.getInstance();
        if (console != null)
        {
            console.ShowMessage("Energia insuficiente!");
        }
        
        // Log para debug
        Debug.Log("[GameplayUtilities] Ação bloqueada: energia insuficiente.");
    }
    
    /// <summary>
    /// Calcula a porcentagem de energia atual do jogador.
    /// </summary>
    public static float GetEnergyPercent(StealthPlayerController player)
    {
        if (player == null || player.maxEnergy <= 0) return 0f;
        return Mathf.Clamp01(player.energy / player.maxEnergy);
    }
    
    /// <summary>
    /// Verifica se o jogador está com energia baixa (threshold configurável).
    /// </summary>
    public static bool IsLowEnergy(StealthPlayerController player, float threshold = 0.2f)
    {
        return GetEnergyPercent(player) <= threshold;
    }
    
    #endregion
    
    #region Player State Validation
    
    /// <summary>
    /// Verifica se o jogador pode executar uma habilidade.
    /// Considera estado atual, energia e se o jogo está em gameplay.
    /// </summary>
    /// <param name="player">Referência ao player controller</param>
    /// <param name="requiredEnergy">Energia necessária (0 para não verificar)</param>
    /// <returns>True se pode executar a habilidade</returns>
    public static bool CanUseAbility(StealthPlayerController player, float requiredEnergy = 0f)
    {
        // Null check
        if (player == null) return false;
        
        // Verifica estado do jogo
        if (!IsGameplayActive()) return false;
        
        // Verifica estado do jogador
        if (!IsPlayerStateValid(player)) return false;
        
        // Verifica energia se necessário
        if (requiredEnergy > 0 && !HasEnoughEnergy(player, requiredEnergy))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Verifica se o estado atual do jogador permite usar habilidades.
    /// </summary>
    public static bool IsPlayerStateValid(StealthPlayerController player)
    {
        if (player == null) return false;
        
        // Estados que bloqueiam uso de habilidades
        Character.States state = player.state;
        
        bool isValidState = state == Character.States.idle || 
                           state == Character.States.moving;
        
        return isValidState;
    }
    
    /// <summary>
    /// Verifica se o jogo está em estado de gameplay (não pausado, etc).
    /// </summary>
    public static bool IsGameplayActive()
    {
        if (GameLogic.instance == null) return false;
        return GameLogic.instance.gameState == GameLogic.GameStates.gameplay;
    }
    
    /// <summary>
    /// Verifica se o jogador está invisível (cloaked).
    /// </summary>
    public static bool IsPlayerCloaked(StealthPlayerController player)
    {
        return player != null && player.cloaked;
    }
    
    /// <summary>
    /// Verifica se o jogador está escondido.
    /// </summary>
    public static bool IsPlayerHidden(StealthPlayerController player)
    {
        return player != null && player.hidden;
    }
    
    #endregion
    
    #region Ability Helpers
    
    /// <summary>
    /// Tenta ativar uma habilidade com validações completas.
    /// Retorna true se a habilidade foi ativada com sucesso.
    /// </summary>
    /// <param name="player">Referência ao player</param>
    /// <param name="energyCost">Custo de energia</param>
    /// <param name="abilityName">Nome da habilidade para feedback</param>
    /// <param name="onSuccess">Callback executado se a ativação for bem sucedida</param>
    public static bool TryActivateAbility(
        StealthPlayerController player, 
        float energyCost, 
        string abilityName,
        System.Action onSuccess)
    {
        if (!CanUseAbility(player, energyCost))
        {
            Debug.Log($"[GameplayUtilities] Habilidade '{abilityName}' bloqueada.");
            return false;
        }
        
        // Consome energia
        player.SpendEnergy(energyCost);
        
        // Executa callback de sucesso
        onSuccess?.Invoke();
        
        Debug.Log($"[GameplayUtilities] Habilidade '{abilityName}' ativada. Energia restante: {player.energy}");
        return true;
    }
    
    #endregion
    
    #region Enemy Helpers
    
    /// <summary>
    /// Verifica se um inimigo está ativo e pode interagir.
    /// </summary>
    public static bool IsEnemyActive(AIAgent enemy)
    {
        if (enemy == null) return false;
        if (!enemy.aiEnabled) return false;
        if (enemy.character != null && enemy.character.dead) return false;
        
        return true;
    }
    
    /// <summary>
    /// Verifica se um inimigo está perseguindo o jogador.
    /// </summary>
    public static bool IsEnemyChasing(AIAgent enemy)
    {
        return enemy != null && enemy.chasing;
    }
    
    #endregion
}
