using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            collision.transform.GetComponent<Player>()._hp = 0;
            collision.transform.GetComponent<Player>().OnDamage();
        }
    }
}
