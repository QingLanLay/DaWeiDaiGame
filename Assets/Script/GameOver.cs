using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Image gameOverImage;
    public Button restartButton;
    public Button quitButton;
    public TextMeshProUGUI scoreText;
    public event Action StartGame;
    public PlayerController player;
    public DavidDie davidDieCp;
    public GameObject gameSettingPanel;
    
    // Start is called before the first frame update
     void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "得分:" + davidDieCp.currentExp;
    }

   public void Quit()
    {
       Application.Quit();
    }

    public void RetunGame()
    {
        player.InitializePlayer();
        FallingObjManager.Instance.InitializeFallingObjManager();
        EnemyManager.Instance.InitializeEnemyManager();
        davidDieCp.InitializeDavidDie();
        BulletManager.Instance.InitializeBulletManager();

        if (gameObject.activeSelf == true)
        {
            ClosePanel();
        }
        
        AudioManager.Instance.StopEffectAudio();
        
        var gameSettingPanelControl = gameSettingPanel.GetComponent<GameSettingPanelControl>();
        Debug.Log("设置面板开启");
        if (gameSettingPanelControl != null)
        {
            Debug.Log("成功获取");

            gameSettingPanelControl.CloseGameSettingPanel();
        }
    }

    public void OpenPanel()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }

    void ClosePanel()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
    
}
