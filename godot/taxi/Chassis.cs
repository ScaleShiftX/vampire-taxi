using Godot;
using System;

public partial class Chassis : RigidBody3D
{
    [Export] private float _propelMagNm = 500f;
    [Export] private float _turnMaxTurns = 0.08f;

    [Export] public Generic6DofJoint3D JointFrontLeft;
    [Export] public Generic6DofJoint3D JointFrontRight;

    [Export] public RigidBody3D WheelFrontLeft;
    [Export] public RigidBody3D WheelFrontRight;

    public override void _PhysicsProcess(double delta)
    {
        if (!Engine.IsEditorHint())
        {


            if (Input.IsActionPressed("thrust_forward"))
            {
                WheelFrontLeft.ApplyTorque(-WheelFrontLeft.Basis.X * _propelMagNm);
                WheelFrontRight.ApplyTorque(-WheelFrontRight.Basis.X * _propelMagNm);
            }

            HandleTurnKeyboard();
        }
    }

    private void HandleTurnKeyboard()
    {
        if (!Engine.IsEditorHint())
        {
            if (
                (Input.IsActionPressed("turn_left") && Input.IsActionPressed("turn_right"))
                || (!Input.IsActionPressed("turn_left") && !Input.IsActionPressed("turn_right"))
            )
            {
                JointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, 0f);
                JointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, 0f);

                JointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, 0f);
                JointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, 0f);
            }
            else if (Input.IsActionPressed("turn_left"))
            {
                JointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, -_turnMaxTurns * Mathf.Tau);
                JointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, -_turnMaxTurns * Mathf.Tau);

                JointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, -_turnMaxTurns * Mathf.Tau);
                JointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, -_turnMaxTurns * Mathf.Tau);
            }
            else if (Input.IsActionPressed("turn_right"))
            {
                JointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, _turnMaxTurns * Mathf.Tau);
                JointFrontLeft.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, _turnMaxTurns * Mathf.Tau);

                JointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularLowerLimit, _turnMaxTurns * Mathf.Tau);
                JointFrontRight.SetParamY(Generic6DofJoint3D.Param.AngularUpperLimit, _turnMaxTurns * Mathf.Tau);
            }
        }
    }
}