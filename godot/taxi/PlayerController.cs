using Godot;
using System;

public partial class PlayerController : VehicleBody3D
{
    [ExportCategory("Model")]
    [Export] private PackedScene _blend;
    private Node3D _blendInstance;

    [ExportCategory("Movement")]
    [Export] private float _engineForce = 300f;

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

    public override void _PhysicsProcess(double deltaDouble)
    {
        float delta = (float)deltaDouble;

        if (Input.IsActionPressed("thrust_dir_forward"))
        {
            GD.Print("Thurst forward");
            EngineForce = _engineForce;
        }
        else
        {
            EngineForce = 0f;
        }
    }
}