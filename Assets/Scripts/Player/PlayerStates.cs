using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace TrianCatStudio
{
    public enum CharacterParam
    {
        MoveSpeed,
        IsGrounded,
        IsJumping,
        IsAiming,
        IsSliding
    }

    public enum CharacterState
    {
        Idle,
        Running,
        Jumping,
        Aiming,
        Sliding,
        Reloading,
        Melee
    }

}