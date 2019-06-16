using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public enum Step {Cross, F2L, OLL, PLL};
public class CubeSolver : MonoBehaviour {
    CubeVisualizer visualizer;
    [SerializeField]
    Text solveOutput;

    public Dictionary<Color, Color[,]> orientation;
    Dictionary<Color, Color[]> sideRelations;
    Dictionary<Color, char> colorToNotation;
    Dictionary<char, Color> notationToColor;
    enum SideRelations {Top, Right, Bottom, Left}
    Color w;
    Color y;
    Color g;
    Color r;
    Color b;
    Color o;
    void OnEnable() {
        //Intializes Variables
        orientation = new Dictionary<Color, Color[,]>();
        sideRelations = new Dictionary<Color, Color[]>();
        colorToNotation = new Dictionary<Color, char>();
        notationToColor = new Dictionary<char, Color>();

        Reference reference = GetComponent<Reference>();
        visualizer = reference.visualizer;

        //Colors
        w = Color.white;
        y = Color.yellow;
        g = Color.green;
        r = Color.red;
        b = Color.blue;
        o = Color.Lerp(r, y, 0.5F);
        //Sets the orientation as an unscrambled cube
        Color[] colors = new Color[] {w, y, g, r, b, o};
        foreach (Color color in colors)  {
            orientation[color] = new Color[,] {{color, color, color}, {color, color, color}, {color, color, color}};
        }

        //Sets up side relations
        sideRelations[w] = new Color[] {b, r, g, o};
        sideRelations[y] = new Color[] {b, o, g, r};
        sideRelations[g] = new Color[] {w, r, y, o};
        sideRelations[r] = new Color[] {w, b, y, g};
        sideRelations[b] = new Color[] {w, o, y, r};
        sideRelations[o] = new Color[] {w, g, y, b};

        //Sets up dictionary for notation
        colorToNotation[w] = 'U';
        colorToNotation[g] = 'F';
        colorToNotation[r] = 'R';
        colorToNotation[b] = 'B';
        colorToNotation[o] = 'L';
        colorToNotation[y] = 'D';

        foreach(KeyValuePair<Color, char> pair in colorToNotation){
            notationToColor[pair.Value] = pair.Key;
        }
    }

    //Returns the positions that correspond with an edge of a face
    (int, int)[] GetEdgeIndexes(int edge) {
        //Returns the positions of the values representing the pieces making up the edge
        //Goes in clockwise order
        switch (edge) { 
            case((int)SideRelations.Top):
                return(new (int,int)[] {(0,0),(0,1),(0,2)});
            case((int)SideRelations.Right):
                return(new (int,int)[] {(0,2), (1,2), (2,2)});
            case((int)SideRelations.Bottom):
                return(new (int,int)[] {(2,2), (2,1), (2,0)});
            case((int)SideRelations.Left):
                return(new (int,int)[] {(2,0), (1,0), (0,0)});
            default:
                throw new ArgumentException("Value isn't in SideRelations enum");
        }
    }

    //Rotates sides
    //With rotation tracking
    public void Rotate(Color side, int turns, ref List<(Color, int)> tracker)  {
        if (turns == 0) {
            return;
        }
        //Creates a deep clone of orientation
        //https://stackoverflow.com/a/139841
        Dictionary<Color, Color[,]> originalOrientation = Extension.CloneDictionaryCloningValues<Color, Color[,]>(orientation);
        //Gets adjacent sides
        Color[] adjSides = sideRelations[side];

        //Rotates the colors on the sides adjacent to the side being turned
        for (int s = 0; s < adjSides.Length; s++) {
            Color adjSide = adjSides[s];
            //Gets the updated positions
            int newIndex = Extension.mod(s + turns, adjSides.Length);
            //Changes the colors
            (int, int)[] colorPositions = GetEdgeIndexes(Array.IndexOf(sideRelations[adjSide], side));
            (int, int)[] newColorPositions = GetEdgeIndexes(Array.IndexOf(sideRelations[adjSides[newIndex]], side));
            for (int c = 0; c < 3; c++){
                (int, int) colorPosition = colorPositions[c];
                (int, int) newColorPosition = newColorPositions[c];
                orientation[adjSides[newIndex]][newColorPosition.Item1, newColorPosition.Item2] = originalOrientation[adjSide][colorPosition.Item1, colorPosition.Item2];
            }
        }            
        //Rotates colors on the side being turned
        Color[] newSideColors = new Color[8];
        for (int c = 0; c < 8; c++) {
            //Gets the position of the color being modified
            (int,int) colorPosition = GetEdgeIndexes(Mathf.FloorToInt(c / 2))[Extension.mod(c, 2)];
            //Gets the new position for that color
            int newIndex = Extension.mod(Mathf.FloorToInt(c/ 2) + turns, 4);
            //Changes the color
             (int,int) newColorPosition = GetEdgeIndexes(newIndex)[Extension.mod(c, 2)];
             newSideColors[c] = orientation[side][newColorPosition.Item1, newColorPosition.Item2];
             orientation[side][newColorPosition.Item1, newColorPosition.Item2] = originalOrientation[side][colorPosition.Item1, colorPosition.Item2];
        }
        //Keeps track of the rotations
        //Adds it to the previous if they're the same side
        if (tracker.Count != 0) {
            (Color, int) lastItem = tracker[tracker.Count - 1];
            if (lastItem.Item1 == side) {
                int combinedTurns = Extension.mod(turns + lastItem.Item2, 4);
                if (combinedTurns != 0) {
                    tracker[tracker.Count - 1] = (side, combinedTurns);
                }
                return;
            }
        }
        //Adds a new tuple
        tracker.Add((side, Extension.mod(turns, 4)));
    }
    //Without
    public void Rotate(Color side, int turns) {
        List<(Color,int)> trash = new List<(Color, int)>();
        Rotate(side, turns, ref trash);
    }
    //From string
    public void RotateFromNotation(string notation) {;
        string[] rotations = notation.Split(' ');
        char[] numbers = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        foreach (string rotation in rotations) {    
            char side = rotation[0];
            int prime = (Convert.ToInt32(rotation.Contains('\u0027')) * -2) + 1;
            string turnsString = "";
            for (int i = 0; i < notation.Length; i++) {
                foreach(char numChar in numbers) {
                    if (notation[i] == numChar) {
                        turnsString += notation[i];
                    }
                }
            }
            Rotate(notationToColor[side], prime * Int32.Parse(turnsString));
        }
        visualizer.UpdateVisualization();
    }

