using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    public float maxSpeed;
    public float jumpPower;
    public int maxJump;
   
    int flip;
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    public CapsuleCollider2D capsuleCollider;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    AudioSource audioSource;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        flip = 1;
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>(); ;
    }


    void Playsound(string action)
    {
        switch (action) {
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
    private void Update()//단발적인 키 입력
    {
        //jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            Playsound("JUMP");
        }
           
        //stop horizontal
        if (Input.GetButtonUp("Horizontal"))
        {
            //벡터의 단위(방향)을 구할 때 rigid.velocity.normalized
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        //flip player
        if (Input.GetButton("Horizontal"))
        {

            if(flip * Input.GetAxisRaw("Horizontal") == -1)
            {
                spriteRenderer.flipX = true;
            }

            if (flip * Input.GetAxisRaw("Horizontal") == 1)
            {
                spriteRenderer.flipX = false;
            }
        }

        //idle or walk animation activate
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            anim.SetBool("isWalking", false);
        }
        else
            anim.SetBool("isWalking", true);
    }
    void FixedUpdate() //1초에 50번 정도,물리 기반
    {
        //Move by Key Control
        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.velocity.x > maxSpeed) //Right Max Speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if(rigid.velocity.x < (-1)*maxSpeed) //Left Max  Speed
            rigid.velocity = new Vector2(maxSpeed*(-1), rigid.velocity.y);
        
        //RayCast (Landing at platform)
        if (rigid.velocity.y < 0)
        {

            Debug.DrawRay(rigid.position, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 2, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance <1f)
                {
                    //Debug.Log(rayHit.collider.name);
                    anim.SetBool("isJumping", false);
                }

            }

        }
        
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Spike")
        {   
            if(collision.gameObject.tag == "Spike")
            {
                OnDameged(collision.transform.position);
                Playsound("DAMAGED");
            }

            //When player Attack
            else if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
                Playsound("ATTACK");

            }
            else
            {
                OnDameged(collision.transform.position);
                Playsound("DAMAGED");
            }
        }


    }

    void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (collision.gameObject.tag == "Item")
        {
            //point
            //bool isBronze = collision.gameObject.name.Contains("Bronze");
            gameManager.stagePoint += 100;

            //Deactive Item
            Playsound("ITEM");
            collision.gameObject.SetActive(false);
            Destroy(collision.gameObject);
        }  
        else if (collision.gameObject.tag == "Finish")
        {
            // Next stage
            gameManager.NextStage();
            Playsound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        //Point

        //Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        
        //Enemy Dead
        Enemy_Move enemy_Move = enemy.GetComponent<Enemy_Move>();
        enemy_Move.OnDameged();
    }
    void OnDameged(Vector2 targetPos)
    {
        //Health down
        gameManager.HealthDown();
        //sound
        Playsound("DAMAGED");
        //Change Layer
        gameObject.layer = 11;

        //View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);

        //Reaction Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1)*7, ForceMode2D.Impulse);


        //Animation
        anim.SetTrigger("isDamaged");

        Invoke("OffDameged",0.5f);

    }

    void OffDameged()
    {
        //Change Layer
        gameObject.layer = 10;

        //View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 1);

    }

    public void OnDie()
    {
        //sound
        Playsound("DIE");
        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);

        //Sprite flipY
        spriteRenderer.flipY = true;

        //Collider Disable
        capsuleCollider.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}
