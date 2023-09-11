using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static Cinemachine.DocumentationSortingAttribute;

[Serializable]
public struct Section
{
    public Vector2 position;
    public GameObject firstSelected;
    public int backIndex;
}

public class MainMenu : MonoBehaviour
{
    [Header("Sections")]
    public Section[] sections;
    public int currentPosition;

    [Header("Options")]
    public bool useLerp;
    public float lerpTime = 0.5f;

    private MenuInputActions menuActions;

    public void GotoSection(int section)
    {
        if (useLerp)
            StartCoroutine(LerpSection(section));
        else
            Camera.main.transform.position = sections[section].position;
        currentPosition = section;
        EventSystem.current.SetSelectedGameObject(sections[section].firstSelected, new BaseEventData(EventSystem.current));
    }
    private IEnumerator LerpSection(int section)
    {
        float timer = lerpTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(sections[section].position, sections[currentPosition].position, timer / lerpTime);
            yield return new WaitForNextFrameUnit();
        }

        Camera.main.transform.position = sections[section].position;
    }

    private void Start()
    {
        menuActions = new MenuInputActions();
        menuActions.Enable();
        menuActions.MainMenu.No.performed += (InputAction.CallbackContext ctx) =>
        {
            if (sections[currentPosition].backIndex != -1)
            {
                GotoSection(sections[currentPosition].backIndex);
            }
        };
    }

    public void NewGame()
    {
        PlayerPrefs.SetInt("Level", 0);
        PlayerPrefs.SetInt("Exp", 0);
        PlayerPrefs.SetInt("Life", 10);
        PlayerPrefs.SetInt("UnlockSlide", 0);
        PlayerPrefs.SetInt("UnlockBible", 0);
        LoadScene("Level1");
    }

    public void LoadGame()
    {
        LoadScene(PlayerPrefs.GetString("CurrentLevel", "Level1"));
    }

    public void LoadScene(string sceneName)
    {
        menuActions.Disable();
        SceneManager.LoadScene(sceneName);
    }
}
