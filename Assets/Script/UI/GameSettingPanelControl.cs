using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingPanelControl : MonoBehaviour
{
    // 设置面板
    public GameObject gameSettingPanel;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (gameSettingPanel == null)
        {
            gameSettingPanel = transform.Find("Button/GameSettingPanel")?.gameObject;
        }
    }
    

    public void OpenGameSettingPanel()
    {
        gameSettingPanel.SetActive(true);
    }

    public void CloseGameSettingPanel()
    {
        gameSettingPanel.SetActive(false);
    }
}
