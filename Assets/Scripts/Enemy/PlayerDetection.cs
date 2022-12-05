using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.transform.tag)
        {
            case "player":
                {
                    transform.parent.GetComponent<Enemy>().DetectPlayer = true;
                    transform.parent.GetComponent<Enemy>().DetectedPlayer = collision.gameObject;
                    break;
                }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        switch (collision.transform.tag)
        {
            case "player":
                {
                    transform.parent.GetComponent<Enemy>().DetectPlayer = true;
                    transform.parent.GetComponent<Enemy>().DetectedPlayer = collision.gameObject;
                    break;
                }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.transform.tag)
        {
            case "player":
                {
                    transform.parent.GetComponent<Enemy>().DetectPlayer = false;
                    transform.parent.GetComponent<Enemy>().DetectedPlayer = null;
                    break;
                }
        }
    }
}
