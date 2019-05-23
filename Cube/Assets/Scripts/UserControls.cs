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
    InputField commandLine;


    void Update() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        cameraGimbal.transform.Rotate(new Vector3(vertical, horizontal, -cameraGimbal.transform.eulerAngles.z));
    }

    public void RunCommand() {
        string command = commandLine.text;
        if (command == "Solve") {
            string rotations = cube.Solve();
        } else {
            cube.RotateFromNotation(command);
        }
    }
}