using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defines
{
    public enum GameObjectType
    {
        HERO = 0,
        ZOMBIE = 1,
        BOX = 2,
        COUNT
    }

    public enum ZombieState
    {
        IDLE = 0,
        WALK = 1,
        ATTACK = 2,
        JUMP = 3,
        DEATH = 4,

        COUNT
    }



}
