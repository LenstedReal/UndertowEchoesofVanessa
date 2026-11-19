#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Undertow.Core;
using Undertow.Player;
using Undertow.Lantern;
using Undertow.Sanity;
using Undertow.Audio;
using Undertow.Puzzles;
using Undertow.Narrative;
using Undertow.UI;
using Undertow.World;

namespace Undertow.Editor
{
    /// <summary>Builds deterministic, playable greybox environments from the project's authored meshes.
    /// This is an environment blockout tool, not a substitute for a final art pass.</summary>
    public static class UndertowHDRPSceneBuilder
    {
        private const string SceneDir = "Assets/_Project/Scenes/";
        private const string MaterialDir = "Assets/_Project/Materials/Generated/";

        [MenuItem("Undertow/Build All Canonical Scenes & Prefabs")]
        public static void BuildAllScenes()
        {
            EnsureFolders();
            UndertowAssetImportQuality.ConfigureAllProjectAssets();
            UndertowUHDConfiguration.ApplyUHDPlayerProfile();
            BuildMainMenuScene();
            BuildActIMercifulRequiemVerticalSlice();
            BuildActIIShipGraveyardScene();
            BuildActIIIForsakenPortScene();
            BuildActIVSunkenTempleScene();
            BuildEndingsResolutionScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Undertow] Six blockout scenes generated. These are playable greybox foundations and still require final art dressing.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials")) AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            if (!AssetDatabase.IsValidFolder(MaterialDir.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets/_Project/Materials", "Generated");
        }

        private static Material Mat(string name, Color color, float metallic = 0f, float smoothness = 0.25f, bool emission = false)
        {
            string path = MaterialDir + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("HDRP/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissiveColor")) material.SetColor("_EmissiveColor", color * 3.5f);
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * 3.5f);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyPbrSet(Material material, string prefix)
        {
            Texture albedo = AssetDatabase.LoadAssetAtPath<Texture>("Assets/_Project/Textures/" + prefix + "_Albedo.png");
            Texture normal = AssetDatabase.LoadAssetAtPath<Texture>("Assets/_Project/Textures/" + prefix + "_Normal.png");
            Texture mask = AssetDatabase.LoadAssetAtPath<Texture>("Assets/_Project/Textures/" + prefix + "_Mask.png");
            if (albedo != null && material.HasProperty("_BaseColorMap")) material.SetTexture("_BaseColorMap", albedo);
            if (normal != null && material.HasProperty("_NormalMap")) { material.SetTexture("_NormalMap", normal); material.EnableKeyword("_NORMALMAP"); }
            if (mask != null && material.HasProperty("_MaskMap")) { material.SetTexture("_MaskMap", mask); material.EnableKeyword("_MASKMAP"); }
            if (material.HasProperty("_NormalScale")) material.SetFloat("_NormalScale", 1.15f);
            EditorUtility.SetDirty(material);
        }

        private static Material OceanMaterial(bool underwater)
        {
            string path = MaterialDir + (underwater ? "MAT_Underwater_DeepWater.mat" : "MAT_Storm_Ocean_4K.mat");
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                Shader shader = Shader.Find("HDRP/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                m = new Material(shader) { name = underwater ? "MAT_Underwater_DeepWater" : "MAT_Storm_Ocean_4K" };
                AssetDatabase.CreateAsset(m, path);
            }
            Color water = underwater ? new Color(0.008f, 0.105f, 0.14f, 0.94f) : new Color(0.008f, 0.042f, 0.058f, 0.96f);
            ApplyPbrSet(m, underwater ? "T_UnderwaterWater" : "T_StormOcean");
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", water);
            if (m.HasProperty("_Color")) m.SetColor("_Color", water);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", underwater ? 0.22f : 0.34f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.96f);
            if (m.HasProperty("_SurfaceType")) m.SetFloat("_SurfaceType", 0f); // opaque surface; HDRP SSR supplies reflections
            m.renderQueue = (int)RenderQueue.Geometry;
            EditorUtility.SetDirty(m);
            return m;
        }

        private static GameObject CreateOceanSurface(Transform parent, Vector3 position, float size, bool underwater)
        {
            GameObject ocean = new GameObject(underwater ? "HDRP_Underwater_Ripple_Surface" : "HDRP_Storm_Ocean_Surface");
            ocean.transform.SetParent(parent, false);
            ocean.transform.position = position;
            MeshFilter filter = ocean.AddComponent<MeshFilter>();
            MeshRenderer renderer = ocean.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = OceanMaterial(underwater);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            UndertowOceanSurface waves = ocean.AddComponent<UndertowOceanSurface>();
            SerializedObject so = new SerializedObject(waves);
            so.FindProperty("resolution").intValue = underwater ? 132 : 180;
            so.FindProperty("size").floatValue = size;
            so.FindProperty("waveAmplitude").floatValue = underwater ? 0.10f : 0.36f;
            so.FindProperty("waveSpeed").floatValue = underwater ? 0.42f : 1.0f;
            so.FindProperty("updateRate").floatValue = 30f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return ocean;
        }

        private static GameObject AddViewModelMesh(string fileName, string objectName, Transform parent, Vector3 localPosition, Vector3 localEuler, Vector3 localScale, Material material)
        {
            string path = "Assets/_Project/Art/Meshes/" + fileName + ".obj";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Mesh mesh = null;
            for (int i = 0; i < assets.Length; i++) if (assets[i] is Mesh) { mesh = (Mesh)assets[i]; break; }
            if (mesh == null) return null;
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localEulerAngles = localEuler;
            go.transform.localScale = localScale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject Sphere(string name, Vector3 position, Vector3 scale, Material material, Transform parent = null, bool collider = false)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name; go.transform.position = position; go.transform.localScale = scale;
            if (parent != null) go.transform.SetParent(parent, true);
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.isStatic = true;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material, Transform parent = null, bool collider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            if (parent != null) go.transform.SetParent(parent, true);
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.isStatic = true;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject Cylinder(string name, Vector3 position, Vector3 scale, Material material, Transform parent = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            if (parent != null) go.transform.SetParent(parent, true);
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.isStatic = true;
            return go;
        }

        private static GameObject AddImportedMesh(string fileName, string objectName, Vector3 position, Vector3 scale, Material material, Transform parent = null)
        {
            string path = "Assets/_Project/Art/Meshes/" + fileName + ".obj";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Mesh mesh = null;
            for (int i = 0; i < assets.Length; i++) if (assets[i] is Mesh) { mesh = (Mesh)assets[i]; break; }
            if (mesh == null) { Debug.LogWarning("[Undertow] Mesh not imported: " + path); return null; }
            GameObject go = new GameObject(objectName);
            go.transform.position = position;
            go.transform.localScale = scale;
            if (parent != null) go.transform.SetParent(parent, true);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            go.isStatic = true;
            if (mesh.isReadable) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            return go;
        }

        private static void ConfigureTemporalAA(Camera camera)
        {
            if (camera == null) return;
            HDAdditionalCameraData additional = camera.GetComponent<HDAdditionalCameraData>();
            if (additional == null) additional = camera.gameObject.AddComponent<HDAdditionalCameraData>();
            // TAA is selected explicitly for the high-resolution PC target; temporal
            // accumulation is substantially more suitable than MSAA for HDRP deferred.
            additional.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
        }

        private static void AddPostProcessing(string profileName, float exposure, float contrast, float saturation, float vignetteAmount, float chromaticAmount)
        {
            string profilePath = "Assets/_Project/Settings/HDRP/" + profileName + ".asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = profileName;
                AssetDatabase.CreateAsset(profile, profilePath);
            }
            ColorAdjustments color = profile.components.Find(c => c is ColorAdjustments) as ColorAdjustments;
            if (color == null) color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(exposure);
            color.contrast.Override(contrast);
            color.saturation.Override(saturation);
            Vignette vignette = profile.components.Find(c => c is Vignette) as Vignette;
            if (vignette == null) vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(vignetteAmount);
            vignette.smoothness.Override(0.48f);
            ChromaticAberration chromatic = profile.components.Find(c => c is ChromaticAberration) as ChromaticAberration;
            if (chromatic == null) chromatic = profile.Add<ChromaticAberration>(true);
            chromatic.intensity.Override(chromaticAmount);
            // HDRP Fog override is added by reflection so the editor utility remains compatible
            // with minor HDRP patch versions while retaining a serialized volumetric profile.
            System.Type fogType = typeof(Fog);
            if (fogType != null)
            {
                VolumeComponent fogComponent = profile.components.Find(c => c.GetType() == fogType);
                if (fogComponent == null) fogComponent = profile.Add(fogType, true);
                if (fogComponent != null)
                {
                    SerializedObject fogSerialized = new SerializedObject(fogComponent);
                    SetVolumeFloat(fogSerialized, "meanFreePath", 32f);
                    SetVolumeFloat(fogSerialized, "baseHeight", -2f);
                    SetVolumeFloat(fogSerialized, "maximumHeight", 24f);
                    SetVolumeFloat(fogSerialized, "anisotropy", 0.62f);
                    SetVolumeBool(fogSerialized, "enableVolumetricFog", true);
                    fogSerialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            GameObject volumeObject = new GameObject("Global_HDRP_PostProcessing");
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(profile);
        }

        private static void SetVolumeFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty p = serialized.FindProperty(propertyName);
            if (p == null) return;
            SerializedProperty v = p.FindPropertyRelative("m_Value");
            if (v != null && v.propertyType == SerializedPropertyType.Float) v.floatValue = value;
        }
        private static void SetVolumeBool(SerializedObject serialized, string propertyName, bool value)
        {
            SerializedProperty p = serialized.FindProperty(propertyName);
            if (p == null) return;
            SerializedProperty v = p.FindPropertyRelative("m_OverrideState");
            if (v != null) v.boolValue = true;
            SerializedProperty inner = p.FindPropertyRelative("m_Value");
            if (inner != null && inner.propertyType == SerializedPropertyType.Boolean) inner.boolValue = value;
        }

        private static void AddLighting(Color ambient, Color fog, float fogDensity, Vector3 playerPosition)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = fogDensity;
            GameObject moon = new GameObject("Cold_Moon_Key_Light");
            moon.transform.rotation = Quaternion.Euler(32f, -128f, 0f);
            Light directional = moon.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.color = new Color(0.43f, 0.68f, 0.86f);
            directional.intensity = 1.25f;
            directional.shadows = LightShadows.Soft;
            AddPostProcessing("Undertow_HDRP_Global_Volume", -0.3f, 18f, -22f, 0.28f, 0.12f);
            GameObject fill = new GameObject("Warm_Amber_Practical_Fill");
            fill.transform.position = playerPosition + new Vector3(1.4f, 2.2f, 2.0f);
            Light point = fill.AddComponent<Light>();
            point.type = LightType.Point;
            point.color = new Color(1f, 0.42f, 0.12f);
            point.intensity = 32f;
            point.range = 8f;
            point.shadows = LightShadows.Soft;
        }

        public static void BuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject campaign = new GameObject("CampaignState_Persistent");
            campaign.AddComponent<ActProgressionManager>();
            GameObject root = new GameObject("MainMenu_Root");
            root.AddComponent<MainMenuAndPauseController>();
            root.AddComponent<UndertowMainMenuHUD>();
            root.AddComponent<GraphicsAndAudioSettingsManager>();
            root.AddComponent<DialogueSubtitleSystem>();
            GameObject cam = new GameObject("MainMenu_Camera");
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0, 1.6f, -7f);
            cam.transform.rotation = Quaternion.identity;
            Camera menuCamera = cam.AddComponent<Camera>();
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            ConfigureTemporalAA(menuCamera);
            cam.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(scene, SceneDir + "MainMenu.unity");
        }

        public static void BuildActIMercifulRequiemVerticalSlice()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureFolders();
            Material deck = Mat("MAT_Blockout_WetDeck", new Color(0.105f, 0.075f, 0.052f), 0.18f, 0.72f);
            Material edge = Mat("MAT_Blockout_TarredHull", new Color(0.025f, 0.045f, 0.055f), 0.12f, 0.35f);
            Material brass = Mat("MAT_Blockout_AgedBrass", new Color(0.36f, 0.22f, 0.075f), 0.78f, 0.62f);
            Material sail = Mat("MAT_Blockout_TornSail", new Color(0.035f, 0.045f, 0.052f), 0.02f, 0.18f);
            Material warm = Mat("MAT_Blockout_Flame", new Color(1f, 0.31f, 0.055f), 0f, 0.9f, true);
            Material accent = Mat("MAT_Blockout_Accent", new Color(0.08f, 0.34f, 0.39f), 0.45f, 0.72f, true);
            Material stone = Mat("MAT_Blockout_Stone", new Color(0.12f, 0.15f, 0.17f), 0.04f, 0.24f);
            ApplyPbrSet(deck, "T_WetAgedDeckWood");
            ApplyPbrSet(brass, "T_AntiqueBrass");
            ApplyPbrSet(sail, "T_TornBlackSail");
            ApplyPbrSet(stone, "T_SunkenTempleStone");

            AddLighting(new Color(0.035f, 0.055f, 0.075f), new Color(0.015f, 0.045f, 0.065f), 0.012f, new Vector3(0, 1.6f, -14));
            GameObject world = new GameObject("Galleon_MercifulRequiem_Blockout");
            // Large hull silhouette beneath the deck so the player feels aboard a ship, not a floating platform.
            AddImportedMesh("SM_GalleonHullDeck", "Merciful_Requiem_Hull_Hero", new Vector3(0f, -1.55f, 0f), new Vector3(1.0f, 0.72f, 1.18f), edge, world.transform);
            CreateOceanSurface(world.transform, new Vector3(0f, -1.55f, 0f), 260f, false);
            // Distant moon disk and rock silhouettes establish the reference image's horizon scale.
            Material moonMat = Mat("MAT_Cold_Moon_Emissive", new Color(0.56f, 0.82f, 0.96f), 0f, 0.95f, true);
            Sphere("Full_Moon_Disc", new Vector3(-52f, 34f, 112f), new Vector3(9f, 9f, 1.2f), moonMat, world.transform, false);
            for (int rock = 0; rock < 9; rock++)
            {
                float side = rock % 2 == 0 ? -1f : 1f;
                float z = 36f + rock * 13f;
                Sphere("Distant_Sea_Stack_" + rock, new Vector3(side * (22f + (rock % 3) * 8f), -0.2f + (rock % 4) * 1.1f, z), new Vector3(4.5f + rock % 3, 3.5f + (rock % 4), 5.5f + rock % 2), edge, world.transform, false);
            }
            // Continuous walkable deck with individual raised planks for visible scale and collision.
            Cube("Deck_Collision_Base", new Vector3(0, -0.25f, 0), new Vector3(15f, 0.5f, 46f), deck, world.transform);
            for (int z = -22; z <= 22; z++)
                for (int x = -7; x <= 7; x++)
                    Cube("Deck_Plank", new Vector3(x * 0.96f, 0.015f, z * 0.98f), new Vector3(0.92f, 0.055f, 0.94f), deck, world.transform, false);
            Cube("Port_Hull_Bulwark", new Vector3(-7.55f, 1.35f, 0), new Vector3(0.65f, 2.8f, 46f), edge, world.transform);
            Cube("Starboard_Hull_Bulwark", new Vector3(7.55f, 1.35f, 0), new Vector3(0.65f, 2.8f, 46f), edge, world.transform);
            for (int z = -21; z <= 21; z += 3)
            {
                Cylinder("Port_Rail_Post", new Vector3(-7.1f, 2.2f, z), new Vector3(0.12f, 0.18f, 0.12f), brass, world.transform);
                Cylinder("Starboard_Rail_Post", new Vector3(7.1f, 2.2f, z), new Vector3(0.12f, 0.18f, 0.12f), brass, world.transform);
            }
            Cube("Port_Rail_Top", new Vector3(-7.1f, 2.85f, 0), new Vector3(0.22f, 0.18f, 46f), deck, world.transform);
            Cube("Starboard_Rail_Top", new Vector3(7.1f, 2.85f, 0), new Vector3(0.22f, 0.18f, 46f), deck, world.transform);
            for (int rig=0; rig<7; rig++)
            {
                float x=(rig%2==0?-1f:1f)*(4.2f+(rig%3)*.7f); float z=-17f+rig*5.1f;
                Cylinder("Galleon_Rigging_Shroud",new Vector3(x*.5f,5.2f,z),new Vector3(.055f,3.2f,.055f),brass,world.transform).transform.rotation=Quaternion.Euler(0f,0f,x<0?22f:-22f);
            }
            for (int cannon=0;cannon<4;cannon++)
            {
                float side=cannon%2==0?-1f:1f; float z=-13f+cannon*8f;
                Cylinder("Deck_Cannon_Barrel",new Vector3(side*5.9f,.86f,z),new Vector3(.34f,.34f,1.15f),edge,world.transform).transform.rotation=Quaternion.Euler(90f,0f,0f);
                Cylinder("Deck_Cannon_Muzzle_Ring",new Vector3(side*5.9f,1.22f,z+1.05f),new Vector3(.40f,.08f,.40f),brass,world.transform);
            }
            Cylinder("Mainmast", new Vector3(0, 7.1f, 2.5f), new Vector3(0.72f, 7.1f, 0.72f), edge, world.transform);
            Cube("Mainmast_Crossyard", new Vector3(0, 10.8f, 2.5f), new Vector3(15f, 0.18f, 0.22f), deck, world.transform);
            Cube("Torn_Main_Sail", new Vector3(0, 8.7f, 2.8f), new Vector3(10.5f, 5.1f, 0.12f), sail, world.transform, false);
            GameObject mainSail = Cube("Forward_Sail", new Vector3(0, 6.3f, 12f), new Vector3(8.5f, 3.2f, 0.1f), sail, world.transform, false);
            world.transform.Find("Torn_Main_Sail")?.gameObject.AddComponent<UndertowSailSway>();
            mainSail.AddComponent<UndertowSailSway>();
            AddImportedMesh("SM_GalleonMastAndSails", "Galleon_Rigging_Hero_Mesh", new Vector3(0f, 0.45f, 2.8f), new Vector3(0.88f, 0.92f, 0.88f), sail, world.transform);
            // Rope, barrel, winch and wet deck clutter break up the primitive repetition.
            for (int i = 0; i < 10; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * Random.Range(4.8f, 6.5f);
                float z = -18f + i * 3.8f;
                Cylinder("Deck_Rope_Coil_" + i, new Vector3(x, 0.12f, z), new Vector3(0.55f, 0.08f, 0.55f), brass, world.transform);
                Cylinder("Deck_Barrel_" + i, new Vector3(x * 0.72f, 0.52f, z + 1.05f), new Vector3(0.40f, 0.52f, 0.40f), deck, world.transform);
            }
            // Raised forecastle and aft quarter deck make the deck route readable.
            Cube("Aft_Quarterdeck", new Vector3(0, 1.1f, -16.8f), new Vector3(11.5f, 0.45f, 8.5f), deck, world.transform);
            Cube("Forecastle", new Vector3(0, 0.75f, 17.2f), new Vector3(10.5f, 0.35f, 5.5f), deck, world.transform);
            // Cabin blockout along port side; door openings remain clear toward the central aisle.
            for (int i = 0; i < 4; i++)
            {
                float z = -12f + i * 7.2f;
                Cube("Cabin_Port_Wall_" + i, new Vector3(-5.3f, 1.45f, z), new Vector3(3.5f, 2.9f, 0.28f), edge, world.transform);
                Cube("Cabin_Interior_Back_" + i, new Vector3(-6.9f, 1.45f, z + 2.5f), new Vector3(0.25f, 2.9f, 5.0f), edge, world.transform);
                Cube("Cabin_Roof_" + i, new Vector3(-5.3f, 3.0f, z + 1.25f), new Vector3(3.5f, 0.25f, 5.0f), deck, world.transform);
            }
            // Cargo hatch, rings, and two moveable-looking crate props.
            Cube("Cargo_Hatch_Frame", new Vector3(2.7f, 0.08f, 15f), new Vector3(4.0f, 0.16f, 4.2f), brass, world.transform, false);
            GameObject hatch = Cube("Cargo_Hatch_Door", new Vector3(2.7f, 0.02f, 15f), new Vector3(3.55f, 0.12f, 3.75f), edge, world.transform);
            Cube("Cargo_Hatch_Dark_Opening", new Vector3(2.7f, -0.18f, 15f), new Vector3(3.45f, 0.08f, 3.65f), edge, world.transform, false);
            GameObject ringA = AddImportedMesh("SM_StarboardDeckRing", "Starboard_Ring_A", new Vector3(5.1f, 0.12f, 13.8f), Vector3.one * 0.85f, brass, world.transform);
            GameObject ringB = AddImportedMesh("SM_StarboardDeckRing", "Starboard_Ring_B", new Vector3(5.1f, 0.12f, 16.0f), Vector3.one * 0.85f, brass, world.transform);
            GameObject crateA = Cube("Cargo_Crate_A", new Vector3(1.4f, 0.65f, 8.5f), new Vector3(1.25f, 1.3f, 1.25f), deck, world.transform);
            GameObject crateB = Cube("Cargo_Crate_B", new Vector3(3.0f, 0.65f, 8.5f), new Vector3(1.25f, 1.3f, 1.25f), deck, world.transform);
            // Five bell stands in a clearly visible interaction row.
            GameObject puzzleRoot = new GameObject("ActI_GameplayPuzzles");
            puzzleRoot.transform.SetParent(world.transform, false);
            puzzleRoot.transform.position = new Vector3(0, 0, -3f);
            puzzleRoot.AddComponent<FiveBellPuzzle>();
            CrateRingPuzzle cratePuzzle = puzzleRoot.AddComponent<CrateRingPuzzle>();
            cratePuzzle.ConfigureForAct(GameAct.ActI_CursedGalleon);
            BellNote[] notes = { BellNote.La, BellNote.Mi, BellNote.Do, BellNote.Mi, BellNote.La };
            GameObject[] bellObjects = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                float x = -4f + i * 2f;
                Cylinder("Bell_Stand_" + i, new Vector3(x, 0.65f, -3f), new Vector3(0.16f, 0.65f, 0.16f), brass, world.transform);
                bellObjects[i] = AddImportedMesh("SM_ShipBell", "Bell_Interactable_" + i, new Vector3(x, 1.35f, -3f), Vector3.one * 0.65f, brass, world.transform);
                if (bellObjects[i] != null)
                {
                    BellStrikeInteractable bell = bellObjects[i].AddComponent<BellStrikeInteractable>();
                    bell.Configure(puzzleRoot.GetComponent<FiveBellPuzzle>(), notes[i]);
                }
            }
            CargoHatchInteractable hatchUse = hatch.AddComponent<CargoHatchInteractable>(); hatchUse.Configure(cratePuzzle);
            if (crateA != null) { CrateRingInteractable use = crateA.AddComponent<CrateRingInteractable>(); use.Configure(cratePuzzle, 0, new Vector3(5.1f, 0.65f, 13.8f)); }
            if (crateB != null) { CrateRingInteractable use = crateB.AddComponent<CrateRingInteractable>(); use.Configure(cratePuzzle, 1, new Vector3(5.1f, 0.65f, 16.0f)); }
            // Crates are initially placed in the aisle; ring triggers remain explicit interaction targets.
            GameObject hook = AddImportedMesh("SM_SurgeonsHook", "Surgeons_Hook_Pickup", new Vector3(-5.2f, 1.1f, 3f), Vector3.one * 0.75f, brass, world.transform);
            if (hook != null) { SurgeonsHookPickup pickup = hook.AddComponent<SurgeonsHookPickup>(); pickup.Configure(cratePuzzle); }
            GameObject oil = Cylinder("Whale_Oil_Flask_Pickup", new Vector3(4.8f, 0.55f, -9f), new Vector3(0.22f, 0.5f, 0.22f), warm, world.transform);
            LanternFuelPickup oilPickup = oil.AddComponent<LanternFuelPickup>();
            AddImportedMesh("SM_AntiqueBrassLantern", "Deck_Lantern_Prop", new Vector3(4.8f, 0.8f, -9f), Vector3.one * 0.8f, brass, world.transform);
            AddImportedMesh("SM_VanessaMusicBox", "Vanessa_Music_Box_Prop", new Vector3(-5.2f, 1.0f, -11f), Vector3.one * 0.8f, brass, world.transform);
            // Two story drawings are placed on the Act I route; later drawings live in Acts II-IV.
            GameObject drawing1 = Cube("Vanessa_Drawing_01_Galleon", new Vector3(-5.2f, 1.65f, -11f), new Vector3(0.34f, 0.42f, 0.025f), warm, world.transform, true);
            drawing1.AddComponent<StoryFlagPickup>().Configure("DRAWING_01_GALLEON", true);
            GameObject drawing2 = Cube("Vanessa_Drawing_02_Bells", new Vector3(4.4f, 1.25f, -1.8f), new Vector3(0.38f, 0.46f, 0.025f), warm, world.transform, true);
            drawing2.AddComponent<StoryFlagPickup>().Configure("DRAWING_02_BELLS", true);
            GameObject actExit = Cube("ActI_Fog_Gate_Exit", new Vector3(0f, 2.4f, 22.1f), new Vector3(3.2f, 4.5f, 0.45f), accent, world.transform, true);
            actExit.AddComponent<ActExitInteractable>();
            GameObject storm = new GameObject("Storm_Lightning_Directional");
            Light stormLight = storm.AddComponent<Light>(); stormLight.type = LightType.Directional; stormLight.color = new Color(0.54f, 0.72f, 0.94f); stormLight.intensity = 0.02f; stormLight.shadows = LightShadows.None;
            storm.AddComponent<UndertowStormFlash>();
            // Player and systems.
            GameObject systems = new GameObject("_UndertowSystems");
            GameObject campaignState = new GameObject("CampaignState_Persistent");
            campaignState.AddComponent<ActProgressionManager>();
            HiddenSanitySystem sanity = systems.AddComponent<HiddenSanitySystem>();
            HDRPEnvironmentDistortion distortion = systems.AddComponent<HDRPEnvironmentDistortion>();
            systems.AddComponent<EchoMimicVoiceDirector>();
            systems.AddComponent<ListenMechanicController>();
            systems.AddComponent<SpatialAudioFallbackManager>();
            systems.AddComponent<UndertowProceduralAmbience>();
            systems.AddComponent<ObjectiveJournalSystem>();
            systems.AddComponent<DialogueSubtitleSystem>();
            systems.AddComponent<GraphicsAndAudioSettingsManager>();
            systems.AddComponent<UndertowCampaignAutosave>();
            MainMenuAndPauseController pauseController = systems.AddComponent<MainMenuAndPauseController>();
            UndertowPauseHUD pauseHud = systems.AddComponent<UndertowPauseHUD>();
            SerializedObject pauseSO = new SerializedObject(pauseHud);
            pauseSO.FindProperty("pauseController").objectReferenceValue = pauseController;
            pauseSO.ApplyModifiedPropertiesWithoutUndo();
            GameObject player = new GameObject("Player_AdrianCrane");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.1f, -19f);
            CharacterController cc = player.AddComponent<CharacterController>(); cc.height = 1.78f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.89f, 0);
            FirstPersonController controller = player.AddComponent<FirstPersonController>();
            GameObject cameraRoot = new GameObject("PlayerCameraRoot"); cameraRoot.tag = "MainCamera"; cameraRoot.transform.SetParent(player.transform, false); cameraRoot.transform.localPosition = new Vector3(0, 1.64f, 0);
            Camera camera = cameraRoot.AddComponent<Camera>(); camera.fieldOfView = 72f; camera.nearClipPlane = 0.05f; camera.farClipPlane = 180f; ConfigureTemporalAA(camera);
            cameraRoot.AddComponent<AudioListener>(); cameraRoot.AddComponent<PlayerCameraLook>(); cameraRoot.AddComponent<PlayerInteractionSystem>();
            GameObject lantern = new GameObject("AntiqueBrassOilLantern_ViewModel"); lantern.transform.SetParent(cameraRoot.transform, false); lantern.transform.localPosition = new Vector3(0.28f, -0.24f, 0.42f);
            lantern.AddComponent<UndertowFirstPersonLanternMotion>();
            LanternController lanternController = lantern.AddComponent<LanternController>();
            AddViewModelMesh("SM_AntiqueBrassLantern", "FirstPerson_Lantern_Mesh", lantern.transform, new Vector3(0f, -0.02f, 0.05f), new Vector3(0f, 0f, 0f), Vector3.one * 0.33f, brass);
            GameObject flame = new GameObject("Lantern_Flame_PointLight"); flame.transform.SetParent(lantern.transform, false); flame.transform.localPosition = new Vector3(0, -0.12f, 0.1f);
            Light flameLight = flame.AddComponent<Light>(); flameLight.type = LightType.Point; flameLight.color = new Color(1f, 0.48f, 0.16f); flameLight.intensity = 420f; flameLight.range = 11.5f; flameLight.shadows = LightShadows.Soft;
            // Serialized reference is assigned explicitly so the lantern controller can drive its light.
            SerializedObject lanternSO = new SerializedObject(lanternController); lanternSO.FindProperty("flamePointLight").objectReferenceValue = flameLight; lanternSO.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject oilSO = new SerializedObject(oilPickup); oilSO.FindProperty("targetLantern").objectReferenceValue = lanternController; oilSO.ApplyModifiedPropertiesWithoutUndo();
            cameraRoot.AddComponent<UndertowBasicHUD>();
            EditorSceneManager.SaveScene(scene, SceneDir + "ActI_MercifulRequiem.unity");
        }

        private static void BuildActShell(string sceneName, string rootName, GameAct act, Color groundColor, Color fogColor, bool underwater)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Material ground = Mat("MAT_" + sceneName + "_Ground", groundColor, 0.12f, underwater ? 0.78f : 0.3f);
            Material structure = Mat("MAT_" + sceneName + "_Structure", underwater ? new Color(0.055f, 0.12f, 0.15f) : new Color(0.11f, 0.105f, 0.09f), 0.16f, 0.42f);
            Material accent = Mat("MAT_" + sceneName + "_Accent", new Color(0.08f, 0.34f, 0.39f), 0.45f, 0.72f, true);
            Material warm = Mat("MAT_" + sceneName + "_Warm", new Color(1f, 0.31f, 0.055f), 0f, 0.9f, true);
            ApplyPbrSet(ground, act == GameAct.ActIV_SunkenTemple ? "T_SunkenTempleStone" : "T_WetAgedDeckWood");
            ApplyPbrSet(structure, act == GameAct.ActIV_SunkenTemple ? "T_SunkenTempleStone" : "T_WetAgedDeckWood");
            AddLighting(underwater ? new Color(0.025f, 0.12f, 0.16f) : new Color(0.035f, 0.045f, 0.06f), fogColor, underwater ? 0.028f : 0.018f, new Vector3(0, 1.6f, -8));
            GameObject root = new GameObject(rootName);
            // Each act builds its own walkable route below. A single giant floor would hide
            // the ocean and flatten the environment into a greybox platform.
            for (int i = 0; i < 7; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * (5f + (i % 3) * 3.5f);
                float z = -20f + i * 6.2f;
                Cube("Environment_Blockout_Structure_" + i, new Vector3(x, 2.4f, z), new Vector3(4.5f, 4.8f, 3.2f), structure, root.transform);
                Cylinder("Tall_Architectural_Pillar_" + i, new Vector3(-x * 0.35f, 3.4f, z + 1.5f), new Vector3(0.7f, 3.4f, 0.7f), accent, root.transform);
            }
            if (act == GameAct.ActII_ShipGraveyard)
            {
                CreateOceanSurface(root.transform, new Vector3(0f, -0.65f, 0f), 290f, false);
                // Narrow connected wreck-deck spine; open sea remains visible on both sides.
                Cube("Graveyard_Central_Wreck_Walkway", new Vector3(0f, -0.34f, 0f), new Vector3(8.2f, 0.68f, 58f), ground, root.transform);
                for (int plank = 0; plank < 58; plank++)
                {
                    Cube("Graveyard_Walkway_Plank", new Vector3(0f, 0.018f, -28f + plank), new Vector3(7.95f, 0.06f, 0.94f), ground, root.transform, false);
                    for (int seam = -3; seam <= 3; seam++)
                        if ((plank + seam) % 7 == 0) Cube("Graveyard_Deck_Nail", new Vector3(seam * 1.02f, 0.058f, -28f + plank), new Vector3(0.045f, 0.018f, 0.045f), accent, root.transform, false);
                }
                for (int wreck = 0; wreck < 5; wreck++)
                {
                    float x = wreck % 2 == 0 ? -11f : 10f;
                    float z = -12f + wreck * 10.5f;
                    GameObject hull = AddImportedMesh("SM_GalleonHullDeck", "Wrecked_Galleon_Hull_" + wreck, new Vector3(x, 0.25f, z), new Vector3(1.18f, 0.94f, 1.08f), structure, root.transform);
                    if (hull != null) hull.transform.rotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(-26f, 26f), Random.Range(-7f, 7f));
                    GameObject mast = AddImportedMesh("SM_GalleonMastAndSails", "Broken_Mast_Sails_" + wreck, new Vector3(x, 2.0f, z + 1.8f), Vector3.one * 0.86f, structure, root.transform);
                    if (mast != null) { mast.transform.rotation = Quaternion.Euler(0f, Random.Range(-25f, 25f), Random.Range(-15f, 15f)); mast.AddComponent<UndertowSailSway>(); }
                    for (int plank = 0; plank < 3; plank++)
                        Cube("Floating_Wreck_Plate", new Vector3(x + Random.Range(-3f, 3f), -0.2f, z + Random.Range(-4f, 4f)), new Vector3(3.8f, 0.12f, 0.42f), ground, root.transform, false).transform.rotation = Quaternion.Euler(0f, Random.Range(-40f, 40f), Random.Range(-7f, 7f));
                }
                AddImportedMesh("SM_ShipBell", "Graveyard_Bell", new Vector3(4, 1.3f, 8), Vector3.one, accent, root.transform);
                LanternCountBellPuzzle bellPuzzle = root.AddComponent<LanternCountBellPuzzle>(); bellPuzzle.SetLanternCountPattern(new[] { 1, 2, 3 });
                BellNote[] graveNotes = { BellNote.La, BellNote.Do, BellNote.Mi };
                for (int i = 0; i < 3; i++)
                {
                    GameObject bell = AddImportedMesh("SM_ShipBell", "Graveyard_Sequence_Bell_" + i, new Vector3(-3f + i * 3f, 1.35f, 9f), Vector3.one * .72f, accent, root.transform);
                    if (bell != null) bell.AddComponent<LanternCountBellInteractable>().Configure(bellPuzzle, graveNotes[i]);
                }
                CrateRingPuzzle crate = root.AddComponent<CrateRingPuzzle>(); crate.ConfigureForAct(act);
                GameObject crateBox = Cube("Graveyard_Crate", new Vector3(1f, .55f, 16f), new Vector3(1.2f, 1.1f, 1.2f), ground, root.transform);
                crateBox.AddComponent<CrateRingInteractable>().Configure(crate, 0, new Vector3(5f, .55f, 16f));
                GameObject echoRoute = Sphere("Echo_Safe_Route_Orb", new Vector3(-4f, 1.4f, 24f), Vector3.one * .55f, accent, root.transform, true);
                echoRoute.AddComponent<VoiceRouteChoiceInteractable>().Configure(bellPuzzle, true);
                GameObject mimicRoute = Sphere("Mimic_Trap_Route_Orb", new Vector3(4f, 1.4f, 24f), Vector3.one * .55f, warm, root.transform, true);
                mimicRoute.AddComponent<VoiceRouteChoiceInteractable>().Configure(bellPuzzle, false);
                GameObject drawing3 = Cube("Vanessa_Drawing_03_Graveyard", new Vector3(-10f, 1.4f, 16f), new Vector3(.38f,.46f,.03f), accent, root.transform, true);
                drawing3.AddComponent<StoryFlagPickup>().Configure("DRAWING_03_GRAVEYARD", true);
                Cube("Shipwreck_Stone_Island_A", new Vector3(-13f, -.1f, -17f), new Vector3(8f, 1.4f, 9f), structure, root.transform);
                Cube("Shipwreck_Stone_Island_B", new Vector3(13f, -.05f, 18f), new Vector3(7f, 1.2f, 8f), structure, root.transform);
                // Small debris piers provide physical access to side-route collectibles.
                Cube("Graveyard_Drawing_Pier", new Vector3(-10f, -0.28f, 16f), new Vector3(4.8f, 0.55f, 5.4f), ground, root.transform);
                Cube("Graveyard_Echo_Pier", new Vector3(-4f, -0.28f, 24f), new Vector3(3.5f, 0.55f, 4.2f), ground, root.transform);
                Cube("Graveyard_Mimic_Pier", new Vector3(4f, -0.28f, 24f), new Vector3(3.5f, 0.55f, 4.2f), ground, root.transform);
                for (int debris = 0; debris < 16; debris++)
                {
                    float x = Random.Range(-15f, 15f); float z = Random.Range(-26f, 27f);
                    GameObject log = Cylinder("Graveyard_Floating_Mast_Debris", new Vector3(x, -0.18f, z), new Vector3(Random.Range(.08f,.18f), Random.Range(1.2f,3.8f), .12f), structure, root.transform);
                    log.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-8f,8f));
                    if (log != null) log.AddComponent<UndertowFloatingMotion>();
                }
                GameObject exit = Cube("ActII_Mist_Exit", new Vector3(0f, 2.2f, 27f), new Vector3(3f,4.2f,.45f), accent, root.transform, true); exit.AddComponent<ActExitInteractable>();
            }
            else if (act == GameAct.ActIII_ForsakenPort)
            {
                // Flooded 1724 street with five readable building fronts and a central clock tower.
                CreateOceanSurface(root.transform, new Vector3(0f, -0.72f, 0f), 230f, false);
                // Raised cobblestone causeway through the flooded port; side canals remain open water.
                Cube("Port_Central_Cobblestone_Causeway", new Vector3(0f, -0.33f, 0f), new Vector3(13.5f, 0.66f, 60f), ground, root.transform);
                Material cobble = Mat("MAT_PortSaintClair_Worn_Cobblestone", new Color(.105f,.125f,.14f), .06f, .34f);
                ApplyPbrSet(cobble, "T_SunkenTempleStone");
                for (int z=-28; z<=28; z+=2)
                    for (int x=-6; x<=6; x+=2)
                    {
                        GameObject paver=Cube("Port_Individual_Cobblestone",new Vector3(x,0.012f,z),new Vector3(1.88f,.08f,1.86f),cobble,root.transform,false);
                        paver.transform.rotation=Quaternion.Euler(0,Random.Range(-3f,3f),Random.Range(-1.2f,1.2f));
                    }
                for (int canal=0;canal<10;canal++)
                {
                    float x=(canal%2==0?-1f:1f)*Random.Range(8.2f,15f); float z=Random.Range(-26f,27f);
                    GameObject ripple=Sphere("Port_Flood_Debris_Ripple",new Vector3(x,-.44f,z),new Vector3(Random.Range(.6f,1.6f),.06f,Random.Range(.8f,2.2f)),accent,root.transform,false);
                    if (ripple!=null) ripple.AddComponent<UndertowFloatingMotion>();
                }
                for (int building = 0; building < 5; building++)
                {
                    float x = building % 2 == 0 ? -10.5f : 10.5f;
                    float z = -19f + building * 8.4f;
                    GameObject house = new GameObject("Port_Building_" + building); house.transform.SetParent(root.transform, false);
                    Cube("Stone_Facade", new Vector3(x, 2.6f, z), new Vector3(7.2f,5.2f,5.5f), structure, house.transform);
                    Cube("Roof_Left_Slope", new Vector3(x-1.8f,5.6f,z), new Vector3(3.9f,.38f,6.2f), ground, house.transform).transform.rotation = Quaternion.Euler(0f,0f,-28f);
                    Cube("Roof_Right_Slope", new Vector3(x+1.8f,5.6f,z), new Vector3(3.9f,.38f,6.2f), ground, house.transform).transform.rotation = Quaternion.Euler(0f,0f,28f);
                    Cube("Doorway", new Vector3(x,1.3f,z-2.84f), new Vector3(1.3f,2.6f,.12f), accent, house.transform, false);
                    for (int w=0;w<2;w++) Cube("Window_Glow", new Vector3(x+(w==0?-2.25f:2.25f),3.2f,z-2.9f), new Vector3(.92f,1.05f,.08f), accent, house.transform, false);
                    for (int beam=0;beam<3;beam++) Cube("Facade_Stone_Course",new Vector3(x,0.8f+beam*.12f,z-2.82f),new Vector3(7.15f,.10f,.16f),ground,house.transform,false);
                }
                // Street furniture and masonry detailing keep the port from reading as five isolated boxes.
                for (int prop=0;prop<14;prop++)
                {
                    float side=prop%2==0?-1f:1f; float z=-25f+prop*3.7f;
                    Cube("Port_Stone_Step",new Vector3(side*7.3f,.18f,z),new Vector3(2.4f,.36f,1.3f),cobble,root.transform);
                    Cube("Port_Wrought_Iron_Railing",new Vector3(side*7.7f,.72f,z+1.15f),new Vector3(.12f,.95f,2.2f),accent,root.transform,false);
                    if(prop%3==0) Cylinder("Port_Mooring_Bollard",new Vector3(side*6.4f,.42f,z),new Vector3(.22f,.42f,.22f),accent,root.transform);
                }
                // Clock tower, flying buttress hints and public square.
                Cube("ClockTower_Base", new Vector3(0f,4.5f,20f), new Vector3(7.5f,9f,7.5f), structure, root.transform);
                Cube("ClockTower_Crown", new Vector3(0f,10f,20f), new Vector3(8.2f,1.2f,8.2f), ground, root.transform);
                AddImportedMesh("SM_ClockTowerDial", "Port_Clock_Tower_Dial", new Vector3(0, 5.4f, 16.1f), Vector3.one * 2.2f, accent, root.transform);
                ClockTowerPuzzle clockPuzzle = root.AddComponent<ClockTowerPuzzle>();
                for (int dial=0;dial<3;dial++)
                {
                    GameObject dialObj = Cylinder("Clock_Tower_Interactable_Dial_" + dial,new Vector3(-2.8f+dial*2.8f,3.1f,15.9f),new Vector3(.78f,.08f,.78f),accent,root.transform);
                    dialObj.AddComponent<ClockDialInteractable>().Configure(clockPuzzle,dial);
                }
                Cube("Chapel_Interior_Blockout", new Vector3(-7, 2.5f, 3), new Vector3(8, 5, 10), structure, root.transform);
                for(int buttress=0;buttress<4;buttress++)
                {
                    float x=buttress%2==0?-13.7f:13.7f; float z=-20f+(buttress/2)*31f;
                    Cube("Port_Gothic_Buttress",new Vector3(x,3.2f,z),new Vector3(.8f,6.4f,1.6f),ground,root.transform);
                    Cube("Port_Buttress_Cap",new Vector3(x,6.55f,z),new Vector3(1.6f,.5f,2.1f),accent,root.transform);
                }
                Cube("Port_Ledger_Step",new Vector3(10f,.22f,-6f),new Vector3(2.8f,.44f,2.8f),cobble,root.transform);
                Cube("Port_Drawing_Pier_Left",new Vector3(-10f,-.12f,8f),new Vector3(3.2f,.3f,3.8f),cobble,root.transform);
                Cube("Port_Drawing_Pier_Right",new Vector3(10f,-.12f,22f),new Vector3(3.2f,.3f,3.8f),cobble,root.transform);
                MirrorRoomChoiceSequence mirrorChoice = root.AddComponent<MirrorRoomChoiceSequence>();
                mirrorChoice.Configure(clockPuzzle);
                GameObject ledger = Cube("Crane_Bloodline_Ledger",new Vector3(10f,1.2f,-6f),new Vector3(.7f,.8f,.1f),accent,root.transform);
                ledger.AddComponent<CraneLedgerInteractable>().Configure(mirrorChoice);
                GameObject answer = Sphere("Mirror_Choice_Answer",new Vector3(-4f,1.5f,24f),Vector3.one*.55f,accent,root.transform,true);
                answer.AddComponent<MirrorRoomDecisionInteractable>().Configure(mirrorChoice,true);
                GameObject refuse = Sphere("Mirror_Choice_Refuse",new Vector3(4f,1.5f,24f),Vector3.one*.55f,warm,root.transform,true);
                refuse.AddComponent<MirrorRoomDecisionInteractable>().Configure(mirrorChoice,false);
                Cube("Vanessa_Drawing_04_Port",new Vector3(-10f,1.3f,8f),new Vector3(.35f,.45f,.03f),accent,root.transform,true).AddComponent<StoryFlagPickup>().Configure("DRAWING_04_PORT",true);
                Cube("Vanessa_Drawing_05_Clocktower",new Vector3(10f,1.5f,22f),new Vector3(.35f,.45f,.03f),accent,root.transform,true).AddComponent<StoryFlagPickup>().Configure("DRAWING_05_CLOCKTOWER",true);
                for (int lamp=0;lamp<6;lamp++)
                {
                    float x=lamp%2==0?-4.8f:4.8f; float z=-19f+lamp*8.2f;
                    Cylinder("Port_Lamp_Post",new Vector3(x,2.1f,z),new Vector3(.10f,2.1f,.10f),ground,root.transform);
                    Sphere("Port_Lamp_Glow",new Vector3(x,4.25f,z),Vector3.one*.25f,warm,root.transform,false);
                    GameObject glow=new GameObject("Port_Lamp_PointLight"); glow.transform.position=new Vector3(x,4.1f,z); Light pl=glow.AddComponent<Light>(); pl.type=LightType.Point;pl.color=new Color(1f,.42f,.16f);pl.intensity=7f;pl.range=6f;
                }
                GameObject exit = Cube("ActIII_Drowned_SeaWall_Exit",new Vector3(0f,2.1f,28f),new Vector3(3.2f,4.2f,.45f),accent,root.transform,true); exit.AddComponent<ActExitInteractable>();
            }
            else if (act == GameAct.ActIV_SunkenTemple)
            {
                CreateOceanSurface(root.transform, new Vector3(0f, -0.78f, 0f), 240f, true);
                // Broken basalt causeway and individual mirror-island platforms; no solid floor over the water.
                Cube("Temple_Central_Basalt_Nave",new Vector3(0f,-.32f,4f),new Vector3(13f,.64f,56f),ground,root.transform);
                for(int tileZ=-22;tileZ<=28;tileZ+=2)
                    for(int tileX=-6;tileX<=6;tileX+=2)
                    {
                        GameObject tile=Cube("Temple_Cut_Basalt_Floor_Tile",new Vector3(tileX,.026f,tileZ),new Vector3(1.88f,.10f,1.86f),ground,root.transform,false);
                        tile.transform.rotation=Quaternion.Euler(0,Random.Range(-1.2f,1.2f),0);
                    }
                // Monumental submerged nave: paired columns, ribs, mirror altars and a central aisle.
                for (int row=0;row<4;row++)
                {
                    float z=-6f+row*10.8f;
                    foreach (int side in new[]{-1,1})
                    {
                        float x=side*9.2f;
                        Cylinder("Temple_Basalt_Column_Shaft",new Vector3(x,4.4f,z),new Vector3(.78f,4.4f,.78f),structure,root.transform);
                        Cylinder("Temple_Column_Base",new Vector3(x,.42f,z),new Vector3(1.55f,.42f,1.55f),ground,root.transform);
                        Cylinder("Temple_Column_Capital",new Vector3(x,8.8f,z),new Vector3(1.3f,.6f,1.3f),accent,root.transform);
                        Cube("Temple_Rib_Beam",new Vector3(0f,9.1f,z),new Vector3(18.8f,.42f,.85f),structure,root.transform,false);
                    }
                }
                AddImportedMesh("SM_SunkenTempleColumnAndMirror", "Temple_Hero_Column_Mirror", new Vector3(-5, 0.2f, 12), Vector3.one * 1.65f, accent, root.transform);
                SunkenTempleMirrorPrismPuzzle templePuzzle = root.AddComponent<SunkenTempleMirrorPrismPuzzle>();
                int[] rotationTargets = {1,2,3,1};
                for (int mirror=0;mirror<4;mirror++)
                {
                    float x=mirror%2==0?-5.2f:5.2f; float z=mirror<2?3f:18f;
                    GameObject pedestal=Cylinder("Temple_Mirror_Pedestal_"+mirror,new Vector3(x,1f,z),new Vector3(1.15f,1f,1.15f),ground,root.transform);
                    GameObject mirrorObj=AddImportedMesh("SM_SunkenTempleColumnAndMirror","Temple_Rotating_Mirror_"+mirror,new Vector3(x,2.15f,z),Vector3.one*.72f,accent,root.transform);
                    if (mirrorObj!=null) mirrorObj.AddComponent<TempleMirrorInteractable>().Configure(templePuzzle,mirror);
                }
                GameObject prism=AddImportedMesh("SM_VanessaMusicBox","Temple_Glass_Prism_Pickup",new Vector3(-3f,1.2f,-12f),Vector3.one*.52f,accent,root.transform);
                if(prism!=null) prism.AddComponent<TempleComponentPickup>().Configure(templePuzzle,true);
                GameObject lens=AddImportedMesh("SM_AntiqueBrassLantern","Temple_Focusing_Lens_Pickup",new Vector3(4f,1.2f,23f),Vector3.one*.45f,accent,root.transform);
                if(lens!=null) lens.AddComponent<TempleComponentPickup>().Configure(templePuzzle,false);
                GameObject whale=AddImportedMesh("SM_PaleLeviathanWhale","Pale_Leviathan_Silhouette",new Vector3(0,7.5f,34),Vector3.one*2.0f,structure,root.transform);
                if(whale!=null) whale.AddComponent<UndertowLeviathanSwim>();
                root.AddComponent<UndertowUnderwaterParticles>();
                UndertowPresenceDirector presence = root.AddComponent<UndertowPresenceDirector>();
                if (whale != null) presence.SetLeviathan(whale.transform);
                Cube("Vanessa_Drawing_06_Temple",new Vector3(9f,1.5f,25f),new Vector3(.36f,.46f,.03f),accent,root.transform,true).AddComponent<StoryFlagPickup>().Configure("DRAWING_06_TEMPLE",true);
                GameObject altar=Cube("Final_Altar_Stone",new Vector3(0f,1f,30f),new Vector3(4.4f,2f,3.2f),structure,root.transform);
                GameObject echoChoice=Sphere("Altar_Lullaby_Echo_Choice",new Vector3(-1.25f,2.5f,30f),Vector3.one*.36f,accent,root.transform,true);
                echoChoice.AddComponent<FinalAltarInteractable>().Configure(templePuzzle,LullabyChoice.EchoIncorrect);
                GameObject mimicChoice=Sphere("Altar_Lullaby_Mimic_Choice",new Vector3(1.25f,2.5f,30f),Vector3.one*.36f,warm,root.transform,true);
                mimicChoice.AddComponent<FinalAltarInteractable>().Configure(templePuzzle,LullabyChoice.MimicCorrect);
            }
            GameObject campaign = new GameObject("CampaignState_Persistent"); campaign.AddComponent<ActProgressionManager>();
            GameObject systems = new GameObject("_UndertowSystems");
            systems.AddComponent<HiddenSanitySystem>(); systems.AddComponent<HDRPEnvironmentDistortion>();
            systems.AddComponent<EchoMimicVoiceDirector>(); systems.AddComponent<ListenMechanicController>();
            systems.AddComponent<SpatialAudioFallbackManager>();
            UndertowProceduralAmbience ambience = systems.AddComponent<UndertowProceduralAmbience>();
            if (act == GameAct.ActIV_SunkenTemple)
            {
                SerializedObject ambienceSO = new SerializedObject(ambience);
                ambienceSO.FindProperty("underwaterMode").boolValue = true;
                ambienceSO.ApplyModifiedPropertiesWithoutUndo();
            }
            systems.AddComponent<ObjectiveJournalSystem>(); systems.AddComponent<DialogueSubtitleSystem>();
            systems.AddComponent<GraphicsAndAudioSettingsManager>();
            systems.AddComponent<UndertowCampaignAutosave>();
            MainMenuAndPauseController pauseController = systems.AddComponent<MainMenuAndPauseController>();
            UndertowPauseHUD pauseHud = systems.AddComponent<UndertowPauseHUD>();
            SerializedObject pauseSO = new SerializedObject(pauseHud);
            pauseSO.FindProperty("pauseController").objectReferenceValue = pauseController;
            pauseSO.ApplyModifiedPropertiesWithoutUndo();
            GameObject player = new GameObject("Player_AdrianCrane"); player.tag = "Player"; player.transform.position = new Vector3(0, 0.1f, -24);
            CharacterController cc = player.AddComponent<CharacterController>(); cc.height = 1.78f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.89f, 0); player.AddComponent<FirstPersonController>();
            GameObject cam = new GameObject("PlayerCameraRoot"); cam.tag = "MainCamera"; cam.transform.SetParent(player.transform, false); cam.transform.localPosition = new Vector3(0, 1.64f, 0); Camera playerCam = cam.AddComponent<Camera>(); playerCam.farClipPlane = 240f; playerCam.fieldOfView = 72f; ConfigureTemporalAA(playerCam); cam.AddComponent<AudioListener>(); cam.AddComponent<PlayerCameraLook>(); cam.AddComponent<PlayerInteractionSystem>();
            GameObject lantern = new GameObject("AntiqueBrassOilLantern_ViewModel"); lantern.transform.SetParent(cam.transform, false); lantern.transform.localPosition = new Vector3(0.28f, -0.24f, 0.42f);
            lantern.AddComponent<UndertowFirstPersonLanternMotion>();
            LanternController lanternController = lantern.AddComponent<LanternController>();
            AddViewModelMesh("SM_AntiqueBrassLantern", "FirstPerson_Lantern_Mesh", lantern.transform, new Vector3(0f,-0.02f,0.05f), Vector3.zero, Vector3.one*.33f, accent);
            GameObject flame = new GameObject("Lantern_Flame_PointLight"); flame.transform.SetParent(lantern.transform, false); flame.transform.localPosition = new Vector3(0, -0.12f, 0.1f);
            Light flameLight = flame.AddComponent<Light>(); flameLight.type = LightType.Point; flameLight.color = new Color(1f, 0.48f, 0.16f); flameLight.intensity = 420f; flameLight.range = 11.5f; flameLight.shadows = LightShadows.Soft;
            SerializedObject lanternSO = new SerializedObject(lanternController); lanternSO.FindProperty("flamePointLight").objectReferenceValue = flameLight; lanternSO.ApplyModifiedPropertiesWithoutUndo();
            cam.AddComponent<UndertowBasicHUD>();
            EditorSceneManager.SaveScene(scene, SceneDir + sceneName + ".unity");
        }

        public static void BuildActIIShipGraveyardScene() => BuildActShell("ActII_ShipGraveyard", "ActII_ShipGraveyard_Root", GameAct.ActII_ShipGraveyard, new Color(0.035f, 0.07f, 0.085f), new Color(0.015f, 0.08f, 0.1f), false);
        public static void BuildActIIIForsakenPortScene() => BuildActShell("ActIII_ForsakenPort", "ActIII_PortSaintClair_1724_Root", GameAct.ActIII_ForsakenPort, new Color(0.10f, 0.105f, 0.11f), new Color(0.07f, 0.09f, 0.12f), false);
        public static void BuildActIVSunkenTempleScene() => BuildActShell("ActIV_SunkenTemple", "ActIV_SunkenTemple_Root", GameAct.ActIV_SunkenTemple, new Color(0.025f, 0.12f, 0.15f), new Color(0.005f, 0.12f, 0.17f), true);

        private static GameObject CreateEndingSilhouette(string name, Vector3 position, Material material, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Silhouette_Body"; body.transform.SetParent(root.transform, false); body.transform.localPosition = new Vector3(0f, .88f, 0f); body.transform.localScale = new Vector3(.42f, .88f, .30f);
            body.GetComponent<Renderer>().sharedMaterial = material; Object.DestroyImmediate(body.GetComponent<Collider>());
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Silhouette_Head"; head.transform.SetParent(root.transform, false); head.transform.localPosition = new Vector3(0f, 1.93f, 0f); head.transform.localScale = new Vector3(.32f,.38f,.30f);
            head.GetComponent<Renderer>().sharedMaterial = material; Object.DestroyImmediate(head.GetComponent<Collider>());
            return root;
        }

        public static void BuildEndingsResolutionScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddLighting(new Color(0.08f, 0.08f, 0.09f), new Color(0.015f, 0.025f, 0.04f), 0.008f, new Vector3(0, 1.6f, -5));
            Material shore = Mat("MAT_Ending_Shore", new Color(0.16f, 0.17f, 0.18f), 0.04f, 0.24f);
            Cube("Ending_Shore_Ground", new Vector3(0, -0.4f, 0), new Vector3(30, 0.8f, 24), shore);
            GameObject root = new GameObject("Endings_Resolution_Root"); root.AddComponent<DialogueSubtitleSystem>();
            CreateOceanSurface(root.transform, new Vector3(0f, -0.48f, 9f), 180f, false);
            GameObject adrian = CreateEndingSilhouette("Ending_Adrian_Silhouette", new Vector3(-0.8f, 0.02f, 3.2f), shore, root.transform);
            GameObject vanessa = CreateEndingSilhouette("Ending_Vanessa_Silhouette", new Vector3(0.45f, 0.02f, 3.8f), shore, root.transform);
            GameObject answer = new GameObject("Ending_Answer_Tableau"); answer.transform.SetParent(root.transform, false); adrian.transform.SetParent(answer.transform, true); vanessa.transform.SetParent(answer.transform, true);
            GameObject surface = new GameObject("Ending_Surface_Tableau"); surface.transform.SetParent(root.transform, false);
            GameObject musicBox = AddImportedMesh("SM_VanessaMusicBox", "Silent_Vanessa_Music_Box", new Vector3(0f, 0.28f, 2.8f), Vector3.one * 0.9f, Mat("MAT_Ending_MusicBox_Brass", new Color(.32f,.21f,.09f), .78f,.76f), surface.transform);
            if (musicBox == null) Cube("Silent_Music_Box_Fallback", new Vector3(0f, .35f, 2.8f), new Vector3(.7f,.5f,.55f), shore, surface.transform);
            GameObject undertow = new GameObject("Ending_Undertow_Tableau"); undertow.transform.SetParent(root.transform, false);
            CreateEndingSilhouette("Adrian_Absorbed_By_Undertow", new Vector3(0f, 0.02f, 3.4f), Mat("MAT_Ending_Undertow_Silhouette", new Color(.006f,.018f,.026f),.05f,.12f), undertow.transform);
            GameObject cam = new GameObject("Ending_Camera"); cam.tag = "MainCamera"; cam.transform.position = new Vector3(0, 2.1f, -8); cam.transform.rotation = Quaternion.Euler(5f, 0f, 0f); Camera endingCamera = cam.AddComponent<Camera>(); endingCamera.farClipPlane = 220f; ConfigureTemporalAA(endingCamera); cam.AddComponent<AudioListener>();
            UndertowEndingSequence endingSequence = root.AddComponent<UndertowEndingSequence>();
            SerializedObject endingSO = new SerializedObject(endingSequence);
            endingSO.FindProperty("answerTableau").objectReferenceValue = answer;
            endingSO.FindProperty("surfaceTableau").objectReferenceValue = surface;
            endingSO.FindProperty("undertowTableau").objectReferenceValue = undertow;
            endingSO.FindProperty("dawnLight").objectReferenceValue = GameObject.Find("Cold_Moon_Key_Light") != null ? GameObject.Find("Cold_Moon_Key_Light").GetComponent<Light>() : null;
            endingSO.ApplyModifiedPropertiesWithoutUndo();
            answer.SetActive(false); surface.SetActive(false); undertow.SetActive(false);
            EditorSceneManager.SaveScene(scene, SceneDir + "Endings_Resolution.unity");
        }
    }
}
#endif
