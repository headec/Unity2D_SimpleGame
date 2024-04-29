using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public float maxSpeed;
    public float jumpPower;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    CapsuleCollider2D capsuleCollider;
    AudioSource audioSource;

    void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(string action)
    {
        switch(action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }
        audioSource.Play();
    }

    void Update()
    {   
        // Jump
        if(Input.GetButtonDown("Jump") && !animator.GetBool("isJumping")) {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        // Stop Speed
        if(Input.GetButtonUp("Horizontal")) {
            // 벡터 크기를 1로 만든 상태(단위 벡터)
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }   

        // Direction Sprite
        // if(Input.GetButtonDown("Horizontal")) {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        // }

        // Animation
        // if(rigid.velocity.normalized.x == 0)
        //     animator.SetBool("isWalking", false);
        // else
        //     animator.SetBool("isWalking", true);

        animator.SetBool("isWalking", !(Mathf.Abs(rigid.velocity.x) < 0.5));


    }

    void FixedUpdate()
    {
        // Move Speed (By Key Control)
        float h = Input.GetAxisRaw("Horizontal");
        // float v = Input.GetAxisRaw("Vertical");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        // rigid.AddForce(Vector2.up * v, ForceMode2D.Impulse);

        // Max Speed
        if(rigid.velocity.x > maxSpeed) // Right Max Speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if(rigid.velocity.x < maxSpeed * (-1)) // Left Max Speed
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        // Landing Platform
        // RayCast: 오브젝트 검색을 위해 Ray를 쏘는 방식
        Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0)); // debug 용
        // RayCastHit: Ray에 닿은 오브젝트
        // LayerMask: 물리 효과를 구분하는 정수값 (오른쪽 위 레이어 추가)
        // arguments: 어디에서, 어디로, 단위벡터(1), GetMask() 레이어 이름에 해당하는 정수값을 리턴하는 함수
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if(rayHit.collider != null) {
            if(rayHit.distance < 0.5f && rigid.velocity.y < 0)
                animator.SetBool("isJumping", false);
                // Debug.Log(rayHit.collider.name);   
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Attack
            if(rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
                OnAttack(collision.transform);
            else // Damaged
                OnDamaged(collision.transform.position);
            //Debug.Log("플레이어가 맞았습니다!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Item") {
            // Point
            bool isBronze = collision.gameObject.name.Contains("bronze");
            bool isSilver = collision.gameObject.name.Contains("silver");
            bool isGold = collision.gameObject.name.Contains("gold");

            if (isBronze)      gameManager.stagePoint += 50;
            else if (isSilver) gameManager.stagePoint += 100;
            else if (isGold)   gameManager.stagePoint += 300;

            //gameManager.stagePoint += 100;

            // Deactivate Item
            collision.gameObject.SetActive(false);
            PlaySound("ITEM");
        }
        else if(collision.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.NextStage();
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        // Point
        gameManager.stagePoint += 100;

        // Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
        PlaySound("ATTACK");
    }

    void OnDamaged(Vector2 targetPos)
    {
        // health down
        gameManager.HealthDown();

        // change layer
        gameObject.layer = 11;

        // view alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // reaction force
        // current player pos - collision pos
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1)*7, ForceMode2D.Impulse);

        // animator
        animator.SetTrigger("doDamaged");

        Invoke("OffDamaged", 3);
        PlaySound("DAMAGED");
    }
    
    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Sprite Flip Y
        spriteRenderer.flipY = true;

        // Collider Disable
        capsuleCollider.enabled = false;

        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        PlaySound("DIE");
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}
