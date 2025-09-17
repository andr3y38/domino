// File: _Scripts/UI/Preloader.cs

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Preloader : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject stakeLogoAnimationObject;
    public GameObject companyLogoAnimationObject;
    public Slider progressBar;
    public TextMeshProUGUI statusText;

    [Header("Preloader Sequence")]
    public float logoDisplayTime = 2.0f;

    private void Start()
    {
        StartCoroutine(LoadGameSequence());
    }

    private IEnumerator LoadGameSequence()
    {
        progressBar.value = 0;
        statusText.text = "Initializing...";
        stakeLogoAnimationObject.SetActive(false);
        companyLogoAnimationObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        statusText.text = "";
        stakeLogoAnimationObject.SetActive(true);
        yield return new WaitForSeconds(logoDisplayTime);
        stakeLogoAnimationObject.SetActive(false);

        companyLogoAnimationObject.SetActive(true);
        yield return new WaitForSeconds(logoDisplayTime);
        companyLogoAnimationObject.SetActive(false);

        statusText.text = "Connecting to server...";
        bool isAuthenticated = false;
        bool hasFailed = false;

        GameManager.OnAuthenticationComplete += () => { isAuthenticated = true; };
        GameManager.OnAuthenticationFailed += (errorMsg) => { 
            statusText.text = "Error: Could not connect."; 
            hasFailed = true;
        };
        
        GameManager.Instance.StartAuthentication();

        while (!isAuthenticated && !hasFailed)
        {
            yield return null;
        }

        if (hasFailed) yield break; 

        statusText.text = "Loading game...";
        
        // UPDATED LOGIC: Check the game state to decide which scene to load
        string sceneToLoad = (GameManager.Instance.CurrentState == GameManager.GameState.ResolvingRound)
            ? "MainGameScene" 
            : "MenuScene";

        Debug.Log($"Authentication complete. Loading scene: {sceneToLoad}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            progressBar.value = progress;
            yield return null;
        }
    }
}