using Godot;
using System;

[Tool]
public partial class Road : Node
{
    [ExportToolButton("Create Roads", Icon = "PackedScene")] public Callable ButtonCreate => Callable.From(CreateRoads);
    [ExportToolButton("Clear Roads", Icon = "PackedScene")] public Callable ButtonClear => Callable.From(ClearRoads);

    [Export] private Vector2I _dimensions = new Vector2I(2, 2);

    [Export] private PackedScene _blendNorthSouth;
    [Export] private PackedScene _blendEastWest;
    [Export] private PackedScene _blendIntersection4Way;

    private void CreateRoads()
    {
        var sceneOwner = GetTree().EditedSceneRoot;

        for (int x = 0; x < _dimensions.X; x++)
        {
            for (int z = 0; z < _dimensions.Y; z++)
            {
                InstantiateFromToolAtPos(sceneOwner, _blendIntersection4Way, x, z);
            }
        }

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

    private void InstantiateFromToolAtPos(Node sceneOwner, PackedScene packedScene, float x, float z)
    {
        Node3D instance = (Node3D)packedScene.Instantiate();
        AddChild(instance);
        instance.GlobalPosition = new Vector3(x, 0f, z);
        MakeOwnedRecursive(instance, sceneOwner);
    }
}