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

        //Turn
        if (_isTurnable)
        {
            Turn(state);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        //Accelerate
        if (_isAcceleratable && Input.IsActionPressed("thrust_dir_forward"))
        {
            ApplyTorque(GlobalBasis.X * _playerController.AccelerationDPS2);
        }
    }

    private void Turn(PhysicsDirectBodyState3D state)
    {
        Vector3 targetPosition = new Vector3(_cameraPlayer.GlobalPosition.X, _playerController.GlobalPosition.Y, _cameraPlayer.GlobalPosition.Z);
        
        Vector3 direction = (targetPosition - _playerController.GlobalPosition).Normalized();
        if (direction.LengthSquared() < 1e-6f) return; //distance too small to matter

        Vector3 forward = -GlobalBasis.Z;

        Vector3 axis = forward.Cross(direction);
        float dot = forward.Dot(direction);
        float angle = Mathf.Acos(dot);
        if (angle < 1e-4f || axis.LengthSquared() < 1e-8f) return; //already aligned; also avoids NaN errors with axis
        axis = axis.Normalized();

        Vector3 desiredAV = axis * (angle * _playerController.TurnRate);

        state.AngularVelocity = state.AngularVelocity.Lerp(desiredAV, _playerController.AngularVelocityLinearInterpolationWeight);
    }
}