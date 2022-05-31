// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
#if UNITY_WSA
using UnityEngine;

public class EulerPoseWithName
{

    public string name;
    public Vector3 position;
    public Vector3 eulerRotation;
    public Vector3 correctionPositionDirection=new  Vector3 (-1,-1,1);
    public Quaternion rotation { get { return Quaternion.Euler(eulerRotation); } }
    public bool IsShowingAsWireframe=true;
    public bool IsShowingModelLog=false;
}
#endif // UNITY_WSA

