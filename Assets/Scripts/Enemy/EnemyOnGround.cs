using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyOnGround : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.transform.tag)
        {
            case "floor":
            case "stool":
                {
                    transform.parent.GetComponent<Enemy>().OnGround = true;
                    break;
                }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        switch (collision.transform.tag)
        {
            case "floor":
            case "stool":
                {
                    transform.parent.GetComponent<Enemy>().OnGround = true;
                    break;
                }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.transform.tag)
        {
            case "floor":
            case "stool":
                {
                    transform.parent.GetComponent<Enemy>().OnGround = false;
                    break;
                }
        }
    }
}
