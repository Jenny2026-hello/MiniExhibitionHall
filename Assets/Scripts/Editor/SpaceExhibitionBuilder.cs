using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpaceExhibitionBuilder : EditorWindow
{
    private static Font cachedDisplayFont;

    private static readonly Vector3[] ExhibitPositions =
    {
        new Vector3(-9.5f, 1.35f, -2.8f),
        new Vector3(-9.5f, 1.35f, 2.4f),
        new Vector3(0f, 1.35f, -2.8f),
        new Vector3(0f, 1.35f, 2.4f),
        new Vector3(9.5f, 1.35f, -2.8f),
        new Vector3(9.5f, 1.35f, 2.4f)
    };

    private static readonly Vector3[] FeaturePositions =
    {
        new Vector3(-9.5f, 1.35f, 6.9f),
        new Vector3(0f, 1.35f, 6.9f),
        new Vector3(9.5f, 1.35f, 6.9f)
    };

    [MenuItem("Tools/Build Space Exhibition Hall")]
    public static void ShowWindow()
    {
        GetWindow<SpaceExhibitionBuilder>("Space Exhibition Builder");
    }

    [MenuItem("Tools/Clean Exhibition Visual Clutter")]
    public static void CleanVisualClutterMenu()
    {
        CleanVisualClutter();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Removed scene Light objects and old world-space text clutter.");
    }

    void OnGUI()
    {
        GUILayout.Label("Space Exhibition Hall Builder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        EditorGUILayout.HelpBox("Builds a polished interactive space exhibition hall with stable Chinese text, lighting, exhibits, UI, and audio.", MessageType.Info);

        GUILayout.Space(10);
        if (GUILayout.Button("Build Exhibition Hall", GUILayout.Height(42)))
        {
            BuildScene();
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Clear Current Scene", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Clear Scene", "Delete all top-level scene objects?", "Clear", "Cancel"))
            {
                ClearScene();
            }
        }
    }

    void BuildScene()
    {
        if (!EditorUtility.DisplayDialog("Build Hall", "Build a complete space exhibition hall now?", "Build", "Cancel"))
        {
            return;
        }

        ClearScene();
        BuildEnvironment();
        BuildLighting();
        BuildExhibitionAreas();
        BuildExhibits();
        BuildTourStory();
        BuildPlayer();
        BuildUI();
        BuildAudio();
        BuildZoneTriggers();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Space exhibition hall generated. Press Play to visit.");
    }

    void ClearScene()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        for (int i = allObjects.Length - 1; i >= 0; i--)
        {
            if (allObjects[i] != null && allObjects[i].transform.parent == null)
            {
                Undo.DestroyObjectImmediate(allObjects[i]);
            }
        }
    }

    static void CleanVisualClutter()
    {
        foreach (Light light in FindObjectsOfType<Light>())
        {
            if (light != null)
            {
                Undo.DestroyObjectImmediate(light.gameObject);
            }
        }

        foreach (TextMesh textMesh in FindObjectsOfType<TextMesh>())
        {
            if (textMesh != null && IsLegacyFloatingText(textMesh.gameObject))
            {
                Undo.DestroyObjectImmediate(textMesh.gameObject);
            }
        }

        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && IsLegacyFloatingText(canvas.gameObject))
            {
                Undo.DestroyObjectImmediate(canvas.gameObject);
            }
        }

        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>())
        {
            if (audioSource != null)
            {
                Undo.DestroyObjectImmediate(audioSource.gameObject);
            }
        }
    }

    static bool IsLegacyFloatingText(GameObject obj)
    {
        if (obj == null) return false;
        return obj.name.Contains("WelcomeText") || obj.name.Contains("WorldText") || obj.name.Contains("StartTitle") || obj.name.Contains("Legacy");
    }

    void BuildEnvironment()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.055f, 0.065f, 0.095f);
        RenderSettings.fogDensity = 0.0045f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.42f);

        CreateCube("Floor", new Vector3(0, -0.05f, 0), new Vector3(30, 0.1f, 25), Mat("Mat_Floor", new Color(0.18f, 0.19f, 0.22f), 0.12f));
        CreateCube("Ceiling", new Vector3(0, 5.05f, 0), new Vector3(30, 0.1f, 25), Mat("Mat_Ceiling", new Color(0.075f, 0.085f, 0.12f), 0.38f));

        Material wallMat = Mat("Mat_Wall", new Color(0.19f, 0.2f, 0.24f), 0.08f);
        CreateCube("Wall_North", new Vector3(0, 2.5f, -12.5f), new Vector3(30, 5, 0.2f), wallMat);
        CreateCube("Wall_South", new Vector3(0, 2.5f, 12.5f), new Vector3(30, 5, 0.2f), wallMat);
        CreateCube("Wall_East", new Vector3(15, 2.5f, 0), new Vector3(0.2f, 5, 25), wallMat);
        CreateCube("Wall_West", new Vector3(-15, 2.5f, 0), new Vector3(0.2f, 5, 25), wallMat);

        Material glassMat = Mat("Mat_Divider_Glass", new Color(0.12f, 0.28f, 0.42f, 0.24f), 0.28f);
        SetupTransparent(glassMat, 0.24f);
        CreateCube("Divider_A_Glass", new Vector3(-4.8f, 2.25f, -3.7f), new Vector3(0.06f, 3.0f, 7.6f), glassMat);
        CreateCube("Divider_B_Glass", new Vector3(4.8f, 2.25f, -3.7f), new Vector3(0.06f, 3.0f, 7.6f), glassMat);

        CreateStarField();
        CreateFloorLines();
        CreateArchitecturalLightBands();
        CreatePillars();
    }

    void BuildLighting()
    {
        // Keep the scene free of Light objects so Unity's lightbulb gizmo icons do not clutter the view.
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.44f, 0.5f);
    }

    void BuildExhibitionAreas()
    {
        CreateAreaSign("\u706b\u7bad\u6280\u672f\u533a", new Vector3(-9.5f, 2.65f, -12.18f), new Color(0.38f, 0.66f, 1f));
        CreateAreaSign("\u884c\u661f\u63a2\u7d22\u533a", new Vector3(0f, 2.65f, -12.18f), new Color(1f, 0.72f, 0.32f));
        CreateAreaSign("\u6df1\u7a7a\u672a\u6765\u533a", new Vector3(9.5f, 2.65f, -12.18f), new Color(0.72f, 0.45f, 1f));

        Color[] zoneColors =
        {
            new Color(0.25f, 0.55f, 1f),
            new Color(1f, 0.63f, 0.25f),
            new Color(0.62f, 0.34f, 1f)
        };

        for (int i = 0; i < ExhibitPositions.Length; i++)
        {
            CreatePedestal("Pedestal_" + (i + 1), new Vector3(ExhibitPositions[i].x, 0.42f, ExhibitPositions[i].z), zoneColors[i / 2]);
            CreateExhibitShowcase("Showcase_" + (i + 1), ExhibitPositions[i], zoneColors[i / 2], i != 3);
        }

        for (int i = 0; i < FeaturePositions.Length; i++)
        {
            CreatePedestal("FeaturePedestal_" + (i + 1), new Vector3(FeaturePositions[i].x, 0.42f, FeaturePositions[i].z), zoneColors[i]);
            CreateExhibitShowcase("FeatureShowcase_" + (i + 1), FeaturePositions[i], zoneColors[i], false);
        }

        CreateWelcomeWall();
    }

    void BuildExhibits()
    {
        CreateRocketModel("\u957f\u5f81\u706b\u7bad\u6a21\u578b", ExhibitPositions[0], "\u4e2d\u56fd\u957f\u5f81\u7cfb\u5217\u8fd0\u8f7d\u706b\u7bad\u6a21\u578b\uff0c\u5c55\u793a\u4e3b\u7bad\u4f53\u3001\u52a9\u63a8\u5668\u548c\u53d1\u5149\u5c3e\u7130\u3002", "\u5c3e\u7130\u4e0e\u5c55\u53f0\u706f\u5149\u8425\u9020\u53d1\u5c04\u73b0\u573a\u611f\u3002", "\u6309 E \u62ff\u8d77\u540e\u53ef\u65cb\u8f6c\u89c2\u5bdf\u7ed3\u6784\u3002", "\u628a\u822a\u5929\u5de5\u7a0b\u8f6c\u5316\u4e3a\u53ef\u89e6\u6478\u7684\u865a\u62df\u5c55\u54c1\u3002", true, 13f);
        CreateSatelliteModel("\u4eba\u9020\u536b\u661f\u6a21\u578b", ExhibitPositions[1], "\u901a\u4fe1\u536b\u661f\u6a21\u578b\uff0c\u5305\u542b\u4e3b\u4f53\u3001\u592a\u9633\u80fd\u677f\u548c\u5929\u7ebf\u3002", "\u84dd\u8272\u592a\u9633\u80fd\u677f\u8868\u73b0\u592a\u7a7a\u79d1\u6280\u611f\u3002", "\u53ef\u62ff\u8d77\u89c2\u5bdf\u592a\u9633\u80fd\u7ffc\u548c\u5929\u7ebf\u3002", "\u60f3\u8c61\u536b\u661f\u73af\u7ed5\u5730\u7403\u5de5\u4f5c\u3002", true, 18f);
        CreateMarsModel("\u706b\u661f", ExhibitPositions[2], "\u7ea2\u8272\u884c\u661f\u6a21\u578b\uff0c\u8868\u9762\u5e26\u6709\u9668\u77f3\u5751\u7ec6\u8282\u3002", "\u6696\u8272\u706f\u5149\u5851\u9020\u5f02\u661f\u8352\u539f\u6c1b\u56f4\u3002", "\u53ef\u65cb\u8f6c\u89c2\u5bdf\u4e0d\u540c\u5730\u8c8c\u3002", "\u60f3\u8c61\u4eba\u7c7b\u672a\u6765\u7684\u706b\u661f\u57fa\u5730\u3002", true, 9f);
        CreateSaturnModel("\u571f\u661f", ExhibitPositions[3], "\u5e26\u6709\u534a\u900f\u660e\u53d1\u5149\u5149\u73af\u7684\u571f\u661f\u6a21\u578b\u3002", "\u5149\u73af\u4e0e\u6696\u8272\u706f\u5149\u8425\u9020\u68a6\u5e7b\u89c2\u6d4b\u4f53\u9a8c\u3002", "\u53ef\u62ff\u8d77\u4ece\u4e0d\u540c\u89d2\u5ea6\u89c2\u5bdf\u5149\u73af\u3002", "\u628a\u9065\u8fdc\u5929\u4f53\u53d8\u6210\u53ef\u73af\u7ed5\u89c2\u5bdf\u7684\u7269\u4ef6\u3002", true, 8f);
        CreateSpaceStationModel("\u56fd\u9645\u7a7a\u95f4\u7ad9\u6a21\u578b", ExhibitPositions[4], "\u5c55\u793a\u6841\u67b6\u3001\u8231\u6bb5\u548c\u592a\u9633\u80fd\u677f\u7ed3\u6784\u3002", "\u94f6\u8272\u8231\u6bb5\u4e0e\u6df1\u84dd\u592a\u9633\u80fd\u7ffc\u5177\u6709\u5f3a\u8bc6\u522b\u5ea6\u3002", "\u53ef\u65cb\u8f6c\u67e5\u770b\u7a7a\u95f4\u7ad9\u7ed3\u6784\u3002", "\u5448\u73b0\u4eba\u7c7b\u957f\u671f\u5728\u8f68\u751f\u6d3b\u7684\u672a\u6765\u56fe\u666f\u3002", true, 7f);
        CreateDeepSpaceProbeModel("\u6df1\u7a7a\u63a2\u6d4b\u5668\u6a21\u578b", ExhibitPositions[5], "\u81f4\u656c\u65c5\u884c\u8005\u53f7\uff0c\u5305\u542b\u5929\u7ebf\u3001\u4eea\u5668\u8231\u3001RTG \u548c\u91d1\u5531\u7247\u3002", "\u7d2b\u8272\u5149\u7ebf\u5f3a\u8c03\u6df1\u7a7a\u63a2\u7d22\u7684\u5b64\u72ec\u4e0e\u6d6a\u6f2b\u3002", "\u6309 E \u67e5\u770b\u63a2\u6d4b\u5668\u4fe1\u606f\u3002", "\u60f3\u8c61\u4eba\u7c7b\u95ee\u5019\u62b5\u8fbe\u661f\u9645\u7a7a\u95f4\u3002", false, 12f);
        CreateMoonBaseModel("\u6708\u9762\u57fa\u5730\u6c99\u76d8", FeaturePositions[0], "\u5c55\u793a\u6708\u9762\u5c45\u4f4f\u8231\u3001\u6f2b\u6e38\u8f66\u548c\u8d44\u6e90\u91c7\u96c6\u88c5\u7f6e\uff0c\u7528\u4e8e\u8bf4\u660e\u672a\u6765\u6df1\u7a7a\u9a7b\u7559\u573a\u666f\u3002", "\u5730\u9762\u5bfc\u89c8\u7ebf\u5f15\u5bfc\u7528\u6237\u50cf\u771f\u6b63\u53c2\u89c2\u8005\u4e00\u6837\u8d70\u8fdb\u4efb\u52a1\u533a\u3002", "\u6309 E \u6253\u5f00\u6708\u9762\u57fa\u5730\u7684\u4efb\u52a1\u8bf4\u660e\u3002", "\u628a\u6559\u80b2\u5c55\u9986\u4e0e\u672a\u6765\u6708\u9762\u751f\u6d3b\u60f3\u8c61\u7ed3\u5408\u8d77\u6765\u3002", false, 6f);
        CreateSolarSystemModel("\u592a\u9633\u7cfb\u8f68\u9053\u6a21\u578b", FeaturePositions[1], "\u4ee5\u592a\u9633\u548c\u4e09\u6761\u8f68\u9053\u5c55\u793a\u884c\u661f\u8fd0\u884c\u5173\u7cfb\uff0c\u7528\u5c0f\u6bd4\u4f8b\u5448\u73b0\u5b8f\u5927\u5b87\u5b99\u3002", "\u53d1\u5149\u8f68\u9053\u548c\u5c3a\u5ea6\u5bf9\u6bd4\u589e\u5f3a\u592a\u7a7a\u6c89\u6d78\u611f\u3002", "\u53ef\u9760\u8fd1\u89e6\u53d1\u8bf4\u660e\uff0c\u7528\u4e8e\u89c6\u9891\u4e2d\u89e3\u91ca\u5929\u4f53\u8fd0\u52a8\u3002", "\u7528\u7b80\u5316\u6a21\u578b\u628a\u62bd\u8c61\u7684\u5929\u6587\u77e5\u8bc6\u53d8\u6210\u53ef\u89c2\u5bdf\u7684\u4e09\u7ef4\u573a\u666f\u3002", false, 9f);
        CreateBlackHoleModel("\u9ed1\u6d1e\u5f15\u529b\u4e95", FeaturePositions[2], "\u7528\u9ed1\u8272\u4e2d\u5fc3\u7403\u4f53\u4e0e\u53d1\u5149\u5438\u79ef\u76d8\u8868\u73b0\u9ed1\u6d1e\u5f3a\u5f15\u529b\u548c\u65f6\u7a7a\u5f2f\u66f2\u6982\u5ff5\u3002", "\u6df1\u8272\u533a\u57df\u3001\u73af\u72b6\u5149\u5e26\u548c\u7f13\u6162\u65cb\u8f6c\u5236\u9020\u6df1\u7a7a\u538b\u8feb\u611f\u3002", "\u7528\u6237\u9760\u8fd1\u540e\u53ef\u67e5\u770b\u9ed1\u6d1e\u79d1\u666e\u8bf4\u660e\u3002", "\u5c55\u793a\u73b0\u5b9e\u96be\u4ee5\u4eb2\u81ea\u7ecf\u5386\u7684\u6781\u7aef\u5b87\u5b99\u73b0\u8c61\u3002", false, 16f);
    }

    void BuildTourStory()
    {
        CreateTourRoute();
        CreateThreeIPanels();
        CreateRecordingPlanPanel();
    }
    void BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, 0.05f, 8.5f);
        player.transform.rotation = Quaternion.Euler(0, 180f, 0);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1f, 0);
        cc.height = 2f;
        cc.radius = 0.32f;

        FirstPersonController controller = player.AddComponent<FirstPersonController>();
        controller.walkSpeed = 4.3f;
        controller.runSpeed = 7f;
        controller.mouseSensitivity = 1.8f;

        GameObject camObj = new GameObject("Main Camera");
        camObj.transform.SetParent(player.transform, false);
        camObj.transform.localPosition = new Vector3(0, 1.55f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.fieldOfView = 68f;
        cam.nearClipPlane = 0.03f;
        camObj.AddComponent<AudioListener>();
    }

    void BuildAudio()
    {
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.transform.position = new Vector3(0f, -1000f, 0f);
        audioObj.hideFlags = HideFlags.HideInHierarchy;
        AudioManager am = audioObj.AddComponent<AudioManager>();
        am.bgmVolume = 0.28f;
        am.bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/maksymmalko-space-space-galaxy-universe-music-301239.mp3");
    }

    void BuildUI()
    {
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();

        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        scaler.referencePixelsPerUnit = 100f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject startPanel = CreateUIPanel(canvasObj.transform, "StartPanel", Vector2.zero, new Vector2(2200, 1400), new Color(0.01f, 0.015f, 0.035f, 0.94f));
        CreateUIText(startPanel.transform, "StartTitle", "\u592a\u7a7a\u63a2\u7d22\u865a\u62df\u5c55\u89c8\u9986", new Vector2(0, 112), new Vector2(760, 64), 42, Color.white, true, TextAnchor.MiddleCenter);
        CreateUIText(startPanel.transform, "StartSubtitle", "Space Exploration Virtual Museum", new Vector2(0, 58), new Vector2(640, 34), 20, new Color(0.48f, 0.72f, 1f), false, TextAnchor.MiddleCenter);
        CreateUIText(startPanel.transform, "ControlsHint", "WASD \u79fb\u52a8    \u9f20\u6807\u89c2\u5bdf    E \u4ea4\u4e92    \u6309\u5730\u9762 1-5 \u8def\u7ebf\u5f55\u5236 2-5 \u5206\u949f\u89c6\u9891", new Vector2(0, -174), new Vector2(820, 34), 16, new Color(0.72f, 0.78f, 0.9f), false, TextAnchor.MiddleCenter);
        CreateUIButton(startPanel.transform, "StartButton", "\u5f00\u59cb\u53c2\u89c2", new Vector2(0, -32), new Vector2(250, 58), () => GameManager.Instance?.StartGame());
        CreateUIButton(startPanel.transform, "QuitButton", "\u9000\u51fa", new Vector2(0, -104), new Vector2(250, 50), () => GameManager.Instance?.QuitGame());

        GameObject infoPanel = CreateUIPanel(canvasObj.transform, "InfoPanel", Vector2.zero, new Vector2(760, 520), new Color(0.01f, 0.012f, 0.02f, 0.98f));
        Text titleText = CreateUIText(infoPanel.transform, "InfoTitle", "\u5c55\u54c1\u540d\u79f0", new Vector2(0, 202), new Vector2(660, 56), 30, Color.white, true, TextAnchor.MiddleCenter);
        Text descText = CreateUIText(infoPanel.transform, "InfoDescription", "\u5c55\u54c1\u8bf4\u660e", new Vector2(0, 80), new Vector2(650, 152), 18, new Color(0.9f, 0.92f, 0.97f), false, TextAnchor.UpperLeft);
        Text threeIText = CreateUIText(infoPanel.transform, "Info3I", "", new Vector2(0, -100), new Vector2(650, 142), 16, new Color(0.56f, 0.78f, 1f), false, TextAnchor.UpperLeft);
        CreateUIButton(infoPanel.transform, "CloseInfoButton", "\u5173\u95ed", new Vector2(0, -204), new Vector2(180, 48), () => GameManager.Instance?.HideExhibitInfo());
        infoPanel.SetActive(false);

        GameObject hintPanel = CreateUIPanel(canvasObj.transform, "HintPanel", new Vector2(0, -430), new Vector2(900, 48), new Color(0.02f, 0.025f, 0.04f, 0.62f));
        Text hintText = CreateUIText(hintPanel.transform, "HintText", "", Vector2.zero, new Vector2(860, 44), 22, Color.white, true, TextAnchor.MiddleCenter);
        hintPanel.SetActive(false);

        GameObject pausePanel = CreateUIPanel(canvasObj.transform, "PausePanel", Vector2.zero, new Vector2(440, 300), new Color(0.01f, 0.015f, 0.03f, 0.88f));
        CreateUIText(pausePanel.transform, "PauseTitle", "\u6682\u505c", new Vector2(0, 84), new Vector2(320, 52), 38, Color.white, true, TextAnchor.MiddleCenter);
        CreateUIButton(pausePanel.transform, "ResumeButton", "\u7ee7\u7eed", new Vector2(0, 4), new Vector2(190, 48), () => GameManager.Instance?.TogglePause());
        CreateUIButton(pausePanel.transform, "PauseQuitButton", "\u9000\u51fa", new Vector2(0, -66), new Vector2(190, 48), () => GameManager.Instance?.QuitGame());
        pausePanel.SetActive(false);

        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.startPanel = startPanel;
        gm.infoPanel = infoPanel;
        gm.infoTitleText = titleText;
        gm.infoDescText = descText;
        gm.info3IText = threeIText;
        gm.interactionHint = hintText;
        gm.pausePanel = pausePanel;
    }

    void BuildZoneTriggers()
    {
        CreateZone("Zone_RocketTech", "\u706b\u7bad\u6280\u672f\u533a", "\u8fd9\u91cc\u5c55\u793a\u8fd0\u8f7d\u706b\u7bad\u4e0e\u4eba\u9020\u536b\u661f\u3002", new Vector3(-9.5f, 1.5f, 0), new Vector3(6.2f, 3f, 8f));
        CreateZone("Zone_PlanetExplore", "\u884c\u661f\u63a2\u7d22\u533a", "\u8fd9\u91cc\u5c55\u793a\u706b\u661f\u4e0e\u571f\u661f\u3002", new Vector3(0, 1.5f, 0), new Vector3(6.2f, 3f, 8f));
        CreateZone("Zone_DeepSpace", "\u6df1\u7a7a\u672a\u6765\u533a", "\u8fd9\u91cc\u5c55\u793a\u7a7a\u95f4\u7ad9\u4e0e\u6df1\u7a7a\u63a2\u6d4b\u5668\u3002", new Vector3(9.5f, 1.5f, 0), new Vector3(6.2f, 3f, 8f));
    }

    void CreateRocketModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        Material white = Mat("Mat_Rocket_White", new Color(0.88f, 0.9f, 0.92f), 0.05f);
        Material red = Mat("Mat_Rocket_Red", new Color(0.9f, 0.16f, 0.12f), 0.12f);
        Material black = Mat("Mat_Rocket_Window", new Color(0.02f, 0.035f, 0.055f), 0.08f);
        Material chrome = Mat("Mat_Rocket_Chrome", new Color(0.72f, 0.76f, 0.82f), 0.1f);
        Material flame = Mat("Mat_Rocket_Flame", new Color(1f, 0.43f, 0.05f), 1.7f);
        CreateCylinder(root.transform, "Body", new Vector3(0, 0.55f, 0), new Vector3(0.18f, 1.12f, 0.18f), white);
        CreateCylinder(root.transform, "Nose", new Vector3(0, 1.27f, 0), new Vector3(0.04f, 0.32f, 0.04f), red);
        CreateCube(root.transform, "Red Stripe", new Vector3(0, 0.2f, -0.185f), new Vector3(0.28f, 0.06f, 0.02f), red);
        CreateCube(root.transform, "Payload_Panel", new Vector3(0.01f, 1.04f, -0.188f), new Vector3(0.18f, 0.13f, 0.014f), chrome);
        CreateCylinder(root.transform, "Upper_Ring", new Vector3(0, 0.95f, 0), new Vector3(0.19f, 0.018f, 0.19f), red);
        CreateCylinder(root.transform, "Middle_Ring", new Vector3(0, 0.47f, 0), new Vector3(0.19f, 0.012f, 0.19f), chrome);
        CreateCylinder(root.transform, "Lower_Ring", new Vector3(0, -0.02f, 0), new Vector3(0.19f, 0.018f, 0.19f), red);
        CreateCube(root.transform, "Window_A", new Vector3(0, 0.78f, -0.188f), new Vector3(0.08f, 0.09f, 0.015f), black);
        CreateCube(root.transform, "Window_B", new Vector3(0, 0.64f, -0.188f), new Vector3(0.06f, 0.07f, 0.015f), black);
        CreateCylinder(root.transform, "Fuel_Line_A", new Vector3(0.19f, 0.42f, 0f), new Vector3(0.012f, 0.72f, 0.012f), chrome);
        CreateCylinder(root.transform, "Fuel_Line_B", new Vector3(-0.19f, 0.42f, 0f), new Vector3(0.012f, 0.72f, 0.012f), chrome);
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI * 0.5f;
            CreateCylinder(root.transform, "Booster_" + i, new Vector3(Mathf.Cos(angle) * 0.27f, 0.18f, Mathf.Sin(angle) * 0.27f), new Vector3(0.07f, 0.62f, 0.07f), white);
            CreateCylinder(root.transform, "Booster_Cap_" + i, new Vector3(Mathf.Cos(angle) * 0.27f, 0.55f, Mathf.Sin(angle) * 0.27f), new Vector3(0.072f, 0.035f, 0.072f), red);
            GameObject fin = CreateCube(root.transform, "Fin_" + i, new Vector3(Mathf.Cos(angle) * 0.23f, -0.28f, Mathf.Sin(angle) * 0.23f), new Vector3(0.04f, 0.22f, 0.13f), red);
            fin.transform.localRotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0);
        }
        CreateCylinder(root.transform, "EngineBell", new Vector3(0, -0.26f, 0), new Vector3(0.22f, 0.12f, 0.22f), black);
        CreateCylinder(root.transform, "Flame", new Vector3(0, -0.48f, 0), new Vector3(0.16f, 0.36f, 0.16f), flame);
        CreateCylinder(root.transform, "Flame_Core", new Vector3(0, -0.54f, 0), new Vector3(0.08f, 0.3f, 0.08f), Mat("Mat_Rocket_Flame_Core", new Color(1f, 0.92f, 0.28f), 2.2f));
        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1.1f);
    }

    void CreateSatelliteModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        Material metal = Mat("Mat_Satellite_Metal", new Color(0.72f, 0.75f, 0.82f), 0.08f);
        Material solar = Mat("Mat_Solar_Blue", new Color(0.04f, 0.13f, 0.32f), 0.55f);
        Material gold = Mat("Mat_Satellite_Gold", new Color(0.95f, 0.66f, 0.16f), 0.18f);
        Material lens = Mat("Mat_Satellite_Lens", new Color(0.18f, 0.6f, 1f), 0.75f);
        CreateCube(root.transform, "Core", Vector3.zero, new Vector3(0.42f, 0.34f, 0.42f), metal);
        CreateCube(root.transform, "Core_Gold_Foil", new Vector3(0, 0, -0.225f), new Vector3(0.34f, 0.26f, 0.025f), gold);
        CreateCube(root.transform, "Instrument_Box", new Vector3(0.02f, -0.31f, 0.02f), new Vector3(0.28f, 0.18f, 0.26f), metal);
        CreateSphere(root.transform, "Camera_Lens", new Vector3(0f, -0.32f, -0.18f), Vector3.one * 0.09f, lens);
        CreateCube(root.transform, "SolarPanel_L", new Vector3(-0.72f, 0, 0), new Vector3(0.95f, 0.035f, 0.34f), solar);
        CreateCube(root.transform, "SolarPanel_R", new Vector3(0.72f, 0, 0), new Vector3(0.95f, 0.035f, 0.34f), solar);
        for (int i = -2; i <= 2; i++)
        {
            CreateCube(root.transform, "SolarGrid_L_" + i, new Vector3(-0.72f + i * 0.16f, 0.023f, 0), new Vector3(0.012f, 0.012f, 0.35f), metal);
            CreateCube(root.transform, "SolarGrid_R_" + i, new Vector3(0.72f + i * 0.16f, 0.023f, 0), new Vector3(0.012f, 0.012f, 0.35f), metal);
        }
        CreateCylinder(root.transform, "Antenna_Left", new Vector3(-0.24f, 0.32f, 0), new Vector3(0.014f, 0.32f, 0.014f), metal, Quaternion.Euler(0, 0, -32f));
        CreateCylinder(root.transform, "Antenna_Right", new Vector3(0.24f, 0.32f, 0), new Vector3(0.014f, 0.32f, 0.014f), metal, Quaternion.Euler(0, 0, 32f));
        CreateCylinder(root.transform, "Antenna", new Vector3(0, 0.44f, 0), new Vector3(0.025f, 0.38f, 0.025f), metal);
        CreateCylinder(root.transform, "Dish", new Vector3(0, 0.68f, 0), new Vector3(0.2f, 0.035f, 0.2f), metal);
        CreateSphere(root.transform, "Dish_Feed", new Vector3(0, 0.75f, 0), Vector3.one * 0.045f, lens);
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
            CreateCylinder(root.transform, "Micro_Thruster_" + i, new Vector3(Mathf.Cos(angle) * 0.28f, -0.18f, Mathf.Sin(angle) * 0.28f), new Vector3(0.035f, 0.08f, 0.035f), metal);
        }
        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1.15f);
    }

    void CreateMarsModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject mars = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mars.name = name;
        mars.transform.position = pos;
        mars.transform.localScale = Vector3.one * 0.9f;
        SetMaterial(mars, Mat("Mat_Mars", new Color(0.74f, 0.24f, 0.12f), 0.18f));
        Material craterMat = Mat("Mat_Mars_Crater", new Color(0.35f, 0.09f, 0.045f), 0.03f);
        Material iceMat = Mat("Mat_Mars_Polar_Ice", new Color(0.9f, 0.86f, 0.76f), 0.06f);
        Material ridgeMat = Mat("Mat_Mars_Ridge", new Color(0.55f, 0.15f, 0.07f), 0.04f);
        Material atmosphereMat = Mat("Mat_Mars_Atmosphere", new Color(1f, 0.45f, 0.2f, 0.18f), 0.35f);
        SetupTransparent(atmosphereMat, 0.18f);
        Random.InitState(7);
        for (int i = 0; i < 9; i++) CreateSphere(mars.transform, "Crater_" + i, Random.onUnitSphere * 0.46f, Vector3.one * Random.Range(0.08f, 0.18f), craterMat);
        CreateSphere(mars.transform, "North_Polar_Cap", new Vector3(0, 0.45f, 0), new Vector3(0.28f, 0.055f, 0.28f), iceMat);
        CreateSphere(mars.transform, "South_Polar_Cap", new Vector3(0, -0.45f, 0), new Vector3(0.22f, 0.045f, 0.22f), iceMat);
        GameObject canyon = CreateCube(mars.transform, "Valles_Marineris", new Vector3(0.05f, 0.02f, -0.46f), new Vector3(0.56f, 0.025f, 0.035f), ridgeMat);
        canyon.transform.localRotation = Quaternion.Euler(0, 0, -8f);
        CreateSphere(mars.transform, "Olympus_Mons", new Vector3(-0.32f, 0.22f, -0.32f), Vector3.one * 0.11f, ridgeMat);
        RemoveCollider(CreateSphere(mars.transform, "Thin_Atmosphere", Vector3.zero, Vector3.one * 1.04f, atmosphereMat));
        AddExhibitScript(mars, name, desc, im, inter, img, pickup, speed, 1f);
    }

    void CreateSaturnModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        GameObject planet = CreateSphere(root.transform, "Planet", Vector3.zero, Vector3.one * 0.82f, Mat("Mat_Saturn", new Color(0.82f, 0.66f, 0.38f), 0.12f));
        GameObject ring = CreateCylinder(root.transform, "Transparent Ring", Vector3.zero, new Vector3(1.55f, 0.025f, 1.55f), Mat("Mat_Saturn_Ring", new Color(0.92f, 0.78f, 0.5f, 0.42f), 0.6f));
        GameObject ringInner = CreateCylinder(root.transform, "Inner Ring", Vector3.zero, new Vector3(1.18f, 0.018f, 1.18f), Mat("Mat_Saturn_Ring_Inner", new Color(1f, 0.88f, 0.58f, 0.32f), 0.48f));
        GameObject ringOuter = CreateCylinder(root.transform, "Outer Ring", Vector3.zero, new Vector3(1.9f, 0.014f, 1.9f), Mat("Mat_Saturn_Ring_Outer", new Color(0.62f, 0.78f, 1f, 0.24f), 0.42f));
        ring.transform.localRotation = Quaternion.Euler(0, 0, 18f);
        ringInner.transform.localRotation = Quaternion.Euler(0, 0, 18f);
        ringOuter.transform.localRotation = Quaternion.Euler(0, 0, 18f);
        SetupTransparent(ring.GetComponent<Renderer>().material, 0.42f);
        SetupTransparent(ringInner.GetComponent<Renderer>().material, 0.32f);
        SetupTransparent(ringOuter.GetComponent<Renderer>().material, 0.24f);
        CreateCube(root.transform, "Equator_Band", new Vector3(0, 0, -0.42f), new Vector3(0.72f, 0.055f, 0.025f), Mat("Mat_Saturn_Band", new Color(0.95f, 0.76f, 0.42f), 0.08f));
        CreateCube(root.transform, "Cloud_Band_North", new Vector3(0, 0.18f, -0.39f), new Vector3(0.58f, 0.035f, 0.022f), Mat("Mat_Saturn_Band_North", new Color(0.72f, 0.52f, 0.28f), 0.06f));
        CreateCube(root.transform, "Cloud_Band_South", new Vector3(0, -0.2f, -0.39f), new Vector3(0.52f, 0.03f, 0.022f), Mat("Mat_Saturn_Band_South", new Color(0.95f, 0.82f, 0.55f), 0.08f));
        CreateSphere(root.transform, "Titan_Moon", new Vector3(1.28f, 0.28f, -0.78f), Vector3.one * 0.12f, Mat("Mat_Titan_Moon", new Color(0.88f, 0.62f, 0.32f), 0.1f));
        CreateSphere(root.transform, "Ice_Moon", new Vector3(-1.36f, -0.2f, 0.64f), Vector3.one * 0.07f, Mat("Mat_Saturn_Ice_Moon", new Color(0.7f, 0.82f, 1f), 0.12f));
        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1f);
    }

    void CreateSpaceStationModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        Material metal = Mat("Mat_Station_Metal", new Color(0.78f, 0.8f, 0.84f), 0.08f);
        Material panel = Mat("Mat_Station_Panel", new Color(0.04f, 0.1f, 0.25f), 0.45f);
        Material accent = Mat("Mat_Station_Accent", new Color(0.95f, 0.95f, 1f), 0.22f);
        CreateCube(root.transform, "Truss", Vector3.zero, new Vector3(1.55f, 0.07f, 0.07f), metal);
        CreateCube(root.transform, "Truss_Vertical", Vector3.zero, new Vector3(0.08f, 0.07f, 0.85f), metal);
        CreateStrut(root.transform, "Truss_Diagonal_A", new Vector3(-0.56f, 0.02f, -0.28f), new Vector3(0.56f, 0.02f, 0.28f), 0.018f, accent);
        CreateStrut(root.transform, "Truss_Diagonal_B", new Vector3(-0.56f, 0.02f, 0.28f), new Vector3(0.56f, 0.02f, -0.28f), 0.018f, accent);
        CreateCylinder(root.transform, "Module_A", new Vector3(-0.35f, 0, 0), new Vector3(0.15f, 0.28f, 0.15f), metal, Quaternion.Euler(0, 0, 90));
        CreateCylinder(root.transform, "Module_B", new Vector3(0.35f, 0, 0), new Vector3(0.15f, 0.28f, 0.15f), metal, Quaternion.Euler(0, 0, 90));
        CreateCylinder(root.transform, "Module_C", new Vector3(0, 0, 0.28f), new Vector3(0.12f, 0.22f, 0.12f), metal, Quaternion.Euler(90, 0, 0));
        CreateCylinder(root.transform, "Docking_Port", new Vector3(0, 0, -0.36f), new Vector3(0.09f, 0.16f, 0.09f), accent, Quaternion.Euler(90, 0, 0));
        CreateSphere(root.transform, "Node", Vector3.zero, Vector3.one * 0.22f, metal);
        CreateSphere(root.transform, "Observation_Cupola", new Vector3(0.08f, 0.2f, 0.08f), Vector3.one * 0.11f, Mat("Mat_Cupola_Glass", new Color(0.22f, 0.62f, 1f), 0.55f));
        CreateStrut(root.transform, "Robotic_Arm_Upper", new Vector3(-0.12f, 0.18f, -0.08f), new Vector3(-0.62f, 0.34f, -0.26f), 0.016f, accent);
        CreateStrut(root.transform, "Robotic_Arm_Lower", new Vector3(-0.62f, 0.34f, -0.26f), new Vector3(-0.9f, 0.16f, -0.46f), 0.016f, accent);
        for (int side = -1; side <= 1; side += 2)
        {
            CreateCube(root.transform, "Panel_" + side + "_A", new Vector3(side * 0.78f, 0, 0.32f), new Vector3(0.55f, 0.03f, 0.42f), panel);
            CreateCube(root.transform, "Panel_" + side + "_B", new Vector3(side * 0.78f, 0, -0.32f), new Vector3(0.55f, 0.03f, 0.42f), panel);
            CreateCube(root.transform, "Panel_Frame_" + side + "_A", new Vector3(side * 0.78f, 0.025f, 0.32f), new Vector3(0.6f, 0.018f, 0.035f), accent);
            CreateCube(root.transform, "Panel_Frame_" + side + "_B", new Vector3(side * 0.78f, 0.025f, -0.32f), new Vector3(0.6f, 0.018f, 0.035f), accent);
            CreateCube(root.transform, "Radiator_" + side, new Vector3(side * 0.28f, -0.18f, 0.48f), new Vector3(0.34f, 0.018f, 0.18f), accent);
        }
        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1.12f);
    }

    void CreateDeepSpaceProbeModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        Material metal = Mat("Mat_Probe_Metal", new Color(0.68f, 0.7f, 0.72f), 0.08f);
        Material dark = Mat("Mat_Probe_Dark", new Color(0.13f, 0.13f, 0.16f), 0.03f);
        Material gold = Mat("Mat_Golden_Record", new Color(1f, 0.78f, 0.16f), 0.8f);
        Material blue = Mat("Mat_Probe_Sensor_Blue", new Color(0.2f, 0.58f, 1f), 0.55f);
        CreateCube(root.transform, "Bus", Vector3.zero, new Vector3(0.45f, 0.36f, 0.36f), metal);
        CreateCube(root.transform, "Instrument_Bay", new Vector3(0, -0.02f, -0.25f), new Vector3(0.34f, 0.24f, 0.12f), dark);
        CreateCylinder(root.transform, "Dish", new Vector3(0, 0.05f, 0.42f), new Vector3(0.46f, 0.045f, 0.46f), metal, Quaternion.Euler(90, 0, 0));
        CreateSphere(root.transform, "Dish_Feed_Horn", new Vector3(0, 0.05f, 0.72f), Vector3.one * 0.075f, blue);
        CreateStrut(root.transform, "Dish_Support_A", new Vector3(0.22f, 0.05f, 0.45f), new Vector3(0, 0.05f, 0.69f), 0.012f, metal);
        CreateStrut(root.transform, "Dish_Support_B", new Vector3(-0.22f, 0.05f, 0.45f), new Vector3(0, 0.05f, 0.69f), 0.012f, metal);
        CreateStrut(root.transform, "Dish_Support_C", new Vector3(0, 0.27f, 0.45f), new Vector3(0, 0.05f, 0.69f), 0.012f, metal);
        CreateCylinder(root.transform, "Boom", new Vector3(0, 0, 0.23f), new Vector3(0.025f, 0.35f, 0.025f), metal);
        CreateCylinder(root.transform, "RTG_L", new Vector3(-0.38f, -0.18f, 0), new Vector3(0.06f, 0.28f, 0.06f), dark);
        CreateCylinder(root.transform, "RTG_R", new Vector3(0.38f, -0.18f, 0), new Vector3(0.06f, 0.28f, 0.06f), dark);
        CreateCylinder(root.transform, "RTG_L_Fin_A", new Vector3(-0.38f, -0.18f, 0.1f), new Vector3(0.075f, 0.012f, 0.075f), metal, Quaternion.Euler(90, 0, 0));
        CreateCylinder(root.transform, "RTG_R_Fin_A", new Vector3(0.38f, -0.18f, 0.1f), new Vector3(0.075f, 0.012f, 0.075f), metal, Quaternion.Euler(90, 0, 0));
        CreateCylinder(root.transform, "Magnetometer_Boom", new Vector3(0.62f, 0.18f, 0), new Vector3(0.018f, 0.58f, 0.018f), metal, Quaternion.Euler(0, 0, 72f));
        CreateSphere(root.transform, "Sensor_Tip", new Vector3(1.15f, 0.35f, 0), Vector3.one * 0.08f, blue);
        CreateCube(root.transform, "Science_Camera", new Vector3(-0.14f, 0.18f, -0.29f), new Vector3(0.12f, 0.11f, 0.08f), dark);
        CreateSphere(root.transform, "Camera_Glass", new Vector3(-0.14f, 0.18f, -0.35f), Vector3.one * 0.045f, blue);
        CreateCylinder(root.transform, "Golden Record", new Vector3(0.02f, 0.25f, 0.21f), new Vector3(0.14f, 0.016f, 0.14f), gold, Quaternion.Euler(90, 0, 0));
        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1.1f);
    }

    void CreateMoonBaseModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        Material lunar = Mat("Mat_Lunar_Surface", new Color(0.38f, 0.39f, 0.42f), 0.04f);
        Material habitat = Mat("Mat_Habitat", new Color(0.86f, 0.88f, 0.9f), 0.08f);
        Material glass = Mat("Mat_Habitat_Glass", new Color(0.28f, 0.62f, 1f, 0.45f), 0.45f);
        Material dark = Mat("Mat_Lunar_Shadow", new Color(0.18f, 0.18f, 0.2f), 0.02f);
        Material warning = Mat("Mat_Lunar_Warning", new Color(1f, 0.72f, 0.18f), 0.35f);
        SetupTransparent(glass, 0.45f);

        CreateCylinder(root.transform, "Moon Ground", new Vector3(0, -0.34f, 0), new Vector3(1.0f, 0.08f, 1.0f), lunar);
        CreateCylinder(root.transform, "Crater_A", new Vector3(-0.58f, -0.28f, 0.36f), new Vector3(0.18f, 0.018f, 0.18f), dark);
        CreateCylinder(root.transform, "Crater_B", new Vector3(0.52f, -0.28f, 0.22f), new Vector3(0.13f, 0.016f, 0.13f), dark);
        CreateSphere(root.transform, "Habitat Dome", new Vector3(-0.32f, 0.08f, -0.08f), new Vector3(0.42f, 0.28f, 0.42f), glass);
        CreateCylinder(root.transform, "Habitat Base", new Vector3(-0.32f, -0.1f, -0.08f), new Vector3(0.38f, 0.11f, 0.38f), habitat);
        CreateCylinder(root.transform, "Airlock", new Vector3(-0.03f, -0.1f, -0.08f), new Vector3(0.12f, 0.18f, 0.12f), habitat, Quaternion.Euler(0, 0, 90f));
        CreateCylinder(root.transform, "Secondary_Habitat", new Vector3(-0.72f, -0.08f, -0.1f), new Vector3(0.18f, 0.26f, 0.18f), habitat, Quaternion.Euler(90, 0, 0));
        CreateStrut(root.transform, "Habitat_Tunnel", new Vector3(-0.52f, -0.08f, -0.1f), new Vector3(-0.12f, -0.08f, -0.1f), 0.065f, habitat);
        CreateCylinder(root.transform, "Landing_Pad", new Vector3(0.48f, -0.275f, 0.42f), new Vector3(0.26f, 0.014f, 0.26f), warning);
        CreateCube(root.transform, "Landing_Pad_Stripe", new Vector3(0.48f, -0.255f, 0.42f), new Vector3(0.42f, 0.012f, 0.035f), dark);
        CreateCube(root.transform, "Solar Array", new Vector3(0.42f, -0.04f, -0.22f), new Vector3(0.72f, 0.035f, 0.32f), Mat("Mat_Moon_Solar", new Color(0.04f, 0.13f, 0.32f), 0.5f));
        CreateCube(root.transform, "Solar Array Frame", new Vector3(0.42f, -0.015f, -0.22f), new Vector3(0.78f, 0.018f, 0.035f), habitat);
        CreateCylinder(root.transform, "Comms Mast", new Vector3(-0.68f, 0.02f, -0.36f), new Vector3(0.018f, 0.42f, 0.018f), habitat);
        CreateCylinder(root.transform, "Comms Dish", new Vector3(-0.68f, 0.31f, -0.36f), new Vector3(0.13f, 0.018f, 0.13f), habitat, Quaternion.Euler(35f, 0, 0));
        CreateCube(root.transform, "Rover Body", new Vector3(0.23f, -0.18f, 0.34f), new Vector3(0.36f, 0.14f, 0.24f), habitat);
        CreateCylinder(root.transform, "Rover Camera", new Vector3(0.23f, -0.02f, 0.34f), new Vector3(0.025f, 0.14f, 0.025f), habitat);
        CreateCube(root.transform, "Rover_Solar_Top", new Vector3(0.23f, -0.08f, 0.34f), new Vector3(0.28f, 0.018f, 0.18f), Mat("Mat_Rover_Solar", new Color(0.04f, 0.12f, 0.3f), 0.45f));
        CreateStrut(root.transform, "Rover_Antenna", new Vector3(0.29f, -0.06f, 0.26f), new Vector3(0.42f, 0.12f, 0.12f), 0.012f, habitat);
        for (int i = -1; i <= 1; i += 2)
        {
            CreateCylinder(root.transform, "Rover Wheel L" + i, new Vector3(0.1f * i, -0.27f, 0.48f), new Vector3(0.055f, 0.035f, 0.055f), lunar, Quaternion.Euler(90, 0, 0));
            CreateCylinder(root.transform, "Rover Wheel R" + i, new Vector3(0.36f * i, -0.27f, 0.2f), new Vector3(0.055f, 0.035f, 0.055f), lunar, Quaternion.Euler(90, 0, 0));
        }

        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1f);
    }

    void CreateSolarSystemModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        Material orbit = Mat("Mat_Orbit_Line", new Color(0.35f, 0.62f, 1f, 0.22f), 0.65f);
        SetupTransparent(orbit, 0.22f);
        CreateSphere(root.transform, "Sun", Vector3.zero, Vector3.one * 0.42f, Mat("Mat_Sun", new Color(1f, 0.72f, 0.12f), 1.2f));
        Material corona = Mat("Mat_Sun_Corona", new Color(1f, 0.42f, 0.08f, 0.16f), 0.75f);
        SetupTransparent(corona, 0.16f);
        RemoveCollider(CreateSphere(root.transform, "Sun_Corona", Vector3.zero, Vector3.one * 0.56f, corona));
        CreateCylinder(root.transform, "Orbit_1", Vector3.zero, new Vector3(0.86f, 0.01f, 0.86f), orbit);
        CreateCylinder(root.transform, "Orbit_2", Vector3.zero, new Vector3(1.25f, 0.01f, 1.25f), orbit);
        CreateCylinder(root.transform, "Orbit_3", Vector3.zero, new Vector3(1.64f, 0.01f, 1.64f), orbit);
        CreateCylinder(root.transform, "Orbit_4", Vector3.zero, new Vector3(2.02f, 0.008f, 2.02f), orbit);
        GameObject inclinedOrbit = CreateCylinder(root.transform, "Inclined_Orbit", Vector3.zero, new Vector3(2.34f, 0.006f, 2.34f), orbit);
        inclinedOrbit.transform.localRotation = Quaternion.Euler(0, 0, 12f);
        CreateSphere(root.transform, "Planet_Mercury", new Vector3(0.52f, 0.035f, 0.42f), Vector3.one * 0.08f, Mat("Mat_Mercury", new Color(0.56f, 0.55f, 0.5f), 0.08f));
        CreateSphere(root.transform, "Planet_Venus", new Vector3(-0.66f, 0.045f, -0.52f), Vector3.one * 0.11f, Mat("Mat_Venus", new Color(0.9f, 0.72f, 0.42f), 0.12f));
        CreateSphere(root.transform, "Planet_Blue", new Vector3(0.86f, 0.06f, 0), Vector3.one * 0.16f, Mat("Mat_Planet_Blue", new Color(0.18f, 0.44f, 1f), 0.25f));
        CreateSphere(root.transform, "Moon", new Vector3(1.05f, 0.08f, 0.15f), Vector3.one * 0.045f, Mat("Mat_Mini_Moon", new Color(0.72f, 0.72f, 0.68f), 0.06f));
        CreateSphere(root.transform, "Planet_Red", new Vector3(-0.8f, 0.05f, 0.96f), Vector3.one * 0.13f, Mat("Mat_Planet_Red", new Color(0.8f, 0.22f, 0.12f), 0.18f));
        CreateSphere(root.transform, "Planet_Giant", new Vector3(0.1f, 0.08f, -1.55f), Vector3.one * 0.22f, Mat("Mat_Planet_Giant", new Color(0.82f, 0.68f, 0.42f), 0.16f));
        GameObject miniRing = CreateCylinder(root.transform, "Mini_Saturn_Ring", new Vector3(0.1f, 0.08f, -1.55f), new Vector3(0.42f, 0.008f, 0.42f), orbit);
        miniRing.transform.localRotation = Quaternion.Euler(0, 0, 18f);
        Material asteroid = Mat("Mat_Asteroid_Belt", new Color(0.62f, 0.55f, 0.48f), 0.08f);
        for (int i = 0; i < 20; i++)
        {
            float a = i * Mathf.PI * 2f / 20f;
            float r = 1.42f + ((i % 3) - 1) * 0.055f;
            CreateSphere(root.transform, "Asteroid_" + i, new Vector3(Mathf.Cos(a) * r, 0.035f, Mathf.Sin(a) * r), Vector3.one * (0.025f + (i % 4) * 0.006f), asteroid);
        }
        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1f);
    }

    void CreateBlackHoleModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup, float speed)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;
        CreateSphere(root.transform, "Event Horizon", Vector3.zero, Vector3.one * 0.62f, Mat("Mat_Black_Hole", new Color(0.005f, 0.005f, 0.01f), 0.03f));
        Material diskA = Mat("Mat_Accretion_A", new Color(1f, 0.58f, 0.12f, 0.52f), 0.9f);
        Material diskB = Mat("Mat_Accretion_B", new Color(0.52f, 0.25f, 1f, 0.42f), 0.7f);
        Material jet = Mat("Mat_Black_Hole_Jet", new Color(0.45f, 0.8f, 1f, 0.36f), 0.95f);
        Material particle = Mat("Mat_Black_Hole_Particle", new Color(1f, 0.82f, 0.35f), 1.1f);
        SetupTransparent(diskA, 0.52f);
        SetupTransparent(diskB, 0.42f);
        SetupTransparent(jet, 0.36f);
        GameObject disk1 = CreateCylinder(root.transform, "Accretion Disk Inner", Vector3.zero, new Vector3(1.25f, 0.018f, 1.25f), diskA);
        GameObject disk2 = CreateCylinder(root.transform, "Accretion Disk Outer", Vector3.zero, new Vector3(1.75f, 0.014f, 1.75f), diskB);
        GameObject disk3 = CreateCylinder(root.transform, "Accretion Disk Hot Edge", Vector3.zero, new Vector3(2.08f, 0.01f, 2.08f), diskA);
        disk1.transform.localRotation = Quaternion.Euler(0, 0, 12f);
        disk2.transform.localRotation = Quaternion.Euler(0, 0, -14f);
        disk3.transform.localRotation = Quaternion.Euler(0, 0, 4f);
        CreateCylinder(root.transform, "Polar_Jet_Up", new Vector3(0, 0.78f, 0), new Vector3(0.055f, 0.56f, 0.055f), jet);
        CreateCylinder(root.transform, "Polar_Jet_Down", new Vector3(0, -0.78f, 0), new Vector3(0.055f, 0.56f, 0.055f), jet);
        Material lensGlow = Mat("Mat_Black_Hole_Lens", new Color(0.24f, 0.38f, 1f, 0.14f), 0.5f);
        SetupTransparent(lensGlow, 0.14f);
        RemoveCollider(CreateSphere(root.transform, "Gravitational_Lens_Glow", Vector3.zero, Vector3.one * 1.05f, lensGlow));
        for (int i = 0; i < 18; i++)
        {
            float a = i * Mathf.PI * 2f / 18f;
            float r = 0.8f + (i % 5) * 0.12f;
            CreateSphere(root.transform, "Accretion_Particle_" + i, new Vector3(Mathf.Cos(a) * r, ((i % 3) - 1) * 0.025f, Mathf.Sin(a) * r), Vector3.one * (0.035f + (i % 3) * 0.01f), particle);
        }
        AddExhibitScript(root, name, desc, im, inter, img, pickup, speed, 1f);
    }

    void AddExhibitScript(GameObject obj, string name, string desc, string im, string inter, string img, bool pickup, float rotSpeed, float pickupScale)
    {
        foreach (Collider visualCollider in obj.GetComponentsInChildren<Collider>()) DestroyImmediate(visualCollider);
        SphereCollider trigger = obj.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 3.4f;

        if (pickup)
        {
            ExhibitPickup ep = obj.AddComponent<ExhibitPickup>();
            ep.exhibitName = name;
            ep.exhibitDescription = desc;
            ep.hintText = "\u6309 E \u62ff\u8d77\u89c2\u5bdf";
            ep.interactionDistance = 3.4f;
            ep.autoRotateSpeed = rotSpeed;
            ep.immersionNote = im;
            ep.interactionNote = inter;
            ep.imaginationNote = img;
            ep.pickupDistance = 1.8f;
            ep.pickupScale = Vector3.one * pickupScale;
            ep.rotationSpeed = 120f;
        }
        else
        {
            ExhibitBase eb = obj.AddComponent<ExhibitBase>();
            eb.exhibitName = name;
            eb.exhibitDescription = desc;
            eb.hintText = "\u6309 E \u67e5\u770b\u8be6\u60c5";
            eb.interactionDistance = 3.4f;
            eb.autoRotateSpeed = rotSpeed;
            eb.immersionNote = im;
            eb.interactionNote = inter;
            eb.imaginationNote = img;
        }
    }

    void CreateWelcomeWall()
    {
        CreateCube("Welcome Wall", new Vector3(0, 2.35f, 10.8f), new Vector3(9.2f, 3.1f, 0.18f), Mat("Mat_Welcome_Wall", new Color(0.025f, 0.035f, 0.07f), 0.18f));
        CreateCube("Welcome Light Bar Top", new Vector3(0, 3.55f, 10.66f), new Vector3(7.4f, 0.04f, 0.04f), Mat("Mat_Welcome_Line", new Color(0.32f, 0.58f, 1f), 0.65f));
        CreateCube("Welcome Light Bar Bottom", new Vector3(0, 1.15f, 10.66f), new Vector3(7.4f, 0.04f, 0.04f), Mat("Mat_Welcome_Line", new Color(0.32f, 0.58f, 1f), 0.65f));
        CreateWallLabel("HallLabel_Title", "\u592a\u7a7a\u63a2\u7d22\u865a\u62df\u5c55\u89c8\u9986", new Vector3(0f, 2.72f, 10.62f), 28, Color.white, Quaternion.Euler(0, 180f, 0));
        CreateWallLabel("HallLabel_Subtitle", "Space Exploration Virtual Museum", new Vector3(0f, 2.22f, 10.62f), 15, new Color(0.56f, 0.76f, 1f), Quaternion.Euler(0, 180f, 0));
    }

    void CreateTourRoute()
    {
        Material routeMat = Mat("Mat_Tour_Route", new Color(0.08f, 0.62f, 1f), 0.55f);
        Material stopMat = Mat("Mat_Tour_Stop", new Color(1f, 0.78f, 0.22f), 0.75f);

        CreateCube("TourRoute_Main", new Vector3(0, 0.035f, 2.1f), new Vector3(0.08f, 0.02f, 13.8f), routeMat);
        CreateCube("TourRoute_Rocket", new Vector3(-4.75f, 0.037f, 2.1f), new Vector3(9.5f, 0.02f, 0.08f), routeMat);
        CreateCube("TourRoute_DeepSpace", new Vector3(4.75f, 0.037f, 2.1f), new Vector3(9.5f, 0.02f, 0.08f), routeMat);

        Vector3[] stops =
        {
            new Vector3(0, 0.08f, 8.4f),
            new Vector3(-9.5f, 0.08f, 4.8f),
            new Vector3(0f, 0.08f, 4.8f),
            new Vector3(9.5f, 0.08f, 4.8f),
            new Vector3(0f, 0.08f, -4.9f)
        };

        for (int i = 0; i < stops.Length; i++)
        {
            CreateCylinder(null, "TourStop_" + (i + 1), stops[i], new Vector3(0.46f, 0.018f, 0.46f), stopMat);
            CreateWallLabel("TourStopLabel_" + (i + 1), (i + 1).ToString(), stops[i] + new Vector3(0, 0.04f, 0), 18, Color.white, Quaternion.Euler(90f, 0, 0));
        }
    }

    void CreateThreeIPanels()
    {
        CreateInfoWallPanel(
            "Panel_Immersion",
            "\u6c89\u6d78\u6027 Immersion\n\u7b2c\u4e00\u4eba\u79f0\u89c6\u89d2\u3001\u661f\u7a7a\u9876\u68da\u3001\u53d1\u5149\u5bfc\u89c8\u7ebf\u548c\u4e09\u4e2a\u5c55\u533a\u7ec4\u6210\u5b8c\u6574\u865a\u62df\u573a\u9986\u3002",
            new Vector3(-14.82f, 2.55f, 2.2f),
            new Color(0.38f, 0.66f, 1f),
            Quaternion.Euler(0, 90f, 0));
        CreateInfoWallPanel(
            "Panel_Interaction",
            "\u4ea4\u4e92\u6027 Interaction\nWASD \u884c\u8d70\u3001\u9f20\u6807\u89c2\u5bdf\u3001E \u952e\u67e5\u770b\u8bf4\u660e\u6216\u62ff\u8d77\u5c55\u54c1\uff0c\u7cfb\u7edf\u4f1a\u7ed9\u51fa\u63d0\u793a\u548c\u4fe1\u606f\u53cd\u9988\u3002",
            new Vector3(14.82f, 2.55f, 2.2f),
            new Color(1f, 0.72f, 0.32f),
            Quaternion.Euler(0, -90f, 0));
        CreateInfoWallPanel(
            "Panel_Imagination",
            "\u6784\u60f3\u6027 Imagination\n\u6708\u9762\u57fa\u5730\u3001\u9ed1\u6d1e\u548c\u6df1\u7a7a\u63a2\u6d4b\u628a\u73b0\u5b9e\u96be\u4ee5\u5230\u8fbe\u7684\u573a\u666f\u8f6c\u5316\u4e3a\u53ef\u53c2\u89c2\u7684\u6559\u80b2\u4f53\u9a8c\u3002",
            new Vector3(0f, 2.9f, -12.32f),
            new Color(0.72f, 0.45f, 1f),
            Quaternion.identity);
    }

    void CreateRecordingPlanPanel()
    {
        CreateInfoWallPanel(
            "Panel_VideoPlan",
            "\u89c6\u9891\u5efa\u8bae 2-5 min\n1 \u5165\u53e3\u4e0e\u6574\u4f53\u573a\u666f 20s\n2 \u706b\u7bad\u533a\u4ea4\u4e92 45s\n3 \u884c\u661f\u533a\u89c2\u5bdf 45s\n4 \u6df1\u7a7a\u533a\u8bf4\u660e 45s\n5 3I \u5206\u6790\u603b\u7ed3 30s",
            new Vector3(0f, 2.55f, 12.32f),
            new Color(0.56f, 0.86f, 1f),
            Quaternion.Euler(0, 180f, 0));
    }

    void CreateInfoWallPanel(string name, string content, Vector3 pos, Color accent, Quaternion rotation)
    {
        CreatePanelCube(name + "_Back", pos + rotation * new Vector3(0, 0, 0.035f), new Vector3(4.9f, 1.45f, 0.055f), rotation, Mat("Mat_" + name + "_Back", new Color(0.018f, 0.024f, 0.04f), 0.12f));
        CreatePanelCube(name + "_Accent", pos + rotation * new Vector3(-2.18f, -0.62f, 0.07f), new Vector3(0.08f, 0.18f, 0.05f), rotation, Mat("Mat_" + name + "_Accent", accent, 0.7f));
        CreateWallLabel(name + "_Text", content, pos + rotation * new Vector3(0, 0.02f, 0.1f), 11, Color.white, rotation);
    }

    void CreateAreaSign(string text, Vector3 pos, Color color)
    {
        CreateCube("Zone_Back_" + text, pos + new Vector3(0, 0, -0.08f), new Vector3(4.4f, 0.55f, 0.06f), Mat("Mat_Zone_Back_" + text, new Color(color.r * 0.08f, color.g * 0.08f, color.b * 0.08f), 0.18f));
        CreateCube("Zone_Color_Bar_" + text, pos + new Vector3(0, -0.31f, -0.035f), new Vector3(3.6f, 0.05f, 0.035f), Mat("Mat_Zone_Bar_" + text, color, 0.65f));
        CreateCube("Zone_Color_Dot_" + text, pos + new Vector3(-1.92f, 0, -0.025f), new Vector3(0.12f, 0.42f, 0.035f), Mat("Mat_Zone_Dot_" + text, color, 0.65f));
        CreateWallLabel("ZoneLabel_" + text, text, pos + new Vector3(0.16f, 0.04f, 0.02f), 22, color, Quaternion.identity);
    }

    void CreateWallLabel(string name, string content, Vector3 pos, int fontSize, Color color, Quaternion rotation)
    {
        GameObject label = new GameObject(name);
        label.transform.position = pos;
        // TextMesh is mirrored when viewed from its back side. Flip the label
        // once here so every wall label presents its readable face to visitors.
        label.transform.rotation = rotation * Quaternion.Euler(0f, 180f, 0f);
        label.transform.localScale = Vector3.one * 0.08f;

        TextMesh text = label.AddComponent<TextMesh>();
        text.text = content;
        text.font = GetDisplayFont();
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;

        MeshRenderer renderer = label.GetComponent<MeshRenderer>();
        if (renderer != null && text.font != null)
        {
            renderer.sharedMaterial = text.font.material;
        }
    }

    void CreatePedestal(string name, Vector3 pos, Color color)
    {
        CreateCylinder(null, name + "_Base", pos, new Vector3(0.72f, 0.48f, 0.72f), Mat("Mat_Pedestal_Base", new Color(0.16f, 0.17f, 0.2f), 0.05f));
        CreateCylinder(null, name + "_GlowTop", pos + Vector3.up * 0.5f, new Vector3(0.82f, 0.035f, 0.82f), Mat("Mat_Pedestal_Glow_" + name, color, 0.9f));
        CreateCylinder(null, name + "_Lower_Ring", pos + new Vector3(0f, -0.45f, 0f), new Vector3(0.88f, 0.035f, 0.88f), Mat("Mat_Pedestal_Lower_Ring_" + name, color, 0.42f));
        CreateCylinder(null, name + "_Upper_Metal_Rim", pos + new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 0.018f, 0.9f), Mat("Mat_Pedestal_Metal_Rim", new Color(0.72f, 0.76f, 0.82f), 0.1f));
        CreateCube(name + "_Nameplate", pos + new Vector3(0f, 0.08f, -0.74f), new Vector3(0.92f, 0.18f, 0.035f), Mat("Mat_Pedestal_Nameplate_" + name, new Color(color.r * 0.35f, color.g * 0.35f, color.b * 0.35f), 0.26f));
    }

    void CreateExhibitShowcase(string name, Vector3 exhibitPos, Color accent, bool useGlassColumn)
    {
        Material glass = Mat("Mat_Showcase_Glass_" + name, new Color(0.48f, 0.78f, 1f, 0.12f), 0.18f);
        SetupTransparent(glass, 0.12f);
        Material rim = Mat("Mat_Showcase_Rim_" + name, accent, 0.6f);
        Material chrome = Mat("Mat_Showcase_Chrome_" + name, new Color(0.74f, 0.78f, 0.84f), 0.12f);

        Vector3 center = new Vector3(exhibitPos.x, 1.02f, exhibitPos.z);
        GameObject lower = CreateCylinder(null, name + "_Lower_Display_Ring", center + new Vector3(0f, -0.12f, 0f), new Vector3(1.1f, 0.018f, 1.1f), rim);
        GameObject upper = CreateCylinder(null, name + "_Upper_Display_Ring", center + new Vector3(0f, 0.98f, 0f), new Vector3(0.92f, 0.012f, 0.92f), rim);
        RemoveCollider(lower);
        RemoveCollider(upper);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
            Vector3 p = center + new Vector3(Mathf.Cos(angle) * 0.78f, 0.42f, Mathf.Sin(angle) * 0.78f);
            GameObject post = CreateCylinder(null, name + "_Chrome_Post_" + i, p, new Vector3(0.018f, 0.56f, 0.018f), chrome);
            RemoveCollider(post);
        }

        if (useGlassColumn)
        {
            GameObject glassColumn = CreateCylinder(null, name + "_Glass_Column", center + new Vector3(0f, 0.42f, 0f), new Vector3(0.86f, 0.5f, 0.86f), glass);
            RemoveCollider(glassColumn);
        }
    }

    void CreateStarField()
    {
        Material starMat = Mat("Mat_Star", Color.white, 1.2f);
        Random.InitState(42);
        for (int i = 0; i < 150; i++) CreateSphere(null, "Star_" + i, new Vector3(Random.Range(-14f, 14f), Random.Range(4.72f, 4.96f), Random.Range(-11.6f, 11.6f)), Vector3.one * Random.Range(0.025f, 0.07f), starMat);
    }

    void CreateFloorLines()
    {
        Material lineMat = Mat("Mat_Floor_Lines", new Color(0.08f, 0.48f, 0.95f), 0.48f);
        for (int i = -2; i <= 2; i++) CreateCube("Floor_Guide_X_" + i, new Vector3(i * 4.8f, 0.012f, 0), new Vector3(0.035f, 0.02f, 21.5f), lineMat);
        CreateCube("Floor_Guide_Center", new Vector3(0, 0.014f, -0.2f), new Vector3(25f, 0.02f, 0.035f), lineMat);
    }

    void CreateArchitecturalLightBands()
    {
        Material coolBand = Mat("Mat_Ceiling_Light_Band", new Color(0.5f, 0.76f, 1f), 1.35f);
        Material warmBand = Mat("Mat_Wall_Warm_Band", new Color(1f, 0.78f, 0.42f), 0.85f);
        Material violetBand = Mat("Mat_Wall_Violet_Band", new Color(0.78f, 0.56f, 1f), 0.9f);

        CreateCube("Ceiling_Light_Center", new Vector3(0f, 4.93f, 0f), new Vector3(0.08f, 0.035f, 21.2f), coolBand);
        CreateCube("Ceiling_Light_Left", new Vector3(-7.2f, 4.93f, 0f), new Vector3(0.055f, 0.03f, 19.2f), coolBand);
        CreateCube("Ceiling_Light_Right", new Vector3(7.2f, 4.93f, 0f), new Vector3(0.055f, 0.03f, 19.2f), coolBand);

        CreateCube("North_Wall_Light_Band", new Vector3(0f, 1.05f, -12.36f), new Vector3(25.5f, 0.045f, 0.04f), coolBand);
        CreateCube("South_Wall_Light_Band", new Vector3(0f, 1.05f, 12.36f), new Vector3(25.5f, 0.045f, 0.04f), coolBand);
        CreateCube("West_Wall_Warm_Band", new Vector3(-14.86f, 1.15f, 0f), new Vector3(0.04f, 0.045f, 20.5f), warmBand);
        CreateCube("East_Wall_Violet_Band", new Vector3(14.86f, 1.15f, 0f), new Vector3(0.04f, 0.045f, 20.5f), violetBand);
    }

    void CreatePillars()
    {
        Material pillarMat = Mat("Mat_Pillar", new Color(0.13f, 0.14f, 0.18f), 0.08f);
        Material glowMat = Mat("Mat_Pillar_Glow", new Color(0.22f, 0.5f, 1f), 0.6f);
        Vector3[] points = { new Vector3(-13.3f, 2.4f, -10.7f), new Vector3(13.3f, 2.4f, -10.7f), new Vector3(-13.3f, 2.4f, 10.7f), new Vector3(13.3f, 2.4f, 10.7f) };
        for (int i = 0; i < points.Length; i++)
        {
            CreateCylinder(null, "Pillar_" + i, points[i], new Vector3(0.36f, 2.4f, 0.36f), pillarMat);
            CreateCylinder(null, "Pillar_Glow_" + i, points[i] + Vector3.up * 0.05f, new Vector3(0.39f, 0.025f, 0.39f), glowMat);
        }
    }

    Text CreateUIText(Transform parent, string name, string content, Vector2 pos, Vector2 size, int fontSize, Color color, bool bold, TextAnchor anchor)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform));
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.font = GetDisplayFont();
        return text;
    }

    GameObject CreateUIPanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = panel.AddComponent<Image>();
        img.color = color;
        return panel;
    }

    void CreateUIButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = CreateUIPanel(parent, name, pos, size, new Color(0.12f, 0.36f, 0.78f, 0.95f));
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.2f, 0.5f, 1f, 1f);
        colors.pressedColor = new Color(0.08f, 0.24f, 0.55f, 1f);
        btn.colors = colors;
        CreateUIText(btnObj.transform, "Label", label, Vector2.zero, size, 22, Color.white, true, TextAnchor.MiddleCenter);
    }

    Font GetDisplayFont()
    {
        if (cachedDisplayFont != null) return cachedDisplayFont;
        string[] preferredFonts = { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial Unicode MS", "Arial" };
        cachedDisplayFont = Font.CreateDynamicFontFromOSFont(preferredFonts, 28);
        if (cachedDisplayFont == null) cachedDisplayFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return cachedDisplayFont;
    }

    void CreateZone(string name, string zoneName, string desc, Vector3 pos, Vector3 size)
    {
        GameObject zone = new GameObject(name);
        zone.transform.position = pos;
        BoxCollider bc = zone.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = size;
        ZoneTrigger zt = zone.AddComponent<ZoneTrigger>();
        zt.zoneName = zoneName;
        zt.zoneDescription = desc;
    }

    GameObject CreateCube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        SetMaterial(obj, mat);
        return obj;
    }

    GameObject CreatePanelCube(string name, Vector3 pos, Vector3 scale, Quaternion rotation, Material mat)
    {
        GameObject obj = CreateCube(name, pos, scale, mat);
        obj.transform.rotation = rotation;
        return obj;
    }

    GameObject CreateCube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = localScale;
        SetMaterial(obj, mat);
        return obj;
    }

    GameObject CreateCylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, Quaternion? rotation = null)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = pos;
        }
        else
        {
            obj.transform.position = pos;
        }
        obj.transform.localScale = scale;
        obj.transform.localRotation = rotation ?? Quaternion.identity;
        SetMaterial(obj, mat);
        return obj;
    }

    GameObject CreateSphere(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = name;
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = pos;
        }
        else
        {
            obj.transform.position = pos;
        }
        obj.transform.localScale = scale;
        SetMaterial(obj, mat);
        return obj;
    }

    GameObject CreateStrut(Transform parent, string name, Vector3 start, Vector3 end, float thickness, Material mat)
    {
        Vector3 mid = (start + end) * 0.5f;
        Vector3 direction = end - start;
        GameObject obj = CreateCylinder(parent, name, mid, new Vector3(thickness, direction.magnitude * 0.5f, thickness), mat);
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        return obj;
    }

    Material Mat(string name, Color color, float emission = 0f)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        mat.SetFloat("_Glossiness", emission > 0f ? 0.62f : 0.46f);
        if (name.Contains("Metal") || name.Contains("Chrome") || name.Contains("Rim") || name.Contains("Station") || name.Contains("Satellite") || name.Contains("Probe"))
        {
            mat.SetFloat("_Metallic", 0.35f);
            mat.SetFloat("_Glossiness", 0.72f);
        }
        if (emission > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emission);
        }
        return mat;
    }

    void SetupTransparent(Material mat, float alpha)
    {
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    void SetMaterial(GameObject obj, Material mat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null) renderer.material = mat;
    }

    void RemoveCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
    }

    string Cn(string value)
    {
        return value;
    }
}



