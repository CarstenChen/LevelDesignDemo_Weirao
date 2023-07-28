using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Gamekit3D;
using Gamekit3D.Message;
public class PlayerController : MonoBehaviour, IMessageReceiver
{
    private static PlayerController instance;
    public static PlayerController Instance { get { return instance; } private set {; } }
    [Header ("Movement")]
    [SerializeField] protected CinemachineFreeLook TPSCamera;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float fastRunSpeed;
    [SerializeField] protected float stemina = 100;
    [SerializeField] protected float steminaCostForFasetRun;
    [SerializeField] protected float recoverRate;
    [SerializeField] protected float maxRotateSpeed;
    [SerializeField] protected float gravity = 10f;
    [SerializeField] protected float initialJumpSpeed = 10f;
    [SerializeField] protected Transform lowStepChecker;
    [SerializeField] protected Transform highStepChecker;
    [SerializeField] protected float stepSmooth;
    [SerializeField]  protected LayerMask stepLayer;

    [SerializeField] protected float stepHeight;

    protected float maxSpeedRef = 1f;  //虚拟运动进程区间最大值
    protected float targetSpeedRef;  //实际运动进程区间（需要插值到的）值
    protected float curSpeedRef;  //当前运动进程区间值
    protected float groundAcceleration = 4f;
    protected float groundDeceleration = 5f;
    protected Quaternion targetRotation;  //角色forward转向输入方向的四元数
    protected float shortestDeltaRotDegree;  //当前旋转的delta角（最小角度）
    protected float curRotateSpeed;  //当前速度下的旋转速度
    [SerializeField] protected bool isGrounded = true;
    [SerializeField] protected bool isReadyToJump;
    protected float curVerticalSpeed; //当前垂直速度速度
    [SerializeField] protected bool isFastRunning;
    [SerializeField] protected float currentStemina;

    [Header("Animation")]
    protected Animator animator;
    protected float idleTimeout = 5f;  //多久开始考虑idle动画
    protected float idleTimer;
    protected AnimatorInfo animatorCache;

    [Header("Input")]
    protected PlayerInput playerInput;
    protected CharacterController character;

    [Header("Skills")]
    public MeleeWeapon meleeWeapon;
    protected bool isAttacking;

    public bool isCostingStemina;
    public bool isRecoveringStemina;
    private bool fastRunBlock;
    public bool isRespawning;
    public bool inNarrative;
    public bool waitToRespawn;

    protected Damageable damageable;

    public Vector3 currentRespawnPos;

    public void RegisterRespawnPos(Vector3 pos)
    {
        currentRespawnPos = pos;
    }
    void Start()
    {
        if (instance == null) instance = this;

        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        character = GetComponentInChildren<CharacterController>();
        animatorCache = new AnimatorInfo(animator);
        meleeWeapon.SetOwner(gameObject);
        animator.SetBool("IsGrounded", IsGrounded());
        animator.applyRootMotion = false;
        currentStemina = stemina;
        highStepChecker.transform.localPosition = new Vector3(highStepChecker.transform.localPosition.x, stepHeight, highStepChecker.transform.localPosition.z);
        ShowHideMeleeWeapon(false);

        character.Move(Vector3.forward) ;
        //Debug.Log(movement);

        //注意使用这种方法判断是否在地面必须要先Move()
        isGrounded = character.isGrounded;
    }

    private void OnEnable()
    {
        damageable = GetComponent<Damageable>();
        damageable.onDamageMessageReceivers.Add(this);
        damageable.isInvulnerable = true;



    }
    public float GetCurrentSteminaRate()
    {
        return currentStemina / stemina;
    }

    public void SetFastRunState(bool active)
    {
        isFastRunning = active;
    }
    void FixedUpdate()
    {
        CacheAnimatorState();
        UpdateInputBlock();

        ShowHideMeleeWeapon(IsWeaponAnimationOnPlay());
        DealWithMeleeAttackAnimation();


        CalculateHorizontalMovement();
        CalculateVerticalMovement();
        SetMoveAnimation();

        CalculateRotation();
        if (IsPressingMoveKey)
        {
            CharacterRotate();
        }


        TimeoutToIdle();

        isFastRunning = playerInput.FastRunInput&&!fastRunBlock;

        DealWithStemina();

        //StepClimb();
    }

    private void Update()
    {
        ///Debug.Log(animatorCache.currentStateInfo.normalizedTime);
    }

