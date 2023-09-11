using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WinOverScreen : MonoBehaviour
{
    private MenuInputActions menuActions;

    void Start()
    {
        menuActions = new MenuInputActions();
        menuActions.Enable();
        menuActions.MainMenu.Yes.performed += Return;
    }

    void Return(InputAction.CallbackContext context)
    {
        menuActions.Disable();
        SceneManager.LoadScene("MainMenu");
    }
}
