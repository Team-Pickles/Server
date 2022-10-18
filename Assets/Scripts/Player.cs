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

    private float _hPoint = 0, _vPoint = 0;
    private const float _hSpeed = 4.0f, _vSpeed = 5.0f;
    float speed = 1.0f;

    private bool[] inputs;
    private float yVelocity = 0;

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

}
