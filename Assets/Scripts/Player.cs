using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public Transform shootOrigin;
    public Rigidbody2D rigidbody;
    public CharacterController controller;
    public float gravity = -9.18f;
    public float moveSpeed = 5.0f;
    public float dashSpeed = 7f;
    public float jumpSpeed = 5f;
    public float throwForce = 200f;
    public float health;
    public float maxHealth = 100f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;
    public bool onGround = false;
    public Vector3 facing = Vector3.zero;
    public GameObject _curItem;

    private float _hPoint = 0, _vPoint = 0;
    private const float _hSpeed = 4.0f, _vSpeed = 5.0f;
    float speed = 1.0f;

    private bool[] inputs;
    private float yVelocity = 0;

    private bool isVaccume = false;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime* Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        inputs = new bool[4];
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        if (inputs[0])
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (inputs[1])
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
            
    }

    public void FixedUpdate()
    {
        if (health <= 0)
            return;

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.x += 1;
        }
        if (inputs[1])
        {
            _inputDirection.x -= 1;
        }
        Move(_inputDirection);

        if (isVaccume==true)
        {
            Vaccume();
        }
    }

    public void Move(Vector2 _inputDirection)
    {
       
        Vector2 _moveDirection = transform.right * _inputDirection.x;
        if (transform.rotation == Quaternion.Euler(0, 180, 0))
            _moveDirection *= -1;
        var _speed = inputs[3] == true ? _hSpeed * 1.3f : _hSpeed ;
        if (IsGrouned())
        {
            yVelocity = 0f;
            if (inputs[2])
            {
                _vPoint = jumpSpeed;
            }
        }
 
        //controller.Move(_moveDirection);

        rigidbody.velocity = new Vector2(_moveDirection.x * _speed, GetComponent<Rigidbody2D>().velocity.y + _vPoint * _vSpeed);
        _vPoint = 0.0f;
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public bool IsGrouned()
    {
 
        RaycastHit2D _hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y-1.3f), Vector2.down, 0.2f);
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
    public void ThrowItem(Vector3 _throwDirection)
    {
        if (health <= 0f)
        {
            return;
        }
        NetworkManager.instance.InstantiatProjectile(shootOrigin).Initialize(_throwDirection, throwForce, id);

    }

    public void Shoot(Vector3 _viewDirection)
    {
        if (health <= 0f)
        {
            return;
        }
        /*
        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
            else if (_hit.collider.CompareTag("Enemy"))
            {
                _hit.collider.GetComponent<Enemy>().TakeDamage(50f);
            }
        }*/

        NetworkManager.instance.InstantiatbBulletPrefab(shootOrigin).Initialize(_viewDirection, throwForce, id);
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
        float _yPosition = shootOrigin.position.y >= 0 ? shootOrigin.position.y * 0.9f : shootOrigin.position.y * 1.1f;
        Vector2 _startOrigin = new Vector2(shootOrigin.position.x + shootOrigin.right.x * 1.2f, _yPosition);
        float _rayLength = 8.0f;

        int layerMask = 1 << LayerMask.NameToLayer("Item") | 1 << LayerMask.NameToLayer("Platform");

        for (int i = -5; i <= 5; i++)
        {
            Vector2 _rayDirection = new Vector2(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i * Mathf.Deg2Rad));
            RaycastHit2D hit = Physics2D.Raycast(_startOrigin, _rayDirection* shootOrigin.right.x, _rayLength, layerMask);
            
            if (hit.collider != null)
            {
                Debug.DrawLine(_startOrigin, hit.point, Color.green);
                if (hit.transform.CompareTag("Item"))
                {
                    _curItem = hit.transform.gameObject;
                    _curItem.GetComponent<Rigidbody2D>().AddForce((hit.point - _startOrigin).normalized * -0.5f);
                }
            }

        }

    }
    
    /*
    private void OnCollisionEnter2D(Collision2D _collision)
    {
        
        if(_collision.gameObject.CompareTag("Item") && isVaccume)
        {
            Item _item = _collision.gameObject.GetComponent<Item>();
            _item.DeleteItem();
            ServerSend.ItemCollide(_item);
        }
    }
    */
}
