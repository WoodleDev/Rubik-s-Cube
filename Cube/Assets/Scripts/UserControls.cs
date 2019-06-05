using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UserControls : MonoBehaviour {
    CubeSolver cube;
    CubeVisualizer visualizer;

    [SerializeField]
    GameObject cameraGimbal;

    [SerializeField]
    Slider slider;
    
    [SerializeField]
    Text sliderStep;

    [SerializeField]
    Text scramble;

    [SerializeField]
    InputField commandLine;

    //Gets other components
    void Start() {
        Reference reference = GetComponent<Reference>();
        cube = reference.solver;
        visualizer = reference.visualizer;
    }

    void Update() {
        //Camera movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        cameraGimbal.transform.Rotate(new Vector3(vertical, horizontal, -cameraGimbal.transform.eulerAngles.z));
    }

    //Scramble
    public void Scramble(int length = 25) { 
        Color[] sides = new Color[] {Color.white, Color.green, Color.red, Color.blue, Color.Lerp(Color.yellow, Color.red, 0.5F), Color.yellow};
        List<(Color, int)> output = new List<(Color, int)>();
        for (int i = 0; i < length; i++) {
            int index = Mathf.RoundToInt(UnityEngine.Random.Range(0, sides.Length - 1));
            int rotation = Mathf.RoundToInt(UnityEngine.Random.Range(-3, 3));
            while (rotation == 0) {
                rotation = Mathf.RoundToInt(UnityEngine.Random.Range(-3, 3));
            }
            cube.Rotate(sides[index], rotation, ref output);
        }
        scramble.text = cube.RotationsToString(output);
        visualizer.UpdateVisualization();
    }

    //Updates the step based on the slider
    public void UpdateSlider() {
        sliderStep.text = Enum.GetNames(typeof(Step))[(int)slider.value];
    }

    //Runs solve with the step selected by the slider
    public void Solve() {
        cube.Solve((int)slider.value);
    }

    //Runs commands from the command line
    public void RunCommand() {
        string command = commandLine.text;
        switch(command){
            case "Solve":
                cube.Solve();
                visualizer.UpdateVisualization();
                break;

            case "UpdateVisual":
                visualizer.UpdateVisualization();
                break;

            default: 
                cube.RotateFromNotation(command);
                break;
        }
    }
}