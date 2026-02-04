using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// BUG FIX 3: Removido 'using System.Linq' - não é mais necessário após otimização
public class AISight : MonoBehaviour {
    public bool testSight = false;
    public enum SightStates { disabled,searching,seeingEnemy,closeToEnemy}
    public SightStates sightState = SightStates.disabled;

    public bool requireAngleOfView = true;
    public bool requireLineOfSight = true;
    public float viewAngle = 10;
    public float viewDistance = 10;
    public float nearVisionDistance = 3.0f;
    public float loseSightTimer=1.0f;
    public float currentTimeWithoutSight = 0.0f;
    //CharacterAnimationController animationController;
    EnemyEyeController eyeController;

    Character character;
    AIAgent aiAgent;
    
    // BUG FIX 3: Substituído HashSet + ToArray() por List para evitar alocações
    // desnecessárias de memória. ToArray() criava um novo array a cada chamada,
    // causando pressão no Garbage Collector e engasgos periódicos.
    List<Character> nearbyCharacters = new List<Character>();
    
    // Flag para indicar quando a lista foi modificada (evita iteração desnecessária)
    bool nearbyCharactersDirty = false;

    public LayerMask sightMask;
    public Character seenCharacter;
    
    public void OnPlayerDeath()
    {
        // BUG FIX 3: Usar Clear() em vez de criar novas instâncias para evitar alocações
        nearbyCharacters.Clear();
        sightState = SightStates.disabled;
        seenCharacter = null;
    }

	// Use this for initialization
	void Awake () {
        character = transform.parent.GetComponent<Character>();
        aiAgent = transform.parent.GetComponent<AIAgent>();
        if (eyeController == null)
        {
            eyeController = transform.parent.GetComponent<EnemyEyeController>();
        }
	}
	
    public void setSightState(SightStates newSightState)
    {
        sightState = newSightState;
        switch (sightState)
        {
            case SightStates.disabled:

                break;
            case SightStates.searching:

                break;
            case SightStates.seeingEnemy:

                break;
            case SightStates.closeToEnemy:

                break;
        }
        
    }

	// Update is called once per frame
	public void Update() {
        if (!testSight)
        {
            if (character.dead || GameLogic.instance.gameState != GameLogic.GameStates.gameplay || (!aiAgent.aiEnabled))
            {
                return;
            }
        }

        switch (sightState)
        {
            case SightStates.searching:
                // BUG FIX 3: Iterar diretamente na List em vez de usar array intermediário
                for (int i = 0; i < nearbyCharacters.Count; i++)
                {
                    if (nearbyCharacters[i] != null && canSeecharacter(nearbyCharacters[i].transform))
                    {
                        seeCharacter(nearbyCharacters[i]);
                    }
                }
                break;
            case SightStates.seeingEnemy:        
                Vector3 vectorToSeenEnemy = Vector3.Normalize(seenCharacter.transform.position - transform.position);
                if (!canSeecharacter(seenCharacter.transform))
                {
                    currentTimeWithoutSight += Time.deltaTime;
                    canSeecharacter(seenCharacter.transform);
                    if (currentTimeWithoutSight >= loseSightTimer) { 
                        tryUnseeCharacter();
                    }
                }
                else
                {
                    currentTimeWithoutSight = 0;
                }
                break;
            case SightStates.closeToEnemy:
                if (Vector3.Distance(seenCharacter.transform.position, transform.position) > nearVisionDistance)
                {
                    Vector3 vectorToEnemy = Vector3.Normalize(seenCharacter.transform.position - transform.position);
                    if (Vector3.Angle(transform.forward, vectorToEnemy) > viewAngle)
                    {
                        unseeCharacter();
                    }
                    else
                    {
                        setSightState(SightStates.seeingEnemy);
                    }
                }
                    break;
        }
	}


