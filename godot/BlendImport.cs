using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class BlendImport : Node
{
    [ExportToolButton("Import .blend", Icon = "PackedScene")]
    public Callable ButtonImport => Callable.From(ImportBlendPreamble);

    [ExportToolButton("Clear .blend", Icon = "PackedScene")]
    public Callable ButtonClear => Callable.From(ClearBlend);

    [Export] private bool _isGenerateChassisCollider = true;

    private bool _isImported = false;

    [Export] private PackedScene _blend;
    private List<Node> _blendNodes;

    [Export] Node3D _cameraPivot;

    private Chassis _chassis;

    private RigidBody3D _wheelFrontLeft;
    private RigidBody3D _wheelFrontRight;
    private RigidBody3D _wheelBackLeft;
    private RigidBody3D _wheelBackRight;

    private Generic6DofJoint3D _jointFrontLeft;
    private Generic6DofJoint3D _jointFrontRight;
    private Generic6DofJoint3D _jointBackLeft;
    private Generic6DofJoint3D _jointBackRight;

    private List<CollisionShape3D> _colliders = [];

    private async void ImportBlendPreamble()
    {
        //Get owner
        var sceneOwner = GetTree().EditedSceneRoot;
        if (sceneOwner == null)
        {
            GD.PushError("No EditedSceneRoot yet. Open a scene and select a node in it.");
            return;
        }

        //Clear previous blend if there is one
        if (_isImported)
        {
            ClearBlend();

            //Need to wait 2 frames I guess (CallDeferred isn't enough time)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        ImportBlend(sceneOwner);
    }

    private void ImportBlend(Node sceneOwner)
    {
        GD.Print("ImportBlend()");

        //Flag
        _isImported = true;

        //Add and set up rigidbodies and joints
        AddPhysicsNodes(sceneOwner);

        //Reparent player camera
        _cameraPivot.GlobalPosition = _chassis.GlobalPosition;
        _cameraPivot.Reparent(_chassis);

        //GET INTO SCENE TREE
        Node3D _blendInstance = (Node3D)_blend.Instantiate();
        sceneOwner.AddChild(_blendInstance);
        
        //Position
        _blendInstance.GlobalPosition = _chassis.GlobalPosition;
        
        //Place the whole subtree in the scene tree
        MakeOwnedRecursive(_blendInstance, sceneOwner);
        
        //Get a list of every node
        _blendNodes = GetAllChildren(_blendInstance);
        
        //Move wheel rigidbodies+joints into position
        //Add save the chassis and tire mesh for generating colliders later
        MeshInstance3D chassisMesh = null;
        MeshInstance3D tireMesh = null;
        foreach (var node in _blendNodes)
        {
            if (node is Node3D node3D)
            {
                if (node.Name == "car_body")
                {
                    chassisMesh = node as MeshInstance3D;
                }
                else if (node.Name == "tire_bl")
                {
                    tireMesh = node as MeshInstance3D;
        
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
            if (node.Name == "Chassis")
            {
                if (_isGenerateChassisCollider)
                {
                    CollisionShape3D collider = new()
                    {
                        Shape = chassisMesh.Mesh.CreateConvexShape(),
                        RotationDegrees = new Vector3(0f, 180f, 0f)
                    };
        
                    _chassis.AddChild(collider);
                    MakeOwnedRecursive(collider, sceneOwner);
                    _colliders.Add(collider);
        
                    //Can't use CreateTrimeshCollision() because it creates a concave collider
                }
            }
            else if (node.Name == "WheelBackLeft")
            {
                CollisionShape3D collider = CreateWheelCollider(tireMesh);
                _wheelBackLeft.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
                _colliders.Add(collider);
            }
            else if (node.Name == "WheelBackRight")
            {
                CollisionShape3D collider = CreateWheelCollider(tireMesh);
                _wheelBackRight.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
                _colliders.Add(collider);
            }
            else if (node.Name == "WheelFrontLeft")
            {
                CollisionShape3D collider = CreateWheelCollider(tireMesh);
                _wheelFrontLeft.AddChild(collider);
                MakeOwnedRecursive(collider, sceneOwner);
                _colliders.Add(collider);
            }
            else if (node.Name == "WheelFrontRight")
            {
                CollisionShape3D collider = CreateWheelCollider(tireMesh);
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

    private void AddPhysicsNodes(Node sceneOwner)
    {
        //Chassis rigidbody
        _chassis = new Chassis { Name = "Chassis", Mass = 300f };
        sceneOwner.AddChild(_chassis);
        _chassis.Owner = sceneOwner;

        //Wheel rigidbodies
        _wheelFrontLeft = new RigidBody3D()     { Name = "WheelFrontLeft",  Mass = 30f };
        _wheelFrontRight = new RigidBody3D()    { Name = "WheelFrontRight", Mass = 30f };
        _wheelBackLeft = new RigidBody3D()      { Name = "WheelBackLeft",   Mass = 30f };
        _wheelBackRight = new RigidBody3D()     { Name = "WheelBackRight",  Mass = 30f };
        sceneOwner.AddChild(_wheelFrontLeft);
        sceneOwner.AddChild(_wheelFrontRight);
        sceneOwner.AddChild(_wheelBackLeft);
        sceneOwner.AddChild(_wheelBackRight);
        _wheelFrontLeft.Owner = sceneOwner;
        _wheelFrontRight.Owner = sceneOwner;
        _wheelBackLeft.Owner = sceneOwner;
        _wheelBackRight.Owner = sceneOwner;
        
        //Joints
        _jointFrontLeft = new Generic6DofJoint3D()  { Name = "JointFrontLeft", NodeA = _chassis.GetPath(), NodeB = _wheelFrontLeft.GetPath() };
        _jointFrontRight = new Generic6DofJoint3D() { Name = "JointFrontRight", NodeA = _chassis.GetPath(), NodeB = _wheelFrontRight.GetPath() };
        _jointBackLeft = new Generic6DofJoint3D()   { Name = "JointBackLeft", NodeA = _chassis.GetPath(), NodeB = _wheelBackLeft.GetPath() };
        _jointBackRight = new Generic6DofJoint3D()  { Name = "JointBackRight", NodeA = _chassis.GetPath(), NodeB = _wheelBackRight.GetPath() };
        sceneOwner.AddChild(_jointFrontLeft);
        sceneOwner.AddChild(_jointFrontRight);
        sceneOwner.AddChild(_jointBackLeft);
        sceneOwner.AddChild(_jointBackRight);
        _jointFrontLeft.Owner = sceneOwner;
        _jointFrontRight.Owner = sceneOwner;
        _jointBackLeft.Owner = sceneOwner;
        _jointBackRight.Owner = sceneOwner;
        _jointFrontLeft.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
        _jointFrontRight.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
        _jointBackLeft.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, false);
        _jointBackRight.SetFlagX(Generic6DofJoint3D.Flag.EnableAngularLimit, false);

        //Chassis script references
        _chassis.JointFrontLeft = _jointFrontLeft;
        _chassis.JointFrontRight = _jointFrontRight;
        _chassis.WheelFrontLeft = _wheelFrontLeft;
        _chassis.WheelFrontRight = _wheelFrontRight;
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

    private static CollisionShape3D CreateWheelCollider(MeshInstance3D  tireMesh)
    {
        //This is actual voodoo magic

        //1.) Get the mesh AABB in mesh-local space
        var aabbLocal = tireMesh.Mesh.GetAabb();

        //2.) Convert to world-scaled size (assumes no shear; fine for typical rigs)
        //   This accounts for the node’s global scaling (including parents).
        Vector3 worldScale = new Vector3(
            tireMesh.GlobalTransform.Basis.X.Length(),
            tireMesh.GlobalTransform.Basis.Y.Length(),
            tireMesh.GlobalTransform.Basis.Z.Length()
        ).Abs();

        Vector3 sizeWorld = aabbLocal.Size * worldScale;

        //3.) Pick cylinder axis = smallest dimension (tire thickness)
        //   radius = half of the max of the other two dimensions
        float radius, height;

        Vector3 rotationDeg;
        if (sizeWorld.X <= sizeWorld.Y && sizeWorld.X <= sizeWorld.Z)
        {
            //Thickness along X -> cylinder axis should be X
            height = sizeWorld.X;
            radius = 0.5f * Mathf.Max(sizeWorld.Y, sizeWorld.Z);
            rotationDeg = new Vector3(0f, 0f, 90f);  //rotate so axis becomes X
        }
        else if (sizeWorld.Z <= sizeWorld.X && sizeWorld.Z <= sizeWorld.Y)
        {
            //Thickness along Z -> cylinder axis should be Z
            height = sizeWorld.Z;
            radius = 0.5f * Mathf.Max(sizeWorld.X, sizeWorld.Y);
            rotationDeg = new Vector3(90f, 0f, 0f);  //axis becomes Z
        }
        else
        {
            //Thickness along Y -> cylinder axis already matches Y
            height = sizeWorld.Y;
            radius = 0.5f * Mathf.Max(sizeWorld.X, sizeWorld.Z);
            rotationDeg = Vector3.Zero;              //no rotation needed
        }

        var collider = new CollisionShape3D
        {
            Shape = new CylinderShape3D
            {
                Height = height,
                Radius = radius
            },
            RotationDegrees = rotationDeg
        };

        return collider;
    }

    private void ClearBlend()
    {
        GD.Print("ClearBlend()");

        if (_isImported)
        {
            //Flag
            _isImported = false;

            //Save camera
            _cameraPivot.Reparent(GetTree().EditedSceneRoot);
            _cameraPivot.Owner = GetTree().EditedSceneRoot;

            //Physics nodes
            ClearPhysicsNodes();

            //Models
            _blendNodes.Clear();
            _colliders.Clear();
        }
        else
        {
            GD.Print("Nothing to clear!");
        }
    }

    private void ClearPhysicsNodes()
    {
        //Chassis
        _chassis.QueueFree();

        //Wheels
        _wheelFrontLeft.QueueFree();
        _wheelFrontRight.QueueFree();
        _wheelBackLeft.QueueFree();
        _wheelBackRight.QueueFree();

        //Joints
        _jointFrontLeft.QueueFree();
        _jointFrontRight.QueueFree();
        _jointBackLeft.QueueFree();
        _jointBackRight.QueueFree();
    }
}