    private IEnumerator ResetFastRunBlock()
    {
        yield return new WaitUntil(() => !playerInput.FastRunInput);
        fastRunBlock = false;
    }
    private void DealWithStemina()
    {
        if (isFastRunning)
        {
            if (currentStemina > 0)
            {
                currentStemina = Mathf.Clamp(currentStemina - steminaCostForFasetRun, 0, stemina);
                isCostingStemina = true;
                isRecoveringStemina = false;
            }
            else
            {
                fastRunBlock = true;
                StartCoroutine(ResetFastRunBlock());
            }
            
        }
        else
        {
            if (currentStemina < stemina)
            {
                currentStemina = Mathf.Clamp(currentStemina + recoverRate * steminaCostForFasetRun, 0, stemina);
                isCostingStemina = false;
                isRecoveringStemina = true;
            }
            else
            {
                isCostingStemina = false;
                isRecoveringStemina = false;
            }
        }


    }
    private void OnAnimatorMove()
    {
        if (isRespawning||waitToRespawn) return;
        Vector3 movement = Vector3.zero;



        if (/*isGrounded*/IsGrounded())
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * 0.5f, -Vector3.up);

            if (Physics.Raycast(ray, out hit, 1f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                //if (isFastRunning)
                //{
                //    movement = fastRunSpeed * curSpeedRef * transform.forward * Time.deltaTime;
                //}
                //else
                //{
                //    movement = moveSpeed * curSpeedRef * transform.forward * Time.deltaTime;
                //}
                if (Physics.Raycast(ray, out hit, 1f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    //用动画计算移动距离的一种方式
                    movement = Vector3.ProjectOnPlane(animator.deltaPosition, hit.normal);
                }
                else
                {
                    movement = animator.deltaPosition;
                }

            }
            else
            {
                //if (isFastRunning)
                //{
                //    movement = fastRunSpeed * curSpeedRef * transform.forward * Time.deltaTime;
                //}
                //else
                //{
                //    movement = moveSpeed * curSpeedRef * transform.forward * Time.deltaTime;
                //}
                if (Physics.Raycast(ray, out hit, 1f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    //用动画计算移动距离的一种方式
                    movement = Vector3.ProjectOnPlane(animator.deltaPosition, hit.normal);
                }
                else
                {
                    movement = animator.deltaPosition;
                }
            }
        }
        else
        {
            movement = moveSpeed * curSpeedRef * transform.forward * Time.deltaTime;
        }

        character.transform.rotation *= animator.deltaRotation;

        //加入垂直速度
        movement += curVerticalSpeed * Vector3.up * Time.deltaTime;

        character.Move(movement);
        //Debug.Log(movement);

        //注意使用这种方法判断是否在地面必须要先Move()
        isGrounded = character.isGrounded;

        if (IsGrounded()) { }
        else
        {
            animator.SetFloat("VerticalSpeed", curVerticalSpeed);
        }

        animator.SetBool("IsGrounded", /*isGrounded*/IsGrounded());
    }
    void SetMoveAnimation()
    {
        animator.SetFloat("ForwardSpeed", curSpeedRef);
        animator.SetBool("IsFastRunning", isFastRunning);
    }

    void UpdateInputBlock()
    {
        bool currentInputBlock = !animatorCache.isAnimatorTransitioning && animatorCache.currentStateInfo.tagHash == Animator.StringToHash("BlockInput");

        currentInputBlock |= animatorCache.nextStateInfo.tagHash == Animator.StringToHash("BlockInput");

        playerInput.inputBlock = currentInputBlock || inNarrative;
    }

    protected bool IsPressingMoveKey
    {
        get { return !Mathf.Approximately(playerInput.MoveInput.sqrMagnitude, 0f); }
    }

    void CacheAnimatorState()
    {
        //上一帧动画信息记录
        animatorCache.previousCurrentStateInfo = animatorCache.currentStateInfo;
        animatorCache.previousIsAnimatorTransitioning = animatorCache.isAnimatorTransitioning;
        animatorCache.previousNextStateInfo = animatorCache.nextStateInfo;

        //当前帧动画信息更新
        animatorCache.currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animatorCache.isAnimatorTransitioning = animator.IsInTransition(0);
        animatorCache.nextStateInfo = animator.GetNextAnimatorStateInfo(0);
    }

    void CalculateHorizontalMovement()
    {
        Vector2 moveInput = playerInput.MoveInput;

        if (moveInput.sqrMagnitude > 1)
            moveInput.Normalize();

        targetSpeedRef = moveInput.magnitude;

        float a = IsPressingMoveKey ? groundAcceleration : groundDeceleration;

        //Vt = Vo +at
        curSpeedRef = Mathf.MoveTowards(curSpeedRef, targetSpeedRef, Time.deltaTime * a);
    }

    void CalculateVerticalMovement()
    {
        if (!playerInput.JumpInput && /*isGrounded*/IsGrounded())
            isReadyToJump = true;

        if (/*isGrounded*/IsGrounded())
        {
            curVerticalSpeed = -2f;

            if (playerInput.JumpInput && isReadyToJump)
            {
                curVerticalSpeed = initialJumpSpeed;
                isGrounded = false;
                isReadyToJump = false;
            }
        }
        else
        {
            //长按跳的更高
            //if (playerInput.JumpInput && curVerticalSpeed > 0f)
            //{
            //    curVerticalSpeed -= gravity * Time.deltaTime;
            //}
            if (Mathf.Approximately(curVerticalSpeed, 0f))
            {
                curVerticalSpeed = 0f;
            }
            else
                curVerticalSpeed -= 2 * gravity * Time.deltaTime;
        }
    }

    void CalculateRotation()
    {
        Vector3 moveInputDir = new Vector3(playerInput.MoveInput.x, 0, playerInput.MoveInput.y).normalized;

        Vector3 TPSCamForward = Quaternion.Euler(0, TPSCamera.m_XAxis.Value, 0) * Vector3.forward;

        Quaternion desiredRotation;

        if (Mathf.Approximately(Vector3.Dot(moveInputDir, Vector3.forward), -1)/*互为反向向量*/)
        {
            desiredRotation = Quaternion.LookRotation(-TPSCamForward);
        }
        else
        {
            Quaternion TPSCamForward2DesiredDir = Quaternion.FromToRotation(Vector3.forward, moveInputDir);
            desiredRotation = Quaternion.LookRotation(TPSCamForward2DesiredDir * TPSCamForward);
        }

        targetRotation = desiredRotation;

        Vector3 desiredDir = targetRotation * Vector3.forward;
        float currentAngle = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;
        shortestDeltaRotDegree = Mathf.DeltaAngle(currentAngle, targetAngle);
    }

    void CharacterRotate()
    {
        animator.SetFloat("DeltaDeg2Rag", shortestDeltaRotDegree * Mathf.Deg2Rad);

        curRotateSpeed = maxRotateSpeed * curSpeedRef / maxSpeedRef;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, curRotateSpeed * Time.deltaTime);
    }

