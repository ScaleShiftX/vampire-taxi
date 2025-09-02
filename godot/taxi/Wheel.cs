using Godot;
using System;

public partial class Wheel : RigidBody3D
{
    [ExportCategory("Diagnostics")]
    [Export] private bool _isPrint = false;

    [ExportCategory("External")]
    [Export] private PlayerController _chassis;
    [Export] private Camera3D _cameraPlayer;

    [ExportCategory("Movement")]
    [Export] private bool _isTurn = false;
    [Export] private bool _isPropel = false;

    [ExportCategory("PD")]
    [Export] private Node3D _hub;
    [Export] private float _stiffness = 33f; // N/m (k)
    [Export] private float _forceMaxY = 100f;
    [Export] private float _damping = 4_000f;  // N·s/m (c)
    private float _restY; //set up in ready

    public override void _Ready()
    {
        // Measure rest separation along chassis up
        //Vector3 up = _chassis.GlobalBasis.Y;
        //_restY = (GlobalTransform.Origin - _hub.GlobalTransform.Origin).Dot(up);
    }

    public override void _PhysicsProcess(double delta)
    {
        //PD();
        //Turn();
        //Propel();
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
            ApplyTorque(axis * angle * _chassis.TurnMagNm);
        }
    }

    private void Propel()
    {
        if (_isPropel && Input.IsActionPressed("thrust_dir_forward"))
        {
            //X axis (right axis, to propel): use wheel's local X
            ApplyTorque(-GlobalBasis.X * _chassis.PropelMagNm);
        }
    }

    private void PD()
    {
        //Interpret the positional difference between this wheel and the player controller
        //as a force pushing the player controller in that direction
        //and reset the position of the wheel to snap directly to the car again

        //Vector3 up = _chassis.GlobalBasis.Y;
        //if (up.LengthSquared() < 0.5f) return; // safety
        //
        //Vector3 chassisPos = _hub.GlobalTransform.Origin;
        //Vector3 wheelPos = GlobalTransform.Origin;
        //
        //// Position & relative speed along chassis up
        //float y = (wheelPos - chassisPos).Dot(up);
        //float ySpeed = (LinearVelocity - _chassis.LinearVelocity).Dot(up);
        //
        //// Pure vertical spring–damper
        //float fy = -_stiffness * (y - _restY) - _damping * ySpeed;
        //if (Mathf.Abs(fy) > _forceMaxY) { GD.Print("Maximum y force reached!"); }
        //fy = Mathf.Clamp(fy, -_forceMaxY, _forceMaxY);
        //Vector3 f = up * fy;
        //
        //// CENTRAL forces only (no off-centre = no accidental torques)
        //_chassis.ApplyCentralForce(-f);
        //ApplyCentralForce(f);
    }
}