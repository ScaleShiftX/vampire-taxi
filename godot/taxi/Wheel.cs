using Godot;
using System;

public partial class Wheel : RigidBody3D
{
    [ExportCategory("Diagnostics")]
    [Export] private bool _isPrint = false;

    [ExportCategory("External")]
    [Export] private PlayerController _playerController;
    [Export] private Camera3D _cameraPlayer;

    [ExportCategory("Movement")]
    [Export] private bool _isTurn = false;
    [Export] private bool _isPropel = false;

    public override void _PhysicsProcess(double delta)
    {
        //Interpret the positional difference between this wheel and the player controller
        //as a force pushing the player controller in that direction
        //and reset the position of the wheel to snap directly to the car again
        if (_isPrint) GD.Print(Position);
        //Position = Vector3.Zero;

        //New stuff
        Turn();
        Propel();
    }

    private void Turn()
    {
        if (_isTurn)
        {
            //Y axis (up axis, to yaw): use car's local Y
            //ApplyTorque(_playerController.GlobalBasis.Y * _playerController.TurnMagNm);

            //Forward axis
            Vector3 forward = -GlobalBasis.Z;

            //Direction to camera
            Vector3 toCamera = (_cameraPlayer.GlobalPosition - GlobalPosition).Normalized();

            //Axis to rotate around to move forward axis to point in the direction to the camera
            Vector3 axis = forward.Cross(toCamera).Normalized();
            float angle = Mathf.Acos(Mathf.Clamp(forward.Dot(toCamera), -1f, 1f));

            //Torque in that direction
            ApplyTorque(axis * angle * _playerController.TurnMagNm);
        }
    }

    private void Propel()
    {
        if (_isPropel && Input.IsActionPressed("thrust_dir_forward"))
        {
            //X axis (right axis, to propel): use wheel's local X
            ApplyTorque(GlobalBasis.X * _playerController.PropelMagNm);
        }
    }
}