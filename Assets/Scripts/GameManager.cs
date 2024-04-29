using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;
    
    public Image[] UIhealth;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public GameObject UIRestartBtn;

    private void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Stop the editor play mode
            #else
                Application.Quit(); // Quit the built game
            #endif
        }
    }
    public void NextStage()
    {
        // change stage
        if (stageIndex < Stages.Length - 1) {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE " + (stageIndex + 1);
        } else {    // Game Clear
            // Player Control Lock
            Time.timeScale = 0;

            // Result UI
            Debug.Log("게임클리어!");

            // Restart button
            TextMeshProUGUI btnText = UIRestartBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = "Game Clear!";
            UIRestartBtn.SetActive(true);
        }

        // calculate point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        health--;
        UIhealth[health].color = new Color(1, 0, 0, 0.4f);
       
        if(health < 1) {
            // player die effect
            player.OnDie();

            // result ui
            Debug.Log("죽었습니다!");

            // retry button ui
            UIRestartBtn.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player") {
            // Player reposition
            if (health > 1) {
                PlayerReposition();
            }

            // Health down
            HealthDown();
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
}
