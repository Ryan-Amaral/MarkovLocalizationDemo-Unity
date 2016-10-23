/* Created by: Ryan Amaral
 * Date: October 21, 2016
 * License: MIT
 * 
 * Description: This code was hastily scrawled for my desire to learn about markov localization,
 * and to teach other people about how markov localization works.
 */

using UnityEngine;
using System.Collections;

public class MainScript : MonoBehaviour {

    public int cellWidth = 1; // width and height of each square cell
    public int numCellRows = 8; // amount of cells vertically down the grid
    public int numCellCols = 10; // amount of cells horizontally across the grid
    private GameObject[,] cells; // the cell objects that make up the grid

    private GameObject robot; // the robot that is trying to find where it is
    private Vector2 robotPos; // position of the robot (by cell numbers)
    private float[,] cellProbs; // the probability that the robot is on each cell

    private enum Direction { Up, Down, Left, Right };

    private bool isSetup; // if the scene is ready

    // Use this for initialization
    void Start() {
        cells = new GameObject[numCellRows, numCellCols];
        cellProbs = new float[numCellRows, numCellCols];

        // create the robot
        robot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        robot.GetComponent<MeshRenderer>().materials[0].color = Color.yellow;
        robot.transform.localScale = new Vector3(5, 5, 5); // get good size
        robot.name = "Robot";
        robotPos = new Vector2(0, 0);

        isSetup = false;
    }

    // Update is called once per frame
    void Update() {
        // keyboard input for moving robot
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            MoveUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            MoveDown();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            MoveLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            MoveRight();
        }
        // r for random
        if (Input.GetKeyDown(KeyCode.R)) {
            MoveRandomly();
        }

        // s to setup scene
        if (Input.GetKeyDown(KeyCode.S)) {
            Setup();
        }

