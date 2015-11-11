using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

/*  This class is responsible for the layout generation, button click event callback functions and
 *  is an interface to the game logic. This generates a nxn grid objects and all of them are numbered sequentially
 */
public class LayoutManager : MonoBehaviour
{
    public GameObject cubeUp, cubeDown, cubeLeft, cubeRight; // Reference to the grid cell prefab
    public AudioClip impact;
    public InputField dim; // Reference to the text box
    public MazeGame mGame; // Reference to the Game script
    public Button resetBtn, restartBtn; // Reference to the Reset and the Restart Buttons
    public GameObject ModalPanel; // Reference to the Panel that holds the Message Box
    bool envSetup, gameStart, displayOn;
    float updateInterval , xOff, zOff;
    GameObject prevTransform; // Reference to the previous object on which the transform was changed
    AudioSource audioSrc;
    int rows;
    int[] indices;

// Button Event Call back Functions------------------------------------------------------------------

    /* Resets the env variables, destroys the existing objects if any, 
     * reads the inputted number and generates the env layout.
     * This is triggered on Generate (Play) button click event
     */
    public void OnClick()
    {
        if (!displayOn)
        {
            if (!ParseNumGrids())
                return;
            gameStart = false;
            envSetup = false;
            if(!GenerateEnvCells())
                return;
        }
    }

    /* Resets the env variables and clears the existing env layout
     * This gets triggered on Reset Button's click event
     */
    private void ResetGame()
    {
        if (!displayOn)
        {
            gameStart = false;
            envSetup = false;
        }
        ClearGrid();
        rows = 0;
    }

    /* Restarts a new game as ResetGame but retains the number of rows generated before
     * This gets triggered on Restart Button click event
     */
    private void RestartNewGame()
    {
        if (!displayOn)
        {
            if (!displayOn)
            {
                gameStart = false;
                envSetup = false;
            }
            ClearGrid();
            if (!GenerateEnvCells())
                return;
        }
    }

