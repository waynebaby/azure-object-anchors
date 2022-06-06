// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using Microsoft.Azure.ObjectAnchors.Unity;
using Microsoft.Azure.ObjectAnchors.Unity.Sample;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CustomerModelShowCase : MonoBehaviour
{

    private void Awake()
    {

        ManualPanel.SetActive(CustomerModelShowCaseController.Instance?.IsRecalibrationModeEnabled ?? true);

        SetCheckboxes();
    }
    public float positionSliderScale = 0.55f;
    private EulerPoseWithName currentBaseOffset = new EulerPoseWithName() { name = "default" };
    private EulerPoseWithName currentAdjustedOffset
    {
        get
        {
            var smallAdjustmentPosition = new Vector3(SliderX.SliderValue - 0.5f, SliderY.SliderValue - .5f, SliderZ.SliderValue - .5f);

            return new EulerPoseWithName
            {
                position = currentBaseOffset.position + smallAdjustmentPosition,
                eulerRotation = currentBaseOffset.eulerRotation,
                correctionPositionDirection = currentBaseOffset.correctionPositionDirection,
                name = currentBaseOffset.name,
                IsShowingAsWireframe = currentBaseOffset.IsShowingAsWireframe,
                IsShowingModelLog = currentBaseOffset.IsShowingModelLog
            };
        }
    }
    private TrackedObjectData trackedObjectData;
    public GameObject OffsetStage;
    public GameObject CenterFixStage;
    public GameObject RotationFixStage;
    public TextMeshPro LogTextMesh;
    public TextMeshPro CurrentOffsetRotationText;
    public PinchSlider SliderX;
    public PinchSlider SliderY;
    public PinchSlider SliderZ;
    public Interactable CheckboxX;
    public Interactable CheckboxY;
    public Interactable CheckboxZ;
    public GameObject ManualPanel;

    public GameObject DisplayingModel { get; private set; }

    public void ResetDisplayModels()
    {
        CustomerModelShowCaseController.Instance.ResetDisplayModels();
    }
    public void RemoveDisplayModel()
    {
        CustomerModelShowCaseController.Instance.RemoveDisplayModel(trackedObjectData?.InstanceId??Guid.Empty);

    }
    private List<(MeshRenderer renderer, Material[] original, Material[] wiredNew)> MaterialBackups;
    // private Boolean IsShowingAsWireFrame = false;
    public void ApplyModelToStage(EulerPoseWithName additionalOffset, GameObject createdDisplayModel, TrackedObjectData state, string message = "")
    {
        trackedObjectData = state;
        createdDisplayModel.transform.parent = CenterFixStage.transform;
        createdDisplayModel.transform.localPosition = Vector3.zero;
        createdDisplayModel.transform.localRotation = Quaternion.identity;
        createdDisplayModel.SetActive(true);
        LogTextMesh.text = message;
        DisplayingModel = createdDisplayModel;
        currentBaseOffset = additionalOffset;
        currentBaseOffset.name = state.ModelFileName;
        MaterialBackups = CenterFixStage
            .GetComponentsInChildren<MeshRenderer>()
            .Select(x => (x, x.materials, Enumerable.Repeat(CustomerModelShowCaseController.Instance.WireframeMaterial, x.materials.Length).ToArray()))
            .ToList();

        SetCheckboxes();
        ApplyCenterFixFromAOAService();
        ResetOffsetSmallAdjuestments();
        ApplyOffsetToStage();
        ApplyWireframeDisplayAndLogBooleans();
    }

    private void ApplyCenterFixFromAOAService()
    {
#if WINDOWS_UWP
        var state = trackedObjectData;
        var service = ObjectAnchorsService.GetService();
        var matrix = service.GetModelOriginToCenterTransform(state.ModelId);
        if (matrix != null)
        {

            var positionCorrection = matrix.Value.MultiplyPoint(Vector3.zero);
            var correctionDirection = currentAdjustedOffset.correctionPositionDirection;

            positionCorrection = new Vector3(correctionDirection.x * positionCorrection.x,
                correctionDirection.y * positionCorrection.y,
                correctionDirection.z * positionCorrection.z);

            var correctionPose = new Pose(positionCorrection, matrix.Value.rotation);
            Debug.Log("Successfully get transform from center");
            LogTextMesh.text = LogTextMesh.text + @$"
Successfully get transform from center
correction  :{JsonUtility.ToJson(correctionPose)}
correctionel  :{JsonUtility.ToJson(correctionPose.rotation.eulerAngles)}";


            CenterFixStage.transform.localPosition = correctionPose.position;
            CenterFixStage.transform.localRotation = correctionPose.rotation;
            //   CenterFixStage.transform.localScale = matrix.Value.lossyScale;

        }
#endif
    }

    private void ResetOffsetSmallAdjuestments()
    {
        SliderX.SliderValue = 0.5f;
        SliderY.SliderValue = 0.5f;
        SliderZ.SliderValue = 0.5f;
    }

    private void ApplyOffsetToStage()
    {

        OffsetStage.transform.localPosition = currentAdjustedOffset.position;
        OffsetStage.transform.localRotation = currentAdjustedOffset.rotation;
        CurrentOffsetRotationText.text = JsonUtility.ToJson(currentAdjustedOffset);
        LogTextMesh.text = LogTextMesh.text + @$"
additional  :{JsonUtility.ToJson(currentAdjustedOffset)}
additionalel:{JsonUtility.ToJson(currentAdjustedOffset.rotation.eulerAngles)}";
    }

    public void RefreshCenterFixDirectionFromUI()
    {

        currentBaseOffset.correctionPositionDirection = new Vector3(
             CheckboxX.IsToggled ? -1 : 1,
             CheckboxY.IsToggled ? -1 : 1,
             CheckboxZ.IsToggled ? -1 : 1);
        Debug.Log(nameof(RefreshCenterFixDirectionFromUI));
        ApplyCenterFixFromAOAService();
        ApplyOffsetToStage();

    }
    public void ChangeRotationX(float offset)
    {
        ChangeRotation(new Vector3(offset, 0, 0));
    }
    public void ChangeRotationY(float offset)
    {
        ChangeRotation(new Vector3(0, offset, 0));
    }
    public void ChangeRotationZ(float offset)
    {
        ChangeRotation(new Vector3(0, 0, offset));
    }


    public void ChangePositionX(float offset)
    {

        ChangePosition(new Vector3(offset * positionSliderScale, 0, 0));
    }
    public void ChangePositionY(float offset)
    {
        ChangePosition(new Vector3(0, offset * positionSliderScale, 0));
    }
    public void ChangePositionZ(float offset)
    {
        ChangePosition(new Vector3(0, 0, offset * positionSliderScale));
    }
    public void ChangeRotation(Vector3 offset)
    {


        currentBaseOffset.eulerRotation = currentBaseOffset.eulerRotation + offset;
        currentBaseOffset.eulerRotation = new Vector3(
            currentBaseOffset.eulerRotation.x % 360,
            currentBaseOffset.eulerRotation.y % 360,
            currentBaseOffset.eulerRotation.z % 360);

        ApplyOffsetToStage();
    }


    public void ChangePosition(Vector3 offset)
    {
        currentBaseOffset.position = currentBaseOffset.position + offset;
        ApplyOffsetToStage();
    }


    public async void ReloadOffsetFromStorage()
    {

        (var additionalOffset, var message) = await CustomerModelShowCaseController.LoadRelativePositionAsync(trackedObjectData.ModelFileName.ToLower());
        currentBaseOffset = additionalOffset;
        SetCheckboxes();
        ResetOffsetSmallAdjuestments();
        ApplyCenterFixFromAOAService();
        ApplyOffsetToStage();
        ApplyWireframeDisplayAndLogBooleans();
        LogTextMesh.text = (((LogTextMesh.text?.Length ?? 0) > 1000) ? string.Empty : LogTextMesh.text) + @$"
Reloaded Offset From Storage 
additional  :{JsonUtility.ToJson(additionalOffset)}
message: {message} ";
    }

    private void ApplyWireframeDisplayAndLogBooleans()
    {
        LogTextMesh.gameObject.SetActive(currentBaseOffset.IsShowingModelLog);
        currentBaseOffset.IsShowingAsWireframe = !currentBaseOffset.IsShowingAsWireframe;
        ToggleWireframeDisplay();
    }

    private void SetCheckboxes()
    {
        CheckboxX.IsToggled = currentBaseOffset.correctionPositionDirection.x < 0;
        CheckboxY.IsToggled = currentBaseOffset.correctionPositionDirection.y < 0;
        CheckboxZ.IsToggled = currentBaseOffset.correctionPositionDirection.z < 0;
    }

    public async void SaveOffsetToStorage()
    {
        var offsetTosave = currentAdjustedOffset;
        var message = await CustomerModelShowCaseController.SaveRelativePositionAsync(trackedObjectData.ModelFileName.ToLower(), offsetTosave);
        LogTextMesh.text = (((LogTextMesh.text?.Length ?? 0) > 1000) ? string.Empty : LogTextMesh.text) + @$"
Saved Offset to Storage 
additional  :{JsonUtility.ToJson(offsetTosave)}
message: {message} ";
    }

    public void ToggleWireframeDisplay()
    {
        try
        {


            if (currentBaseOffset.IsShowingAsWireframe)
            {
                foreach (var item in MaterialBackups)
                {
                    item.renderer.materials = item.original;
                }
                currentBaseOffset.IsShowingAsWireframe = false;
                LogTextMesh.text = LogTextMesh.text + "\r\nIsShowingAsWireFrame = false;";
            }
            else
            {

                foreach (var item in MaterialBackups)
                {
                    item.renderer.materials = item.wiredNew;
                }
                currentBaseOffset.IsShowingAsWireframe = true;
                LogTextMesh.text = LogTextMesh.text + "\r\nIsShowingAsWireFrame = true;";


            }
        }
        catch (Exception ex)
        {
            LogTextMesh.text = LogTextMesh.text + $"\r\nerror:{ex}";


        }

    }

    public void ToggleLog()
    {
        LogTextMesh.gameObject.SetActive(!LogTextMesh.gameObject.activeSelf);
        currentBaseOffset.IsShowingModelLog = LogTextMesh.gameObject.activeSelf;
    }
}