    //Returns the position of where a color can be found on a side
    (int, int)[] SideContains(Color side, Color color) { 
        List<(int,int)> positions = new List<(int, int)>();
        Color[,] sideColors = orientation[side];
        for (int c = 0; c < sideColors.GetLength(0); c++) { 
            for (int r = 0; r < sideColors.GetLength(1); r++) { 
                if (sideColors[c,r] == color) { 
                    positions.Add((c,r));
                }    
            }
        }
        return (positions.ToArray());
    }

    //Gets the edges that a color is on from its position
    int[] PositionToEdges((int,int) pos) {
        List<int> edges = new List<int>();
        //Checks for vertical edges
        if (pos.Item1 == 0) {
            edges.Add((int)SideRelations.Top);
        } else if (pos.Item1 == 2) {
            edges.Add((int)SideRelations.Bottom);
        }
        //Checks for horizontal edges
        if (pos.Item2 == 0) {
            edges.Add((int)SideRelations.Left);
        } else if (pos.Item2 == 2) {
            edges.Add((int)SideRelations.Right);
        }
        if (edges.Count == 0) {
            throw new ArgumentException("Piece is in the middle");
        }
        return(edges.ToArray());
    }
    
    //Returns the position of a piece on a side given a known position that the piece takes up on another side
    (int,int) GetOtherPiecePosition(Color side1, Color side2, (int,int) pos) {
        //Checks if the new side is valid
        if (!sideRelations[side1].Contains(side2)) {
            throw new ArgumentException("Piece cannot take up a position on an opposite side");
        }
        int s1e = Array.IndexOf(sideRelations[side1], side2);
        int s2e = Array.IndexOf(sideRelations[side2], side1);
        if (!PositionToEdges(pos).Contains(s1e)) {
            throw new ArgumentException("Position isn't adjacent to side 2");
        }
        (int,int) s2pos = GetEdgeIndexes(s2e)[2 - Array.IndexOf(GetEdgeIndexes(s1e), pos)];
        return s2pos;
    }
    
    //Gets colors on a piece
    Color[] GetPieceColors(Color side, (int,int) pos) {
        int[] adjEdges = PositionToEdges(pos);

        List<Color> colors = new List<Color>();
        colors.Add(orientation[side][pos.Item1, pos.Item2]);
        foreach (int adjEdge in adjEdges) {
            Color adjSide = sideRelations[side][adjEdge];
            (int,int) adjPos = GetOtherPiecePosition(side, adjSide, pos);
            colors.Add(orientation[adjSide][adjPos.Item1, adjPos.Item2]);
        }
        return(colors.ToArray());
    }

    //Converts a List<(Color,int)> into a string of notations
    public string RotationsToString(List<(Color, int)> rotations) {
        string output = "";
        foreach ((Color, int) rotation in rotations) {
            Color color = rotation.Item1;
            int turns = rotation.Item2;
            output += colorToNotation[color];
            //3 turns one way is 1 turn the other
            if (Mathf.Abs(turns) == 3) {
                turns = Mathf.RoundToInt(-Mathf.Sign(turns));
            }
            //Prime
            if (turns < 0) {
                output += "'";
            }
            output += Mathf.Abs(turns);
            output += " ";
        }
        return output;
    }

