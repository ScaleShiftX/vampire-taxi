using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class BlendImport : Node
{
    private bool _isImported = false;
    private List<Node> _blendNodes;

    [ExportToolButton("Import .blend", Icon = "PackedScene")]
    public Callable ButtonImport => Callable.From(() => {CallDeferred(nameof(ImportBlend));});

    [ExportToolButton("Clear .blend", Icon = "PackedScene")]
    public Callable ButtonClear => Callable.From(ClearBlend);

    [Export] private PackedScene _blend;
    private Node3D _blendInstance;

    private List<CollisionShape3D> _colliders = [];

    [Export] private RigidBody3D _chassis;

    [Export] private RigidBody3D _wheelFrontLeft;
    [Export] private RigidBody3D _wheelFrontRight;
    [Export] private RigidBody3D _wheelBackLeft;
    [Export] private RigidBody3D _wheelBackRight;

    [Export] private Generic6DofJoint3D _jointFrontLeft;
    [Export] private Generic6DofJoint3D _jointFrontRight;
    [Export] private Generic6DofJoint3D _jointBackLeft;
    [Export] private Generic6DofJoint3D _jointBackRight;

    private void ImportBlend()
    {
        GD.Print("ImportBlend()");

        //Get owner
        var sceneOwner = GetTree().EditedSceneRoot;
        if (sceneOwner == null)
        {
            GD.PushError("No EditedSceneRoot yet. Open a scene and select a node in it.");
            return;
        }

        //Clear previous blend
        if (_isImported)
        {
            ClearBlend();
        }

        //Flag
        _isImported = true;

        //GET INTO SCENE TREE
        _blendInstance = (Node3D)_blend.Instantiate();
        sceneOwner.AddChild(_blendInstance);

        //Position
        _blendInstance.GlobalPosition = _chassis.GlobalPosition;

        //Place the whole subtree in the scene tree
        MakeOwnedRecursive(_blendInstance, sceneOwner);

        //Get a list of every node
        _blendNodes = GetAllChildren(_blendInstance);

        //Move wheel rigidbodies+joints into position
        foreach (var node in _blendNodes)
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
        foreach (var node in _blendNodes)
        {
            if (node.Name == "WheelBackLeft")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelBackLeft.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
                _colliders.Add(collider);
            }
            else if (node.Name == "WheelBackRight")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelBackRight.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
                _colliders.Add(collider);
            }
            else if (node.Name == "WheelFrontLeft")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelFrontLeft.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
                _colliders.Add(collider);
            }
            else if (node.Name == "WheelFrontRight")
            {
                CollisionShape3D collider = CreateWheelCollider();
                _wheelFrontRight.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
                _colliders.Add(collider);
            }
        }

        //Set all models to be children of their respective rigidbodies
        foreach (var node in _blendNodes)
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
        foreach (var node in _blendNodes)
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

        //Delete the original parent (now empty)
        _blendInstance.QueueFree();
        _blendInstance = null;
        //Have to do some weird stuff
        //I think because it's an inherited packed scene so there's some hidden stuff inside it still
        //We'll be tolerant of this in the ClearBlend() method
        _blendNodes.RemoveAll(node => node == null || !GodotObject.IsInstanceValid(node) || node.IsQueuedForDeletion());
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

    private void ClearBlend()
    {
        //Flag
        _isImported = false;

        //Models
        if (_blendNodes != null)
        {
            foreach (var node in _blendNodes)
            {
                if (node != null && IsInstanceValid(node) && !node.IsQueuedForDeletion())
                {
                    node.QueueFree();
                }
            }
            _blendNodes.Clear();
        }

        //Colliders
        foreach (var collider in _colliders)
        {
            if (collider != null && IsInstanceValid(collider) && !collider.IsQueuedForDeletion())
            {
                collider.QueueFree();
            }
        }
        _colliders.Clear();
    }
}