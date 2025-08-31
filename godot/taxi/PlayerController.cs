using Godot;
using System;

public partial class PlayerController : RigidBody3D
{
    [ExportCategory("Model")]
    [Export] private PackedScene _blend;
    private Node3D _blendInstance;

    [ExportCategory("Movement")]
    [Export] public float AccelerationDPS2 = 3000f;
    [Export] public float TurnRate = 6f;
    [Export] public float AngularVelocityLinearInterpolationWeight = 0.2f;

    public override void _Ready()
    {
        ////Import blend
        //_blendInstance = (Node3D)_blend.Instantiate();
        //AddChild(_blendInstance);
        //
        ////fix the .blend offsets
        //_blendInstance.Position = new Vector3(0f, 1.023f, 0f);
        //_blendInstance.RotationDegrees = new Vector3(0f, 90f, 0f);
    }
}