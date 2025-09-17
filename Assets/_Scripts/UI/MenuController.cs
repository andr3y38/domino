// File: _Scripts/UI/MenuController.cs

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }
}