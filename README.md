# Azure Object Anchors

Welcome to Azure Object Anchors. Azure Object Anchors enables an application to detect an object in the physical world using a 3D model and estimate its 6-DoF pose. The SDK enables a HoloLens application to load an object model, detect, and track instance(s) of that model in the physical world. For more information, see the [Azure Object Anchors documentation](https://docs.microsoft.com/azure/object-anchors).

## Contents

| File/folder          | Description                                 |
|-------------------   |---------------------------------------------|
| `quickstarts`        | Quickstart sample code.                     |
| `.gitattributes`     | Defines attributes for files stored in Git. |
| `CHANGELOG.md`       | List of changes to the sample.              |
| `CODE_OF_CONDUCT.md` | Microsoft Open Source Code of Conduct.      |
| `CONTRIBUTING.md`    | Guidelines for contributing to the sample.  |
| `LICENSE`            | The license for the sample.                 |
| `README.md`          | This README file.                           |
| `SECURITY.md`        | Microsoft Open Source Security Guidelines.  |

## Key concepts

* [SDK Overview](https://docs.microsoft.com/azure/object-anchors/concepts/sdk-overview)

## Quickstarts

The quickstart samples can be found in the `quickstarts` folder.

### Model Conversion

Learn how to use the Azure Object Anchors service to convert a 3D asset into an Azure Object Anchors model to be used in an app.

* [Create a model](https://docs.microsoft.com/azure/object-anchors/quickstarts/get-started-model-conversion)

### Apps

Learn how to use an Azure Object Anchors model in an app to detect physical objects. All prerequisites and instructions can be found in the documentation.

* [HoloLens with DirectX](https://docs.microsoft.com/azure/object-anchors/quickstarts/get-started-hololens-directx)
* [HoloLens with Unity](https://docs.microsoft.com/azure/object-anchors/quickstarts/get-started-unity-hololens)
* [HoloLens with Unity and MRTK](https://docs.microsoft.com/azure/object-anchors/quickstarts/get-started-unity-hololens-mrtk)

# HoloLens with Unity and MRTK enhancement includes parts below:

- A unity editor script that adds a menu item `Assets/Create AssetBundles for Runtime Loading`, which can generate assets in project (`.fbx`, `.obj` files) into `Asset Bundles`. Those `Asset Bundles` file can be loaded from `3D Object` folder in runtime. 

- A Prefeab/Controller suite allows user:
     - Display the `.model` Asset Bundle file when the same named `.ou`  AOA model was detected.
     - Use `Recalibration Panel` of the displayed model hologram to recalibrate potential position/rotation mislocating and save the relative offset-pose into a `.json` file with same name. The file will be automatically loaded next time. (The rotation was saved as Eular angle for manual editing friendly).
     

## How to create appbundles for .fbx files 
- Import you own `.fbx` files into `Assets/Models` folder.
-  Select each file you imported, perform following actions in `Inspector` <br/>![AssetBundle setting](https://docs.unity3d.com/530/Documentation/uploads/Main/AssetBundleInspectorNewBundle.png)
   -   Create an new `AssetBundle` name same as your `.fbx` file name. (`cattoy` for `cattoy.fbx`) and assign it to this file.
   -   Create an new extension name `model` in the dropbox on the right, and assign it to this file. 
-  In Unity main menu, select `Assets` -> `Create AssetBundles for Runtime Loading`.
-  In the popuped window, exam the text field `Target Directory` if it is your desired target, then click 'Okay'.
-  You can see the new `.model` files for each `.fbx` （other files are not necessary）. 
-  You can upload files to `3D Objects` folder in your HoloLens when you need. 



