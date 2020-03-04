using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class DrawCurrentTimeScale : MonoBehaviour
{
    Text text;
    static bool showText = true;
    OverrideWorldTimeSystem timeSystem;

    void Start()
    {
        text = GetComponent<Text>();
        text.enabled = showText;
        timeSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<OverrideWorldTimeSystem>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            showText = !showText;
            text.enabled = showText;
        }

        text.text = "Current Time Scale: " + timeSystem.timeScale.ToString();
    }
}