        // k to kick robot
        if (Input.GetKeyDown(KeyCode.K)) {
            KickRobot();
        }
    }

    /// <summary>
    /// Destroys all objects we added to the scene, and resets values
    /// </summary>
    void DestroyScene() {
        for (int row = 0; row < numCellRows; row++) {
            for (int col = 0; col < numCellCols; col++) {
                GameObject.Destroy(cells[row, col]);
            }
        }
    }

    /// <summary>
    /// Sets up the scene for the demo, call this any time.
    /// </summary>
    public void Setup() {
        isSetup = true;

        // get rid of old objects to not get duplicates
        DestroyScene();

        GameObject tempCell;
        GameObject tempText;

        // random location to put robot
        int randRow = Random.Range(0, numCellRows);
        int randCol = Random.Range(0, numCellCols);

        // create a plane for each cell
        for (int row = 0; row < numCellRows; row++) {
            for (int col = 0; col < numCellCols; col++) {
                cellProbs[row, col] = 1f / (numCellRows * numCellCols); // all cells have same probability

                tempCell = GameObject.CreatePrimitive(PrimitiveType.Plane); // create plane
                // position plane
                tempCell.transform.position = new Vector3((10 * cellWidth * (col - (numCellCols / 2))) - (((numCellCols + 1) % 2) * -5 * cellWidth), 0,
                                                          (-10 * cellWidth * ((row) - (numCellRows / 2))) - (((numCellRows + 1) % 2) * 5 * cellWidth));
                tempCell.transform.localScale = new Vector3(cellWidth, 1, cellWidth); // set scale of plane
                tempCell.name = "Cell[" + row + ", " + col + "]";

                // decide if robot is on this cell
                if (row == randRow && col == randCol) {
                    robot.transform.position = tempCell.transform.position;
                    robot.transform.Translate(0, 1.5f, 0);
                    robotPos.Set(randCol, randRow); // regularly row col, but I mixed that up with xy convention
                }

                // 50/50 to make cell black or white
                if (Random.Range(0f, 1f) > 0.5f) {
                    tempCell.GetComponent<MeshRenderer>().materials[0].color = Color.grey;
                }
                else {
                    tempCell.GetComponent<MeshRenderer>().materials[0].color = Color.white;
                }

                // add text component for probability
                tempText = new GameObject();
                tempText.AddComponent("TextMesh");
                Font textFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                tempText.GetComponent<TextMesh>().font = textFont;
                tempText.GetComponent<TextMesh>().renderer.sharedMaterial = textFont.material;
                tempText.GetComponent<TextMesh>().text = cellProbs[0, 0].ToString().Substring(0, 6); // all the same prob
                tempText.GetComponentInChildren<TextMesh>().color =
                        Color.Lerp(Color.red, Color.green, cellProbs[row, col]); // set color of certainty
                tempText.GetComponent<TextMesh>().fontSize = 22;
                tempText.transform.position = tempCell.transform.position;
                tempText.transform.Translate(-2.5f, 3f, -2.5f);
                tempText.transform.Rotate(90, 0, 0);
                tempText.name = "ProbText";
                tempText.transform.parent = tempCell.transform;


                cells[row, col] = tempCell; // add plane to array of cells
            }
        }
    }

    /// <summary>
    /// Moves the robot up.
    /// </summary>
    public void MoveUp() {
        // only do if not on top row
        if (robotPos.y > 0) {
            MoveRobot(Direction.Up);
        }
    }

    /// <summary>
    /// Moves the robot down.
    /// </summary>
    public void MoveDown() {
        // only do if not on bottom row
        if (robotPos.y < numCellRows - 1) {
            MoveRobot(Direction.Down);
        }
    }

    /// <summary>
    /// Moves the robot left.
    /// </summary>
    public void MoveLeft() {
        // only do if not on left column
        if (robotPos.x > 0) {
            MoveRobot(Direction.Left);
        }
    }

    /// <summary>
    /// Moves the robot Right.
    /// </summary>
    public void MoveRight() {
        // only do if not on right row
        if (robotPos.x < numCellCols - 1) {
            MoveRobot(Direction.Right);
        }
    }

    /// <summary>
    /// Kick the robot to a random location in the map, probabilities do not follow.
    /// </summary>
    public void KickRobot() {
        if (isSetup) {
            int randRow = Random.Range(0, numCellRows);
            int randCol = Random.Range(0, numCellCols);

            robotPos.Set(randCol, randRow);
            robot.transform.position = cells[randRow, randCol].transform.position;
            robot.transform.Translate(0, 1.5f, 0); // put to good height
        }
    }

    /// <summary>
    /// Moves the robot randomly.
    /// </summary>
    public void MoveRandomly() {
        // loop until valid move
        // potentially ineficient, but small selection to choose from
        while (true) {

            int rand = Random.Range(0, 4);
            // up
            if (rand == 0) {
                // only do if not on top row
                if (robotPos.y > 0) {
                    MoveRobot(Direction.Up);
                    return;
                }
            }
            // down
            else if (rand == 1) {
                // only do if not on bottom row
                if (robotPos.y < numCellRows - 1) {
                    MoveRobot(Direction.Down);
                    return;
                }
            }
            // left
            else if (rand == 2) {
                // only do if not on left column
                if (robotPos.x > 0) {
                    MoveRobot(Direction.Left);
                    return;
                }
            }
            // right
            else {
                // only do if not on right row
                if (robotPos.x < numCellCols - 1) {
                    MoveRobot(Direction.Right);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Does the actual moving of the robot.
    /// </summary>
    private void MoveRobot(Direction direction) {
        if (isSetup) {
            // go in the direction told and actually move robot
            if (direction == Direction.Up) {
                robotPos.Set(robotPos.x, --robotPos.y);
                robot.transform.Translate(0, 0, cellWidth * 10);
            }
            else if (direction == Direction.Down) {
                robotPos.Set(robotPos.x, ++robotPos.y);
                robot.transform.Translate(0, 0, -cellWidth * 10);
            }
            else if (direction == Direction.Left) {
                robotPos.Set(--robotPos.x, robotPos.y);
                robot.transform.Translate(-cellWidth * 10, 0, 0);
            }
            else if (direction == Direction.Right) {
                robotPos.Set(++robotPos.x, robotPos.y);
                robot.transform.Translate(cellWidth * 10, 0, 0);
            }

            RecalculateProbabilities(direction);

            // update the squares to show new probabilities
            for (int row = 0; row < numCellRows; row++) {
                for (int col = 0; col < numCellCols; col++) {
                    cells[row, col].GetComponentInChildren<TextMesh>().text = cellProbs[row, col].ToString("F5");
                    // color more red if uncertain, more green if certain
                    cells[row, col].GetComponentInChildren<TextMesh>().color =
                        Color.Lerp(Color.red, Color.green, cellProbs[row, col]);
                }
            }
        }
    }

    /// <summary>
    /// Recalculates the probabilities for the robot being on a cell. This is essentially the 
    /// driving force of markov localization.
    /// </summary>
    private void RecalculateProbabilities(Direction direction) {
        // the color of the cell that the robot is on
        // x and y backwards here because I got confused with rows columns and the typical x and y of math
        Color cellColor = cells[(int)robotPos.y, (int)robotPos.x].GetComponent<MeshRenderer>().materials[0].color;

        // other array for keeping original value of probs to redistribute fairly
        float[,] origProbs = new float[numCellRows, numCellCols];
        // copy cellProbs array
        for (int row = 0; row < numCellRows; row++) {
            for (int col = 0; col < numCellCols; col++) {
                origProbs[row, col] = cellProbs[row, col];
            }
        }

        // redistribute probabilities for each cell, most goes in the intended direction,
        // a little bit goes in others to simulate robot uncertainty in motion
        if (direction == Direction.Up) {

            for (int row = 0; row < numCellRows; row++) {
                for (int col = 0; col < numCellCols; col++) {
                    // 91% goes in intended direction if can
                    if (row > 0) {
                        cellProbs[row - 1, col] += origProbs[row, col] * 0.91f;
                    }
                    // 3% goes to either perpendicular direction if can
                    if (col > 0) {
                        cellProbs[row, col - 1] += origProbs[row, col] * 0.03f;
                    }
                    if (col < numCellCols - 1) {
                        cellProbs[row, col + 1] += origProbs[row, col] * 0.03f;
                    }
                    // 1% goes backwards if can
                    if (row < numCellRows - 1) {
                        cellProbs[row + 1, col] += origProbs[row, col] * 0.01f;
                    }
                    // remove 98% from the cell, 2% stays due to uncertainty in motion
                    cellProbs[row, col] -= origProbs[row, col] * 0.98f;
                }
            }
        }
        if (direction == Direction.Down) {
            for (int row = 0; row < numCellRows; row++) {
                for (int col = 0; col < numCellCols; col++) {
                    // 91% goes in intended direction if can
                    if (row < numCellRows - 1) {
                        cellProbs[row + 1, col] += origProbs[row, col] * 0.91f;
                    }
                    // 3% goes to either perpendicular direction if can
                    if (col > 0) {
                        cellProbs[row, col - 1] += origProbs[row, col] * 0.03f;
                    }
                    if (col < numCellCols - 1) {
                        cellProbs[row, col + 1] += origProbs[row, col] * 0.03f;
                    }
                    // 1% goes backwards if can
                    if (row > 0) {
                        cellProbs[row - 1, col] += origProbs[row, col] * 0.01f;
                    }
                    // remove 98% from the cell, 2% stays due to uncertainty in motion
                    cellProbs[row, col] -= origProbs[row, col] * 0.98f;
                }
            }
        }
        if (direction == Direction.Left) {
            for (int row = 0; row < numCellRows; row++) {
                for (int col = 0; col < numCellCols; col++) {
                    // 91% goes in intended direction if can
                    if (col > 0) {
                        cellProbs[row, col - 1] += origProbs[row, col] * 0.91f;
                    }
                    // 3% goes to either perpendicular direction if can
                    if (row > 0) {
                        cellProbs[row - 1, col] += origProbs[row, col] * 0.03f;
                    }
                    if (row < numCellRows - 1) {
                        cellProbs[row + 1, col] += origProbs[row, col] * 0.03f;
                    }
                    // 1% goes backwards if can
                    if (col < numCellCols - 1) {
                        cellProbs[row, col + 1] += origProbs[row, col] * 0.01f;
                    }
                    // remove 98% from the cell, 2% stays due to uncertainty in motion
                    cellProbs[row, col] -= origProbs[row, col] * 0.98f;
                }
            }
        }
        if (direction == Direction.Right) {
            for (int row = 0; row < numCellRows; row++) {
                for (int col = 0; col < numCellCols; col++) {
                    // 91% goes in intended direction if can
                    if (col < numCellCols - 1) {
                        cellProbs[row, col + 1] += origProbs[row, col] * 0.91f;
                    }
                    // 3% goes to either perpendicular direction if can
                    if (row > 0) {
                        cellProbs[row - 1, col] += origProbs[row, col] * 0.03f;
                    }
                    if (row < numCellRows - 1) {
                        cellProbs[row + 1, col] += origProbs[row, col] * 0.03f;
                    }
                    // 1% goes backwards if can
                    if (col > 0) {
                        cellProbs[row, col - 1] += origProbs[row, col] * 0.01f;
                    }
                    // remove 98% from the cell, 2% stays due to uncertainty in motion
                    cellProbs[row, col] -= origProbs[row, col] * 0.98f;
                }
            }
        }

        float cellSum = 0; // sum of all cells for normalizing
        // multiply likelyhood of being on square with respect to color
        for (int row = 0; row < numCellRows; row++) {
            for (int col = 0; col < numCellCols; col++) {
                // give more weight to those cells with more color
                if (cellColor == cells[row, col].GetComponent<MeshRenderer>().materials[0].color) {
                    cellProbs[row, col] *= 0.9f;
                }
                else {
                    cellProbs[row, col] *= 0.1f;
                }

                cellSum += cellProbs[row, col]; // increment with current cell
            }
        }

        // normalize values so they all add up to 1
        for (int row = 0; row < numCellRows; row++) {
            for (int col = 0; col < numCellCols; col++) {
                cellProbs[row, col] /= cellSum;
            }
        }
    }
}





























