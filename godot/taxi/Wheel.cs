using Godot;
using System;

public partial class Wheel : RigidBody3D
{
    [ExportCategory("External")]
    [Export] private PlayerController _playerController;
    [Export] private Camera3D _cameraPlayer;

    [ExportCategory("Movement")]
    [Export] private bool _isTurnable = false;
    [Export] private bool _isAcceleratable = false;

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        float delta = state.Step;

        ////Turn
        //if (_isTurnable)
        //{
        //    //Y axis (up axis, to yaw): use car's local Y
        //
        //    //Turn(state);
        //    //ApplyTorque(Basis.Y * 2f);
        //}
    }

    public override void _PhysicsProcess(double delta)
    {
        //Turn
        if (_isTurnable)
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

        //Propel
        if (_isAcceleratable && Input.IsActionPressed("thrust_dir_forward"))
        {
            //X axis (right axis, to propel): use wheel's local X
            ApplyTorque(GlobalBasis.X * _playerController.PropelMagNm);
        }
    }
}