    bool canSeecharacter(Transform character)
    {
        if (!aiAgent.aiEnabled && !testSight)
        {
            return false;
        }
        if (character == StealthPlayerController.getInstance().transform)
        {
            if (StealthPlayerController.getInstance().cloaked)
            {
                return false;
            }
            if (StealthPlayerController.getInstance().hidden)
            {
                if(sightState != SightStates.seeingEnemy)
                {
                    return false;
                }
                
            }
        }

        Vector3 vectorToEnemy = Vector3.Normalize(character.transform.position - transform.position);
        if (Vector3.Distance(transform.position, character.position) <= viewDistance)
        {

            if (!requireAngleOfView)
            {
                return true;
            }
            if(Vector3.Angle(transform.forward, vectorToEnemy) < viewAngle)
            {
                if (!requireLineOfSight)
                {
                    return true;
                }
                if(!checkLineOfSight(character))
                {
                    return true;
                }
            }

            
        }
        return false;
        
    }
    public bool raycastResult = false;
    bool checkLineOfSight(Transform target)
    {
        RaycastHit hit;
        raycastResult = Physics.Raycast(transform.position, target.position - transform.position,out hit, Vector3.Distance(transform.position, target.position), sightMask);

        //raycastResult = Physics.Raycast(transform.position, target.position-transform.position, Vector3.Distance(transform.position,target.position), sightMask);
        return raycastResult;
    }

    void seeCharacter(Character newSeenCharacter)
    {
        seenCharacter = newSeenCharacter;
        setSightState(SightStates.seeingEnemy);

        if (aiAgent.aiEnabled)
        {
            aiAgent.SeeEnemy(seenCharacter.transform);
        }
        
        
        //Debug.Log(gameObject.name + " sees " + seenCharacter.gameObject.name);
        if (eyeController != null)
        {
            eyeController.SetExclamation();
            eyeController.SetAngry(true);
        }
    }

    void tryUnseeCharacter()
    {
        if (Vector3.Distance(seenCharacter.transform.position, transform.position) > nearVisionDistance)
        {
            unseeCharacter();
        }
        else
        {
            setSightState(SightStates.closeToEnemy);
        }
    }

    void unseeCharacter()
    {
        if (!aiAgent.aiEnabled && !testSight)
        {
            return;
        }
        setSightState(SightStates.searching);
        if (aiAgent.aiEnabled)
        {
            aiAgent.OnLoseSight();
        }
        
        if (eyeController != null)
        {
            eyeController.SetQuestion();
            eyeController.SetAngry(false);
        }
    }
    void OnTriggerEnter(Collider col)
    {
        

        Character colliderCharacter = col.GetComponent<Character>();
        if (colliderCharacter != null && colliderCharacter.friend != character.friend)
        {
            if (!requireLineOfSight)
            {

                seeCharacter(colliderCharacter);
            }
            else
            {
                // BUG FIX 3: Removido ToArray() - adiciona diretamente na List
                // Verifica se já não está na lista para evitar duplicatas
                setSightState(SightStates.searching);
                if (!nearbyCharacters.Contains(colliderCharacter))
                {
                    nearbyCharacters.Add(colliderCharacter);
                }
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
 
        Character colliderCharacter = col.GetComponent<Character>();
        if (colliderCharacter != null && colliderCharacter.friend != character.friend)
        {
            if (!requireLineOfSight)
            {

                unseeCharacter();
            }
            else
            {
                // BUG FIX 3: Removido ToArray() - remove diretamente da List
                nearbyCharacters.Remove(colliderCharacter);

                if (colliderCharacter == seenCharacter)
                {
                    unseeCharacter();
                }
                if (nearbyCharacters.Count <= 0 && sightState == SightStates.searching)
                {
                    if (!aiAgent.aiEnabled)
                    {
                        return;
                    }
                    setSightState(SightStates.disabled);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, nearVisionDistance);
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        if(seenCharacter!=null && sightState == SightStates.searching)
        {
            //Gizmos.DrawLine(transform.position, seenCharacter.transform.position);
            Gizmos.DrawRay(transform.position, seenCharacter.transform.position - transform.position);
        }

    }
}
