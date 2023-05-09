using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadGameScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(5);
    }

    public void LoadMainMenuScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
