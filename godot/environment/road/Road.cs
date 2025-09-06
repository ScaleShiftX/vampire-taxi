using Godot;
using System;

[Tool]
public partial class Road : Node
{
    [ExportToolButton("(Re)Create Roads", Icon = "PackedScene")] public Callable ButtonCreate => Callable.From(CreateRoads);
    [ExportToolButton("Clear Roads", Icon = "PackedScene")] public Callable ButtonClear => Callable.From(ClearRoads);

    [Export] private Vector2I _dimensions = new Vector2I(2, 2);

    [ExportCategory(".blend Specifications")]
    [Export] private float _totalWidth = 9.4f;
    [Export] private float _northSouthLength = 200f;
    [Export] private float _eastWestLength = 100f;

    [Export] private PackedScene _blendNorthSouth;
    [Export] private PackedScene _blendEastWest;
    [Export] private PackedScene _blendIntersection4Way;

    private void CreateRoads()
    {
        //Clear old roads first
        if (GetChildCount() > 0)
        {
            ClearRoads();
        }

        //Since this is a tool script running in-editor the owner will be different
        var sceneOwner = GetTree().EditedSceneRoot;

        //Rest of the fucking owl
        int i = 0;
        for (int x = 0; x <= _dimensions.X; x++) //<= instead of < so we create the corners/intersections ;)
        {
            for (int z = 0; z <= _dimensions.Y; z++)
            {
                float xMultiple = x * (_eastWestLength + _totalWidth);
                float zMultiple = z * (_northSouthLength + _totalWidth);

                //Intersection
                InstantiateFromToolAtPos(sceneOwner,
                    _blendIntersection4Way,
                    xMultiple,
                    zMultiple,
                    i
                );

                //Road stretches - don't create if on the last iteration as these need to just be corners/intersections
                if (x < _dimensions.X)
                {
                    InstantiateFromToolAtPos(sceneOwner,
                        _blendEastWest,
                        xMultiple + (_eastWestLength / 2f) + (_totalWidth / 2f),
                        zMultiple,
                        i
                    );
                }

                if (z < _dimensions.Y)
                {
                    InstantiateFromToolAtPos(sceneOwner,
                        _blendNorthSouth,
                        xMultiple,
                        zMultiple + (_northSouthLength / 2f) + (_totalWidth / 2f),
                        i
                    );
                }

                //Count total interations
                i++;
            }
        }

        //Colliders last
        CreateColliders();
    }

    private void CreateColliders()
    {
        foreach (Node blend in GetChildren()) //.blend inherited packed scenes
        {
            //GD.Print($"blend: {blend.Name}");

            //Create static colliders for all mesh children
            foreach (Node mesh in blend.GetChildren()) //meshes
            {
                //GD.Print($"mesh: {mesh.Name}");

                //Create Static Body child and a Single Convex collider as a child of that
                MeshInstance3D meshInstance = mesh as MeshInstance3D;
                meshInstance.CreateConvexCollision();
            }
        }
    }

    private void ClearRoads()
    {
        foreach (Node blend in GetChildren()) //.blend inherited packed scenes
        {
            blend.QueueFree();
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

    private void InstantiateFromToolAtPos(Node sceneOwner, PackedScene packedScene, float x, float z, int iterationForName)
    {
        Node3D instance = (Node3D)packedScene.Instantiate();
        AddChild(instance);
        instance.GlobalPosition = new Vector3(x, 0f, z);
        MakeOwnedRecursive(instance, sceneOwner);

        //Collapse this node in the scene tree
        instance.SetDisplayFolded(true);

        //Make sure the name remains human readable
        instance.Name += iterationForName;
    }
}