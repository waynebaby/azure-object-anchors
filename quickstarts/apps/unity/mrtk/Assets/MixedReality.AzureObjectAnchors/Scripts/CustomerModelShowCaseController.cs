// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using Microsoft.Azure.ObjectAnchors.Unity.Sample;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;


public class CustomerModelShowCaseController : MonoBehaviour
{
    public static CustomerModelShowCaseController Instance { get; private set; }
    public GameObject[] preloadedDisplayModelPrefabs;
    public CustomerModelShowCase ShowCaseTemplate;
    public Shader replaceShader;
    public Material WireframeMaterial;


    public bool IsRecalibrationModeEnabled;
    private Dictionary<string, ((EulerPoseWithName pose, string message), GameObject gameObject)> loadedDisplayModelPrefabs = new Dictionary<string, ((EulerPoseWithName pose, string message), GameObject)>();
    private Dictionary<Guid, CustomerModelShowCase> loadedDisplayModelObjects = new Dictionary<Guid, CustomerModelShowCase>();
    public CustomerModelShowCaseController()
    {
        Instance = this;
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private async void Awake()
    {
        if (preloadedDisplayModelPrefabs != null)
        {
            loadedDisplayModelPrefabs.Clear();
            for (int i = 0; i < preloadedDisplayModelPrefabs.Length; i++)
            {
                var model = preloadedDisplayModelPrefabs[i];
                loadedDisplayModelPrefabs.Add(model.name.ToLower(), (await LoadRelativePositionAsync(model.name.ToLower()), model));
            }
        }
    }

    public void ResetDisplayModels()
    {
        var oldLloadedDisplayModelObjects = loadedDisplayModelObjects;
        loadedDisplayModelObjects = new Dictionary<Guid, CustomerModelShowCase>();
        foreach (var item in oldLloadedDisplayModelObjects.Values)
        {
            Destroy(item.gameObject);
        }
    }

    public void RemoveDisplayModel(Guid instanceId)
    {
        if (loadedDisplayModelObjects.TryGetValue(instanceId ,out var outputItem))
        {
            loadedDisplayModelObjects.Remove(instanceId);
            Destroy(outputItem.gameObject);
        }
    }
    public async void ShowObject(Vector3 position, Quaternion rotation, TrackedObjectData state)
    {
        var templateName = state.ModelFileName.ToLower();
        string loadAssetBundleMessage = string.Empty;
        var instanceId = state.InstanceId;
        CustomerModelShowCase loadedObject;
        if (!loadedDisplayModelObjects.TryGetValue(instanceId, out loadedObject))
        {
            loadedObject = GameObject.Instantiate(ShowCaseTemplate.gameObject).GetComponent<CustomerModelShowCase>();
            loadedDisplayModelObjects.Add(instanceId, loadedObject);
        }

        loadedObject.transform.position = position;
        loadedObject.RotationFixStage.transform.rotation = rotation;
        if (loadedObject.DisplayingModel == null)
        {
            loadedObject.gameObject.SetActive(false);
            var stageObject = loadedObject.CenterFixStage;
            EulerPoseWithName additionalOffset;
            string additionalOffsetMessage;
            GameObject createdDisplayModel = null;
            if (loadedDisplayModelPrefabs.TryGetValue(templateName.ToLower(), out var modelOffsetAndTemplate))
            {
                GameObject template;
                ((additionalOffset, additionalOffsetMessage), template) = modelOffsetAndTemplate;
                createdDisplayModel = GameObject.Instantiate(template, stageObject.transform);
                loadAssetBundleMessage = "using exist model in app";
            }
            else
            {
                (additionalOffset, additionalOffsetMessage) = await LoadRelativePositionAsync(templateName);
#if WINDOWS_UWP

                var storageFolder = Windows.Storage.KnownFolders.Objects3D;
                var path = storageFolder.Path;
                var assetbundlePath = Path.Combine(path, $"{templateName}.model");
                if (File.Exists(assetbundlePath))
                {

                    try
                    {
                        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(assetbundlePath);
                        var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
                        var memoryBytes = buffer.ToArray();

                        AssetBundle myLoadedAssetBundle = await LoadAssetBundleAsync(memoryBytes);

                        if (myLoadedAssetBundle == null)
                        {
                            loadAssetBundleMessage = @$"bundle: {assetbundlePath} load failed";

                        }
                        else
                        {
                            var prefab = await LoadAssetAsync(templateName, myLoadedAssetBundle);

                            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
                            foreach (var meshRenderer in meshRenderers)
                            {
                                foreach (var mat in meshRenderer.materials)
                                {
                                    meshRenderer.material.shader = replaceShader;
                                }
                            }
                            loadedDisplayModelPrefabs.Add(templateName.ToLower(), ((additionalOffset, additionalOffsetMessage), prefab));
                            createdDisplayModel = new GameObject();
                            createdDisplayModel.name = templateName;

                            var prefabInstance = Instantiate(prefab, createdDisplayModel.transform);
                            prefabInstance.SetActive(true);
                            loadAssetBundleMessage = $"bundle: {assetbundlePath} loaded with Name {prefabInstance.name}";
                            myLoadedAssetBundle.Unload(false);
                        }


                    }
                    catch (Exception ex)
                    {
                        var strex = ex.ToString();
                        loadAssetBundleMessage = $"exception: {ex.Message} {(strex.Length > 200 ? strex.Substring(0, 200) : strex)}";
                    }
                }

                else
                {
                    loadAssetBundleMessage = $"bundle: {assetbundlePath} notfound";
                }


#endif //WINDOWS_UWP
            }
            if (createdDisplayModel != null)
            {
                loadedObject.gameObject.SetActive(true);
                loadedObject.ApplyModelToStage(additionalOffset, createdDisplayModel, state,
             $@"ID:  {state.InstanceId},
Name:   {state.ModelFileName}
offset: {JsonUtility.ToJson(additionalOffset)}
loadasset: {loadAssetBundleMessage}
message: {additionalOffsetMessage}");
            }         
        }
        else
        {
            loadedObject.gameObject.SetActive(true);
        }
    }
    private static async Task<GameObject> LoadAssetAsync(string templateName, AssetBundle myLoadedAssetBundle)
    {
        var prefabSyncResult = myLoadedAssetBundle.LoadAssetAsync<GameObject>($"{templateName}");

        while (!prefabSyncResult.isDone)
        {
            await Task.Delay(100);
        }
        var prefab = prefabSyncResult.asset as GameObject;
        return prefab;
    }

    private static async Task<AssetBundle> LoadAssetBundleAsync(byte[] memoryBytes)
    {
        var myLoadedAssetBundleStub
                = AssetBundle.LoadFromMemoryAsync(memoryBytes);

        while (!myLoadedAssetBundleStub.isDone)
        {
            await Task.Delay(100);
        }
        var myLoadedAssetBundle = myLoadedAssetBundleStub.assetBundle;
        return myLoadedAssetBundle;
    }

    public static async Task<(EulerPoseWithName, string)> LoadRelativePositionAsync(string modelName)
    {
        //Load offset files
#if WINDOWS_UWP
        try
        {

            var folder = Windows.Storage.KnownFolders.Objects3D;
            var fileName = $"{modelName}.json";
            var jsonFile = await folder.GetFileAsync(fileName);
            if (jsonFile == null)
            {
                var message = $@"Pose file {modelName}.json in 3DObjects Folder not exists";
                return (new EulerPoseWithName { name = "default", position = Vector3.zero, eulerRotation = Vector3.zero }, message);

            }
            using (var jsonStream = await jsonFile.OpenStreamForReadAsync())
            {
                var jsonsr = new StreamReader(jsonStream);
                var json = await jsonsr.ReadToEndAsync();
                return (JsonUtility.FromJson<EulerPoseWithName>(json), $"read from file {fileName}");
            }
        }
        catch (Exception ex)
        {
            var message = $@"Failed to read pose file {modelName}.json in 3DObjects Folder:
returning default identity
{ex} ";
            Debug.Log(message);
            return (new EulerPoseWithName { name = "default", position = Vector3.zero, eulerRotation = Vector3.zero }, message);
        }
#else //WINDOWS_UWP
        await Task.CompletedTask;
        return (new EulerPoseWithName { name = modelName, position = Vector3.zero, eulerRotation = Vector3.zero }, modelName);
#endif //WINDOWS_UWP

    }

    public static async Task<string> SaveRelativePositionAsync(string modelName, EulerPoseWithName pose)
    {
        //Load offset files
#if WINDOWS_UWP
        try
        {

            var folder = Windows.Storage.KnownFolders.Objects3D;
            var fileName = $"{modelName}.json";

            var jsonFile = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            using (var jsonStream = await jsonFile.OpenStreamForWriteAsync())
            {
                var jsonsw = new StreamWriter(jsonStream);
                await jsonsw.WriteAsync(JsonUtility.ToJson(pose));
                await jsonsw.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            var message = $@"Failed to read pose file {modelName}.json in 3DObjects Folder:
returning default identity
{ex} ";
            Debug.Log(message);
            return message;
        }
#else //WINDOWS_UWP
        await Task.CompletedTask;
#endif //WINDOWS_UWP
        return "okay";
    }
}
