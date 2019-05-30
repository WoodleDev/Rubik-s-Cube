using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserControls : MonoBehaviour {
    [SerializeField]
    GameObject cameraGimbal;

    [SerializeField]
    CubeSolver cube;

    [SerializeField]
    CubeVisualizer visualizer;

    [SerializeField]
    Text scramble;

    [SerializeField]
    InputField commandLine;


    void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        cameraGimbal.transform.Rotate(new Vector3(vertical, horizontal, -cameraGimbal.transform.eulerAngles.z));

        if (Input.GetKeyDown(KeyCode.Return)) {
            commandLine.Select();
        }
    }

    public void Scramble(int length = 25) { 
        Color[] sides = new Color[] {Color.white, Color.green, Color.red, Color.blue, Color.Lerp(Color.yellow, Color.red, 0.5F), Color.yellow};
        List<(Color, int)> output = new List<(Color, int)>();
        for (int i = 0; i < length; i++) {
            int index = Mathf.RoundToInt(Random.Range(0, sides.Length - 1));
            int rotation = Mathf.RoundToInt(Random.Range(-3, 3));
            while (rotation == 0) {
                rotation = Mathf.RoundToInt(Random.Range(-3, 3));
            }
            cube.Rotate(sides[index], rotation, ref output);
        }
        scramble.text = cube.RotationsToString(output);
        visualizer.UpdateVisualization();
    }

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