using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class BlendImport : Node
{
    [ExportToolButton("Import .blend", Icon = "PackedScene")]
    public Callable ClickMeButton => Callable.From(() =>
    {
        CallDeferred(nameof(RunImportDeferred));

        //Reset so it behaves like a button
        SetDeferred(nameof(Action), 0); //0 == None
    });

    [Export] private PackedScene _blend;
    private Node3D _blendInstance;

    [Export] private RigidBody3D _chassis;

    [Export] private RigidBody3D _wheelFrontLeft;
    [Export] private RigidBody3D _wheelFrontRight;
    [Export] private RigidBody3D _wheelBackLeft;
    [Export] private RigidBody3D _wheelBackRight;

    [Export] private Generic6DofJoint3D _jointFrontLeft;
    [Export] private Generic6DofJoint3D _jointFrontRight;
    [Export] private Generic6DofJoint3D _jointBackLeft;
    [Export] private Generic6DofJoint3D _jointBackRight;
    
    private void RunImportDeferred()
    {
        var sceneRoot = GetTree().EditedSceneRoot;
        if (sceneRoot == null)
        {
            GD.PushError("No EditedSceneRoot yet. Open a scene and select a node in it.");
            return;
        }

        ImportBlend(sceneRoot);
    }

    private void ImportBlend(Node sceneOwner)
    {
        GD.Print("ImportBlend()");

        //GET INTO SCENE TREE
        _blendInstance = (Node3D)_blend.Instantiate();

        _blendInstance.GlobalPosition = _chassis.GlobalPosition;
        sceneOwner.AddChild(_blendInstance);

        //Place the whole subtree in the scene tree
        MakeOwnedRecursive(_blendInstance, sceneOwner);

        //Get a list of every node
        List<Node> blendNodes = GetAllChildren(_blendInstance);

        //Move wheel rigidbodies+joints into position
        foreach (var node in blendNodes)
        {
            if (node is Node3D node3D)
            {
                if (node.Name == "tire_bl")
                {
                    _wheelBackLeft.GlobalPosition = node3D.GlobalPosition;
                    _jointBackLeft.GlobalPosition = node3D.GlobalPosition;
                }
                else if (node.Name == "tire_br")
                {
                    _wheelBackRight.GlobalPosition = node3D.GlobalPosition;
                    _jointBackRight.GlobalPosition = node3D.GlobalPosition;
                }
                else if (node.Name == "tire_fl")
                {
                    _wheelFrontLeft.GlobalPosition = node3D.GlobalPosition;
                    _jointFrontLeft.GlobalPosition = node3D.GlobalPosition;
                }
                else if (node.Name == "tire_fr")
                {
                    _wheelFrontRight.GlobalPosition = node3D.GlobalPosition;
                    _jointFrontRight.GlobalPosition = node3D.GlobalPosition;
                }
            }
        }

        //Generate colliders
        foreach (var node in blendNodes)
        {
            if (node.Name == "WheelBackLeft")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelBackLeft.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
            }
            else if (node.Name == "WheelBackRight")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelBackRight.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
            }
            else if (node.Name == "WheelFrontLeft")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelFrontLeft.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
            }
            else if (node.Name == "WheelFrontRight")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelFrontRight.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
            }
        }

        //Set all models to be children of their respective rigidbodies
        foreach (var node in blendNodes)
        {
            if (node.Name == "Chassis")
            {
                node.Reparent(_chassis);
                MakeOwnedRecursive(node, sceneOwner);
            }
            else if (node.Name == "WheelBackLeft")
            {
                node.Reparent(_wheelBackLeft);
                MakeOwnedRecursive(node, sceneOwner);
            }
            else if (node.Name == "WheelBackRight")
            {
                node.Reparent(_wheelBackRight);
                MakeOwnedRecursive(node, sceneOwner);
            }
            else if (node.Name == "WheelFrontLeft")
            {
                node.Reparent(_wheelFrontLeft);
                MakeOwnedRecursive(node, sceneOwner);
            }
            else if (node.Name == "WheelFrontRight")
            {
                node.Reparent(_wheelFrontRight);
                MakeOwnedRecursive(node, sceneOwner);
            }
        }

        //Hide locked upgrades
        foreach (var node in blendNodes)
        {
            if (node is Node3D node3D)
            {
                if (
                    (
                        node.GetParent().Name ==    "WheelBackLeft"
                        && node.Name !=             "WheelBackLeft"
                        && node.Name !=             "AlwaysBL"
                        && node.Name !=             "Upgrade0BL"
                    )
                    || (
                        node.GetParent().Name ==    "WheelBackRight"
                        && node.Name !=             "WheelBackRight"
                        && node.Name !=             "AlwaysBR"
                        && node.Name !=             "Upgrade0BR"
                    )
                    || (
                        node.GetParent().Name ==    "WheelFrontLeft"
                        && node.Name !=             "WheelFrontLeft"
                        && node.Name !=             "AlwaysFL"
                        && node.Name !=             "Upgrade0FL"
                    )
                    || (
                        node.GetParent().Name ==    "WheelFrontRight"
                        && node.Name !=             "WheelFrontRight"
                        && node.Name !=             "AlwaysFR"
                        && node.Name !=             "Upgrade0FR"
                    )
                )
                {
                    node3D.Visible = false;
                }
            }
        }
    }

    private static void MakeOwnedRecursive(Node node, Node owner)
    {
        node.Owner = owner;
        foreach (var child in node.GetChildren())
        {
            MakeOwnedRecursive(child, owner);
        }
    }

    public static List<Node> GetAllChildren(Node parent)
    {
        var result = new List<Node>();

        void Recurse(Node node)
        {
            foreach (Node child in node.GetChildren())
            {
                result.Add(child);
                Recurse(child);
            }
        }

        Recurse(parent);
        return result;
    }

    private CollisionShape3D CreateWheelCollider()
    {
        return new CollisionShape3D()
        {
            Shape = new CylinderShape3D
            {
                Height = 0.37f,
                Radius = 0.601f
            },
            RotationDegrees = new Vector3(0f, 0f, 90f)
        };
    }
}