    /* Restarts the game by resetting the object transforms to their original
     * This gets triggered each time a new grid is pressed within the same set of grids without restarting/resetting
     */
    private void RestartGame()
    {
        if (!displayOn)
        {
            gameStart = false;
            for (int count = 0; count < rows * rows; count++)
            {
                GameObject go = GameObject.Find(count.ToString());
                go.transform.position = new Vector3(go.transform.position[0], 0.25f, go.transform.position[2]);
                go.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

// Environment Layout Control functions--------------------------------------------------------------

    // Cleans up the environment by destroying the grid cells
    private void ClearGrid()
    {
        for (int count = 0; count < rows*rows; count++)
        {
            Destroy(GameObject.Find(count.ToString()));
        }
    }

    /* Reads the Text input and assigns it as the number of rows / cols
     * returns:  true if a valid number was inputted else displays a message and returns false
     */
    private bool ParseNumGrids()
    {
        int temp;
        if (int.TryParse(dim.text.ToString(), out temp) && temp >= 3 && temp <= 13)
        {
            ClearGrid();
            rows = temp;
            xOff = zOff = -(rows / 2);
            return true;
        }
        else
        {
            StartCoroutine(DisplayMsg("Please Enter only Numbers (3-13)"));
            return false;
        }
    }

    /* Generates the grid environment by laying out the individual grid cells. 
     * Also informs the Game about the order of access to the up, down, left and right nodes
     * returns:  true if the env was successfully generated and returns false 
     *           if the assignment of indices were not successful
     */
    private bool GenerateEnvCells()
    {
        indices = new int[] { -1, 1, -rows, rows }; // Assign the number to be added to the current node number to access Up, Down, Right and LEft Nodes repectively
        int[,] nodes;
        // Assign the sequence for up, down, left and right direction access
        if (!mGame.AssignIndex(indices, rows))
            return false;

        nodes = mGame.GenerateNodes(); // nodes hold the n x n array with random assignment indicating up, down, left or right access based on the indices provided above
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                CreateObject(i, j, nodes[i,j]);
            }
        }
        envSetup = true;
        return true;
    }

    // Creates individual grid objects (up, left, right, bottom) based on the value of the nodes
    private void CreateObject(int i, int j, int val)
    {
        GameObject cellInputField;
        if (val == indices[0])
        {
            cellInputField = (GameObject)Instantiate(cubeUp, new Vector3(i+xOff, 0.25f, j+zOff), Quaternion.identity);
            cellInputField.name = (i*rows + j).ToString();
        }
        else if (val == indices[1])
        {
            cellInputField = (GameObject)Instantiate(cubeDown, new Vector3(i+xOff, 0.25f, j+zOff), Quaternion.identity);
            cellInputField.name = (i*rows + j).ToString();
        }
        else if (val == indices[2])
        {
            cellInputField = (GameObject)Instantiate(cubeRight, new Vector3(i+xOff, 0.25f, j+zOff), Quaternion.identity);
            cellInputField.name = (i*rows + j).ToString();
        }
        else if (val == indices[3])
        {
            cellInputField = (GameObject)Instantiate(cubeLeft, new Vector3(i+xOff, 0.25f, j+zOff), Quaternion.identity);
            cellInputField.name = (i*rows + j).ToString();
        }
    }

    /* Pushes the grid cells into the surface by half a magnitude to create an impression of being pressed,
     * changes its color and resets the transform of the previously pressed grid cell
     */
    private void TransformObj(GameObject go)
    {
        go.transform.position = new Vector3(go.transform.position[0], 0.0f, go.transform.position[2]);
        go.GetComponent<Renderer>().material.color = new Color(150f / 255f, 255f / 255f, 220f / 255f);
        audioSrc.PlayOneShot(impact, 0.7f);
        if (prevTransform != null)
        {
            prevTransform.transform.position = new Vector3(prevTransform.transform.position[0], 0.25f, prevTransform.transform.position[2]);
        }
        prevTransform = go;
    }

    // Use this for initialization
    void Start()
    {
        ModalPanel.SetActive(false);
        gameStart = envSetup = displayOn = false;
        updateInterval = 0.5f;
        xOff = zOff = 0.0f;
        audioSrc = GetComponent<AudioSource>();
        InvokeRepeating("UpdateInterval", updateInterval, updateInterval); // Call back function called every 2 seconds
        // Listener events for the Reset and the Restart button
        restartBtn.onClick.AddListener(() => RestartNewGame()); // Equivalent to start a new game
        resetBtn.onClick.AddListener(() => ResetGame()); // Delete the contents
        rows = 0;
    }

// Periodic update functions that also interfaces with the game -------------------------------------

    // Update is called once per frame
    void Update()
    {
        try
        {
            // Check if the mouse button was clicked on a grid cell using a Ray Cast
            if (Input.GetMouseButtonDown(0) && !displayOn)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    if (hit.transform != null)
                    {
                        int res;
                        // If the environment is setup and the game is not already started start the game
                        if (int.TryParse(hit.transform.name, out res) && envSetup && !gameStart)
                        {
                            RestartGame();
                            TransformObj(GameObject.Find(hit.transform.name));
                            if (!mGame.InitGame(res))
                                return;
                            gameStart = true;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            print(e.ToString());
        }
    }

    /* This function gets called every 2 seconds. It retrieves the next node to be transformed from the game
     * and displays the end message upon game end
     */
    void UpdateInterval()
    {
        try
        {
            if (gameStart)
            {
                int nodeNo = mGame.FindNextNode();
                if (nodeNo != -999 && nodeNo != -1000)
                {
                    TransformObj(GameObject.Find(nodeNo.ToString()));
                }
                else if(nodeNo  == -999)
                {
                    StartCoroutine(DisplayMsg("GAME OVER! (Path Found)"));
                    gameStart = false;
                }
                else if(nodeNo == -1000)
                {
                    StartCoroutine(DisplayMsg("GAME OVER! (Loop Found)"));
                    gameStart = false;
                }
            }
        }
        catch (System.Exception e)
        {
            print(e.ToString());
        }

    }

    // Displays the message box and waits for n seconds
    IEnumerator DisplayMsg(String msg)
    {
        displayOn = true;
        ModalPanel.SetActive(true);
        ModalPanel.GetComponentInChildren<Text>().text = msg;
        yield return new WaitForSeconds(3);
        ModalPanel.SetActive(false);
        displayOn = false;

    }
}

