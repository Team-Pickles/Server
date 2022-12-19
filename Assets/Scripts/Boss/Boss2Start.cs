using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2Start : MonoBehaviour
{
    public GameObject boss;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player"))
        {
            boss.SetActive(true);
            Destroy(gameObject);
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
