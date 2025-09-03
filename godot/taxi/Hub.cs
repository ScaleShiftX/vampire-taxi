using Godot;
using System;

public partial class Hub : RigidBody3D
{
    [ExportCategory("Nodes")]
    [Export] private Chassis _chassis;
    [Export] private RigidBody3D _wheel;
    [Export] private Camera3D _cameraPlayer;

    [ExportCategory("Movement")]
    [Export] private bool _isTurn = false;
    [Export] private bool _isPropel = false;

    public override void _PhysicsProcess(double delta)
    {
        if (_isTurn)
        {
            Turn();
        }

        if (_isPropel && Input.IsActionPressed("thrust_dir_forward"))
        {
            Propel();
        }
    }

    private void Turn()
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
        ApplyTorque(axis * angle * _chassis.TurnMagNm);
    }

    private void Propel()
    {
        Vector3 axis = _wheel.GlobalBasis.Z;
        if (_chassis.AxisPropel == Chassis.Axis.X)
        {
            axis = _wheel.GlobalBasis.X;
        }
        else if (_chassis.AxisPropel == Chassis.Axis.Y)
        {
            axis = _wheel.GlobalBasis.Y;
        }
        else if (_chassis.AxisPropel == Chassis.Axis.Z)
        {
            axis = _wheel.GlobalBasis.Z;
        }

        _wheel.ApplyTorque(axis * _chassis.PropelMagNm);
    }
}