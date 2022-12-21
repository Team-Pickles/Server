using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

enum EnemyState { Normal, Captive}

public class Enemy : MonoBehaviour
{
    public int id;
    private static int nextEnemyId = 1;
    public Server server;
    public Room room;
    
    private GameObject _player;
    private bool _onGround = false;
    private bool _detectPlayer = false;
    public bool _isMove = false;
    private float _xSpeed = 5.0f;
    public int type=0;
    public bool OnGround
    {
        get { return _onGround; }
        set { _onGround = value; }
    }
    public bool DetectPlayer
    {
        get { return _detectPlayer; }
        set { _detectPlayer = value; }
    }
    public GameObject DetectedPlayer
    {
        get { return _player; }
        set { _player = value; }
    }
    private bool isDead = false;
    private int hitPoint = 1;

    private EnemyState state = EnemyState.Normal;

    public void Initialize(Server _server, Room _room, int _type)
    {
        id = nextEnemyId;
        nextEnemyId++;
        server = _server;
        room = _room;
        type = _type;
    }
    void FixedUpdate()
    {
       if(tag != "Boss")
        {
            MoveEnemy();
            if (GetComponent<Rigidbody2D>().velocity.x != 0 || GetComponent<Rigidbody2D>().velocity.y != 0 || CompareTag("ImmortalEnemy"))
            {
                server.serverSend.EnemyPosition(this);
            }
                
        }
    }

    private void MoveEnemy()
    {
        if (_onGround && CompareTag("enemy"))
        {
            if (_detectPlayer && _player != null)
            {
                _isMove = true;

                if (_player.transform.localPosition.x < transform.localPosition.x) // left
                {
                    Vector2 scale = transform.localScale;
                    scale.x = scale.x > 0.0f ? scale.x : -scale.x;
                    transform.localScale = scale;

                    GetComponent<Rigidbody2D>().velocity = new Vector2(-_xSpeed, 4.8f);
                }
                else // right
                {
                    Vector2 scale = transform.localScale;
                    scale.x = scale.x > 0.0f ? -scale.x : scale.x;
                    transform.localScale = scale;

                    GetComponent<Rigidbody2D>().velocity = new Vector2(_xSpeed, 4.8f);
                }
            }
            else
            {
                _isMove = false;
                GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, 0.0f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.transform.tag);
            switch (collision.transform.tag)
            {
                case "player":
                    {
                        collision.transform.GetComponent<Player>().OnDamage();
                        break;
                    }
                case "bullet":
                    {
                    if (CompareTag("enemy") || CompareTag("immortalenemy"))
                            StartCoroutine(HitAction());
                        break;
                    }
            }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.transform.tag)
        {
            case "player":
                {
                    collision.transform.GetComponent<Player>().OnDamage();
                    break;
                }
            case "bullet":
                {
                    if (CompareTag("enemy") || CompareTag("immortalenemy"))
                        StartCoroutine(HitAction());
                    break;
                }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.transform.tag)
        {
            case "player":
                {
                    other.transform.GetComponent<Player>().OnDamage();
                    break;
                }
            case "bullet":
                {
                    if (CompareTag("enemy") || CompareTag("immortalenemy"))
                        StartCoroutine(HitAction());
                    break;
                }
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        switch (other.transform.tag)
        {
            case "player":
                {
                    other.transform.GetComponent<Player>().OnDamage();
                    break;
                }
            case "bullet":
                {
                    if (CompareTag("enemy") || CompareTag("immortalenemy"))
                        StartCoroutine(HitAction());
                    break;
                }
        }
    }

    IEnumerator HitAction()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        server.serverSend.EnemyHit(this);
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}