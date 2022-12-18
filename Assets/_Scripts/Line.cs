using System;
using UnityEngine;

public class Line : MonoBehaviour
{
    public static event Action<Vector2> LineClicked;

    public int XPosition;
    public int YPosition;

    private bool _clicked;

    public void OnMouseDown()
    {
        if(_clicked) 
            return;

        _clicked = true;
        Clicked();
    }

    public void SetXAndYPosition(int x, int y)
    {
        XPosition = x;
        YPosition = y;
    }

    public void Clicked()
    {
        LineClicked?.Invoke(new Vector2(XPosition, YPosition));
    }
}
