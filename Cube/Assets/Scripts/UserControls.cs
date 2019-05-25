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
    InputField commandLine;


    void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        cameraGimbal.transform.Rotate(new Vector3(vertical, horizontal, -cameraGimbal.transform.eulerAngles.z));
    }

    public void RunCommand() {
        string command = commandLine.text;
        switch(command){
            case "Solve":
                cube.Solve();
                Debug.Log("Solved");
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