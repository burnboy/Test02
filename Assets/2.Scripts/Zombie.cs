using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using GameObjectType = Defines.GameObjectType;
using ZombieState = Defines.ZombieState;

public class Zombie : MonoBehaviour
{
    public GameObjectType GameType = GameObjectType.ZOMBIE;
    public ZombieState CurrentState = ZombieState.IDLE;
    public void SetState(ZombieState state)
    {
        CurrentState = state;
    }

    [SerializeField] private Animator animator = null;

    public Box TargetBox = null;

    [SerializeField] private BoxCollider2D zombieBoxCollider2D = null;
    [SerializeField] private Rigidbody2D zombieRigidBody2D = null;
    public Rigidbody2D GetRigidBody2D() => zombieRigidBody2D;

    float speed = 2.5f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        zombieBoxCollider2D = GetComponent<BoxCollider2D>();
        zombieRigidBody2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

    }

    void Update()
    {
#if UNITY_EDITOR && true
        //if(Input.GetKeyDown(KeyCode.Z))
        //{
        //    Attack();
        //}
        //if(Input.GetKeyDown(KeyCode.X))
        //{
        //    Idle();
        //}
        //if(Input.GetKeyDown(KeyCode.C))
        //{
        //    Dead();
        //}

        //if(Input.GetKeyDown(KeyCode.C))
        //{
        //    CurrentState = ZombieState.WALK;
        //}

#endif
        CheckState();
    }

    private void FixedUpdate()
    {
        //TargetBox = ZombieManager.Instance.GetNearBox(this);
    }

    private void CheckState()
    {
        switch(CurrentState)
        {
            case ZombieState.IDLE:
                Idle();
                break;

            case ZombieState.WALK:
                Walk();
                break;

            case ZombieState.ATTACK:
                Attack();
                break;

            case ZombieState.JUMP:
                ZombieJumpState();
                break;

            case ZombieState.DEATH:
                break;
        }

    }

    private void ZombieJumpState()
    {
        if(HasZombieOnTop())
            return;

        if(IsOnGround() == true && IsZombieInFront() == false)
        {
            if(CurrentState != ZombieState.WALK)
                CurrentState = ZombieState.WALK;
        }


        if(IsOnGround() && IsZombieInFront())
        {
            if(isJumping == false)
            {
                isJumping = true;

                float Randomtime = Random.Range(0.0f, 2.5f);
                if(jumpCor != null)
                {
                    StopCoroutine(jumpCor);
                    jumpCor = null;
                }

                jumpCor = StartCoroutine(Jump(Randomtime));
            }
        }
    }

    bool isJumping = false;

    public void InitializeZombie(Box targetBox)
    {
        this.TargetBox = targetBox;
        CurrentState = ZombieState.IDLE;
    }


    //가서 박스가 범위 이내에 보이면 공격
    //박스까지 거리 계산
    public void GoToBox()
    {
        CurrentState = ZombieState.WALK;

    }

    public void Walk()
    {
        if(TargetBox == null)
            return;

        float distance = Vector2.Distance(TargetBox.transform.position, transform.position);
        if(distance < 2.2f)
        {
            CurrentState = ZombieState.ATTACK;
            return;
        }


        if(IsOnGround() == true && IsZombieInFront() == false && isJumping == false)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetBox.transform.position, Time.deltaTime * speed);
        }

        //가다가 앞에 좀비가 있다면 점프상태로 변경
        else if(IsOnGround() == true && IsZombieInFront() == true)
        {
            CurrentState = ZombieState.JUMP;
        }

    }

    public void Attack()
    {
        //TargetBox = ZombieManager.Instance.GetNearBox(this);
        //약간의 오차가 있음
        float distance = Vector2.Distance(TargetBox.transform.position, transform.position);
        if(distance > 2.21f)
        {
            CurrentState = ZombieState.WALK;
            zombieRigidBody2D.constraints = RigidbodyConstraints2D.None |
                                            RigidbodyConstraints2D.FreezeRotation;
            Idle();
            return;
        }

        if(animator.GetBool("IsAttacking") == false)
        {
            zombieRigidBody2D.constraints = RigidbodyConstraints2D.FreezePositionX |
                                            RigidbodyConstraints2D.FreezeRotation;

            animator.SetBool("IsAttacking", true);
        }
    }

    public void Idle()
    {
        if(animator.GetBool("IsDead") == true)
        {
            animator.SetBool("IsDead", false);
        }

        if(animator.GetBool("IsAttacking") == true)
        {
            animator.SetBool("IsAttacking", false);
        }
    }

    public void Dead()
    {
        if(animator.GetBool("IsAttacking") == true)
        {
            animator.SetBool("IsAttacking", false);
        }

        animator.SetBool("IsDead", true);
    }

    public LayerMask groundLayer;
    public LayerMask zombieLayer; // 좀비 레이어 감지
    private float raycastDistance = 0.2f;

    //바닥 감지
    private bool IsOnGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(Down.transform.position, Vector2.down, raycastDistance*0.5f);
        bool check = false;
        if(hit.collider != null)
        {
            if(hit.collider.CompareTag("Ground") == true)
            {
                //isJumping = false;
                demp = 0.0f;
                check = true;
            }
            else if(hit.collider.CompareTag("Zombie") == true)
            {
                //isJumping = false;
                demp = 1.0f;
                check = true;
            }
        }

        return check;
    }

    //머리 위에 부딪혔는지
    private bool HasZombieOnTop()
    {
        RaycastHit2D hit = Physics2D.Raycast(Up.transform.position, Vector2.up, raycastDistance, groundLayer);
        return hit.collider != null && hit.collider.CompareTag("Zombie");
    }

    private bool IsZombieInFront()
    {
        RaycastHit2D hit = Physics2D.Raycast(Left.transform.position, Vector3.left, raycastDistance, zombieLayer);
        return hit.collider != null; // 좀비가 감지되면 true 
    }

    //
    public Transform Up, Left, Down;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Left.transform.position, Left.transform.position + Vector3.left * raycastDistance);
        Gizmos.DrawLine(Up.transform.position, Up.transform.position + Vector3.up * raycastDistance);
        Gizmos.DrawLine(Down.transform.position, Down.transform.position + Vector3.down * raycastDistance * 0.5f);

    }

    bool check = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //check = collision.gameObject.TryGetComponent<Box>(out Box box);
        //if(check == true)
        //{
        //    CurrentState = ZombieState.ATTACK;
        //    return;
        //}

        //check = collision.gameObject.TryGetComponent<Zombie>(out Zombie zombie);
        //if(check == false)
        //    return;

        //Vector2 otherPos = zombie.transform.position;
        //Vector2 myPos = transform.position;

        //Vector2 dir = otherPos - myPos;
        //Vector2 dirNormalized = dir.normalized;

        //CurrentState = ZombieState.JUMP;        //check = collision.gameObject.TryGetComponent<Box>(out Box box);
        //if(check == true)
        //{
        //    CurrentState = ZombieState.ATTACK;
        //    return;
        //}

        //check = collision.gameObject.TryGetComponent<Zombie>(out Zombie zombie);
        //if(check == false)
        //    return;

        //Vector2 otherPos = zombie.transform.position;
        //Vector2 myPos = transform.position;

        //Vector2 dir = otherPos - myPos;
        //Vector2 dirNormalized = dir.normalized;

        //CurrentState = ZombieState.JUMP;

        //if(CurrentState == ZombieState.WALK)
        //    CurrentState = ZombieState.IDLE;


        //zombieRigidBody2D.velocity = new Vector2(zombieRigidBody2D.velocity.x, 5.0f);


    }

    Coroutine jumpCor = null;
    float demp = 0.0f;
    IEnumerator Jump(float time)
    {
        yield return new WaitForSeconds(time);
        zombieRigidBody2D.velocity = new Vector2(zombieRigidBody2D.velocity.x, 5.0f + demp);
        Debug.Log($"바닥점프+ 보간치:{demp}");

        yield return new WaitUntil(() => IsOnGround() == true);
        isJumping = false;
        Debug.Log("점핑 끝!!");

        //yield return new WaitForSeconds(2.0f);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        check = collision.gameObject.TryGetComponent<Zombie>(out Zombie zombie);
        if(check == false)
            return;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        check = collision.gameObject.TryGetComponent<Zombie>(out Zombie zombie);
        if(check == false)
            return;

        //CurrentState = ZombieState.WALK;
    }


    public void MoveRight()
    {
        if(moveCor != null)
        {
            StopCoroutine(moveCor);
            moveCor = null;
        }
        moveCor = StartCoroutine(nameof(MoveRightCor));
    }

    Coroutine moveCor = null;
    IEnumerator MoveRightCor()
    {
        CurrentState = ZombieState.IDLE;
        Vector3 pos = transform.position;

        pos = pos + new Vector3(1.0f, 0.0f, 0.0f);

        float timer = 1.0f;
        float percent = 0.0f;
        while(percent < 1.0f)
        {
            percent += Time.deltaTime / timer;
            Vector3 targetPos = Vector3.Lerp(transform.position, pos, percent);
            transform.position = targetPos;
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);
        CurrentState = ZombieState.WALK;
    }


}
