using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spring : MonoBehaviour
{
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
                        GetComponent<Item>().SpringColorChange();
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
