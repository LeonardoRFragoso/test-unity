using UnityEngine;

/// <summary>
/// TAREFA 6: Debug Mode - Sistema de visualização de debug para desenvolvimento e testes.
/// Mostra informações em tempo real sobre o estado do jogo.
/// 
/// Ativação: Pressionar tecla configurável (padrão: F1)
/// 
/// DECISÃO TÉCNICA: Implementado como singleton para fácil acesso global.
/// Usa OnGUI para renderização simples sem dependência de UI Canvas.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Tecla para ativar/desativar o debug overlay")]
    public KeyCode toggleKey = KeyCode.F1;
    
    [Tooltip("Debug overlay está ativo?")]
    public bool isEnabled = false;
    
    [Header("Aparência")]
    [Tooltip("Cor do texto")]
    public Color textColor = Color.white;
    
    [Tooltip("Cor do fundo")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    [Tooltip("Tamanho da fonte")]
    public int fontSize = 14;
    
    [Header("Assist Mode - TAREFA 6")]
    [Tooltip("Ativa modo de assistência (reduz consumo de energia)")]
    public bool assistModeEnabled = false;
    
    [Tooltip("Multiplicador de consumo de energia no Assist Mode (0.5 = metade)")]
    [Range(0.1f, 1f)]
    public float assistModeEnergyMultiplier = 0.5f;
    
    // Singleton com auto-criação
    private static DebugOverlay _instance;
    private static bool _searchedForInstance = false;
    
    /// <summary>
    /// TAREFA 2: Singleton com fallback automático.
    /// Cria automaticamente se não existir.
    /// </summary>
    public static DebugOverlay Instance
    {
        get
        {
            if (_instance == null && !_searchedForInstance)
            {
                _searchedForInstance = true;
                _instance = FindObjectOfType<DebugOverlay>();
                
                // Auto-criar se não existir (plug-and-play)
                if (_instance == null)
                {
                    GameObject debugManager = new GameObject("DebugManager (Auto)");
                    _instance = debugManager.AddComponent<DebugOverlay>();
                    _instance.isEnabled = false;
                    Debug.Log("[DebugOverlay] Criado automaticamente. F1=Debug, F2=Assist");
                }
            }
            return _instance;
        }
    }
    
    // Cache de referências
    private StealthPlayerController player;
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    
    // Dados para exibição
    private int enemyCount = 0;
    private int activeEnemyCount = 0;
    private int chasingEnemyCount = 0;

    void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    void Start()
    {
        player = StealthPlayerController.getInstance();
        
        // Inicializa estilos GUI
        InitializeStyles();
    }

    void Update()
    {
        // Toggle debug overlay
        if (Input.GetKeyDown(toggleKey))
        {
            isEnabled = !isEnabled;
            Debug.Log($"[DebugOverlay] Debug mode: {(isEnabled ? "ATIVADO" : "DESATIVADO")}");
        }
        
        // Toggle assist mode (F2)
        if (Input.GetKeyDown(KeyCode.F2))
        {
            assistModeEnabled = !assistModeEnabled;
            Debug.Log($"[DebugOverlay] Assist mode: {(assistModeEnabled ? "ATIVADO" : "DESATIVADO")}");
        }
        
        // Atualiza contagem de inimigos periodicamente
        if (isEnabled && Time.frameCount % 30 == 0)
        {
            UpdateEnemyStats();
        }
    }

    void InitializeStyles()
    {
        boxStyle = new GUIStyle();
        boxStyle.normal.background = MakeTexture(2, 2, backgroundColor);
        
        labelStyle = new GUIStyle();
        labelStyle.normal.textColor = textColor;
        labelStyle.fontSize = fontSize;
        labelStyle.padding = new RectOffset(5, 5, 2, 2);
    }

    void UpdateEnemyStats()
    {
        AIAgent[] enemies = FindObjectsOfType<AIAgent>();
        enemyCount = enemies.Length;
        activeEnemyCount = 0;
        chasingEnemyCount = 0;
        
        foreach (AIAgent enemy in enemies)
        {
            if (GameplayUtilities.IsEnemyActive(enemy))
            {
                activeEnemyCount++;
            }
            if (GameplayUtilities.IsEnemyChasing(enemy))
            {
                chasingEnemyCount++;
            }
        }
    }

    void OnGUI()
    {
        if (!isEnabled) return;
        
        // Reinicializa estilos se necessário (podem ser perdidos em hot reload)
        if (boxStyle == null || labelStyle == null)
        {
            InitializeStyles();
        }
        
        float boxWidth = 280;
        float boxHeight = 220;
        float margin = 10;
        
        // Desenha caixa de fundo
        Rect boxRect = new Rect(margin, margin, boxWidth, boxHeight);
        GUI.Box(boxRect, "", boxStyle);
        
        // Título
        float y = margin + 5;
        GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), "<b>=== DEBUG MODE (F1) ===</b>", labelStyle);
        y += 25;
        
        // Estado do jogo
        string gameState = GameLogic.instance != null ? GameLogic.instance.gameState.ToString() : "N/A";
        GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), $"Game State: {gameState}", labelStyle);
        y += 18;
        
        // Informações do jogador
        if (player != null)
        {
            GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), "<b>--- PLAYER ---</b>", labelStyle);
            y += 20;
            
            // Energia
            float energyPercent = GameplayUtilities.GetEnergyPercent(player) * 100f;
            string energyColor = energyPercent < 20 ? "red" : (energyPercent < 50 ? "yellow" : "green");
            GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), 
                $"Energy: <color={energyColor}>{player.energy:F1}/{player.maxEnergy}</color> ({energyPercent:F0}%)", labelStyle);
            y += 18;
            
            // Estado
            GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), $"State: {player.state}", labelStyle);
            y += 18;
            
            // Flags
            string flags = "";
            if (player.cloaked) flags += "[CLOAKED] ";
            if (player.hidden) flags += "[HIDDEN] ";
            if (player.running) flags += "[RUNNING] ";
            if (player.hoverAbility != null && player.hoverAbility.IsHovering) flags += "[HOVER] ";
            if (string.IsNullOrEmpty(flags)) flags = "[NORMAL]";
            GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), $"Flags: {flags}", labelStyle);
            y += 18;
            
            // Posição
            Vector3 pos = player.transform.position;
            GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), $"Position: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})", labelStyle);
            y += 22;
        }
        
        // Informações dos inimigos
        GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), "<b>--- ENEMIES ---</b>", labelStyle);
        y += 20;
        
        GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), $"Total: {enemyCount} | Active: {activeEnemyCount} | Chasing: {chasingEnemyCount}", labelStyle);
        y += 22;
        
        // Assist Mode
        string assistStatus = assistModeEnabled ? "<color=green>ON</color>" : "<color=red>OFF</color>";
        GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), $"<b>Assist Mode (F2):</b> {assistStatus}", labelStyle);
        y += 18;
        
        if (assistModeEnabled)
        {
            GUI.Label(new Rect(margin + 5, y, boxWidth - 10, 20), $"  Energy cost: {assistModeEnergyMultiplier * 100:F0}%", labelStyle);
        }
    }

    /// <summary>
    /// Cria uma textura sólida para uso como fundo.
    /// </summary>
    Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Retorna o multiplicador de energia do Assist Mode.
    /// Usado pelo StealthPlayerController para ajustar consumo de energia.
    /// </summary>
    public float GetAssistModeMultiplier()
    {
        return assistModeEnabled ? assistModeEnergyMultiplier : 1f;
    }
}
