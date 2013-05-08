using UnityEngine;
using System.Collections;

public class MainMenuGUI : MonoBehaviour 
{
    public GUIStyle Play;

    void OnGUI()
    { 
        if(GUI.Button(new Rect(10,10,150,100),"Play",Play))
            print("You clicked the button!");
    }
}
