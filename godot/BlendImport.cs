using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class BlendImport : Node
{
    private enum EditorAction { None, RunImport }

    [Export]
    private EditorAction Action
    {
        get => EditorAction.None;
        set
        {
            if (value == EditorAction.RunImport)
            {
                //ImportBlend();
                CallDeferred(nameof(RunImportDeferred));
                // reset so it behaves like a button
                SetDeferred(nameof(Action), 0); //0 == None
            }
        }
    }

    [Export] private PackedScene _blend;
    private Node3D _blendInstance;

    [Export] private RigidBody3D _chassisRb;

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

        _blendInstance.GlobalPosition = _chassisRb.GlobalPosition;
        sceneOwner.AddChild(_blendInstance);

        //Place the whole subtree in the scene tree
        MakeOwnedRecursive(_blendInstance, sceneOwner);

        //ORGANIZE
        List<Node> allNodes = GetAllChildren(_blendInstance);

        foreach (var node in allNodes)
        {
            if (node is Node3D n3d)
            {
                GD.Print(n3d.GlobalPosition);
            }
        }

        //Move wheel rigidbodies+joints into position
        foreach (var node in allNodes)
        {
            if (node is Node3D n3d)
            {
                if (node.Name == "tire_bl")
                {
                    GD.Print($"{node.Name}: {n3d.GlobalPosition}");
                    _wheelBackLeft.GlobalPosition = n3d.GlobalPosition;
                    _jointBackLeft.GlobalPosition = n3d.GlobalPosition;
                }
                else if (node.Name == "tire_br")
                {
                    GD.Print($"{node.Name}: {n3d.GlobalPosition}");
                    _wheelBackRight.GlobalPosition = n3d.GlobalPosition;
                    _jointBackRight.GlobalPosition = n3d.GlobalPosition;
                }
                else if (node.Name == "tire_fl")
                {
                    GD.Print($"{node.Name}: {n3d.GlobalPosition}");
                    _wheelFrontLeft.GlobalPosition = n3d.GlobalPosition;
                    _jointFrontLeft.GlobalPosition = n3d.GlobalPosition;
                }
                else if (node.Name == "tire_fr")
                {
                    GD.Print($"{node.Name}: {n3d.GlobalPosition}");
                    _wheelFrontRight.GlobalPosition = n3d.GlobalPosition;
                    _jointFrontRight.GlobalPosition = n3d.GlobalPosition;
                }
            }
        }
        
        //Set all models to be children of their respective rigidbodies
        foreach (var node in allNodes)
        {
            if (node.Name == "Chassis")
            {
                node.Reparent(_chassisRb);
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
        foreach (var node in allNodes)
        {
            if (node is Node3D n3d)
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
                    n3d.Visible = false;
                }
            }
        }
    }

    private static void MakeOwnedRecursive(Node n, Node owner)
    {
        n.Owner = owner;
        foreach (var c in n.GetChildren())
            MakeOwnedRecursive(c, owner);
    }

    public static List<Node> GetAllChildren(Node parent)
    {
        var result = new List<Node>();

        void Recurse(Node n)
        {
            foreach (Node child in n.GetChildren())
            {
                result.Add(child);
                Recurse(child);
            }
        }

        Recurse(parent);
        return result;
    }
}