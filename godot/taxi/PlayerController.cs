using Godot;
using System;

public partial class PlayerController : RigidBody3D
{
    //[ExportCategory("Diagnostics")]
    //[Export] private Node3D _controlArmFront;
    //[Export] private Node3D _controlArmBack;

    [ExportCategory("Camera")]
    [Export] private Camera3D _cameraPlayer;

    [ExportCategory("Model")]
    [Export] private PackedScene _blend;
    private Node3D _blendInstance;
    [Export] private RigidBody3D _wheelLeft;

    [ExportCategory("Movement")]
    [Export] private float _accelerationDPS2 = 1f;
    [Export] private float _frictionCoefficient = 1f;

    public override void _Ready()
    {
        //Friction
        PhysicsMaterialOverride.Friction = _frictionCoefficient;

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

        ////Snap Rotation
        //Vector3 targetPosition = new Vector3(_cameraPlayer.GlobalPosition.X, _wheelLeft.GlobalPosition.Y, _cameraPlayer.GlobalPosition.Z);
        //_wheelLeft.LookAt(targetPosition, Vector3.Up);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        float delta = state.Step;

        //Update friction in realtime just for diagnostics - remove on release
        PhysicsMaterialOverride.Friction = _frictionCoefficient;


        //wheelLeft.LookAt(wheelLeft.GlobalPosition - _cameraPlayer.GlobalPosition, Vector3.Up);



        //Vector3 targetPosition = new Vector3(_cameraPlayer.GlobalPosition.X, _wheelLeft.GlobalPosition.Y, _cameraPlayer.GlobalPosition.Z);
        //
        //Vector3 direction = (targetPosition - _wheelLeft.GlobalPosition).Normalized();
        //
        //Vector3 forward = -_wheelLeft.GlobalBasis.Z;
        //
        //Vector3 axis = forward.Cross(direction).Normalized();
        //float dot = forward.Dot(direction);
        //float angle = Mathf.Acos(dot);
        //
        //float turnRate = 6f;
        //Vector3 desiredAV = axis * (angle * turnRate);
        //
        //float avSmoothing = 0.2f;
        //state.AngularVelocity = state.AngularVelocity.Lerp(desiredAV, avSmoothing);




        //Front control arm rotation
        //Vector3 frontControlArmTarget = new Vector3(_cameraPlayer.GlobalPosition.X, _controlArmFront.GlobalPosition.Y, _cameraPlayer.GlobalPosition.Z);
        //Vector3 currentRot = _controlArmFront.RotationDegrees;
        //currentRot.X += delta;
        //_controlArmFront.RotationDegrees = currentRot;
        //_controlArmFront.LookAt(frontControlArmTarget);
        //LookAtVector(
        //    _controlArmFront,           //Node
        //    _controlArmFront.GlobalPosition,//Position of the origin
        //    _cameraPlayer.GlobalPosition,   //Position of the target
        //    0.5f * delta * 60f              //Interpolation weight
        //);



        ////Old Astroviolet code
        ////TORQUE DRIECTION
        ////Vector of where the camera is pointing
        //Vector3 shipRelativeToCamera = -_cameraPlayer.GlobalBasis.Z;
        //
        ////The rotation to look at that point
        ////Quaternion rotationToWhereCameraIsLooking = Quaternion.LookRotation(shipRelativeToCamera);
        //Basis desiredBasis = Basis.LookingAt(shipRelativeToCamera, Vector3.Up);
        //Quaternion rotationToWhereCameraIsLooking = desiredBasis.GetRotationQuaternion();
        //
        ////The rotation from how the ship is currently rotated to where the camera is looking
        ////Multiplying by inverse is equivalent to subtracting
        //Quaternion rotation = rotationToWhereCameraIsLooking * new Quaternion(_controlArmFront.GlobalBasis).Inverse();
        //
        ////Parse Quaternion to Vector3
        //Vector3 torqueVectorDirection = new Vector3(rotation.X, rotation.Y, rotation.Z) * rotation.W;
        //
        ////Adding all modifiers together
        //float torqueBaseStrength = 500f;
        //float torqueStrength = torqueBaseStrength * AngularDamp * delta;
        //
        ////APPLY TORQUE
        //Vector3 torqueFinal = torqueVectorDirection * torqueStrength;
        //if (torqueFinal.Length() != 0f) //so we don't get NaN error
        //{
        //    ApplyTorque(torqueFinal);
        //}










        ////Accelerate
        //if (Input.IsActionPressed("thrust_dir_forward"))
        //{
        //    _wheelLeft.ApplyTorque(Vector3.Left * _accelerationDPS2);
        //    //LinearVelocity -= GlobalBasis.Z * _accelerationMPS2;
        //}
    }

    //private static void LookAtVector(Node3D node, Vector3 originPosition, Vector3 targetPosition, float interpolationWeightSlerp)
    //{
    //    // Direction we want to face
    //    Vector3 direction = (targetPosition - originPosition).Normalized();
    //
    //    // Current forward in global space
    //    Vector3 forward = -node.GlobalBasis.Z;
    //
    //    // Delta rotation to turn forward -> direction
    //    Quaternion delta = new Quaternion(forward, direction);
    //
    //    // Current orientation
    //    Quaternion qFrom = new Quaternion(node.GlobalBasis);
    //
    //    // Apply delta to get target orientation
    //    Quaternion qTo = delta * qFrom;
    //
    //    // Interpolate between the two absolute orientations
    //    Quaternion qNew = qFrom.Slerp(qTo, interpolationWeightSlerp);
    //
    //    // Apply back
    //    node.GlobalBasis = new Basis(qNew).Orthonormalized();
    //}

    //private static void LookAtVector(ref Node3D node, Vector3 originPosition, Vector3 targetPosition, float interpolationWeightSlerp)
    //{
    //    //Rotate in the direction of the target position
    //
    //    //Get direction
    //    Vector3 direction = (targetPosition - originPosition).Normalized();
    //
    //    //Choose forward vector
    //    Vector3 forward = -node.GlobalBasis.Z;
    //
    //    //Get rotation from current forward to target direction
    //    Quaternion rotation = new(forward, direction);
    //
    //    //Interpolate to prevent snapping
    //    Quaternion interpolatedRotation = new Quaternion(node.Basis).Slerp(rotation, interpolationWeightSlerp);
    //
    //    //Apply rotation
    //    node.Basis = new(interpolatedRotation);
    //}
}