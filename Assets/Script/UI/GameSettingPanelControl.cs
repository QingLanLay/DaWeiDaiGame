using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingPanelControl : MonoBehaviour
{
#region 变量声明
    // 设置面板
    public GameObject gameSettingPanel;
    public Button gameSettingButton;
    public Button backButton;
    public GameObject startPanel;
#endregion

#region Unity 生命周期方法
    // Start is called before the first frame update
    void Start()
    {
        if (gameSettingPanel == null)
        {
            gameSettingPanel = gameObject;
        }

        Time.timeScale = 0;

        gameSettingButton.onClick.AddListener(OpenGameSettingPanel);
        backButton.onClick.AddListener(CloseGameSettingPanel);
    }
#endregion

#region 面板控制方法
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
#endregion
}