#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering;
using UnityEditor;

namespace NeonCubeStudio
{
    public class GenerateScene : MonoBehaviour
    {
        [MenuItem("Tools/Generate scene")]
        public static void GenerateNewScene()
        {
            // delete all old objects
            GameObject[] GameObjects = (FindObjectsOfType<GameObject>() as GameObject[]);

            for (int i = 0; i < GameObjects.Length; i++)
            {
                DestroyImmediate(GameObjects[i]);
            }

            // root
            GameObject _root = new GameObject("Root");

            // dynamic
            GameObject _dynamic = new GameObject("Dynamic");
            _dynamic.transform.parent = _root.transform;

            // static
            GameObject _static = new GameObject("Static");
            _static.transform.parent = _root.transform;

            // GUI
            GameObject _GUI = new GameObject("GUI");
            _GUI.transform.parent = _root.transform;

#region dynamic
            // objects
            GameObject _objectsDynamic = new GameObject("Objects");
            _objectsDynamic.transform.parent = _dynamic.transform;

            // camera
            GameObject _objectsDynamicCameraPivot = new GameObject("Camera pivot");
            _objectsDynamicCameraPivot.transform.parent = _objectsDynamic.transform;
            _objectsDynamicCameraPivot.transform.position = new Vector3(0, 1, -10);

            GameObject _objectsDynamicCameraMain = new GameObject("Main Camera");
            _objectsDynamicCameraMain.transform.parent = _objectsDynamicCameraPivot.transform;
            _objectsDynamicCameraMain.transform.position = _objectsDynamicCameraPivot.transform.position;
            _objectsDynamicCameraMain.tag = "MainCamera";
            _objectsDynamicCameraMain.AddComponent<AudioListener>();
            Camera _mainCamera = _objectsDynamicCameraMain.AddComponent<Camera>();
            _mainCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            _mainCamera.farClipPlane = 500.0f;
            _mainCamera.allowMSAA = false;
            PostProcessingBehaviour _mainCameraPost = _objectsDynamicCameraMain.AddComponent<PostProcessingBehaviour>();
            _mainCameraPost.profile = (PostProcessingProfile)AssetDatabase.LoadAssetAtPath("Assets/Profiles/Default Post-Processing Profile.asset", typeof(PostProcessingProfile));

            // lights
            GameObject _lightsDynamic = new GameObject("Lights");
            _lightsDynamic.transform.parent = _dynamic.transform;

            // light types
            GameObject _lightsDynamicDirectional = new GameObject("Directional");
            _lightsDynamicDirectional.transform.parent = _lightsDynamic.transform;

            GameObject _lightsDynamicPoint = new GameObject("Point");
            _lightsDynamicPoint.transform.parent = _lightsDynamic.transform;

            GameObject _lightsDynamicSpot = new GameObject("Spot");
            _lightsDynamicSpot.transform.parent = _lightsDynamic.transform;

            GameObject _lightsDynamicArea = new GameObject("Area");
            _lightsDynamicArea.transform.parent = _lightsDynamic.transform;

            // probes
            GameObject _probesDynamic = new GameObject("Probes");
            _probesDynamic.transform.parent = _dynamic.transform;

            // probe types
            GameObject _probesDynamicLight = new GameObject("Light");
            _probesDynamicLight.transform.parent = _probesDynamic.transform;

            GameObject _probesDynamicReflection = new GameObject("Reflection");
            _probesDynamicReflection.transform.parent = _probesDynamic.transform;

            // Audio
            GameObject _audioDynamic = new GameObject("Audio");
            _audioDynamic.transform.parent = _dynamic.transform;

            // Audio types
            GameObject _audioDynamicAmbient = new GameObject("Ambient");
            _audioDynamicAmbient.transform.parent = _audioDynamic.transform;

            GameObject _audioDynamicMusic = new GameObject("Music");
            _audioDynamicMusic.transform.parent = _audioDynamic.transform;
#endregion

#region static
            // objects
            GameObject _objectsStatic = new GameObject("Objects");
            _objectsStatic.transform.parent = _static.transform;

            // lights
            GameObject _lightsStatic = new GameObject("Lights");
            _lightsStatic.transform.parent = _static.transform;

            // light types
            GameObject _lightsStaticDirectional = new GameObject("Directional");
            _lightsStaticDirectional.transform.parent = _lightsStatic.transform;

            GameObject _lightsStaticDirectionalSun = new GameObject("Sun");
            _lightsStaticDirectionalSun.transform.parent = _lightsStaticDirectional.transform;
            _lightsStaticDirectionalSun.transform.position = new Vector3(0, 3, 0);
            _lightsStaticDirectionalSun.transform.rotation = Quaternion.Euler(50, -30, 0);
            Light _sunLight = _lightsStaticDirectionalSun.AddComponent<Light>();
            _sunLight.lightmapBakeType = LightmapBakeType.Baked;
            _sunLight.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            _sunLight.type = LightType.Directional;
            _sunLight.intensity = 1.0f;

            GameObject _lightsStaticPoint = new GameObject("Point");
            _lightsStaticPoint.transform.parent = _lightsStatic.transform;

            GameObject _lightsStaticSpot = new GameObject("Spot");
            _lightsStaticSpot.transform.parent = _lightsStatic.transform;

            GameObject _lightsStaticArea = new GameObject("Area");
            _lightsStaticArea.transform.parent = _lightsStatic.transform;

            // probes
            GameObject _probesStatic = new GameObject("Probes");
            _probesStatic.transform.parent = _static.transform;

            // probe types
            GameObject _probesStaticLight = new GameObject("Light");
            _probesStaticLight.transform.parent = _probesStatic.transform;

            GameObject _probesStaticReflection = new GameObject("Reflection");
            _probesStaticReflection.transform.parent = _probesStatic.transform;
#endregion

#region Scene settings
            RenderSettings.sun = _sunLight;
            RenderSettings.defaultReflectionResolution = 2048;

            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            Lightmapping.ClearLightingDataAsset();
            Lightmapping.realtimeGI = true;
            Lightmapping.bakedGI = true;

            LightmapEditorSettings.reflectionCubemapCompression = ReflectionCubemapCompression.Uncompressed;
            LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
            LightmapEditorSettings.textureCompression = false;
            LightmapEditorSettings.realtimeResolution = 8.0f;
            LightmapEditorSettings.padding = 4;
#if UNITY_2018_1_OR_NEWER
            LightmapEditorSettings.maxAtlasSize = 2048;
#else
            LightmapEditorSettings.maxAtlasWidth = 2048;
            LightmapEditorSettings.maxAtlasHeight = 2048;
#endif
            LightmapEditorSettings.sampling = LightmapEditorSettings.Sampling.Auto;
            LightmapEditorSettings.enableAmbientOcclusion = true;
            LightmapEditorSettings.aoExponentDirect = 1.0f;
#endregion
        }
    }
}
#endif