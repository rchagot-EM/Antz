using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Unity.Entities;

public class KeyboardInput : MonoBehaviour
{
    Text text;
    static bool showText = true;
    EntityQuery universalQuery;
    String antz;

    void Start()
    {
        text = GetComponent<Text>();
        text.enabled = showText;
        universalQuery = World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery;
        antz = SceneManager.GetActiveScene().ToString();
    }

    void Update()
    {
        //Debug.Log(Time.timeScale);

        if (Input.GetKeyDown(KeyCode.H))
        {
            showText = !showText;
            text.enabled = showText;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(universalQuery);
            SceneManager.LoadScene("Antz");
        }

    }
}
