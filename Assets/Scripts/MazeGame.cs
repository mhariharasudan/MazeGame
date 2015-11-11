using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// The Actual Game Logic
public class MazeGame : MonoBehaviour {
    int[,] nodes;
    int[] dirValues;
    int size, startNodeNo, met;
    bool incFastNode;
    struct Coords
    {
        public int i;
        public int j;
    };
    Coords slowNode, fastNode;

    void Start()
    {
        
        dirValues = new int[] { -1, 1, -1, 1 };
        size = 0;
        met = 0;
        incFastNode = false;
    }

    /*  Generates a nxn array of nodes with random directions in each.
     *  returns the randomly generated nxn array of nodes
     */
    public int[,] GenerateNodes()
    {
        slowNode = fastNode = new Coords();
        nodes = new int[size, size];
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                //Randomly assign the next cell's relative location to each cell
                nodes[i,j] = (dirValues[Random.Range(0, dirValues.Length)]);
            }
        }
        return nodes;
    }

    // Reads the size and the possible direction values for the random nodes
    public bool AssignIndex(int[] ind, int sz)
    {
        int[] cmpDirValues = new int[] { -1, 1, -sz, sz };
        // Compare if there are only 4 elements and both the array has the same elements
        if (ind.Length == 4 && Enumerable.SequenceEqual(cmpDirValues, ind))
        {
            size = sz;
            dirValues = ind;
            return true;
        }
        return false ;
    }

    // Resets the game parameters and sets the start node to the specified node locations
    public bool InitGame(int ndNo)
    {
        if (nodes == null)
            return false;
        Reset();
        startNodeNo = ndNo;
        SetStart(ndNo, ndNo);
        DetectLoop();
        return true;
    }

    // Resets the game parameters
    private void Reset()
    {
        incFastNode = false;
        met = 0;
    }
    // If a valid number is not inputted assign the start node to the origin
    private void SetStart(int a, int b)
    {
        slowNode.i = a / size;
        slowNode.j = a % size;
        fastNode.i = b / size;
        fastNode.j = b % size;
    }

    // Detects loop using Floyd's Cycle Algorithms. Sets the Fast node to the meeting point and the Slow node to the start
    private void DetectLoop()
    {
        while(fastNode.i < size && fastNode.i >= 0 && fastNode.j < size && fastNode.j >= 0)
        {
            slowNode = JumpNode(slowNode, 1);
            fastNode = JumpNode(fastNode, 2);

            if (fastNode.Equals(slowNode)) //If they have both met
            {
                SetStart(startNodeNo, ((fastNode.i * size) + fastNode.j)); // Set the slow node to the start pos & retain fast node at its current pos
                incFastNode = true;
                break;
            }
        }
        if(!incFastNode)
        SetStart(startNodeNo, startNodeNo);
    }

    /* Returns the Next possible node's number or negative numbers if there are no possible options
     *  returns:  -1000 when it is a loop, -999 when there are no possible options or next node's number
     */
    public int FindNextNode()
    {
        if (fastNode.Equals(slowNode))
        {
            met++;
            if (met == 1)// If both node pointer are at the start of the loop stop incrementing fast node
                incFastNode = false;
            else if (met == 2)// Exit if both the nodes meet the second time (at the end of the loop)
                return -1000;
        }
        slowNode = JumpNode(slowNode, 1);
        if (incFastNode)
            fastNode = JumpNode(fastNode, 1);
        if (slowNode.i >= size || slowNode.i < 0 || slowNode.j >= size || slowNode.j < 0)
            return -999;
        else
            return ((slowNode.i * size) + slowNode.j);
    }
    
    /* Jumps nodes n number of times based on its next directions
     *  param[0] src: Coordinate of the source node
     *  param[1] loop: Number of jumps to be performed
     *  returns:  The new coordinate of the node
     */
    private Coords JumpNode(Coords src, int iter)
    {
        int tmpI = 0, tmpJ = 0;
        while (iter != 0)
        {
            if (src.i < size && src.i >= 0 && src.j < size && src.j >= 0)
            {
                tmpI = nodes[src.i, src.j] / size;
                tmpJ = nodes[src.i, src.j] % size;
                src.i += tmpI;
                src.j += tmpJ;
            }
            iter--;
        }
        return src;
    }
}
