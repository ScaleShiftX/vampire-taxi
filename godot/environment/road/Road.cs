using Godot;
using System;

public partial class Road : Node3D
{
    public override void _Ready()
    {
        //Create static colliders for all mesh children
        foreach (Node child in GetChildren())
        {
            if (child is MeshInstance3D meshInstance)
            {
                //Create Static Body child and a Single Convex collider as a child of that
                meshInstance.CreateConvexCollision();
            }
        }
    }
}