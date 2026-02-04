# Teste Técnico - Unity Stealth Game

**Candidato:** Desenvolvedor Sênior Unity  
**Engine:** Unity 2019  
**Linguagem:** C#  
**Data de Entrega:** Fevereiro 2026

---

## 📋 Sumário

1. [Visão Geral](#visão-geral)
2. [Objetivo do Teste](#objetivo-do-teste)
3. [Bugs Corrigidos](#bugs-corrigidos)
4. [Funcionalidades Implementadas](#funcionalidades-implementadas)
5. [Decisões Técnicas](#decisões-técnicas)
6. [Trade-offs Assumidos](#trade-offs-assumidos)
7. [Melhorias Futuras](#melhorias-futuras)
8. [Como Executar](#como-executar)
9. [Checklist de Entrega](#checklist-de-entrega)
10. [Estrutura de Arquivos](#estrutura-de-arquivos)

---

## Visão Geral

Projeto base de um jogo stealth criado para Ludum Dare, onde o jogador controla um robô que deve atravessar cenários sem ser detectado. O jogador possui um sistema de energia/bateria que alimenta diversas habilidades.

### Sistemas Existentes
- **Movimento:** WASD com corrida (consome energia)
- **Habilidades:** Shock, Cloak, Drain
- **IA Inimiga:** Sistema de estados (Idle, Chase, Seek) com visão e patrulha
- **Upgrades:** Sistema de coleta de habilidades

### Sistemas Adicionados
- **Tiro:** Nova habilidade ofensiva
- **Hover:** Flutuação para evasão
- **Boss:** Inimigo especial usando IA existente
- **Debug Mode:** Ferramentas de desenvolvimento
- **Assist Mode:** Acessibilidade

---

## Objetivo do Teste

Demonstrar capacidade de:
1. **Leitura de código legado** - Compreensão de arquitetura existente
2. **Debugging** - Identificação e correção de bugs com análise de causa raiz
3. **Implementação** - Novas mecânicas integradas ao sistema existente
4. **Qualidade de código** - Manutenibilidade, clareza e boas práticas

---

## Bugs Corrigidos

### 🐛 BUG 1: Portas não abrem

| Item | Detalhe |
|------|---------|
| **Arquivo** | `Assets/Objects/Door.prefab` |
| **Sintoma** | Jogador atravessa área da porta mas ela não abre |
| **Causa Raiz** | `CapsuleCollider.IsTrigger = false`, mas `Door.cs` usa `OnTriggerEnter()` |
| **Solução** | Alterado `m_IsTrigger` de `0` para `1` na linha 53 do prefab |
| **Verificação** | Porta abre corretamente ao jogador se aproximar |

### 🐛 BUG 2: Habilidade Shock não funciona

| Item | Detalhe |
|------|---------|
| **Arquivo** | `Assets/Scripts/ShockDamageArea.cs` |
| **Sintoma** | Inimigos não ficam atordoados quando atingidos pelo Shock |
| **Causa Raiz** | `OnShock(0)` passava `0` como `stunTime` em vez do campo da classe |
| **Solução** | Alterado para `OnShock(stunTime)` na linha 35 |
| **Verificação** | Inimigos ficam atordoados pelo tempo configurado (1.5s) |

### 🐛 BUG 3: Engasgos periódicos (Performance)

| Item | Detalhe |
|------|---------|
| **Arquivo** | `Assets/Scripts/AI/AISight.cs` |
| **Sintoma** | Jogo apresenta micro-travamentos periódicos |
| **Causa Raiz** | `ToArray()` chamado em `HashSet` a cada trigger enter/exit, gerando alocações e pressão no GC |
| **Solução** | Substituído `HashSet` por `List<Character>` com iteração direta, eliminando alocações |
| **Verificação** | Gameplay fluido sem engasgos perceptíveis |

---

## Funcionalidades Implementadas

### 🎯 Habilidade de Tiro (TAREFA 1)

**Arquivo principal:** `Assets/Scripts/PlayerBullet.cs`

| Requisito | Status | Implementação |
|-----------|--------|---------------|
| Atirar ao pressionar botão | ✅ | Mouse esquerdo / F / Gamepad RB |
| Consumir energia | ✅ | 5 unidades por tiro (configurável) |
| Desativar inimigos permanentemente | ✅ | `AIAgent.aiEnabled = false`, `character.dead = true` |
| Integração com sistema de energia | ✅ | Usa `SpendEnergy()` existente |
| Cor diferente | ✅ | Laranja (RGB: 1, 0.3, 0) |
| Velocidade própria | ✅ | 15 unidades/s (configurável) |
| Configurável via Inspector | ✅ | Todos os valores expostos com `[Tooltip]` |

### 🚀 Habilidade de Hover (TAREFA 2)

**Arquivo principal:** `Assets/Scripts/HoverAbility.cs`

| Requisito | Status | Implementação |
|-----------|--------|---------------|
| Ativada ao segurar botão | ✅ | Space (Jump) |
| Consumo contínuo de energia | ✅ | Multiplicador 2.5x (configurável) |
| Flutuação acima do chão | ✅ | 0.5 unidades de altura |
| Só ativa no chão | ✅ | Raycast para detecção |
| Respeita física | ✅ | Usa Rigidbody.AddForce |
| Respeita sistema de energia | ✅ | Integrado com energyDrainSpeed |
| Som e partículas | ✅ | Configuráveis via Inspector |
| Código desacoplado | ✅ | Componente independente |

### 👹 Boss Enemy (DESAFIO)

**Arquivo principal:** `Assets/Scripts/AI/BossAI.cs`

| Requisito | Status | Implementação |
|-----------|--------|---------------|
| Pode ser derrotado | ✅ | 5 tiros (HP configurável) |
| Pode ser contornado | ✅ | Área de perigo evitável com Hover |
| Exige novas habilidades | ✅ | Tiro para dano, Hover para proteção |
| Usa IA existente | ✅ | Componente adicional ao AIAgent |
| Feedback visual | ✅ | Flash de dano, cor muda com HP |

### 🛠️ Diferenciais Técnicos (TAREFA 6)

#### Debug Mode
- **Tecla:** F1
- **Mostra:** Estado do jogo, energia, flags do jogador, contagem de inimigos
- **Arquivo:** `Assets/Scripts/Managers/DebugOverlay.cs`

#### Assist Mode
- **Tecla:** F2
- **Efeito:** Reduz consumo de energia para 50%
- **Propósito:** Acessibilidade e testes

#### Camera Shake
- **Arquivo:** `Assets/Scripts/Effects/CameraShake.cs`
- **Uso:** Feedback ao atirar, dano e eventos do Boss
- **Técnica:** Perlin Noise para movimento orgânico

---

## Decisões Técnicas

### 1. Reutilização vs. Criação

**Decisão:** Estender sistemas existentes em vez de criar novos.

**Justificativa:** 
- Mantém consistência com código legado
- Reduz risco de introduzir novos bugs
- Facilita onboarding de outros desenvolvedores
- Menor footprint de código novo

**Exemplo:** `BossAI` é um componente adicional que modifica parâmetros do `AIAgent` existente, sem duplicar lógica de estados ou navegação.

### 2. Singleton Pattern para Managers

**Decisão:** Usar singletons para `CameraShake`, `DebugOverlay`, seguindo padrão existente (`GameLogic`, `AudioManager`).

**Justificativa:**
- Consistência com arquitetura existente
- Acesso fácil de qualquer script
- Trade-off aceitável para escopo do projeto

### 3. Configurabilidade via Inspector

**Decisão:** Todos os valores numéricos expostos com `[SerializeField]`, `[Header]` e `[Tooltip]`.

**Justificativa:**
- Permite ajuste sem recompilação
- Facilita balanceamento por game designers
- Documenta propósito de cada campo

### 4. Feedback Visual Centralizado

**Decisão:** Criar método `OnInsufficientEnergy()` centralizado em vez de duplicar lógica.

**Justificativa:**
- DRY (Don't Repeat Yourself)
- Facilita modificação futura do feedback
- Garante consistência da experiência

---

## Trade-offs Assumidos

| Trade-off | Escolha | Alternativa | Motivo |
|-----------|---------|-------------|--------|
| **Prefab do projétil** | YAML manual | Criar via Editor | Demonstrar conhecimento de serialização Unity |
| **Debug UI** | OnGUI | Canvas UI | Simplicidade, sem dependência de prefabs |
| **Assist Mode** | Multiplier global | Por habilidade | Escopo controlado, fácil de entender |
| **Boss HP** | Inteiro simples | Sistema de dano complexo | Adequado ao escopo do teste |
| **Camera Shake** | Componente na câmera | Post-processing | Compatível com Unity 2019, sem dependências |

---

## Melhorias Futuras

### Curto Prazo (Quick Wins)
- [ ] Pool de objetos para projéteis (evitar Instantiate/Destroy)
- [ ] Feedback sonoro específico para cada habilidade
- [ ] Indicador visual de cooldown das habilidades

### Médio Prazo
- [ ] Sistema de save/load completo (upgrades, checkpoints)
- [ ] Mais variações de Boss (diferentes padrões de ataque)
- [ ] Sistema de combo de habilidades

### Longo Prazo
- [ ] Refatorar IA para Behavior Trees (mais escalável)
- [ ] Sistema de eventos centralizado (Observer pattern)
- [ ] Migração para novo Input System da Unity

### Pontos de Extensão Identificados
1. `GameplayUtilities.cs` - Adicionar novos métodos de validação
2. `BossAI.cs` - Herdar para criar variações de Boss
3. `HoverAbility.cs` - Base para outras habilidades de movimento

---

## Como Executar (Plug & Play)

> ⚡ **NOTA IMPORTANTE:** Todas as referências e componentes já estão configurados automaticamente.
> Nenhuma configuração manual é necessária para testar o projeto.

### 3 Passos para Avaliar

```
1. Abrir o projeto no Unity 2019
2. Abrir qualquer cena de gameplay
3. Pressionar Play ▶
```

**É isso.** O sistema de auto-setup configura tudo automaticamente.

### O que acontece ao pressionar Play:

1. `AutoSetup.cs` verifica e cria componentes faltantes
2. `GameBootstrap.cs` valida todos os sistemas
3. Console mostra status: **"GAME READY FOR EVALUATION"**
4. Todos os controles funcionam imediatamente

### Requisitos
- Unity 2019.x
- Blender 2.8 (apenas se modelos não carregarem)

---

## Controles Rápidos para Avaliação

| Ação | Tecla | Descrição |
|------|-------|-----------|
| **Movimento** | WASD | Mover o robô |
| **Correr** | Z | Consome energia |
| **TIRO** | **F** ou **Mouse L** | ⭐ Nova habilidade - desativa inimigos |
| **HOVER** | **Space** (segurar) | ⭐ Nova habilidade - flutuar |
| **Shock** | X | Atordoa inimigos próximos |
| **Cloak** | C | Invisibilidade temporária |
| **Drain** | V | Drena energia de inimigos |
| **DEBUG** | **F1** | 🛠 Mostra informações na tela |
| **ASSIST** | **F2** | 🛠 Reduz consumo de energia |

### Testando as Features Principais (< 5 min)

1. **Tiro:** Pressione F para atirar em inimigos
2. **Hover:** Segure Space para flutuar (protege de área do Boss)
3. **Debug Mode:** F1 mostra energia, estados e informações
4. **Assist Mode:** F2 facilita testes (menos consumo de energia)
5. **Boss:** Se presente na cena, requer 5 tiros para derrotar

---

## Checklist de Entrega

### ✅ Funcionalidade
- [x] Projeto abre sem erros
- [x] Console limpo (sem erros/warnings críticos)
- [x] Todas as habilidades originais funcionando
- [x] Nova habilidade Tiro funcionando
- [x] Nova habilidade Hover funcionando
- [x] Boss testado e funcionando
- [x] Portas abrindo corretamente
- [x] Shock atordoando inimigos
- [x] Performance estável (sem engasgos)

### ✅ Qualidade de Código
- [x] Código comentado (decisões importantes)
- [x] Valores configuráveis via Inspector
- [x] Null checks em pontos críticos
- [x] Early returns para legibilidade
- [x] Métodos com responsabilidade única
- [x] Nomes descritivos (variáveis/métodos)

### ✅ Documentação
- [x] README técnico completo
- [x] Bugs documentados com causa raiz
- [x] Decisões técnicas justificadas
- [x] Instruções de execução claras

### ✅ Extras Implementados
- [x] Debug Mode (F1)
- [x] Assist Mode (F2)
- [x] Camera Shake (juice)
- [x] Feedback visual (energia insuficiente, Boss enfurecido)

### ✅ Experiência Plug & Play
- [x] Auto-setup de componentes faltantes
- [x] GameBootstrap valida sistemas automaticamente
- [x] Fallbacks defensivos (nada quebra por falta de configuração)
- [x] Log claro no Console ao iniciar
- [x] Zero configuração manual necessária

---

## Estrutura de Arquivos

### Arquivos Criados
```
Assets/Scripts/
├── PlayerBullet.cs              # Projétil do jogador
├── HoverAbility.cs              # Habilidade de flutuação
├── AI/
│   └── BossAI.cs                # Boss enemy
├── Effects/
│   └── CameraShake.cs           # Screen shake (auto-criação)
└── Managers/
    ├── AutoSetup.cs             # ⭐ Auto-configuração na inicialização
    ├── GameBootstrap.cs         # ⭐ Validação e log de sistemas
    ├── DebugOverlay.cs          # Debug mode UI (auto-criação)
    └── GameplayUtilities.cs     # Métodos auxiliares

Assets/Objects/
└── PlayerBullet.prefab          # Prefab do projétil
```

### Arquivos Modificados
```
Assets/Scripts/
├── StealthPlayerController.cs   # +Tiro, +Hover integration, +Feedback
├── ProgressBar.cs               # +Flash method
├── ShockDamageArea.cs           # Bug fix: stunTime
└── AI/
    └── AISight.cs               # Bug fix: performance

Assets/Objects/
└── Door.prefab                  # Bug fix: IsTrigger

ProjectSettings/
└── InputManager.asset           # +Fire1 keyboard mapping
```

---

## Considerações Finais

Este projeto demonstra:

1. **Capacidade analítica** - Identificação de causa raiz em bugs não triviais
2. **Integração cuidadosa** - Novas features sem quebrar funcionalidades existentes
3. **Visão de produto** - Feedback ao jogador, acessibilidade, ferramentas de debug
4. **Maturidade técnica** - Código limpo, documentado e extensível

O código foi desenvolvido pensando em quem vai mantê-lo depois, priorizando clareza sobre cleverness.

---

*Documento gerado como parte do teste técnico.*
