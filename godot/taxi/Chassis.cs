using Godot;
using System;
using static Godot.XRBodyTracker;

public partial class Chassis : RigidBody3D
{
    [Export] private float _propelMagNm = 500f;
    [Export] private float _turnMaxTurns = 0.08f;

    [Export] private Generic6DofJoint3D _jointFrontLeft;
    [Export] private Generic6DofJoint3D _jointFrontRight;

    [Export] private RigidBody3D _wheelFrontLeft;
    [Export] private RigidBody3D _wheelFrontRight;

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("thrust_forward"))
        {
            _wheelFrontLeft.ApplyTorque(-_wheelFrontLeft.Basis.X * _propelMagNm);
            _wheelFrontRight.ApplyTorque(-_wheelFrontRight.Basis.X * _propelMagNm);
        }

        HandleTurnKeyboard();
    }

    private void HandleTurnKeyboard()
    {
        if (
            (Input.IsActionPressed("turn_left") && Input.IsActionPressed("turn_right"))
            || (!Input.IsActionPressed("turn_left") && !Input.IsActionPressed("turn_right"))
        )
        {
            _jointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, 0f);
            _jointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, 0f);

            _jointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, 0f);
            _jointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, 0f);
        }
        else if (Input.IsActionPressed("turn_left"))
        {
            _jointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, -_turnMaxTurns * Mathf.Tau);
            _jointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, -_turnMaxTurns * Mathf.Tau);

            _jointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, -_turnMaxTurns * Mathf.Tau);
            _jointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, -_turnMaxTurns * Mathf.Tau);
        }
        else if (Input.IsActionPressed("turn_right"))
        {
            _jointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, _turnMaxTurns * Mathf.Tau);
            _jointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, _turnMaxTurns * Mathf.Tau);

            _jointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, _turnMaxTurns * Mathf.Tau);
            _jointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, _turnMaxTurns * Mathf.Tau);
        }
    }
}