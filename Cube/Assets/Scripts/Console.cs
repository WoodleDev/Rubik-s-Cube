using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour {
    [SerializeField]
    InputField commandLine;
    [SerializeField]
    Scrollbar scrollbar;
    public Text output;

    string consoleText;

    public void WriteConsole(string text) {
        consoleText = consoleText + Environment.NewLine + text;
        output.text = consoleText;
    }

    void Update() {
        
    }
}
