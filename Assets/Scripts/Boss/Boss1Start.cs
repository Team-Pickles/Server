using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1Start : MonoBehaviour
{
    public Boss1 boss;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        if (collision.name.Contains("Player"))
        {
            Player player = collision.GetComponent<Player>();
            SpawningPool spawningPool = GameObject.Find("JumpSpringSpawner").GetComponent<SpawningPool>();
            spawningPool.init(player.server, player.room);
            boss.SetState(new Boss1Idle(boss));
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
