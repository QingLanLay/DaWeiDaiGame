using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Image gameOverImage;
    public Button restartButton;
    public Button quitButton;
    public Text scoreText;
    public event Action StartGame;
    public PlayerController player;
    public GameObject davidDie;
    private DavidDie davidDieCp;
    
    // Start is called before the first frame update
     void Awake()
    {

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        davidDieCp = davidDie.GetComponent<DavidDie>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "得分:" + davidDieCp.currentExp;
    }

    void Quit()
    {
        Quit();
    }

    public void RetunGame()
    {
        
    }

    public void OpenPanel()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);

    }
    
}
