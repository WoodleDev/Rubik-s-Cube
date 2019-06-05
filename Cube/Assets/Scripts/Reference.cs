using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reference : MonoBehaviour {
    public CubeSolver solver;
    public CubeVisualizer visualizer;
    public UserControls user;

    void Awake() {
        solver = gameObject.GetComponent<CubeSolver>();
        visualizer = gameObject.GetComponent<CubeVisualizer>();
        user = gameObject.GetComponent<UserControls>();
    }
}
