using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CubeVisualizer : MonoBehaviour {
    [SerializeField]
    float size;
    
    CubeSolver cube;
    [SerializeField]
    Material[] mats;
    List<GameObject> previous;
    void Start() {
        Reference reference = GetComponent<Reference>();
        cube = reference.solver;

        previous = new List<GameObject>();
        UpdateVisualization();
    }
    public void UpdateVisualization() {
        //Clears the previous objects
        if (previous != null) {
            foreach (GameObject previousColorCube in previous) {
                GameObject.Destroy(previousColorCube);
            }
        }
        previous = new List<GameObject>();
        foreach(KeyValuePair<Color, Color[,]> side in cube.orientation) {
            for (int r = 0; r < side.Value.GetLength(0); r++) {
                for (int c = 0; c < side.Value.GetLength(1); c++) {
                    GameObject colorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    colorCube.name = "R:" + r + ", C:" + c;
                    colorCube.transform.position = new Vector3((c-1) * -1.5F, size, (r-1) * 1.5F);
                    foreach (Material mat in mats) {
                        if (ColorUtility.ToHtmlStringRGB(mat.color) == ColorUtility.ToHtmlStringRGB(side.Value[r,c])) {
                            colorCube.GetComponent<Renderer>().material = mat;
                        }
                    }
                    //colorCube.GetComponent<Renderer>().material.color = side.Value[r, c];
                    previous.Add(colorCube);
                    //Puts the cubes in the right position based on their color
                    //Uses if-else statements because switch doesn't work with Color
                    Color sideColor = side.Key;
                    if (sideColor == Color.green) {
                        colorCube.transform.RotateAround(Vector3.zero, Vector3.right, 90);
                        //colorCube.transform.RotateAround(new Vector3(0, 0 , size), Vector3.forward, 180);
                    } else if (sideColor == Color.red) {
                        colorCube.transform.RotateAround(Vector3.zero, Vector3.forward, 90);
                        colorCube.transform.RotateAround(new Vector3(-size, 0, 0), Vector3.left, -90);
                    } else if (sideColor == Color.blue) {
                        colorCube.transform.RotateAround(Vector3.zero, Vector3.right, -90);
                        colorCube.transform.RotateAround(new Vector3(0,0,-size), Vector3.back, 180);
                    } else if (sideColor == Color.Lerp(Color.red, Color.yellow, 0.5F)) {
                        colorCube.transform.RotateAround(Vector3.zero, Vector3.forward, -90F);
                        colorCube.transform.RotateAround(new Vector3(-size, 0, 0), Vector3.left, -90);
                    } else if (sideColor == Color.yellow) {
                        colorCube.transform.RotateAround(Vector3.zero, Vector3.right, 180);
                        colorCube.transform.RotateAround(new Vector3(0, -size, 0), Vector3.down, 180);
                    }
                }
            }
        }
    }
}
