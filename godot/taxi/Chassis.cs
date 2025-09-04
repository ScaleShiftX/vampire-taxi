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

    [Export] private RigidBody3D _wheelBackLeft;
    [Export] private RigidBody3D _wheelBackRight;

    [Export] private PackedScene _blend;
    private Node3D _blendInstance;

    public override void _Ready()
    {
        //ImportBlend();
    }

    private void ImportBlend()
    {
        _blendInstance = (Node3D)_blend.Instantiate();

        //Chassis
        _blendInstance.GlobalPosition = GlobalPosition;
        AddChild(_blendInstance);

        foreach (Node3D node in _blendInstance.GetChild(0).GetChildren())
        {
            //Wheels
            if (node.Name == "WheelFrontLeft")
            {
                foreach (Node3D wheelNode in node.GetChildren())
                {
                    GD.Print($"wheelNode: {wheelNode.Name}");

                    if (wheelNode.Name == "AlwaysFL")
                    {
                        //Set rigidbody origin to wheel origin and generate collider
                        foreach (Node3D alwaysNode in wheelNode.GetChildren()) //this is probably a mesh, but meshes inherit Node3D
                        {
                            GD.Print($"alwaysNode: {alwaysNode.Name}");

                            if (alwaysNode.Name == "tire_fl")
                            {
                                //Set rigidbody origin to wheel origin
                                _wheelFrontLeft.GlobalPosition = alwaysNode.GlobalPosition;

                                //Generate cylinder collision shape
                                CollisionShape3D collisionShape = new()
                                {
                                    Shape = new CylinderShape3D
                                    {
                                        Height = 0.37f,
                                        Radius = 0.601f
                                    },
                                    RotationDegrees = new Vector3(0f, 0f, 90f)
                                };

                                _wheelFrontLeft.AddChild(collisionShape);

                                ////Generate the trimesh collision shape
                                //MeshInstance3D mesh = alwaysNode as MeshInstance3D;
                                //mesh.CreateTrimeshCollision();
                                //
                                ////Remove the static body
                                //Node collisionShape = mesh.GetChild(0).GetChild(0);
                                //mesh.GetChild(0).RemoveChild(collisionShape);
                                //mesh.AddChild(collisionShape);
                                //mesh.GetChild(0).QueueFree();
                            } 
                        }
                    }
                    else if (wheelNode.Name != "Upgrade0FL")
                    {
                        //Hide locked upgrades
                        wheelNode.Visible = false;
                    }
                }

                node.GetParent().RemoveChild(node);
                _wheelFrontLeft.AddChild(node);
            }
            else if (node.Name == "WheelFrontRight")
            {
                _wheelFrontRight.GlobalPosition = node.GlobalPosition;

                node.GetParent().RemoveChild(node);
                _wheelFrontRight.AddChild(node);

                foreach (Node3D wheelnode in node.GetChildren())
                {
                    //Hide locked upgrades
                    if (wheelnode.Name != "AlwaysFR" && wheelnode.Name != "Upgrade0FR")
                    {
                        wheelnode.Visible = false;
                    }
                }
            }
            else if (node.Name == "WheelBackLeft")
            {
                _wheelBackLeft.GlobalPosition = node.GlobalPosition;

                node.GetParent().RemoveChild(node);
                _wheelBackLeft.AddChild(node);

                foreach (Node3D wheelnode in node.GetChildren())
                {
                    //Hide locked upgrades
                    if (wheelnode.Name != "AlwaysBL" && wheelnode.Name != "Upgrade0BL")
                    {
                        wheelnode.Visible = false;
                    }
                }
            }
            else if (node.Name == "WheelBackRight")
            {
                _wheelBackRight.GlobalPosition = node.GlobalPosition;

                node.GetParent().RemoveChild(node);
                _wheelBackRight.AddChild(node);

                foreach (Node3D wheelnode in node.GetChildren())
                {
                    //Hide locked upgrades
                    if (wheelnode.Name != "AlwaysBR" && wheelnode.Name != "Upgrade0BR")
                    {
                        wheelnode.Visible = false;
                    }
                }
            }
        }
    }

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