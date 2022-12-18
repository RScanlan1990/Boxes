using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public Vector2 CellLocation;
    private int linesClicked = 0;

    private void OnEnable()
    {
        Line.LineClicked += UpdateSelf;
    }

    private void OnDisable()
    {
        Line.LineClicked -= UpdateSelf;
    }

    void UpdateSelf(Vector2 locationClicked)
    {
        Debug.Log(locationClicked.ToString());
        //Left and Bottom
        if(locationClicked == new Vector2(CellLocation.x, CellLocation.y))
        {
            linesClicked++;
        }       

        //Top
        if(locationClicked == new Vector2(CellLocation.x, CellLocation.y + 1))
        {
            linesClicked++;
        }

        //Right
        if(locationClicked == new Vector2(CellLocation.x + 1, CellLocation.y))
        {
            linesClicked++;
        }

        if(linesClicked == 4)
        {
            Destroy(gameObject);
        }
    }
}
