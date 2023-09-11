using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public string levelName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterMovement character = collision.GetComponent<CharacterMovement>();
            character.actions.Disable();
            PlayerPrefs.SetInt("Level", character.level);
            PlayerPrefs.SetInt("Exp", character.exp);
            PlayerPrefs.SetInt("Life", character.life);
            PlayerPrefs.SetString("CurrentLevel", levelName);
            SceneManager.LoadScene(levelName);
        }
    }
}
