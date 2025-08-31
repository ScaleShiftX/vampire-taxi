using Godot;
using System;

public partial class Wheel : RigidBody3D
{
    [ExportCategory("External")]
    [Export] private Node3D _playerRoot;
    [Export] private Camera3D _cameraPlayer;

    [ExportCategory("Movement")]
    [Export] private bool _isTurnable = false;
    [Export] private float _accelerationDPS2 = 1f;
    [Export] private float _turnRate = 6f;
    [Export] private float _angularVelocityLinearInterpolationWeight = 0.2f;

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        float delta = state.Step;

        //Turn
        if (_isTurnable)
        {
            Turn(state);
        }

        //Accelerate
        if (Input.IsActionPressed("thrust_dir_forward"))
        {
            ApplyTorque(Vector3.Left * _accelerationDPS2);
        }
    }

    private void Turn(PhysicsDirectBodyState3D state)
    {
        Vector3 targetPosition = new Vector3(_cameraPlayer.GlobalPosition.X, _playerRoot.GlobalPosition.Y, _cameraPlayer.GlobalPosition.Z);
        
        Vector3 direction = (targetPosition - _playerRoot.GlobalPosition).Normalized();
        if (direction.LengthSquared() < 1e-6f) return; //distance too small to matter

        Vector3 forward = -GlobalBasis.Z;

        Vector3 axis = forward.Cross(direction);
        float dot = forward.Dot(direction);
        float angle = Mathf.Acos(dot);
        if (angle < 1e-4f || axis.LengthSquared() < 1e-8f) return; //already aligned; also avoids NaN errors with axis
        axis = axis.Normalized();

        Vector3 desiredAV = axis * (angle * _turnRate);

        state.AngularVelocity = state.AngularVelocity.Lerp(desiredAV, _angularVelocityLinearInterpolationWeight);
    }
}