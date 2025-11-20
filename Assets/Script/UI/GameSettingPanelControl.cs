using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingPanelControl : MonoBehaviour
{
    // 设置面板
    public GameObject gameSettingPanel;
    public Button gameSettingButton;
    public Button backButton;

    public GameObject startPanel;

    // Start is called before the first frame update
    void Start()
    {
        if (gameSettingPanel == null)
        {
            gameSettingPanel = transform.Find("Button/GameSettingPanel")?.gameObject;
        }

        Time.timeScale = 0;

        gameSettingButton.onClick.AddListener(OpenGameSettingPanel);
        backButton.onClick.AddListener(CloseGameSettingPanel);
    }

    //攻击力//
    public void OpenGameSettingPanel()
    {
        if (!gameSettingPanel.activeSelf)
        {
            gameSettingPanel.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            gameSettingPanel.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void CloseGameSettingPanel()
    {
        gameSettingPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void StartGame()
    {
        Time.timeScale = 1;
        gameSettingPanel.SetActive(false);
        startPanel.SetActive(false);
    }
    
    
    
}