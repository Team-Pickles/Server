using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    List<GameObject> Players = new List<GameObject>();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.transform.tag)
        {
            case "player":
                {
                    if (transform.parent.GetComponent<Enemy>().DetectPlayer == false)
                    {
                        transform.parent.GetComponent<Enemy>().DetectPlayer = true;
                        transform.parent.GetComponent<Enemy>().DetectedPlayer = collision.gameObject;
                        Players.Add(collision.gameObject);
                    }

                    else
                    {
                        Vector3 enemyTransform = transform.parent.GetComponent<Enemy>().transform.position;
                        List<float> distance = new List<float>();
                        Players.Add(collision.gameObject);
                        foreach (GameObject player in Players)
                        {
                            distance.Add(Vector3.Distance(enemyTransform, player.transform.position));
                        }
                        var minValue = distance.Min();
                        var index = distance.IndexOf(minValue);
                        transform.parent.GetComponent<Enemy>().DetectedPlayer = Players[index];
                    }
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
                    Vector3 enemyTransform = transform.parent.GetComponent<Enemy>().transform.position;
                    transform.parent.GetComponent<Enemy>().DetectPlayer = true;
                    List<float> distance = new List<float>();
                    foreach (GameObject player in Players)
                    {
                        distance.Add(Vector3.Distance(enemyTransform, player.transform.position));
                    }
                    var minValue = distance.Min();
                    var index = distance.IndexOf(minValue);
                    transform.parent.GetComponent<Enemy>().DetectedPlayer = Players[index];
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
                    Players.Remove(collision.gameObject);
                    break;
                }
        }
    }
}