    public void Solve(int step = (int)Step.PLL) {
        List<(Color, int)> rotations = new List<(Color,int)>();
        void End() {
            solveOutput.text = RotationsToString(rotations);
            visualizer.UpdateVisualization();
            return;
        }
        //White cross
        //Gets white pieces
        Color[] wAdjSides = sideRelations[w];

        int positionCount = 0;

        void GetPositionCount() {
           //Counts the number of white edges
           positionCount = 0;
            for (int s = 0; s < 4; s++) {
                for (int p = 0; p < 4; p++) {
                    (int,int) edgePos = GetEdgeIndexes(p)[1];
                    if (orientation[wAdjSides[s]][edgePos.Item1, edgePos.Item2] == w) {
                        positionCount++;
                    }
                }
            }
        }
        GetPositionCount();
        while (positionCount != 0) {
            foreach (Color wAdjSide in wAdjSides) {
                for (int e = 0; e < 4; e++) {
                    //Moves piece to right
                    (int,int) position = GetEdgeIndexes(e)[1];
                    if (orientation[wAdjSide][position.Item1, position.Item2] != w) {
                        continue;
                    }
                    Rotate(wAdjSide, -e + (int)SideRelations.Right, ref rotations);

                    //Moves empty white cross slot over the right side
                    Color right = sideRelations[wAdjSide][(int)SideRelations.Right];
                    (int, int) targetWPosition = GetOtherPiecePosition(right, w, GetEdgeIndexes((int)SideRelations.Top)[1]);
                    while (orientation[w][targetWPosition.Item1, targetWPosition.Item2] == w) { 
                        Rotate(w, 1, ref rotations);
                    }
                    //Rotates right to move piece in
                    Rotate(right, 1, ref rotations);
                }
            }

            //Recounts the number of edges to see if another pass is necessary
            GetPositionCount();
        }
        
        //Finds white positions on yellow
        (int,int)[] yWPositions = SideContains(y, w);
        foreach((int,int) yWPosition in yWPositions) {
            int[] edges = PositionToEdges(yWPosition);
            if (edges.Length == 1) {
                int edge = edges[0];
                (int,int) topMiddle = GetEdgeIndexes((int)SideRelations.Top)[1];
                (int,int) targetWPos = GetOtherPiecePosition(sideRelations[y][edge], w, topMiddle);
                while (orientation[w][targetWPos.Item1, targetWPos.Item2] == w) {
                    Rotate(w, 1, ref rotations);
                }
                Rotate(sideRelations[y][edge], 2, ref rotations);
            }
        }
        //Fixes the orientation of the adj sides
        int passes = 2;
        while (passes != 0) {
            foreach (Color wAdjSide in wAdjSides) {
                (int, int) topMiddle = GetEdgeIndexes((int)SideRelations.Top)[1];
                if (orientation[wAdjSide][topMiddle.Item1, topMiddle.Item2] != wAdjSide) {
                    Color leftSide = sideRelations[wAdjSide][(int)SideRelations.Left];
                    Color rightSide = sideRelations[wAdjSide][(int)SideRelations.Right];
                    Color oppositeSide = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], wAdjSide) + 2, 4)];
                    if (orientation[leftSide][topMiddle.Item1, topMiddle.Item2] == wAdjSide || orientation[wAdjSide][topMiddle.Item1, topMiddle.Item2] == leftSide) {
                        //Left side
                        Rotate(wAdjSide, -1, ref rotations);
                        Rotate(w, -1, ref rotations);
                        Rotate(wAdjSide, 1, ref rotations);
                        Rotate(w, 1, ref rotations);
                        Rotate(wAdjSide, -1, ref rotations);
                    } else if (orientation[rightSide][topMiddle.Item1, topMiddle.Item2] == wAdjSide || orientation[wAdjSide][topMiddle.Item1, topMiddle.Item2] == rightSide) {
                        //Right side
                        Rotate(wAdjSide, 1, ref rotations);
                        Rotate(w, 1, ref rotations);
                        Rotate(wAdjSide, -1, ref rotations);
                        Rotate(w, -1, ref rotations);
                        Rotate(wAdjSide, 1, ref rotations);
                    } else if (orientation[oppositeSide][topMiddle.Item1, topMiddle.Item2] == wAdjSide || orientation[wAdjSide][topMiddle.Item1, topMiddle.Item2] == oppositeSide) {
                        //Opposite
                        Rotate(wAdjSide, 2, ref rotations);
                        Rotate(oppositeSide, 2, ref rotations);
                        Rotate(y, 2, ref rotations);
                        Rotate(wAdjSide, -2, ref rotations);
                        Rotate(oppositeSide, -2, ref rotations);
                    }
                }
            }
        passes--;
        }
        if (step == (int)Step.Cross) {
            End();
            return;
        }
        //F2L
        //Finds empty pair slots 
        List<(Color,Color)> emptyPairs = new List<(Color, Color)>();
        for (int c = 0; c < 4; c++) {
            (int,int) cornerPos = GetEdgeIndexes(c)[0];
            int[] edges = PositionToEdges(cornerPos);
            Color c1 = sideRelations[y][edges[0]];
            Color c2 = sideRelations[y][edges[1]];
            int otherEdge = Array.IndexOf(sideRelations[c1], c2);
            (int,int)[] positions = GetEdgeIndexes(otherEdge);
            foreach ((int,int) position in positions) {
                if (position == GetOtherPiecePosition(y, c1, cornerPos)) {
                    continue;
                }
                if (!(orientation[c1][position.Item1, position.Item2] == c1 && GetPieceColors(c1, position).Contains(c2))) {
                    emptyPairs.Add((c1,c2));
                    break;
                }
            }
        }
        //Creates pairs
        for (int pi = 0; pi < emptyPairs.Count; pi++) {
            (Color, Color) emptyPair = emptyPairs[pi];
            //Finds the edge and corner pieces
            (Color, (int,int))? cornerLoc = null;
            (Color, (int,int))? edgeLoc = null;
            foreach (Color side in new Color[] {g, r, b, o}) {
                List<(int,int)> positions = GetEdgeIndexes((int)SideRelations.Right).ToList();
                positions.AddRange(new (int, int)[] {GetEdgeIndexes((int)SideRelations.Top)[1], GetEdgeIndexes((int)SideRelations.Bottom)[1]});
                for (int p = 0; p < positions.Count; p++) {
                    (int,int) position = positions[p];
                    Color[] colors = GetPieceColors(side, position);
                    if (colors.Contains(emptyPair.Item1) && colors.Contains(emptyPair.Item2)) {
                        if (colors.Contains(w)) {
                            cornerLoc = (side, position);
                        } else if (colors.Length == 2) {
                            edgeLoc = (side, position);
                        }
                    }
                }
            }


            //Moves any remaining piece needed to make the pair onto the yellow side
            (int,int)[] yellowSidePositions = GetEdgeIndexes((int)SideRelations.Bottom);
            bool yHasCorner = yellowSidePositions.Contains(cornerLoc.Value.Item2);
            bool yHasEdge = yellowSidePositions.Contains(edgeLoc.Value.Item2);
            (int,int) edgePos = (-1, -1);
            (int,int) cornerPos = (-1, -1);
            if (!yHasCorner || !yHasEdge) {

                //Rotates corner in
                Color cornerSide = cornerLoc.Value.Item1;
                int cornerEdge = Array.IndexOf(sideRelations[y], cornerSide);
                if (!yHasCorner) {
                    if (yHasEdge) {
                        edgePos = GetOtherPiecePosition(edgeLoc.Value.Item1, y, edgeLoc.Value.Item2);
                        while (PositionToEdges(edgePos)[0] != Extension.mod(Array.IndexOf(sideRelations[y], cornerSide) + 2, 4)) {
                            Rotate(y, 1, ref rotations);
                            edgePos = GetEdgeIndexes(Extension.mod(PositionToEdges(edgePos)[0] + 1, 4))[1];
                        }
                    }
                    Rotate(cornerSide, 1, ref rotations);
                    Rotate(y, 1, ref rotations);
                    Rotate(cornerSide, -1, ref rotations);

                    cornerEdge = Extension.mod(cornerEdge + 1, 4);
                    cornerSide = sideRelations[y][cornerEdge];
                    cornerPos = GetEdgeIndexes(cornerEdge)[0];
                }
                
                //Rotates edge in
                if (!yHasEdge) {
                    Color edgeSide = edgeLoc.Value.Item1;
                    //cornerPos = GetEdgeIndexes(Array.IndexOf(sideRelations[y], cornerSide))[2];   
                    (int,int) targetCornerPos = GetEdgeIndexes(Extension.mod(Array.IndexOf(sideRelations[y], edgeSide) - 2, 4))[0];
                    while (cornerPos != targetCornerPos) {
                        Rotate(y, 1, ref rotations);
                        cornerEdge = Extension.mod(cornerEdge + 1, 4);
                        cornerPos = GetEdgeIndexes(cornerEdge)[0];
                    }
                    Rotate(edgeSide, 1, ref rotations);
                    Rotate(y, 1, ref rotations);
                    Rotate(edgeSide, -1, ref rotations);
                    edgePos = GetEdgeIndexes(Extension.mod(Array.IndexOf(sideRelations[y], edgeSide) + 1, 4))[1];
                    cornerEdge = Extension.mod(cornerEdge + 1, 4);
                    cornerPos = GetEdgeIndexes(cornerEdge)[0];
                }

                //DEBUG NOTE
                //EDGE POS IS UNDEFINED
            }
            //Gets position - temporary
            for (int e = 0; e < 4; e++) {
                (int,int)[] positions = GetEdgeIndexes(e);
                Color[] edge = GetPieceColors(y, positions[1]);
                Color[] corner = GetPieceColors(y, positions[0]);
                if (edge.Contains(emptyPair.Item1) && edge.Contains(emptyPair.Item2)) {
                    edgePos = positions[1];
                }
                if (corner.Contains(emptyPair.Item1) && corner.Contains(emptyPair.Item2) && corner.Contains(w)) {
                    cornerPos = positions[0];
                }
            }

            //Solves the pair
            int[] cornerEdges;
            //Separates the corner
            if (PositionToEdges(cornerPos).Intersect(PositionToEdges(edgePos)).Count() == 1) {
                //Finds side where there is only the corner and an empty pair is on the opposite side of the corner
                cornerEdges = PositionToEdges(cornerPos);
                int edgeEdge = Array.IndexOf(cornerEdges, PositionToEdges(edgePos)[0]);
                int cornerOnlyEdge = cornerEdges[1 - edgeEdge];
                int relativeEdge = new int();
                if (Extension.mod(cornerOnlyEdge - 1, 4) == edgeEdge) {
                    relativeEdge = -1;
                } else {
                    relativeEdge = 1;
                }
                Color cornerOnlySide = sideRelations[y][cornerOnlyEdge];
                bool empty = false;
                while (!empty) {
                    Rotate(y, 1, ref rotations);
                    cornerOnlyEdge = Extension.mod(cornerOnlyEdge + 1, 4);
                    cornerOnlySide = sideRelations[y][cornerOnlyEdge];
                    cornerPos = GetEdgeIndexes(cornerOnlyEdge)[0];

                    edgeEdge = Extension.mod(edgeEdge + 1, 4);
                    edgePos = GetEdgeIndexes(edgeEdge)[1];
                    //Finds empty pair that hasn't been solved yet
                    for (int ep = 0; ep < emptyPairs.Count; ep ++) {
                        if (ep < pi) {
                            continue;
                        }
                        int[] oppositeEdges = PositionToEdges(GetEdgeIndexes(cornerOnlyEdge)[2]);
                        Color[] oppositeColors = new Color[] {sideRelations[y][oppositeEdges[0]], sideRelations[y][oppositeEdges[1]]};
                        for (int i = pi; i < emptyPairs.Count; i++) {
                            Color[] colors = new Color[] {emptyPairs[i].Item1, emptyPairs[i].Item2};
                            if (oppositeColors.Intersect(colors).Count() == 2) { 
                                empty = true;    
                            }
                        }
                    }   
                }

                //Rotates the corner out of the way
                //DEBUG NOTE CORNERONLYEDGE IS WRONG
                cornerOnlySide = sideRelations[y][cornerOnlyEdge];
                int cornerOnlySideRot =  1 - GetOtherPiecePosition(y, sideRelations[y][cornerOnlyEdge], cornerPos).Item2;
                Rotate(cornerOnlySide,  cornerOnlySideRot, ref rotations);

                //Rotates edge to right spot
                Rotate(y, -2 - Extension.mod(cornerOnlySideRot, -1), ref rotations);
                Rotate(cornerOnlySide, -cornerOnlySideRot, ref rotations);

                edgeEdge = Extension.mod(edgeEdge -2, 4);
                edgePos = GetEdgeIndexes(edgeEdge)[1];

            }

            //Moves corner above empty pair
            //Gets corner position
            cornerEdges = new int[2];

            //Gets positions
            for (int e = 0; e < 4; e++) {
                (int,int)[] positions = GetEdgeIndexes(e);
                Color[] edge = GetPieceColors(y, positions[1]);
                Color[] corner = GetPieceColors(y, positions[0]);
                if (edge.Contains(emptyPair.Item1) && edge.Contains(emptyPair.Item2)) {
                    edgePos = positions[1];
                }
                if (corner.Contains(emptyPair.Item1) && corner.Contains(emptyPair.Item2) && corner.Contains(w)) {
                    cornerPos = positions[0];
                    cornerEdges = PositionToEdges(cornerPos);
                }
            }

            //Rotates yellow side until above the right empty pair
            int c1Index = Array.IndexOf(sideRelations[y], emptyPair.Item1);
            int c2Index = Array.IndexOf(sideRelations[y], emptyPair.Item2);
            while (!(cornerEdges.Contains(c1Index) && cornerEdges.Contains(c2Index))) {
                Rotate(y, 1, ref rotations);
                for (int i = 0; i < cornerEdges.Length; i++) {
                    cornerEdges[i] = Extension.mod(cornerEdges[i] + 1, 4);
                }
            }

            //Rotates edge to the right place
            //[ ][e][ ]
            //[ ][ ][ ] R
            //[ ][ ][c]
            //    F
            //gets the corner of the side with the edge and checks if that corner has the same edges as the corner
            Color right = emptyPair.Item1;
            if (Array.IndexOf(sideRelations[emptyPair.Item1], emptyPair.Item2) == (int)SideRelations.Left){
                right = emptyPair.Item2;
            }
            if (PositionToEdges(cornerPos).Intersect(PositionToEdges(GetEdgeIndexes(PositionToEdges(edgePos)[0])[2])).Count() == 0) { 
                Rotate(y, 1, ref rotations);
                Rotate(right, 1, ref rotations);
                Rotate(y, -1, ref rotations);
                Rotate(right, -1, ref rotations);
                Rotate(y, 1, ref rotations);
                Rotate(right, 1, ref rotations);
                Rotate(y, -2, ref rotations);
                Rotate(right, -1, ref rotations);
                Rotate(y, 2, ref rotations);
            }
            Color cornerTop = new Color();
            Color edgeTop = new Color();
            //Gets colors
            for (int e = 0; e < 4; e++) {
                (int,int)[] positions = GetEdgeIndexes(e);
                Color[] edge = GetPieceColors(y, positions[1]);
                Color[] corner = GetPieceColors(y, positions[0]);
                if (edge.Contains(emptyPair.Item1) && edge.Contains(emptyPair.Item2)) {
                    edgePos = positions[1];
                    edgeTop = orientation[y][positions[1].Item1, positions[1].Item2];
                }
                if (corner.Contains(emptyPair.Item1) && corner.Contains(emptyPair.Item2) && corner.Contains(w)) {
                    cornerPos = positions[0];
                    cornerTop = orientation[y][positions[0].Item1, positions[0].Item2];
                }
            }
            Color up = y;
            Color front = sideRelations[y][Extension.mod(PositionToEdges(edgePos)[0] + 2, 4)];
            right = sideRelations[y][Extension.mod(PositionToEdges(edgePos)[0] + 1, 4)];

            //White on top
            //F' U F R U' R' U'
            if (cornerTop == w) {
                if (edgeTop == right) {
                    //R U' R' U R U' R' U2 F' U2 F U'2 F' U F
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(front, -1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(front, 1, ref rotations);
                    Rotate(up, -2, ref rotations);
                    Rotate(front, -1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(front, 1, ref rotations);
                } else {
                    //F' U F U2 F U2 F' U R U' R'
                    //F' U F U R U2 R' U2 R U' R'
                    Rotate(front, -1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(front, 1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                }
            } else if (cornerTop == edgeTop) {
                if (cornerTop == right) {
                    //Same color
                    //Right
                    //U F' U2 F U'2 F' U F
                    Rotate(up, 1, ref rotations);
                    Rotate(front, -1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(front, 1, ref rotations);
                    Rotate(up, -2, ref rotations);
                    Rotate(front, -1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(front, 1, ref rotations);
                } else {
                    //U' R U R' U2 R U' R'
                    Rotate(up, -1, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                }
                
            } else if (cornerTop != edgeTop) {
                //Different color
                (int,int) rightPos = GetOtherPiecePosition(y, right, cornerPos);
                if (orientation[right][rightPos.Item1, rightPos.Item2] == w) {
                    //W on right
                    //R U R'    
                    Rotate(right, 1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                } else {
                    //W on front
                    //F' U F U' F' U' F U2' F' U F
                    Rotate(front, -1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(front, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(front, -1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(front, 1, ref rotations);
                    Rotate(up, -2, ref rotations);
                    Rotate(front, -1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(front, 1, ref rotations);
                }              
            }
        }
        if (step == (int)Step.F2L) {
            End();
            return;
        }
        //Yellow cross
        List<(int,int)> yCrossEmptyPositions = new List<(int, int)>();
        for (int e = 0; e < 4; e++) {
            (int,int) position = GetEdgeIndexes(e)[1];
            if (orientation[y][position.Item1, position.Item2] != y) {
                yCrossEmptyPositions.Add(position);
            }
        }
        //Algorithm
        void FRURUF(Color front, Color up, Color right) {
            //F (R U R' U') F'
            Rotate(front, 1, ref rotations);
            Rotate(right, 1, ref rotations);
            Rotate(up, 1, ref rotations);
            Rotate(right, -1, ref rotations);
            Rotate(up, -1, ref rotations);
            Rotate(front, -1, ref rotations);
        }

        if (yCrossEmptyPositions.Count != 0) {
            //No edges
            if (yCrossEmptyPositions.Count == 4) {
                FRURUF(g, y, o);
            }
            //L and Bar
            for (int e = 0; e < 4; e++) {
                (int,int) pos1 = GetEdgeIndexes(e)[1];
                (int,int) pos2 = GetEdgeIndexes(Extension.mod(e + 1, 4))[1];
                (int,int) pos3 = GetEdgeIndexes(Extension.mod(e + 2, 4))[1];
                if (orientation[y][pos1.Item1, pos1.Item2] == y && orientation[y][pos2.Item1, pos2.Item2] == y) {
                    //L
                    FRURUF(sideRelations[y][PositionToEdges(pos2)[0]], y, sideRelations[y][PositionToEdges(pos1)[0]]);
                    Rotate(y, 1, ref rotations);
                }
                if (orientation[y][pos1.Item1, pos1.Item2] == y && orientation[y][pos3.Item1, pos3.Item2] == y) {
                    //Bar
                    FRURUF(sideRelations[y][PositionToEdges(pos2)[0]], y, sideRelations[y][PositionToEdges(pos1)[0]]);
                    break;
                }
                
            }
        }

        //Orientation
        List<int> yLessCorners = new List<int>();
        for(int e = 0; e < 4; e++) {
            (int,int) cornerPos = GetEdgeIndexes(e)[0];
            if (orientation[y][cornerPos.Item1, cornerPos.Item2] != y) {
                yLessCorners.Add(e);
            }
        }
        //2 Corners
        if (yLessCorners.Count == 2) {
            int corner1 = yLessCorners[0];
            int corner2 = yLessCorners[1];

            (int,int) corner1Pos = GetEdgeIndexes(corner1)[0];
            Color corner1Side = sideRelations[y][corner1];

            (int,int) corner2Pos = GetEdgeIndexes(corner2)[0];
            Color corner2Side = sideRelations[y][corner2];

            //Diagonal
            //[y][y][ ]y 
            //[y][y][y]
            //[ ][y][y]
            // y
            if (Extension.mod(corner1 + 2, 4) == corner2) {
                //Gets the sides
                int leftCornerEdge = Extension.mod(corner1 - 1, 4);
                (int,int) pos = GetEdgeIndexes(leftCornerEdge)[0];
                int[] edges = PositionToEdges(pos);
                for (int e = 0; e < 2; e++) {
                    Color otherSide = sideRelations[y][edges[0]];
                    (int,int) otherPos = GetOtherPiecePosition(y, otherSide, pos);
                    if (orientation[otherSide][otherPos.Item1, otherPos.Item2] == y && PositionToEdges(otherPos).Contains((int)SideRelations.Left)) {
                        leftCornerEdge = Extension.mod(corner2 - 1, 4);
                    }
                }
                Color front = sideRelations[y][leftCornerEdge];
                Color right = sideRelations[y][Extension.mod(leftCornerEdge - 1, 4)];
                Color back = sideRelations[y][Extension.mod(leftCornerEdge - 2, 4)];

                //(R' F) (R B') (R' F') (R B)
                Rotate(right, -1, ref rotations);
                Rotate(front, 1, ref rotations);
                Rotate(right, 1, ref rotations);
                Rotate(back, -1, ref rotations);
                Rotate(right, -1, ref rotations);
                Rotate(front, -1, ref rotations);
                Rotate(right, 1, ref rotations);
                Rotate(back, 1, ref rotations);            
            }
            //Adjacent
            if (Extension.mod(corner1 + 1, 4) == corner2 || Extension.mod(corner2 + 1, 4) == corner1) {

                Color commonSide = new Color();
                for (int e = 0; e < 4; e++) { 
                    (int,int)[] positions = GetEdgeIndexes(e);
                    if (positions.Contains(corner1Pos) && positions.Contains(corner2Pos)) {
                        commonSide = sideRelations[y][e];
                    }
                }
                (int,int) corner1CommonPos = GetOtherPiecePosition(y, commonSide, corner1Pos);
                Color corner1CommonColor = orientation[commonSide][corner1CommonPos.Item1, corner1CommonPos.Item2];

                (int,int) corner2CommonPos = GetOtherPiecePosition(y, commonSide, corner2Pos);
                Color corner2CommonColor = orientation[commonSide][corner2CommonPos.Item1, corner2CommonPos.Item2];
                if (corner1CommonColor == y && corner1CommonColor == y) {
                    //Facing same side
                    //[y][y][y]
                    //[y][y][y]
                    //[ ][y][ ]
                    // y     y
                    //(R2' D) (R' U2) (R D') (R' U2 R')
                    Color right = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], commonSide) - 1, 4)];
                    Color up = y;
                    Color down = w;
                    Rotate(right, -2, ref rotations);
                    Rotate(down, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(down, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(up, 2, ref rotations);
                    Rotate(right, -1, ref rotations);
                } else {
                    //Facing opposite sides
                    //       y
                    //[y][y][ ]
                    //[y][y][y]
                    //[y][y][ ]
                    //       y
                    //(Lw' U') (L U) (R U') (Rw' F)
                    Color right = commonSide;
                    Color left = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], commonSide) + 2, 4)];
                    Color up = y;
                    
                    Rotate(right, -1, ref rotations);
                    up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) - 1, 4)];
                    Rotate(up, -1, ref rotations);
                    Rotate(left, 1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(left, -1, ref rotations);
                    up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) - 1, 4)];
                    Color forward = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) + 1, 4)];
                    Rotate(forward, 1, ref rotations);
                    
                }
            }
        }
        
        //3 Corners
        if (yLessCorners.Count == 3) {
            int middleCorner = new int();
            foreach (int corner in yLessCorners) {
                if (!yLessCorners.Contains(Extension.mod(corner + 2, 4))) {
                    middleCorner = corner;
                }
            }
            int rightCorner = Extension.mod(middleCorner + 1, 4);
            int[] rightEdges = PositionToEdges(GetEdgeIndexes(rightCorner)[0]);
            int leftCorner = Extension.mod(middleCorner - 1, 4);
            int[] leftEdges = PositionToEdges(GetEdgeIndexes(leftCorner)[0]);

            foreach (int edge in PositionToEdges(GetEdgeIndexes(middleCorner)[0])) {
                (int,int) position = GetEdgeIndexes(middleCorner)[0];
                (int,int) otherPosition = GetOtherPiecePosition(y, sideRelations[y][edge], position);
                if (orientation[sideRelations[y][edge]][otherPosition.Item1, otherPosition.Item2] == y) {
                    Color up = y;
                    if (rightEdges.Contains(edge)) {
                        //Sune
                        //(R U R' U) (R U2 R')
                        // y
                        //[ ][y][ ]y
                        //[y][y][y]
                        //[y][y][ ]
                        //       y
                        Color right = sideRelations[y][edge];
                        Rotate(right, 1, ref rotations);
                        Rotate(up, 1, ref rotations);
                        Rotate(right, -1, ref rotations);
                        Rotate(up, 1, ref rotations);
                        Rotate(right, 1, ref rotations);
                        Rotate(up, 2, ref rotations);
                        Rotate(right, -1, ref rotations);
                        break;
                    } else if (leftEdges.Contains(edge)) {
                        //Antisune
                        //(R U2) (R' U' R U' R')
                        //y[ ][y][y]
                        // [y][y][y]
                        // [ ][y][ ]y
                        //  y
                        Color right = sideRelations[y][Extension.mod(edge - 1, 4)];
                        Rotate(right, 1, ref rotations);
                        Rotate(up, 2, ref rotations);
                        Rotate(right, -1, ref rotations);
                        Rotate(up, -1, ref rotations);
                        Rotate(right, 1, ref rotations);
                        Rotate(up, -1, ref rotations);
                        Rotate(right, -1, ref rotations);
                        break;
                    }
                }
            }
        }
        //4 Corners
        if (yLessCorners.Count == 4) {
            List<int> commonSides = new List<int>();
            int GetYellowSide(int edge) {
                (int,int) pos = GetEdgeIndexes(edge)[0];
                int[] edges = PositionToEdges(pos);
                int yEdge = new int();
                foreach (int e in edges) {
                    (int,int) otherPos = GetOtherPiecePosition(y, sideRelations[y][e], pos);
                    if (orientation[sideRelations[y][e]][otherPos.Item1, otherPos.Item2] == y) {
                        return e;
                    }
                }
                return -1;
            }

            for (int c = 0; c < 4; c++) {
                if (GetYellowSide(c) == GetYellowSide(Extension.mod(c + 1, 4))) {
                    commonSides.Add(c);
                }
            }
            if (commonSides.Count == 2) {
                //Symmetric
                //(R U2) (R' U' R U R' U' R U' R')
                Color right = sideRelations[y][Extension.mod(commonSides[0] - 1, 4)];
                Color up = y;
                Rotate(right, 1, ref rotations);
                Rotate(up, 2, ref rotations);
                Rotate(right, -1, ref rotations);
                Rotate(up, -1, ref rotations);
                Rotate(right, 1, ref rotations);
                Rotate(up, 1, ref rotations);
                Rotate(right, -1, ref rotations);
                Rotate(up, -1, ref rotations);
                Rotate(right, 1, ref rotations);
                Rotate(up, -1, ref rotations);
                Rotate(right, -1, ref rotations);
            } else if (commonSides.Count == 1) {
                //Not symmetric
                //(R U2') (R2' U') (R2 U') (R2' U2' R)
                Color right = sideRelations[y][Extension.mod(commonSides[0] + 2, 4)];
                Color up = y;
                Rotate(right, 1, ref rotations);
                Rotate(up, -2, ref rotations);
                Rotate(right, -2, ref rotations);
                Rotate(up, -1, ref rotations);
                Rotate(right, 2, ref rotations);
                Rotate(up, -1, ref rotations);
                Rotate(right, -2, ref rotations);
                Rotate(up, -2, ref rotations);
                Rotate(right, 1, ref rotations);
            }
        }
        if (step == (int)Step.OLL) {
            End();
            return;
        }
        //PLL
        //Edges
        (int,int) ePosition = GetEdgeIndexes((int)SideRelations.Bottom)[1];
        List<int> unorientedEdges = new List<int>();
        void GetUnorientedEdges() {
            unorientedEdges = new List<int>();
            for(int e = 0; e < 4; e++) {
                Color c = orientation[sideRelations[y][e]][ePosition.Item1, ePosition.Item2];
                if (c != sideRelations[y][e]) {
                    unorientedEdges.Add(e);
                }
            }
        }
        GetUnorientedEdges();
        while (unorientedEdges.Count != 0) {     
            if (unorientedEdges.Count == 3) {
                for (int e = 0; e < 4; e++) {
                    //Makes sure that e is the most clockwise edge
                    if (unorientedEdges.Contains(Extension.mod(e + 1, 4))) {
                        continue;
                    } 
                    int e2 = Extension.mod(e - 1, 4);
                    int e3 = Extension.mod(e - 2, 4);

                    Color nc1 = sideRelations[y][e];
                    Color nc2 = sideRelations[y][e2];
                    Color nc3 = sideRelations[y][e3];

                    Color c1 = orientation[nc1][ePosition.Item1, ePosition.Item2];
                    Color c2 = orientation[nc2][ePosition.Item1, ePosition.Item2];
                    Color c3 = orientation[nc3][ePosition.Item1, ePosition.Item2];

                    Color right = nc3;
                    Color up = y;
                    if (c1 == nc2 && c2 == nc3 && c3 == nc1) {
                        //3 edges
                        //(R U' R U) (R U) (R U') (R' U' R2)
                        Rotate(right, 1, ref rotations);
                        Rotate(up, -1, ref rotations);
                        Rotate(right, 1, ref rotations);
                        Rotate(up, 1, ref rotations);
                        Rotate(right, 1, ref rotations);
                        Rotate(up, 1, ref rotations);
                        Rotate(right, 1, ref rotations);
                        Rotate(up, -1, ref rotations);
                        Rotate(right, -1, ref rotations);
                        Rotate(up, -1, ref rotations);
                        Rotate(right, 2, ref rotations);
                    } else if (c1 == nc3 && c2 == nc1 && c3 == nc2) {
                        //Inverse 3 edges
                        //(R2 U) (R U R' U') (R' U') (R' U R')
                        Rotate(right, 2, ref rotations);
                        Rotate(up, 1, ref rotations);
                        Rotate(right, 1, ref rotations);
                        Rotate(up, 1, ref rotations);
                        Rotate(right, -1, ref rotations);
                        Rotate(up, -1, ref rotations);
                        Rotate(right, -1, ref rotations);
                        Rotate(up, -1, ref rotations);
                        Rotate(right, -1, ref rotations);
                        Rotate(up, 1, ref rotations);
                        Rotate(right, -1, ref rotations);
                    }
                break;
                }
            }
        
            if (unorientedEdges.Count == 4) {
                Color esc1 = sideRelations[y][unorientedEdges[0]];
                Color esc2 = sideRelations[y][unorientedEdges[1]];
                Color esc3 = sideRelations[y][unorientedEdges[2]];
                Color esc4 = sideRelations[y][unorientedEdges[3]];

                Color ec1 = orientation[esc1][ePosition.Item1, ePosition.Item2];
                Color ec2 = orientation[esc2][ePosition.Item1, ePosition.Item2];
                Color ec3 = orientation[esc3][ePosition.Item1, ePosition.Item2];
                Color ec4 = orientation[esc4][ePosition.Item1, ePosition.Item2];
            
                Color left = r;
                Color right = o;
                Color up = y;
                if (ec1 == esc3 && ec3 == esc1 && ec2 == esc4 && ec4 == esc2) {
                    //Across
                    //[ ][↑][ ]
                    //[←][+][→]
                    //[ ][↓][ ]
                    //(M2' U) (M2' U2) (M2' U) M2'
                    Rotate(left, 2, ref rotations);
                    Rotate(right, -2, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 2, 4)];
                    Rotate(up, 1, ref rotations);
                    Rotate(left, 2, ref rotations);
                    Rotate(right, -2, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 2, 4)];
                    Rotate(up, 2, ref rotations);
                    Rotate(left, 2, ref rotations);
                    Rotate(right, -2, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 2, 4)];
                    Rotate(up, 1, ref rotations);
                    Rotate(left, 2, ref rotations);
                    Rotate(right, -2, ref rotations);
                } else if (ec1 == esc4 && ec4 == esc1 && ec2 == esc3 && ec3 == esc2) {
                    //Diagonal
                    //[ ][↗][ ]
                    //[↙][ ][↗]
                    //[ ][↙][ ]
                    //(M2' U) (M2' U) (M' U2) (M2' U2) (M' U2)
                    //DEBUGOO NOTES
                    //FIX ALGORITHM
                    Rotate(left, 2, ref rotations);
                    Rotate(right, -2, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 2, 4)];
                    Rotate(up, 1, ref rotations);
                    Rotate(left, 2, ref rotations);
                    Rotate(right, -2, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 2, 4)];
                    Rotate(up, 1, ref rotations);
                    Rotate(left, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 1, 4)];
                    Rotate(up, 2, ref rotations);
                    Rotate(left, 2, ref rotations);
                    Rotate(right, -2, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 2, 4)];
                    Rotate(up, 2, ref rotations);
                    Rotate(left, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    up = sideRelations[left][Extension.mod(Array.IndexOf(sideRelations[left], up) + 1, 4)];
                    Rotate(up, 2, ref rotations);
                }
            }
            GetUnorientedEdges();
            if (unorientedEdges.Count != 0) {
                Rotate(y, 1, ref rotations);           
                GetUnorientedEdges();
            }
        }

        //Corners
        (int,int) cPosition = GetEdgeIndexes((int)SideRelations.Bottom)[0];
        List<int> unorientedCorners = new List<int>();
        for (int c = 0; c < 4; c++) {
            (int,int) pos = GetEdgeIndexes(c)[0];
            int[] edges = PositionToEdges(GetEdgeIndexes(c)[0]);
            Color c1 = sideRelations[y][edges[0]];
            Color c2 = sideRelations[y][edges[1]];
            Color[] colors = GetPieceColors(y, pos);
            if (!(colors.Contains(c1) && colors.Contains(c2))) {
                unorientedCorners.Add(c);
            }
        }
        if (unorientedCorners.Count != 0) {
            //fe is the least clockwise corner if such exists
            int fe = unorientedCorners[0];
            if (unorientedCorners.Count != 4) {
                while (unorientedCorners.Contains(Extension.mod(fe - 1, 4))) {
                    fe = Extension.mod(fe - 1, 4);
                }
            }
            //Converts it to colors
            Color[][] unorientedColors = new Color[unorientedCorners.Count][];
            for (int c = 0; c < unorientedCorners.Count; c++) {
                int index = Extension.mod(fe + c, 4);
                (int,int) pos = GetEdgeIndexes(index)[0];
                List<Color> colors = new List<Color>();
                foreach (Color color in GetPieceColors(y, pos)) {
                    if (color != y) {
                        colors.Add(color);
                    }
                }
                unorientedColors[c] = colors.ToArray();
            }

            bool CompareColors(Color[] a1, Color[] a2) {
                bool sameElements = true;
                foreach(Color c1 in a1) {
                    if (!a2.Contains(c1)) {
                        sameElements = false;
                    }
                }
                foreach(Color c2 in a2) {
                    if (!a1.Contains(c2)) {
                        sameElements = false;
                    }
                }
                return sameElements;
            }
            Color ColorIntersect(Color[] a1, Color[] a2) {
                return a1.Intersect(a2).ElementAt(0);
            }
            Color[] CorrectCornerColors(int edge) {
                int[] edges = PositionToEdges(GetEdgeIndexes(edge)[0]);
                Color c1 = sideRelations[y][edges[0]];
                Color c2 = sideRelations[y][edges[1]];
                return (new Color[] { c1,c2});
            }

            Color[] cc1 = CorrectCornerColors(fe);
            Color[] cc2 = CorrectCornerColors(Extension.mod(fe + 1, 4));
            Color[] cc3 = CorrectCornerColors(Extension.mod(fe + 2, 4));

            Color[] uc1 = unorientedColors[0];
            Color[] uc2 = unorientedColors[1];
            Color[] uc3 = unorientedColors[2];
            
            if (unorientedCorners.Count == 3) {
                Color right = ColorIntersect(cc2, cc3);
                Color up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], y) - 1, 4)];
                Color down = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) + 2, 4)];
                if (CompareColors(uc1, cc2) && CompareColors(uc2, cc3) && CompareColors(uc3, cc1)) {
                    //3 Corner
                    //[1][→][2]
                    //[ ][↖][↓]
                    //[ ][ ][3]
                    //x (R' U R') D2 (R U' R') D2 R2
                    Rotate(right, -1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(down, 2, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(down, 2, ref rotations);
                    Rotate(right, 2, ref rotations);
                }
                if (CompareColors(uc1, cc3) && CompareColors(uc2, cc1) && CompareColors(uc3, cc2)) {
                    //Inverse 3 Corner
                    //[1][←][2]
                    //[ ][↘][↑]
                    //[ ][ ][3]
                    //x R2 D2 (R U R') D2 (R U' R)
                    Rotate(right, 2, ref rotations);
                    Rotate(down, 2, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(down, 2, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, 1, ref rotations);
                }
            }
            if (unorientedCorners.Count == 4) {
                //Straight across
                //[ ][ ][ ]
                //[↕][ ][↕]
                //[ ][ ][ ]
                //x' (R U') (R' D) (R U R' D') (R U R' D) (R U') (R' D')
                Color right = new Color();
                for (int e = 0; e < 4; e++) {
                        Color[] colors = GetPieceColors(y, GetEdgeIndexes(e)[0]);
                        Color[] oppositeSides = CorrectCornerColors(Extension.mod(e - 1, 4));
                        if (colors.Intersect(oppositeSides).Count() == 2) {
                            right = sideRelations[y][Extension.mod(e - 1, 4)];
                            break;
                        }
                    }

                //Solves the 4 corner case as a 3 corner case if it's diagonal
                //[1][ ][2]   [2][←][4]   [3][←][2]
                //[ ][x][ ] → [↓][↗][ ] ↷ [ ][↘][↑]
                //[3][ ][4]   [3][ ][ ]   [ ][ ][4]
                if (right == new Color()) {
                    //Inverse 3 corner
                    Color i3u = new Color();
                    Color i3r = new Color();
                    Color i3d = new Color();
                    for (int e = 0; e < 4; e++) {
                        Color[] colors = GetPieceColors(y, GetEdgeIndexes(e)[0]);
                        Color[] oppositeSides = CorrectCornerColors(Extension.mod(e + 2, 4));
                        if (colors.Intersect(oppositeSides).Count() == 2) {
                            i3u = sideRelations[y][Extension.mod(e + 2, 4)];
                            i3r = sideRelations[y][Extension.mod(e + 1, 4)];
                            i3d = sideRelations[i3r][Extension.mod(Array.IndexOf(sideRelations[i3r], i3u) + 2, 4)];
                            break;
                        }
                    }
                    Rotate(i3r, 2, ref rotations);
                    Rotate(i3d, 2, ref rotations);
                    Rotate(i3r, 1, ref rotations);
                    Rotate(i3u, 1, ref rotations);
                    Rotate(i3r, -1, ref rotations);
                    Rotate(i3d, 2, ref rotations);
                    Rotate(i3r, 1, ref rotations);
                    Rotate(i3u, -1, ref rotations);
                    Rotate(i3r, 1, ref rotations);
                    //Solves the case as a 3 corner
                    i3u = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], i3u) - 1, 4)];
                    i3r = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], i3r) - 1, 4)];
                    i3d = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], i3d) - 1, 4)];

                    Rotate(i3r, 2, ref rotations);
                    Rotate(i3d, 2, ref rotations);
                    Rotate(i3r, 1, ref rotations);
                    Rotate(i3u, 1, ref rotations);
                    Rotate(i3r, -1, ref rotations);
                    Rotate(i3d, 2, ref rotations);
                    Rotate(i3r, 1, ref rotations);
                    Rotate(i3u, -1, ref rotations);
                    Rotate(i3r, 1, ref rotations);
                } else {              
                    Color up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], y) + 1, 4)];
                    Color down = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) + 2, 4)];

                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(down, 1, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(down, -1, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, 1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(down, 1, ref rotations);
                    Rotate(right, 1, ref rotations);
                    Rotate(up, -1, ref rotations);
                    Rotate(right, -1, ref rotations);
                    Rotate(down, -1, ref rotations);
                }
            }
        }
        End();
    }
}