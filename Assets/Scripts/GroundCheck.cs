using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
         transform.parent.GetComponent<Enemy>().OnGround = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        transform.parent.GetComponent<Enemy>().OnGround = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
