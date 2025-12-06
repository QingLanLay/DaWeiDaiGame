using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Image gameOverImage;
    public Image endImage;
    public Button restartButton;
    public Button quitButton;
    public TextMeshProUGUI scoreText;
    public event Action StartGame;
    public PlayerController player;
    public DavidDie davidDieCp;
    public GameObject gameSettingPanel;

    public List<Sprite> endImageList = new List<Sprite>();
    public TextMeshProUGUI endText;
    
    // Start is called before the first frame update
     void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "得分:" + davidDieCp.currentExp;
        if (davidDieCp.currentExp <= 5000)
        {
            endImage.sprite = endImageList[0];
            endText.text = "菜就多练！";
        }
        
        if (davidDieCp.currentExp >= 5000 && davidDieCp.currentExp<= 50000)
        {
            endImage.sprite = endImageList[1];
            endText.text = "带派不老铁！";
        }
        
        if (davidDieCp.currentExp >= 50000 && davidDieCp.currentExp <= 200000)
        {
            endImage.sprite = endImageList[2];
            endText.text = "良子大胃袋，属实挺带派！";
        }
        
        if (davidDieCp.currentExp >= 200000)
        {
            endImage.sprite = endImageList[3];
            endText.text = "拯救了瑞贝卡，你就是夜之城活着的传奇！";
        }
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
