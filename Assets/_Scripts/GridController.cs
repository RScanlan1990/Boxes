using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private Cell _cell;
    [SerializeField] private Line _line;
    [SerializeField] private float _cellSize;

    private Cell[,] Cells = new Cell[10, 10];

    private void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        var xSize = 10;
        var ySize = 10;

        for (var yLocation = 0; yLocation < ySize; yLocation++)
        {

            for (var xLocation = 0; xLocation < xSize; xLocation++)
            {
                var cell = Instantiate(_cell, new Vector3(xLocation, yLocation), Quaternion.identity);
                cell.transform.name = $"Cell({xLocation}, {yLocation})";
                cell.CellLocation = new Vector2(xLocation, yLocation);
                Cells[xLocation, yLocation] = cell;

                var verticalLine = Instantiate(_line, new Vector3(xLocation, yLocation, -1f), Quaternion.Euler(0f, 0f, 90f));
                verticalLine.transform.name = $"Vertical Line({xLocation}, {yLocation})";
                verticalLine.SetXAndYPosition(xLocation, yLocation);

                var horizontalLine = Instantiate(_line, new Vector3(xLocation, yLocation, -1f), Quaternion.Euler(0f, 0f, 0f));
                horizontalLine.transform.name = $"Horizontal Line({xLocation}, {yLocation})";
                horizontalLine.SetXAndYPosition(xLocation, yLocation);

                if(xLocation == xSize - 1)
                {
                    var endLine = Instantiate(_line, new Vector3(xLocation + _cellSize, yLocation, -1f), Quaternion.Euler(0f, 0f, 90f));
                    endLine.transform.name = $"Vertical Line({xLocation}, {yLocation})";
                    endLine.SetXAndYPosition(xLocation, yLocation);
                }


                if (yLocation == ySize - 1)
                {
                    var topLine = Instantiate(_line, new Vector3(xLocation, yLocation + _cellSize, -1f), Quaternion.Euler(0f, 0f, 0f));
                    topLine.transform.name = $"Top Line({xLocation}, {yLocation})";
                    topLine.SetXAndYPosition(xLocation, yLocation);
                }
            }
        }
    }
}
