using Godot;
using System;

public partial class CameraPlayerController : Camera3D
{
    [Export] private Node3D _pivot;
    [Export] private float _mouseSensitivity = 0.00075f;

    public override void _Input(InputEvent @event)
    {
        //Look
        if (@event is InputEventMouseMotion mouseMotion)
        {
            //Yaw
            _pivot.Rotation = new Vector3(
                _pivot.Rotation.X,
                _pivot.Rotation.Y - mouseMotion.Relative.X * _mouseSensitivity,
                _pivot.Rotation.Z
            );

            //Pitch, clamp to straight up or down
            _pivot.Rotation = new Vector3(
                Mathf.Clamp(_pivot.Rotation.X - mouseMotion.Relative.Y * _mouseSensitivity,
                    -0.24f * Mathf.Tau,
                    0.24f * Mathf.Tau
                ),
                _pivot.Rotation.Y,
                _pivot.Rotation.Z
            );
        }
    }
}