using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameType
{


    public enum Direction
    {
        BOTTOM,
        BOTTOM_RIGHT,
        RIGHT,
        TOP_RIGHT,
        TOP,
        TOP_LEFT,
        LEFT,
        BOTTOM_LEFT
    }

    public enum State
    {
        IDLE,
        WALK,
        ATTACK,
        DESTROYED
    }
}
