using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum KeyInput
{
    RIGHT = 0,
    LEFT,
    DASH,
    UPARROW,
    DOWNARROW,
    //0: RIGHT
    //1: LEFT
    //3: CHANGE (Z)
    //4: UP
    //5: DOWN
}

public enum PlayerStateFlags
{
    Normal = 1 << 0,
    Stun = 1 << 1,
    Damaged = 1 << 2
}

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public int _hp;
    public int maxHealth = 3;
    public int itemAmount = 0;
    public int maxItemAmount = 3;
    public bool onGround = false;
    public Vector3 facing = Vector3.zero;
    public PlayerStateFlags _state = PlayerStateFlags.Normal;

    //Vacume
    public GameObject _curItem;
    private bool isVaccume = false;

    //Shoot
    private GameObject _firePoint;

    //Hanging
    public bool onRope = false;
    public bool isHanging = false;
    Vector3 ropePosition;

    //Move
    private float _hPoint = 0, _vPoint = 0;
    private const float _hSpeed = 4.0f, _vSpeed = 5.0f;
    float speed = 1.5f;
    public bool _flip;
    public bool isJumping;

    private bool[] inputs;

    public Server server;
    public Room room;
   
    private bool isJumpPressed = false;
    bool temp = false;
    public int _fromClient;

    public int BulletCount = 0;
    public int GrenadeCount = 0;

    public void SetPlayerStateFlags(PlayerStateFlags flag)
    {
        _state |= flag;
    }
    public void ResetPlayerStateFlags(PlayerStateFlags flag)
    {
        _state &= ~flag;
    }

    private void Start()
    {
        _firePoint = GameObject.Find("FirePoint");
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        _hp = maxHealth;
        inputs = new bool[5];
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
    }

    public void FixedUpdate()
    {
        if (_hp <= 0)
            return;

        Move();
        if (isHanging)
            GetComponent<Rigidbody2D>().velocity = new Vector2(_hPoint * _hSpeed, _vPoint * _vSpeed);
        else
            GetComponent<Rigidbody2D>().velocity = new Vector2(_hPoint * _hSpeed, GetComponent<Rigidbody2D>().velocity.y + _vPoint * _vSpeed);

        server.serverSend.PlayerPosition(this);
        server.serverSend.PlayerRotation(this);
        server.serverSend.SendFlip(this);

        if (isJumping)
        {
            isJumping = IsGrouned() ? false : true;
        }

        if (isVaccume == true)
        {
            Vaccume();
        }

        _vPoint = 0.0f;
        speed = 1.5f;
        _hPoint = 0.0f;
    }

    public void Move()
    {

        if (isHanging)
        {
            _vPoint = (inputs[(int)KeyInput.UPARROW] ? 0.8f : 0) + (inputs[(int)KeyInput.DOWNARROW] ? -0.8f : 0);
        }


        else if (!isHanging)
        {
            speed += inputs[(int)KeyInput.DASH] ? 1 : 0;
            _hPoint = (inputs[(int)KeyInput.LEFT] ? -speed : 0) + (inputs[(int)KeyInput.RIGHT] ? speed : 0);

            if (_hPoint > 0)
            {
                if (_firePoint.transform.localPosition.x < 0)
                    _firePoint.transform.localPosition = new Vector3(-_firePoint.transform.localPosition.x, _firePoint.transform.localPosition.y, 0.0f);

                Vector2 offset = GetComponent<BoxCollider2D>().offset;
                offset.x = 0.06f;
                _flip = false;
            }
            else if (_hPoint < 0)
            {
                if (_firePoint.transform.localPosition.x > 0)
                    _firePoint.transform.localPosition = new Vector3(-_firePoint.transform.localPosition.x, _firePoint.transform.localPosition.y, 0.0f);

                Vector2 offset = GetComponent<BoxCollider2D>().offset;
                offset.x = 0.06f;
                _flip = true;
            }
        }
    }

    public void Jump()
    {
        if (isJumping)
            return;

        if (isHanging)
        {
            _vPoint = 1.2f;
            isJumping = true;
            isHanging = false;
            GetComponent<Rigidbody2D>().gravityScale = 1.0f;
            server.serverSend.RopeACK(this);
        }

        if (!isJumping && IsGrouned())
        {
            _vPoint = 1.2f;
            isJumping = true;
        }

    }

    public void OnRopeAction()
    {
        if (onRope && !isHanging)
        {
            isHanging = true;
            isJumping = false;
            transform.position = new Vector2(ropePosition.x, transform.position.y);
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Rigidbody2D>().gravityScale = 0.0f;
            _hPoint = 0.0f;
            server.serverSend.RopeACK(this);
        }

    }

    public void OnJumpSpringAction()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0);
        _vPoint = 1.5f;
    }

    public bool IsGrouned()
    {
        RaycastHit2D _hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2.0f), Vector2.down, 0.04f);
        //Debug.DrawRay(new Vector2(transform.position.x, transform.position.y - transform.localScale.y / 2.0f), Vector2.down * 0.04f, Color.red, 0.5f);

        if (_hit.collider == null)
        {
            return false;
        }
        else
        {
            if (_hit.collider.CompareTag("floor") == true)
            {
                return true;
            }
        }
        return false;
    }

    public void ShootBullet(Vector3 _viewDirection)
    {
        GameObject obj;
        if (_hp <= 0f || BulletCount <=0)
        {
            return;
        }
        
        int _isFlip = _flip ? -1 : 1;
        obj = RoomManager.instance.InstatiateBullet(room.PlayerGroup, _firePoint.transform, id);
        Bullet bullet = obj.GetComponent<Bullet>();
        bullet.Initialize(id, this.server, _isFlip);
        BulletCount--;
    }

    public void ShootGrenade(Vector3 _viewDirection)
    {
        GameObject obj;
        if (_hp <= 0f || GrenadeCount <= 0)
        {
            return;
        }

        int _isFlip = _flip ? -1 : 1;
        obj = RoomManager.instance.InstatiateGrenade(room.PlayerGroup, _firePoint.transform, id);
        Projectile projectile = obj.GetComponent<Projectile>();
        projectile.Initialize(id, this.server, this.room, _isFlip);
        GrenadeCount--;
    }

    public void StartVaccume(Vector3 _vaccumeDirection)
    {
        facing = _vaccumeDirection;
        isVaccume = true;
    }

    public void EndVaccume()
    {
        isVaccume = false;
    }

    public void Vaccume()
    {
        float _rayLength = 8.0f;
        Vector2 rayOrigin = _firePoint.transform.position;

        int layerMask = 1 << LayerMask.NameToLayer("Item") | 1 << LayerMask.NameToLayer("Platform");

        for (int i = -5; i <= 5; i++)
        {
            Vector2 _rayDirection = new Vector2(Mathf.Cos(i * Mathf.Deg2Rad) * (_flip ? -1 : 1), Mathf.Sin(i * Mathf.Deg2Rad));
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, _rayDirection, _rayLength, layerMask);

            if (hit.collider != null)
            {
                Debug.DrawLine(rayOrigin, hit.point, Color.green);
                if (hit.transform.CompareTag("trash"))
                {
                    _curItem = hit.transform.gameObject;
                    _curItem.GetComponent<Rigidbody2D>().AddForce((hit.point - rayOrigin).normalized * -3.0f);
                    _curItem.GetComponent<Rigidbody2D>().angularVelocity = 200.0f;
                }
            }

        }

    }

    private void OnTriggerStay2D(Collider2D collision)
    {    
        if (collision.gameObject.CompareTag("rope"))
        {
            onRope = true;
            ropePosition = collision.transform.position;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("rope"))
        {
            onRope = false;
        }
    }

    public void OnDamage()
    {
        server.serverSend.PlayerDamaged(this);
        StartCoroutine(Damaged());
    }

    private IEnumerator Damaged()
    {
        if ((_state & PlayerStateFlags.Damaged) == 0)
        {
            if (_hp > 0)
                _hp--;
            SetPlayerStateFlags(PlayerStateFlags.Damaged);
            SetPlayerStateFlags(PlayerStateFlags.Stun);
            _hPoint = -0.7f * (_flip == false ? 1 : -1);
            _vPoint = 0.5f;
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0);
            for (int i = 0; i < 6; i++)
            {
                yield return new WaitForSeconds(0.1f);
            }
            _hPoint = 0.0f;
            // immortal state
            {
                ResetPlayerStateFlags(PlayerStateFlags.Stun);
                yield return new WaitForSeconds(0.7f);
                ResetPlayerStateFlags(PlayerStateFlags.Damaged);
            }
            yield break;
        }
    }
}
