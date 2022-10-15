using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//점수, 스테이지 관리
public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health=3;
    public Player_Move player;
    public GameObject[] Stages;

    public Image[] UIhealth;
    public Text UiPoint;
    public Text UiStage;
    public GameObject RtButton;

    public AudioClip audioFalling;
    AudioSource audioSource;

    private void Update()
    {
        UiPoint.text = (totalPoint + stagePoint).ToString();
        UiStage.text = "Stage " + (stageIndex + 1);
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    public void NextStage()
    {
        //respone
        if (stageIndex < Stages.Length-1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerRespon();

            UiStage.text = "Stage " + (stageIndex+1);
        }
        else
        {
            //Game Clear

            //Player Control Lock
            Time.timeScale = 0;
            //Result UI
            Debug.Log("Clear!");
            //Restart Button UI
            Text btnText = RtButton.GetComponentInChildren<Text>();
            btnText.text = "Clear!";
            RtButton.SetActive(true);
        }

        //cal point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        Debug.Log(health);
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1, 1, 1, 0.2f);
        }
        else
        {
            health--;
            //All Health UI Off
            UIhealth[health].color = new Color(1, 1, 1, 0.2f);
            //player die effect
            player.OnDie();

            //result ui
            Debug.Log("You Died");

            //retry button ui
            Text btnText = RtButton.GetComponentInChildren<Text>();
            btnText.text = "Retry?";
            RtButton.SetActive(true);
            Invoke("freezeTime",1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Player Respone
            if (health > 1)
            {
                audioSource.clip = audioFalling;
                audioSource.Play();
                PlayerRespon();
            }

            //HP Down
            HealthDown();
        }
            
    }

    void PlayerRespon()
    {
        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();

    }

    public void Restart()
    {
        restTimeScale();
        SceneManager.LoadScene(0);
    }

    public void freezeTime()
    {
        Time.timeScale = 0;
    }

    public void restTimeScale()
    {
        Time.timeScale = 1;
    }
}
