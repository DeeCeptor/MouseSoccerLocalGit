﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour 
{
    public void LoadScene(string scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }


    public void Quit()
    {
        Application.Quit();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Quit();
    }
}
