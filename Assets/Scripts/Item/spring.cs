using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringAction : MonoBehaviour
{
    private IEnumerator ChangeColor()
    {
        float time = 0.0f;
        while (time <= 0.5f)
        {
            time += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "player":
                {
                    Player player;
                    // player check, not collider.
                    if (collision.TryGetComponent<Player>(out player))
                    {
                        player.OnJumpSpringAction();
                        StartCoroutine(ChangeColor());
                    }
                    break;
                }
            case "stool":
                {
                    Vector2 velocity = collision.transform.GetComponent<Rigidbody2D>().velocity;
                    collision.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(velocity.x, 10.0f);
                    break;
                }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "stool":
                {
                    Vector2 velocity = collision.transform.GetComponent<Rigidbody2D>().velocity;
                    collision.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(velocity.x, 10.0f);
                    break;
                }
        }
    }
}
