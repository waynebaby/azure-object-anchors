using UnityEditor;
using System.IO;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;

public class CreateAssetBundles : EditorWindow
{
    [MenuItem("Assets/Create AssetBundles for Runtime Loading")]
    static void BuildAllAssetBundles()
    {

        CreateAssetBundles window = GetWindow<CreateAssetBundles>();
        window.minSize = window.maxSize = new Vector2(360, 170);
        window.titleContent.text = "Create AssetBundles";
        window.ShowModalUtility();
    }

    string inputText = @"AssetBundles";
    void OnGUI()
    {
        EditorGUILayout.HelpBox(@"Before you start your generation, please set each of your model files in different assetbundle, with ""modelname""\""model"" pattern.", MessageType.Warning, true);
        EditorGUILayout.HelpBox(@"If you have a model file with name ""a.fbx"", please set the assetbundle name as ""a""\""model"", and there will be a ""a.model"" file will be generated by clicking ""Okay"".", MessageType.Info, true);
        inputText = EditorGUILayout.TextField("Target Directory", inputText);
        GUILayout.Space(10);
        if (GUILayout.Button("Okay"))
        {
            BuildAllBundles(inputText); 
            Close();
        } 
        else if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    void BuildAllBundles(string assetBundleDirectory)
    {
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                          BuildAssetBundleOptions.DisableWriteTypeTree,
                                          BuildTarget.WSAPlayer);

    }
}
