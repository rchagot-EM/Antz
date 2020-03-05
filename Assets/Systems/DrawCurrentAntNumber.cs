using Unity.Entities;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DrawCurrentAntNumber : MonoBehaviour
{
    Text text;
    static bool showText = true;

    void Start()
    {
        text = GetComponent<Text>();
        text.enabled = showText;
    }

    void Update()
    {
        var TotalAnts = World.DefaultGameObjectInjectionWorld.GetExistingSystem<AntSpawnSystem>().TotalAnts.ToString();

        if (Input.GetKeyDown(KeyCode.H))
        {
            showText = !showText;
            text.enabled = showText;
        }

        text.text = "Total Number of Ants: " + TotalAnts;
    }
}
