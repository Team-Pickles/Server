using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Milestone : MonoBehaviour
{
    public Transform toward;
    public bool isDestroy;
    public float speed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "spring(Clone)")
        {
            GameObject activedObj = collision.gameObject;
            if (isDestroy)
            {
                Item _item = activedObj.GetComponent<Item>();
                _item.server.serverSend.DestroyItem(_item);
                Destroy(activedObj);
            }
            activedObj.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, 0.0f);
            Vector2 direction = (toward.localPosition - transform.localPosition).normalized;
            activedObj.GetComponent<Rigidbody2D>().AddForce(direction * speed);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
