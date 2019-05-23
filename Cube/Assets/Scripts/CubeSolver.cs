using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class CubeSolver : MonoBehaviour {
    [SerializeField]
    CubeVisualizer cubeVisualizer;
    public Dictionary<Color, Color[,]> orientation;
    Dictionary<Color, Color[]> sideRelations;
    public string rotations;
    Dictionary<Color, char> colorToNotation;
    Dictionary<char, Color> notationToColor;
    enum SideRelations {Top = 0, Right, Bottom, Left}
    Color w;
    Color y;
    Color g;
    Color r;
    Color b;
    Color o;
    void Start() {
        //Intializes Variables
        orientation = new Dictionary<Color, Color[,]>();
        sideRelations = new Dictionary<Color, Color[]>();
        colorToNotation = new Dictionary<Color, char>();
        notationToColor = new Dictionary<char, Color>();

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

        //Random scramble
        //Rotate(o, -1);
        //Rotate(r, 3);
        //Rotate(b, -2);
        //Rotate(y, 3);
        //Rotate(g, -3);
        //Rotate(w, -1);s
    }

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

    public void Rotate(Color side, int turns)  {
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
        rotations += colorToNotation[side];
        if (turns < 0){
            rotations += "'";
        }
        rotations += Mathf.Abs(turns);
        rotations += " ";
        //Updates visualiziation
        cubeVisualizer.UpdateVisualization();
    }

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
        return(edges.ToArray());
    }

    //Gets colors on a piece
    Color[] GetPieceColors(Color side, (int,int) pos) {
        int[] adjEdges = PositionToEdges(pos);

        List<Color> colors = new List<Color>();
        foreach (int adjEdge in adjEdges) {
            //Gets the position of the color on the adjacent side
            int sideEdge = Array.IndexOf(sideRelations[sideRelations[side][adjEdge]], side);
            int edgePos = 0;
            if (adjEdge == (int)SideRelations.Top || adjEdge == (int)SideRelations.Bottom) {
                edgePos = pos.Item2;
            }
            if (adjEdge == (int)SideRelations.Left || adjEdge == (int)SideRelations.Right) {
                edgePos = pos.Item1;
            }
            (int,int) colorPosition = GetEdgeIndexes(adjEdge)[2 - edgePos];
            //Adds the color
            colors.Add(orientation[sideRelations[side][adjEdge]][colorPosition.Item1, colorPosition.Item2]);
        }
        return(colors.ToArray());
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
        /*
        int side1PositionIndex = Array.IndexOf(GetEdgeIndexes(Array.IndexOf(sideRelations[side1], side2)), pos);
        (int,int) side2Position = GetEdgeIndexes(Array.IndexOf(sideRelations[side2], side1))[2 - side1PositionIndex];
        return(side2Position);
        */
    }

    public string Solve() {
        //Clears rotations
        rotations = "";
        //Forms cross on white side
        Color[] wAdjSides = sideRelations[w];
        foreach (Color wAdjSide in wAdjSides) {
            //Finds white positions that are on the edges
            (int, int)[] wPositions = SideContains(wAdjSide, w);
            if (wPositions.Length != 0) {
                foreach ((int, int) position in wPositions) { 
                    for (int e = 0; e < 4; e++) { 
                        if (position == GetEdgeIndexes(e)[1]) {
                            //Moves it to an empty position in the cross
                            //Prepares the side to be moved in
                            if (e != (int)SideRelations.Right) {
                                Rotate(wAdjSide, -e + (int)SideRelations.Right);
                            }
                            //Rotates the white side
                            Color rotateSide = sideRelations[wAdjSide][(int)SideRelations.Right];
                            (int, int) targetWPosition = GetOtherPiecePosition(rotateSide, w, GetEdgeIndexes((int)SideRelations.Top)[1]);
                            while (orientation[w][targetWPosition.Item1, targetWPosition.Item2] == w) { 
                                Rotate(w, 1);
                            }
                            Rotate(rotateSide, 1);
                            break;
                        }    
                    }
                }
            }    
        }
        //Finds white positions on yellow
        (int,int)[] yWPositions = SideContains(y, w);
        foreach((int,int) yWPosition in yWPositions) {
            if (PositionToEdges(yWPosition).Length == 1) { 
                Color sideTW = sideRelations[y][PositionToEdges(yWPosition)[0]];
                (int,int) targetWPos = GetEdgeIndexes(Array.IndexOf(sideRelations[w], sideTW))[1];
                while (orientation[w][targetWPos.Item1, targetWPos.Item2] == w) {
                    Rotate(w, 1);
                }
                Rotate(sideTW, 2);
            }
        }
       
        //Fixes the orientation of the adj sides
        foreach (Color wAdjSide in wAdjSides) {
            (int, int) topMiddle = GetEdgeIndexes((int)SideRelations.Top)[1];
            if (orientation[wAdjSide][topMiddle.Item1, topMiddle.Item2] != wAdjSide) {
                Color leftSide = sideRelations[wAdjSide][(int)SideRelations.Left];
                Color oppositeSide = sideRelations[sideRelations[wAdjSide][(int)SideRelations.Top]][(int)SideRelations.Top];
                if (orientation[leftSide][topMiddle.Item1, topMiddle.Item2] == wAdjSide && orientation[wAdjSide][topMiddle.Item1, topMiddle.Item2] == leftSide) {
                    //Left side
                    Rotate(wAdjSide, -1);
                    Rotate(w, -1);
                    Rotate(wAdjSide, 1);
                    Rotate(w, 1);
                    Rotate(wAdjSide, -1);
                    //Checking the right side isn't needed
                } else if (orientation[oppositeSide][topMiddle.Item1, topMiddle.Item2] == wAdjSide && orientation[wAdjSide][topMiddle.Item1, topMiddle.Item2] == oppositeSide) {
                    Rotate(wAdjSide, 2);
                    Rotate(oppositeSide, 2);
                    Rotate(y, 2);
                    Rotate(wAdjSide, -2);
                    Rotate(oppositeSide, -2);
                }
            }
        }
        return "e";
        //F2L
        //Finds empty pair slots
        List<(Color,Color)> emptyPairs = new List<(Color, Color)>();
        for (int c = 0; c < 4; c++) { 
            int corner = Mathf.FloorToInt(c / 2);
            Color pairSide1 = sideRelations[w][Extension.mod(c - 1, 4)];
            Color pairSide2 = sideRelations[w][c];
            
            (int, int) wCornerPosition = GetEdgeIndexes(c)[0];
            int pairSide1EdgeOffset = GetEdgeIndexes(Array.IndexOf(sideRelations[pairSide1], pairSide2))[0].Item2;
            int pairSide2EdgeOffset = GetEdgeIndexes(Array.IndexOf(sideRelations[pairSide2], pairSide1))[0].Item2;
            if (orientation[w][wCornerPosition.Item1, wCornerPosition.Item2] == w &&
                orientation[pairSide1][0, pairSide1EdgeOffset] == pairSide1 &&
                orientation[pairSide1][1, pairSide1EdgeOffset] == pairSide1 &&
                orientation[pairSide2][0, pairSide2EdgeOffset] == pairSide2 &&
                orientation[pairSide2][1, pairSide2EdgeOffset] == pairSide2 ) {

            } else {
                emptyPairs.Add((pairSide1, pairSide2));
            }
        }

        //Creates pairs
        foreach ((Color, Color) emptyPair in emptyPairs) {
           //Checks if the pieces needed to make a pair are already on the yellow side
           (int,int)? cornerOnYellow = null;
           (int,int)? edgeOnYellow = null;
           foreach (Color color in new Color[] {w, emptyPair.Item1, emptyPair.Item2}) {
                (int,int)[] bottomPositions = GetEdgeIndexes((int)SideRelations.Bottom);
                foreach(Color wAdjSide in wAdjSides) {
                    foreach((int,int) bottomPosition in bottomPositions) {
                        Color[] pieceColors = GetPieceColors(wAdjSide, bottomPosition);
                        if (pieceColors.Contains(emptyPair.Item1) && pieceColors.Contains(emptyPair.Item2)) {
                            if (pieceColors.Contains(w)) {
                                //Adds corner position
                                cornerOnYellow = GetOtherPiecePosition(wAdjSide, y, bottomPosition);
                            } else {
                                //Adds edge position
                                edgeOnYellow = GetOtherPiecePosition(wAdjSide, y, bottomPosition);
                            }
                        }
                    }
                }
           }
           //Moves any remaining piece needed to make the pair onto the yellow side
           if (cornerOnYellow == null || edgeOnYellow == null) {

                Color cornerSide = new Color();
                (int,int) cornerPos = GetEdgeIndexes((int)SideRelations.Top)[0];
                if (cornerOnYellow == null) {
                    foreach (Color side in wAdjSides) {
                        Color[] pieceColors = GetPieceColors(side, cornerPos);
                        if (pieceColors.Contains(emptyPair.Item1) && pieceColors.Contains(emptyPair.Item2)) {
                            cornerSide = side;
                        }
                    }
                } else {
                    int[] edges = PositionToEdges(cornerOnYellow.Value);
                    for (int e = 0; e < edges.Length; e++) {
                        if (GetOtherPiecePosition(y, sideRelations[y][e], cornerOnYellow.Value) == cornerPos) {
                            cornerSide = sideRelations[y][e];
                        }
                    }
                }

                Color edgeSide = new Color();
                (int,int) edgePos = GetEdgeIndexes((int)SideRelations.Right)[1];
                if (edgeOnYellow == null) {
                    foreach (Color side in wAdjSides) {
                        Color[] pieceColors = GetPieceColors(side, edgePos);
                        if (pieceColors.Contains(emptyPair.Item1) && pieceColors.Contains(emptyPair.Item2)) {
                            edgeSide = side;
                        }
                    }
                } else {
                    edgeSide = sideRelations[y][PositionToEdges(edgeOnYellow.Value)[0]];
                }

                if (cornerOnYellow == null) {
                    if (edgeOnYellow != null) {
                        (int,int) position = GetEdgeIndexes(Array.IndexOf(sideRelations[y], edgeSide))[1];
                        while (edgeOnYellow != (2 - position.Item1, 2 - position.Item2)) {
                            Rotate(y, 1);
                            edgeOnYellow = (2 - edgeOnYellow.Value.Item2, 2 + edgeOnYellow.Value.Item1);
                        }
                    }
                    Rotate(cornerSide, -1);
                    Rotate(y, -1);
                    Rotate(cornerSide, 1);
                }


                if (edgeOnYellow == null) {
                    if (cornerOnYellow != null) {
                        (int,int) position = GetEdgeIndexes(Array.IndexOf(sideRelations[y], cornerSide))[0];
                        while (cornerOnYellow != (2 - position.Item1, 2 - position.Item2)) {
                            Rotate(y, 1);
                            cornerOnYellow = (2 - cornerOnYellow.Value.Item2, 2 + cornerOnYellow.Value.Item1);
                        }
                    }
                    Rotate(edgeSide, 1);
                    Rotate(y, 1);
                    Rotate(edgeSide, -1);
                }

           } else if (cornerOnYellow != null && edgeOnYellow != null) {
                int[] cornerEdges;
                (int,int) cornerPos = cornerOnYellow.Value;
                (int,int) edgePos = edgeOnYellow.Value;
                //Solves the pair
                //Separates the corner
                if (Mathf.Abs(cornerPos.Item1 - edgePos.Item1) == 1 || Mathf.Abs(cornerPos.Item2 - edgePos.Item2) == 1 ) {
                    //Finds side where there is only the corner and an empty pair is on the opposite side of the corner
                    cornerEdges = PositionToEdges(cornerPos);
                    int cornerOnlyEdge = cornerEdges[1 - Array.IndexOf(cornerEdges, PositionToEdges(edgePos)[0])];
                    Color cornerOnlySide = sideRelations[y][cornerOnlyEdge];
                    bool isEmpty = false;
                    while (!isEmpty) {
                        cornerOnlySide = sideRelations[y][cornerOnlyEdge];
                        foreach ((Color,Color) emptyPairColors in emptyPairs) {
                            if (cornerOnlySide == emptyPairColors.Item1) {
                                if (cornerOnlySide == emptyPairColors.Item2) {
                                    isEmpty = true;
                                }
                            } else if (cornerOnlySide == emptyPairColors.Item2) {
                                if (cornerOnlySide == emptyPairColors.Item1) {
                                    isEmpty = true;
                                }
                            }
                        }
                        Rotate(y,1);
                        cornerOnlyEdge = Extension.mod(cornerOnlyEdge + 1, 4);
                    }

                    //Rotates the corner out of the way
                    cornerOnlySide = sideRelations[y][cornerOnlyEdge];
                    int cornerOnlySideRot =  1 - GetOtherPiecePosition(y, sideRelations[y][cornerOnlyEdge], cornerPos).Item2;
                    Rotate(cornerOnlySide,  cornerOnlySideRot);

                    //Rotates edge to right spot
                    //It's stupid but it works
                    Rotate(y, -2 - Extension.mod(cornerOnlySideRot, -1));

                    //Rotates the corner back into place
                    Rotate(cornerOnlySide, -cornerOnlySideRot);

                }

                //Moves corner above empty pair
                //Gets corner position
                cornerEdges = new int[2];
                List<(int,int)> corners = new List<(int, int)>();
                for (int e = 0; e < 4; e++) {
                    corners.Add(GetEdgeIndexes(e)[0]);
                }
                foreach((int,int) corner in corners) {
                    Color[] pieceColors = GetPieceColors(y, corner);
                    if (pieceColors.Contains(w) && pieceColors.Contains(emptyPair.Item1) && pieceColors.Contains(emptyPair.Item2)) {
                        cornerEdges = PositionToEdges(corner);
                        if (cornerEdges.Length != 2) {
                            throw new Exception("Corner doesn't have 2 edges");
                        }
                    }
                }
                //Rotates yellow side until above the right empty pair
                int c1Index = Array.IndexOf(sideRelations[y], emptyPair.Item1);
                int c2Index = Array.IndexOf(sideRelations[y], emptyPair.Item2);
                while (cornerEdges.Contains(c1Index) && cornerEdges.Contains(c2Index)) {
                    Rotate(y, 1);
                    for(int i = 0; i < cornerEdges.Length; i++) {
                        cornerEdges[i] = Extension.mod(cornerEdges[i] + 1, cornerEdges.Length);
                    }
                }
                //Algorithm
                Color cornerTop = new Color();
                Color edgeTop = new Color();
                foreach((int,int) e1Pos in GetEdgeIndexes(cornerEdges[0])) {
                    foreach((int,int) e2Pos in GetEdgeIndexes(cornerEdges[1])) {
                        if (e1Pos == e2Pos) {
                            cornerPos = e1Pos;
                            cornerTop = orientation[y][e1Pos.Item1, e1Pos.Item2];
                        }
                    }
                }
                for (int i = 0; i < 4; i++){
                    (int, int) position = GetEdgeIndexes(i)[0];
                    Color[] pieceColors = GetPieceColors(y, (position.Item1, position.Item2));
                    if (pieceColors.Contains(emptyPair.Item1) && pieceColors.Contains(emptyPair.Item2)) {
                        edgePos = position;
                        edgeTop = orientation[y][position.Item1, position.Item2];
                    }
                }

                Color up = y;
                Color front = sideRelations[y][Extension.mod(PositionToEdges(edgePos)[0] + 2, 4)];
                Color right = sideRelations[y][Extension.mod(PositionToEdges(edgePos)[0] + 1, 4)];

                //White on top
                //F' U F R U' R' U'
                if (cornerTop == w) {
                    Rotate(front, -1);
                    Rotate(up, 1);
                    Rotate(front, 1);
                    Rotate(right, 1);
                    Rotate(up, -1);
                    Rotate(right, -1);
                    Rotate(up, -1);
                }

                if (cornerTop == edgeTop) {
                    //Same color
                    //F U F' U R U' R'
                    Rotate(front, 1);
                    Rotate(up, 1);
                    Rotate(front, -1);
                    Rotate(up, 1);
                    Rotate(right ,1);
                    Rotate(up, -1);
                    Rotate(right, -1);
                } else if (cornerTop != edgeTop) {
                    //Different color
                    //R U R'
                    Rotate(right, 1);
                    Rotate(up, 1);
                    Rotate(right, -1);
                }

                emptyPairs.Remove(emptyPair);
                    
           }
        }

        //Yellow cross
        (int,int)[] yEdgePositions = new (int,int)[4];
        Color[] yEdgeColors = new Color[4];
        for(int e = 0; e < 4; e++) {
            yEdgePositions[e] = GetEdgeIndexes(e)[1];
            yEdgeColors[e] = orientation[y][yEdgePositions[e].Item1, yEdgePositions[e].Item2];
        }
        for (int e = 0; e > 4; e++) {
            if (yEdgeColors[e] == y) {
                Color nextEdgeColor = yEdgeColors[Extension.mod(e + 1, 4)];
                if (nextEdgeColor == y) {
                    Color front = sideRelations[y][Extension.mod(e + 1, 4)];
                    Color right = sideRelations[y][e];
                    Color up = y;
                    Rotate(front, 1);
                    Rotate(right, 1);
                    Rotate(up, 1);
                    Rotate(right, -1);
                    Rotate(up, -1);
                    Rotate(front, 1);

                    Rotate(up, 1);

                    Rotate(front, 1);
                    Rotate(right, 1);
                    Rotate(up, 1);
                    Rotate(right, -1);
                    Rotate(up, -1);
                    Rotate(front, 1);
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
            Color corner1OtherSide = sideRelations[y][corner1];
            (int,int) corner1OtherPos = GetOtherPiecePosition(y, corner1OtherSide, GetEdgeIndexes(corner1)[0]);
            Color corner1OtherColor = orientation[corner1OtherSide][corner1OtherPos.Item1, corner1OtherPos.Item2];

            (int,int) corner2Pos = GetEdgeIndexes(corner2)[0];
            Color corner2OtherSide = sideRelations[y][corner2];
            (int,int) corner2OtherPos = GetOtherPiecePosition(y, corner2OtherSide, GetEdgeIndexes(corner2)[0]);
            Color corner2OtherColor = orientation[corner2OtherSide][corner2OtherPos.Item1, corner2OtherPos.Item2];

            //Diagonal
            if (Extension.mod(corner1 + 2, 4) == corner2) {
                //Gets the sides
                Color front = new Color();
                Color right = new Color();
                Color back = new Color();
                if (orientation[corner1OtherSide][corner1OtherPos.Item1, corner1OtherPos.Item2] == y) {
                    front = corner1OtherSide;
                    right = sideRelations[y][Extension.mod(corner1 - 1, 4)];
                    back = sideRelations[y][Extension.mod(corner1 - 2, 4)];
                } else {
                    front = sideRelations[y][corner2];
                    right = sideRelations[y][Extension.mod(corner2 - 1, 4)];
                    back = sideRelations[y][Extension.mod(corner2 - 2, 4)];
                }
                //Algorithm
                //(R' F) (R B') (R' F') (R B)
                Rotate(right, -1);
                Rotate(front, 1);
                Rotate(right, 1);
                Rotate(back, -1);
                Rotate(right, -1);
                Rotate(front, -1);
                Rotate(right, 1);
                Rotate(back, 1);
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
                    //(R2' D) (R' U2) (R D') (R' U2 R')
                    Color right = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], commonSide) + 1, 4)];
                    Color up = y;
                    Color down = w;
                    
                    Rotate(right, -2);
                    Rotate(down, 1);
                    Rotate(right, -1);
                    Rotate(up , 2);
                } else {
                    //Facing opposite sides
                    //(Lw' U') (L U) (R U') (Rw' F)
                    Color right = commonSide;
                    Color left = sideRelations[y][Extension.mod(Array.IndexOf(sideRelations[y], commonSide) + 2, 4)];
                    Color up = y;

                    Rotate(right, 1);
                    up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) - 1, 4)];
                    Rotate(up, -1);
                    Rotate(left, 1);
                    Rotate(up, 1);
                    Rotate(right, 1);
                    Rotate(up, -1);
                    Rotate(left, -1);
                    up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) - 1, 4)];
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
                        Color right = sideRelations[y][edge];

                        Rotate(right, 1);
                        Rotate(up, 1);
                        Rotate(right, -1);
                        Rotate(up, 1);
                        Rotate(right, 1);
                        Rotate(up, 2);
                        Rotate(right, -1);
                        break;
                    } else if (leftEdges.Contains(edge)) {
                        //Antisune
                        //(R U2) (R' U' R U' R')
                        Color right = sideRelations[y][Extension.mod(edge + 2, 4)];
                        
                        Rotate(right, 1);
                        Rotate(up, 2);
                        Rotate(right, -1);
                        Rotate(up, -1);
                        Rotate(right, -1);
                        break;
                    }
                }
            }
        }
        //4 Corners
        if (yLessCorners.Count == 4) {
            List<int> headlightEdges = new List<int>();
            bool symmetric = false;
            for (int e = 0; e < 4; e++) {
                (int,int)[] positions =  GetEdgeIndexes((int)SideRelations.Bottom);
                Color c11 = orientation[sideRelations[y][e]][positions[0].Item1, positions[0].Item2];
                Color c12 = orientation[sideRelations[y][e]][positions[2].Item1, positions[2].Item2];
                
                Color c21 = orientation[sideRelations[y][Extension.mod(e + 2, 4)]][positions[0].Item1, positions[0].Item2];
                Color c22 = orientation[sideRelations[y][Extension.mod(e + 2, 4)]][positions[2].Item1, positions[2].Item2];
                
                bool edge1 = false;
                bool edge2 = false;

                if (c11 == y && c12 == y) {
                    edge1 = true;
                    headlightEdges.Add(e);
                }
   
                if (c21 == y && c22 == y) {
                    edge2 = true;
                    headlightEdges.Add(Extension.mod(e + 2, 4));
                }
                if (edge1 && edge2) {
                    //Symmetric
                    //(R U2) (R' U' R U R' U' R U' R')
                    Color right = sideRelations[y][Extension.mod(e - 1, 4)];
                    Color up = y;
                    Rotate(right, 1);
                    Rotate(up, 2);
                    Rotate(right, -1);
                    Rotate(up, -1);
                    Rotate(right, 1);
                    Rotate(up, 1);
                    Rotate(right, -1);
                    Rotate(up, -1);
                    Rotate(right, 1);
                    Rotate(up, -1);
                    Rotate(right, -1);

                    symmetric = true;
                    break;
                }
            }
            //Not symmetric
            if (!symmetric) {
                //(R U2') (R2' U') (R2 U') (R2' U2' R)
                if (headlightEdges.Count != 1) {
                    throw new Exception("Case would be symmetric");
                }
                Color right = sideRelations[y][Extension.mod(headlightEdges[0] + 2, 4)];
                Color up = y;
                Rotate(right, 1);
                Rotate(up, -2);
                Rotate(right, -2);
                Rotate(up, -1);
                Rotate(right, 2);
                Rotate(up, -1);
                Rotate(right, -2);
                Rotate(up, -2);
                Rotate(right, 1);
            }
        }
        
        //PLL
        //Edges
        (int,int) ePosition = GetEdgeIndexes((int)SideRelations.Bottom)[1];
        List<int> unorientedEdges = new List<int>();
        for (int e = 0; e < 4; e++) {
            Color c = orientation[sideRelations[y][e]][ePosition.Item1, ePosition.Item2];
            if (c != sideRelations[y][e]) {
                unorientedEdges.Add(e);
            }
        }

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
                    Rotate(right, 1);
                    Rotate(up, -1);
                    Rotate(right, 1);
                    Rotate(up, 1);
                    Rotate(right, 1);
                    Rotate(up, 1);
                    Rotate(right, 1);
                    Rotate(up, -1);
                    Rotate(right, -1);
                    Rotate(up, -1);
                    Rotate(right, 2);
                    break;
                } else if (c1 == nc3 && c2 == nc1 && c3 == nc2) {
                    //Inverse 3 edges
                    //(R2 U) (R U R' U') (R' U') (R' U R')
                    Rotate(right, 2);
                    Rotate(up, 1);
                    Rotate(right, 1);
                    Rotate(up, 1);
                    Rotate(right, -1);
                    Rotate(up, -1);
                    Rotate(right, -1);
                    Rotate(up, -1);
                    Rotate(right, -1);
                    Rotate(up, 1);
                    Rotate(right, -1);
                    break;
                }
            }
        }

        if (unorientedEdges.Count == 4) {
            Color esc1 = sideRelations[y][unorientedEdges[0]];
            Color esc2 = sideRelations[y][unorientedEdges[1]];
            Color esc3 = sideRelations[y][unorientedEdges[2]];
            Color esc4 = sideRelations[y][unorientedEdges[3]];

            Color ec1 = orientation[esc1][ePosition.Item1, ePosition.Item2];
            Color ec2 = orientation[esc1][ePosition.Item1, ePosition.Item2];
            Color ec3 = orientation[esc1][ePosition.Item1, ePosition.Item2];
            Color ec4 = orientation[esc1][ePosition.Item1, ePosition.Item2];
            
            Color left = r;
            Color right = o;
            if (ec1 == esc3 && ec3 == esc1 && ec2 == esc4 && ec4 == esc2) {
                //Across
                //(M2' U) (M2' U2) (M2' U) M2'
                Rotate(left, 2);
                Rotate(right, -2);
                Rotate(w, 1);
                Rotate(left, 2);
                Rotate(right, -2);
                Rotate(y, 2);
                Rotate(left, 2);
                Rotate(right, -2);
                Rotate(w, 1);
                Rotate(left, 2);
                Rotate(right, -2);
            } else if (ec1 == esc4 && ec4 == esc1 && ec2 == esc3 && ec3 == esc2) {
                //Diagonal
                //(M2' U) (M2' U) (M' U2) (M2' U2) (M' U2)
                Rotate(left, 2);
                Rotate(right, -2);
                Rotate(y, 1);
                Rotate(left, 2);
                Rotate(right, -2);
                Rotate(w, 1);
                Rotate(left, 1);
                Rotate(right, -1);
                Rotate(y, 2);
                Rotate(left, 2);
                Rotate(right, -2);
                Rotate(w, 2);
                Rotate(left, 1);
                Rotate(right, -1);
                Rotate(y , 2);
            }
        }


        //Corners
        (int,int) cPosition = GetEdgeIndexes((int)SideRelations.Bottom)[0];
        List<int> unorientedCorners = new List<int>();
        for (int c = 0; c < 4; c++) {
            int[] edges = PositionToEdges(cPosition);
            Color[] colors = new Color[2];
            Color[] sides = new Color[2];
            for (int e = 0; e < 2; e++) {
                int edge = edges[e];
                Color otherSide = sideRelations[y][e];
                (int,int) otherPosition = GetOtherPiecePosition(y, otherSide, cPosition);
                colors[e] = orientation[otherSide][otherPosition.Item1, otherPosition.Item2];
                sides[e] = otherSide;
            }
            if (!(colors.Contains(sides[0]) && colors.Contains(sides[1]))) {
                unorientedCorners.Add(c);
            }
        }
        //csc1, csc2, csc3 WON'T NECESSARILY BE CONSECUTIVE

        //fe is the least clockwise corner if such exists
        int? fe = null;
        for (int e = 0; e < 4; e++) {
            if (!unorientedCorners.Contains(Extension.mod(e - 1, 4))) {
                fe = e;
            }
        }
        if (fe == null) {
            fe = unorientedCorners[0];
        }

        Color csc1 = sideRelations[y][fe.Value];
        Color csc2 = sideRelations[y][Extension.mod(fe.Value + 1, 4)];
        Color csc3 = sideRelations[y][Extension.mod(fe.Value + 2, 4)];

        Color cc1 = orientation[csc1][ePosition.Item1, ePosition.Item2];
        Color cc2 = orientation[csc1][ePosition.Item1, ePosition.Item2];
        Color cc3 = orientation[csc1][ePosition.Item1, ePosition.Item2];
        if (unorientedCorners.Count == 3) {

            Color right = new Color();
            foreach (int e1 in PositionToEdges(GetOtherPiecePosition(csc2, y, cPosition))) {
                foreach (int e2 in PositionToEdges(GetOtherPiecePosition(csc3, y, cPosition))) {
                    if (e1 == e2) {
                       right = cc1;
                    }
                }
            }
            Color up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], y) - 1, 4)];
            Color down = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) + 2, 4)];
            if (cc1 == csc2 && cc2 == csc3 && cc3 == csc1) {
                //3 Corner
                //x (R' U R') D2 (R U' R') D2 R2
                Rotate(right, -1);
                Rotate(up, 1);
                Rotate(right, -1);
                Rotate(down, 2);
                Rotate(right, 1);
                Rotate(up, -1);
                Rotate(right, -1);
                Rotate(down, 2);
                Rotate(right, 2);
            }
            if (cc1 == csc3 && cc2 == csc1 && cc3 == csc2) {
                //Inverse 3 Corner
                //x R2 D2 (R U R') D2 (R U' R)
                Rotate(right, 2);
                Rotate(down, 2);
                Rotate(right, 1);
                Rotate(up, 1);
                Rotate(right, -1);
                Rotate(down, 2);
                Rotate(right, 1);
                Rotate(up, -1);
                Rotate(right, 1);
            }
        }
        //Straight Across
        if (unorientedCorners.Count == 4) {
            //x' (R U') (R' D) (R U R' D') (R U R' D) (R U') (R' D')
            Color right = new Color();
            foreach (int e1 in PositionToEdges(GetOtherPiecePosition(csc2, y, cPosition))) {
                foreach (int e2 in PositionToEdges(GetOtherPiecePosition(csc3, y, cPosition))) {
                    if (e1 != e2) {
                       right = cc1;
                    }
                }
            }

            Color up = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], y) + 1, 4)];
            Color down = sideRelations[right][Extension.mod(Array.IndexOf(sideRelations[right], up) + 2, 4)];

            Rotate(right, 1);
            Rotate(up, -1);
            Rotate(right, -1);
            Rotate(down, 1);
            Rotate(right, 1);
            Rotate(up, 1);
            Rotate(right, -1);
            Rotate(down, -1);
            Rotate(right, 1);
            Rotate(up, 1);
            Rotate(right, -1);
            Rotate(down, 1);
            Rotate(right, 1);
            Rotate(up, -1);
            Rotate(right, -1);
            Rotate(down, -1);
        }

        //ANTI SQUIGGLY
        return rotations;    
    }
}
