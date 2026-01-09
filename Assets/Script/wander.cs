using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Meta.WitAi.TTS.Utilities;
[RequireComponent(typeof(NavMeshAgent))]
public class WanderMetaAvatar: MonoBehaviour
{
    public TTSSpeaker speaker;

    [Header("Wander")]
    public float wanderRadius = 12f;
    public float minPause = 1.2f;
    public float maxPause = 3.5f;
    public float maxWalkTime = 2f; // AGGIUNGI: Tempo massimo di camminata
    public float repathInterval = 0.25f;
    public float arriveThreshold = 0.25f;
    public int areaMask = NavMesh.AllAreas;
    [Header("Interaction")]
    public float lookAtSpeed = 6f;
    public Transform interactionTarget;
    private bool wasSpeaking = false;
    [Header("Animator Params (Custom Rig)")]
    public string speedParam = "Speed";
    public string isMovingParam = "IsMoving";
    public string randomIdleTrigger = "RandomIdle";
    [Range(0f, 1f)] public float randomIdleChance = 0.25f;
    [Header("Auto-aggancio Animator")]
    [Tooltip("Suggerimento per il nome del rig istanziato (facoltativo).")]
    public string animatorNameHint = "Humanoid";
    [Tooltip("Ricerca periodica (s) finché il rig non è pronto o se viene ricreato.")]
    public float searchInterval = 0.5f;
    [Tooltip("Ri-aggancia se il rig viene distrutto/ricreato a runtime.")]
    public bool autoReacquire = true;
    [Header("Input Manager Override")]
    [Tooltip("Blocca il movimento da CharacterController mentre usi NavMesh")]
    public bool overrideInputMovement = true;
    private Animator customRigAnimator;
    private NavMeshAgent agent;
    private Vector3 origin;
    private float speedSmooth, speedVel;
    private Coroutine resolveLoopCo, wanderCo;
    private CharacterController characterController; // riferimento al CharacterController
    private Vector3 lastPosition; // per bloccare il movimento
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = true;
        agent.updateRotation = false; // CAMBIA: disabilita rotazione automatica
        origin = transform.position;

        characterController = GetComponent<CharacterController>();
        if (characterController != null && overrideInputMovement)
        {
            characterController.enabled = false;
            Debug.Log("[Wander] CharacterController disabilitato per evitare conflitti");
        }
    }

    void Update()
    {
        if (speaker.IsSpeaking && interactionTarget != null)
        {
            Vector3 dir = interactionTarget.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                // Inverti se il tuo avatar guarda in -Z
                Quaternion targetRot = Quaternion.LookRotation(-dir.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime * lookAtSpeed
                );
            }

            agent.isStopped = true;
            agent.ResetPath();

            if (customRigAnimator)
            {
                customRigAnimator.SetFloat(speedParam, 0f);
                customRigAnimator.SetBool(isMovingParam, false);
            }
            return;
        }

        // Wander rotation
        if (agent.enabled && agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = -agent.velocity.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
        }

        UpdateAnimation();
    }

    void OnEnable()
    {
        resolveLoopCo = StartCoroutine(ResolveAnimatorLoop());
        wanderCo = StartCoroutine(WanderLoop());
    }
    void OnDisable()
    {
        if (resolveLoopCo != null) StopCoroutine(resolveLoopCo);
        if (wanderCo != null) StopCoroutine(wanderCo);
    }

    void LateUpdate()
    {
        // AGGIUNTO: Se qualcos'altro (Input Manager) ha spostato l'oggetto, resettalo
        if (overrideInputMovement && agent.enabled)
        {
            // Ripristina la posizione del NavMeshAgent
            transform.position = agent.nextPosition;
        }
    }
    IEnumerator ResolveAnimatorLoop()
    {
        while (true)
        {
            if (customRigAnimator == null || !customRigAnimator.isActiveAndEnabled)
            {
                customRigAnimator = FindRigAnimatorSafe(animatorNameHint);
                if (customRigAnimator != null)
                {
                    customRigAnimator.applyRootMotion = false;
                    // RIMUOVI la rotazione qui
                }
            }
            if (!autoReacquire && customRigAnimator != null) yield break;

            yield return new WaitForSeconds(searchInterval);
        }
    }
    IEnumerator WanderLoop()
    {
        while (true)
        {
            if (TryGetRandomPoint(origin, wanderRadius, out var target))
                agent.SetDestination(target);

            float walkTimer = 0f; // AGGIUNGI: Timer per il movimento

            while (true)
            {
                UpdateAnimation();
                walkTimer += repathInterval; // AGGIUNGI: Incrementa il timer

                if (!agent.pathPending)
                {
                    bool arrived = agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, arriveThreshold);
                    bool invalid = agent.pathStatus == NavMeshPathStatus.PathInvalid;
                    bool timeout = walkTimer >= maxWalkTime; // AGGIUNGI: Controlla timeout
                    if (arrived || invalid || timeout) // AGGIUNGI: timeout alla condizione
                    {
                        if (timeout)
                            Debug.Log("[Wander] Timeout raggiunto, ferma l'avatar");
                        break;
                    }
                }
                yield return new WaitForSeconds(repathInterval);
            }

            float pause = Random.Range(minPause, maxPause);
            agent.ResetPath();

            if (customRigAnimator && !string.IsNullOrEmpty(randomIdleTrigger) && Random.value < randomIdleChance)
                customRigAnimator.SetTrigger(randomIdleTrigger);

            float t = 0f;
            while (t < pause)
            {
                UpdateAnimation();
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
    private void UpdateAnimation()
    {
        if (customRigAnimator == null || agent == null) return;
        float raw = agent.velocity.magnitude;
        speedSmooth = Mathf.SmoothDamp(speedSmooth, raw, ref speedVel, 0.10f);
        if (!string.IsNullOrEmpty(speedParam))
            customRigAnimator.SetFloat(speedParam, speedSmooth);
        if (!string.IsNullOrEmpty(isMovingParam))
            customRigAnimator.SetBool(isMovingParam, speedSmooth > 0.05f);
    }
    private Animator FindRigAnimatorSafe(string nameHint)
    {
        Animator best = null;
        int bestScore = int.MinValue;
        var anims = GetComponentsInChildren<Animator>(true);
        foreach (var a in anims)
        {
            if (a.gameObject == this.gameObject) continue;
            int score = 0;
            if (a.avatar != null) score += 3;
            if (a.transform.parent == this.transform) score += 2;
            if (!string.IsNullOrEmpty(nameHint) && a.name.Contains(nameHint)) score += 1;
            if (score > bestScore)
            {
                bestScore = score;
                best = a;
            }
        }
        return best;
    }
    private bool TryGetRandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, 2f, areaMask))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.15f);
        Gizmos.DrawSphere(Application.isPlaying ? origin : transform.position, wanderRadius);
    }
}