    void TimeoutToIdle()
    {
        bool inputDetected = IsPressingMoveKey || playerInput.JumpInput|| playerInput.AttackInput;

        if (/*isGrounded*/IsGrounded() && !inputDetected)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTimeout)
            {
                idleTimer = 0f;
                animator.SetTrigger("IdleTrigger");
            }
        }
        else
        {
            idleTimer = 0f;
            animator.ResetTrigger("IdleTrigger");
        }

        animator.SetBool("HasMoveInput", IsPressingMoveKey);
        animator.SetBool("HasJumpInput", playerInput.JumpInput);
    }

    void StepClimb()
    {
        RaycastHit hitLower;
        if(Physics.Raycast(lowStepChecker.transform.position,transform.TransformDirection(Vector3.forward),out hitLower, 0.1f)){
            RaycastHit hitHigh;
            if (!Physics.Raycast(highStepChecker.transform.position, transform.TransformDirection(Vector3.forward), out hitHigh, 0.2f))
           {
                character.transform.position -= new Vector3(0f, -stepSmooth, 0);
            }
        }
    }

    private bool IsGrounded()
    {
        if (curVerticalSpeed > 0) return false;
        float floorDistanceFromFoot = character.stepOffset;

        RaycastHit hit;
       if (Physics.Raycast(transform.position, Vector3.down, out hit, floorDistanceFromFoot, stepLayer) || character.isGrounded)
        {
            return true;
        }

        return false;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public void Respawn()
    {
        isRespawning = true;

        animator.SetTrigger("ShouldRespawn");

        StartCoroutine(ResetRespawnState());
    }

    private IEnumerator ResetRespawnState()
    {

        yield return new WaitUntil(()=> animatorCache.currentStateInfo.IsName("Respawn") &&animatorCache.currentStateInfo.normalizedTime>1f);

        isRespawning = false;
    }

    void ShowHideMeleeWeapon(bool isShow)
    {
        if (meleeWeapon == null) return;

        meleeWeapon.gameObject.SetActive(isShow);

        if (!isShow)
            animator.ResetTrigger("MeleeAttackTrigger");
    }

    bool IsWeaponAnimationOnPlay()
    {
        bool isWeaponEquipped;

        isWeaponEquipped = animatorCache.nextStateInfo.tagHash == Animator.StringToHash("WeaponEquippedAnim") || animatorCache.currentStateInfo.tagHash == Animator.StringToHash("WeaponEquippedAnim");

        return isWeaponEquipped;
    }

    void DealWithMeleeAttackAnimation()
    {
        //动画归一化时间（0-1），在（0-1）上的repeat
        animator.SetFloat("MeleeStateTime", Mathf.Repeat(animatorCache.currentStateInfo.normalizedTime, 1f));


        //每一帧都reset，这样可以保证每次点击都触发一次trigger
        animator.ResetTrigger("MeleeAttackTrigger");

        if (playerInput.AttackInput)
        {
            animator.SetTrigger("MeleeAttackTrigger");
        }
    }

    public void MeleeAttackStart()
    {
        meleeWeapon.BeginAttack();
        isAttacking = true;
    }

    // This is called by an animation event when Ellen finishes swinging her staff.
    public void MeleeAttackEnd()
    {
        meleeWeapon.EndAttack();
        isAttacking = false;
    }

    public void OnReceiveMessage(MessageType type, object sender, object data)
    {

    }
}
