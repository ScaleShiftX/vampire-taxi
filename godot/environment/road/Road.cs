using Godot;
using System;

public partial class Road : Node
{
    public override void _Ready()
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
}