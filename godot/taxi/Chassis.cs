using Godot;
using System;

public partial class Chassis : RigidBody3D
{
    [ExportCategory("Movement")]
    [Export] public Axis AxisPropel = Axis.Z;
    [Export] public float PropelMagNm = 30f;
    [Export] public float TurnMagNm = 3f;
    public enum Axis
    {
        X,
        Y,
        Z
    }
}