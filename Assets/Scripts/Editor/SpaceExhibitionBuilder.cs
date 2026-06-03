using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 太空探索展览馆一键搭建工具
/// 在Unity编辑器中：Tools → 搭建太空探索展览馆
/// </summary>
public class SpaceExhibitionBuilder : EditorWindow
{
    [MenuItem("Tools/搭建太空探索展览馆")]
    public static void ShowWindow()
    {
        GetWindow<SpaceExhibitionBuilder>("太空展览馆搭建");
    }

    void OnGUI()
    {
        GUILayout.Label("太空探索虚拟展览馆 - 一键搭建工具", EditorStyles.boldLabel);
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "点击下方按钮将自动创建：\n" +
            "• 展览馆建筑（地面/墙壁/天花板）\n" +
            "• 3个展区（火箭技术区 / 行星探索区 / 深空未来区）\n" +
            "• 6件太空展品\n" +
            "• 完整的UI系统\n" +
            "• 玩家控制器\n" +
            "• 音频系统\n" +
            "• 光照系统",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("🚀 一键搭建太空展览馆", GUILayout.Height(40)))
        {
            BuildScene();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("🗑 清除现有场景物体", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("确认清除",
                "将删除场景中所有物体（不包括脚本文件），确定吗？", "确定", "取消"))
            {
                ClearScene();
            }
        }
    }

    void BuildScene()
    {
        if (EditorUtility.DisplayDialog("确认搭建",
            "将自动创建完整的太空展览馆场景。\n如果场景中已有物体，建议先清除。\n\n确定继续吗？", "开始搭建", "取消"))
        {
            BuildEnvironment();
            BuildLighting();
            BuildExhibitionAreas();
            BuildExhibits();
            BuildPlayer();
            BuildUI();
            BuildAudio();
            BuildZoneTriggers();
            Debug.Log("✅ 太空探索展览馆搭建完成！点击 Play 开始体验。");
        }
    }

    void ClearScene()
    {
        // 删除所有顶层GameObject
        var allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.transform.parent == null && obj != null)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }
    }

    #region 环境搭建

    void BuildEnvironment()
    {
        // === 地面 ===
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(3, 1, 2.5f);
        Material floorMat = CreateMaterial("Mat_Floor", new Color(0.15f, 0.15f, 0.2f));
        SetMaterial(floor, floorMat);

        // === 天花板 ===
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.position = new Vector3(0, 5, 0);
        ceiling.transform.localScale = new Vector3(30, 0.1f, 25);
        Material ceilingMat = CreateMaterial("Mat_Ceiling", new Color(0.05f, 0.05f, 0.1f));
        SetMaterial(ceiling, ceilingMat);

        // === 墙壁 ===
        CreateWall("Wall_North", new Vector3(0, 2.5f, -12.5f), new Vector3(30, 5, 0.2f));
        CreateWall("Wall_South", new Vector3(0, 2.5f, 12.5f), new Vector3(30, 5, 0.2f));
        CreateWall("Wall_East", new Vector3(15, 2.5f, 0), new Vector3(0.2f, 5, 25));
        CreateWall("Wall_West", new Vector3(-15, 2.5f, 0), new Vector3(0.2f, 5, 25));

        // === 内部隔墙（划分展区）===
        CreateWall("Divider_A", new Vector3(-5, 2.5f, -4), new Vector3(0.2f, 5, 8));
        CreateWall("Divider_B", new Vector3(5, 2.5f, -4), new Vector3(0.2f, 5, 8));

        // === 装饰柱 ===
        for (int i = 0; i < 4; i++)
        {
            float x = (i % 2 == 0) ? -12 : 12;
            float z = (i < 2) ? -10 : 10;
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar_" + i;
            pillar.transform.position = new Vector3(x, 2.5f, z);
            pillar.transform.localScale = new Vector3(0.4f, 5, 0.4f);
            Material pillarMat = CreateMaterial("Mat_Pillar", new Color(0.2f, 0.2f, 0.25f));
            SetMaterial(pillar, pillarMat);
        }
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        Material wallMat = CreateMaterial("Mat_Wall", new Color(0.12f, 0.12f, 0.18f));
        SetMaterial(wall, wallMat);
    }

    void BuildLighting()
    {
        // === 环境光设置 ===
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.08f, 0.15f);

        // === 主方向光（模拟天窗）===
        GameObject dirLight = new GameObject("Directional Light");
        Light dl = dirLight.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.intensity = 0.4f;
        dl.color = new Color(0.8f, 0.85f, 1f); // 冷白色（太空感）
        dl.shadows = LightShadows.Soft;
        dl.shadowStrength = 0.5f;
        dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);

        // === 展区聚光灯 ===
        CreateSpotLight("Spotlight_A", new Vector3(-10, 4.5f, -6), new Color(0.3f, 0.5f, 1f));  // 蓝色 - 火箭区
        CreateSpotLight("Spotlight_B", new Vector3(0, 4.5f, -6), new Color(1f, 0.6f, 0.2f));    // 橙色 - 行星区
        CreateSpotLight("Spotlight_C", new Vector3(10, 4.5f, -6), new Color(0.5f, 0.3f, 1f));   // 紫色 - 深空区

        // === 展台点光源 ===
        float[] exhibitX = { -10, -10, 0, 0, 10, 10 };
        float[] exhibitZ = { -2, 2, -2, 2, -2, 2 };
        for (int i = 0; i < 6; i++)
        {
            CreatePointLight("ExhibitLight_" + i, new Vector3(exhibitX[i], 2.5f, exhibitZ[i]),
                new Color(1f, 0.95f, 0.8f));
        }

        // === 环境氛围灯 ===
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * 8;
            float z = Mathf.Sin(angle) * 6;
            CreatePointLight("AmbientLight_" + i, new Vector3(x, 4f, z),
                new Color(0.1f, 0.15f, 0.3f), 0.3f, 8f);
        }
    }

    void CreateSpotLight(string name, Vector3 pos, Color color)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.position = pos;
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Spot;
        l.color = color;
        l.intensity = 1.5f;
        l.range = 10;
        l.spotAngle = 45;
        l.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void CreatePointLight(string name, Vector3 pos, Color color, float intensity = 0.8f, float range = 5f)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.position = pos;
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
    }

    #endregion

    #region 展区与展台

    void BuildExhibitionAreas()
    {
        // 展区A: 火箭技术区 (左侧, x=-10)
        CreateAreaSign("火箭技术区", new Vector3(-10, 3.5f, -6.5f), new Color(0.3f, 0.5f, 1f));
        CreatePedestal("Pedestal_A1", new Vector3(-10, 0.4f, -2));
        CreatePedestal("Pedestal_A2", new Vector3(-10, 0.4f, 2));

        // 展区B: 行星探索区 (中间, x=0)
        CreateAreaSign("行星探索区", new Vector3(0, 3.5f, -6.5f), new Color(1f, 0.6f, 0.2f));
        CreatePedestal("Pedestal_B1", new Vector3(0, 0.4f, -2));
        CreatePedestal("Pedestal_B2", new Vector3(0, 0.4f, 2));

        // 展区C: 深空未来区 (右侧, x=10)
        CreateAreaSign("深空未来区", new Vector3(10, 3.5f, -6.5f), new Color(0.5f, 0.3f, 1f));
        CreatePedestal("Pedestal_C1", new Vector3(10, 0.4f, -2));
        CreatePedestal("Pedestal_C2", new Vector3(10, 0.4f, 2));

        // === 入口导览墙 ===
        GameObject infoWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        infoWall.name = "InfoWall";
        infoWall.transform.position = new Vector3(0, 2.5f, 10);
        infoWall.transform.localScale = new Vector3(8, 3, 0.2f);
        Material infoMat = CreateMaterial("Mat_InfoWall", new Color(0.08f, 0.08f, 0.12f));
        SetMaterial(infoWall, infoMat);

        // 导览文字
        GameObject infoText = new GameObject("WelcomeText");
        infoText.transform.position = new Vector3(0, 2.5f, 9.8f);
        TextMesh tm = infoText.AddComponent<TextMesh>();
        tm.text = "🚀 太空探索虚拟展览馆\n\nSpace Exploration Virtual Museum\n\nWASD移动 | 鼠标旋转视角 | E键交互";
        tm.fontSize = 18;
        tm.color = Color.white;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        infoText.AddComponent<Billboard>();
    }

    void CreateAreaSign(string name, Vector3 pos, Color color)
    {
        GameObject signObj = new GameObject("Sign_" + name);
        signObj.transform.position = pos;

        TextMesh tm = signObj.AddComponent<TextMesh>();
        tm.text = name;
        tm.fontSize = 24;
        tm.color = color;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.fontStyle = FontStyle.Bold;

        signObj.AddComponent<Billboard>();
    }

    void CreatePedestal(string name, Vector3 pos)
    {
        // 展台底座
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = name + "_Base";
        baseObj.transform.position = pos;
        baseObj.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
        Material baseMat = CreateMaterial("Mat_Pedestal", new Color(0.25f, 0.25f, 0.3f));
        SetMaterial(baseObj, baseMat);

        // 展台顶部（发光环效果）
        GameObject topObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        topObj.name = name + "_Top";
        topObj.transform.position = pos + Vector3.up * 0.75f;
        topObj.transform.localScale = new Vector3(0.55f, 0.03f, 0.55f);
        Material topMat = CreateMaterial("Mat_PedestalTop", new Color(0.3f, 0.35f, 0.5f));
        topMat.SetColor("_EmissionColor", new Color(0.15f, 0.2f, 0.4f));
        topMat.EnableKeyword("_EMISSION");
        SetMaterial(topObj, topMat);
    }

    #endregion

    #region 展品创建

    void BuildExhibits()
    {
        // === 展区A: 火箭技术区 ===
        CreateRocketModel("长征火箭模型", new Vector3(-10, 1.4f, -2),
            "中国长征系列运载火箭的经典模型。长征火箭是中国航天事业的骄傲，已成功完成数百次发射任务，将卫星、空间站舱段和深空探测器送入预定轨道。",
            "火箭发射场景还原了真实发射的紧张与震撼",
            "靠近观看火箭结构细节，按E查看详情",
            "通过虚拟展览近距离接触国之重器",
            true);

        CreateSatelliteModel("人造卫星模型", new Vector3(-10, 1.4f, 2),
            "通信卫星的3D模型展示。卫星是人类探索太空的"千里眼"，它们环绕地球运行，承担着通信、导航、气象观测和科学研究等重要使命。",
            "卫星在太空中缓缓旋转，模拟轨道运行",
            "按E拾取卫星，360度观察太阳能板结构",
            "体验在太空中近距离观察卫星的奇妙感受",
            true);

        // === 展区B: 行星探索区 ===
        CreatePlanetModel("火星", new Vector3(0, 1.4f, -2),
            "太阳系第四行星——火星。这颗红色星球是人类最有可能移民的地外行星。表面遍布陨石坑、峡谷和火山，拥有太阳系最高的山——奥林帕斯山（21.9公里）。",
            "置身红色星球表面，感受异星世界的荒凉与壮美",
            "按E拾取火星模型，旋转观察其标志性地貌",
            "想象人类未来在火星建立城市的场景",
            true);

        CreatePlanetModel("土星", new Vector3(0, 1.4f, 2),
            "太阳系最美的行星——土星。以其壮观的光环系统闻名于世，光环由数十亿冰粒和岩石碎片组成，跨度达28万公里，但厚度仅约10米。",
            "土星光环缓缓旋转，营造梦幻的太空氛围",
            "按E拾取模型，从不同角度欣赏光环的壮丽",
            "在虚拟空间近距离欣赏天文奇观",
            true);

        // === 展区C: 深空未来区 ===
        CreateSpaceStationModel("国际空间站模型", new Vector3(10, 1.4f, -2),
            "国际空间站（ISS）是人类在太空中最大的科研设施，由多国合作建造。它在距地面约400公里的轨道上运行，宇航员在此进行微重力科学实验。",
            "模拟空间站内部环境，体验宇航员工作场景",
            "按E拾取，拖拽旋转查看各舱段结构",
            "构想未来大型太空城市的蓝图",
            true);

        CreateDeepSpaceModel("深空探测器模型", new Vector3(10, 1.4f, 2),
            "旅行者号深空探测器的致敬模型。它已飞行超过45年，穿越太阳系边缘，携带着人类文明的"金唱片"驶向星际空间，是人类飞得最远的探测器。",
            "感受探测器在无尽深空中航行的孤独与壮丽",
            "按E查看探测器的科学仪器和金唱片信息",
            "想象外星文明收到人类问候的那一刻",
            false); // 只查看不拾取
    }

    void CreateRocketModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup)
    {
        GameObject rocket = new GameObject(name);
        rocket.transform.position = pos;

        // 箭体
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(rocket.transform);
        body.transform.localPosition = new Vector3(0, 0.8f, 0);
        body.transform.localScale = new Vector3(0.15f, 1.2f, 0.15f);
        Material bodyMat = CreateMaterial("Mat_Rocket", Color.white);
        SetMaterial(body, bodyMat);

        // 整流罩（顶部锥形）
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nose.name = "Nose";
        nose.transform.SetParent(rocket.transform);
        nose.transform.localPosition = new Vector3(0, 1.5f, 0);
        nose.transform.localScale = new Vector3(0.01f, 0.4f, 0.15f);
        Material noseMat = CreateMaterial("Mat_RocketNose", new Color(0.9f, 0.2f, 0.1f));
        SetMaterial(nose, noseMat);

        // 助推器
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            float bx = Mathf.Cos(angle) * 0.22f;
            float bz = Mathf.Sin(angle) * 0.22f;
            GameObject booster = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            booster.name = "Booster_" + i;
            booster.transform.SetParent(rocket.transform);
            booster.transform.localPosition = new Vector3(bx, 0.3f, bz);
            booster.transform.localScale = new Vector3(0.06f, 0.6f, 0.06f);
            Material bMat = CreateMaterial("Mat_Booster", new Color(0.7f, 0.7f, 0.7f));
            SetMaterial(booster, bMat);
        }

        // 尾焰
        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flame.name = "Flame";
        flame.transform.SetParent(rocket.transform);
        flame.transform.localPosition = new Vector3(0, -0.2f, 0);
        flame.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
        Material flameMat = CreateMaterial("Mat_Flame", new Color(1f, 0.5f, 0.1f));
        flameMat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f));
        flameMat.EnableKeyword("_EMISSION");
        SetMaterial(flame, flameMat);

        // 添加展品脚本
        AddExhibitScript(rocket, name, desc, im, inter, img, pickup, 15f);
    }

    void CreateSatelliteModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup)
    {
        GameObject satellite = new GameObject(name);
        satellite.transform.position = pos;

        // 主体
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(satellite.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Material bodyMat = CreateMaterial("Mat_Satellite", new Color(0.8f, 0.8f, 0.85f));
        SetMaterial(body, bodyMat);

        // 太阳能板
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "SolarPanel_" + (side > 0 ? "R" : "L");
            panel.transform.SetParent(satellite.transform);
            panel.transform.localPosition = new Vector3(side * 0.5f, 0, 0);
            panel.transform.localScale = new Vector3(0.5f, 0.02f, 0.2f);
            Material panelMat = CreateMaterial("Mat_SolarPanel", new Color(0.1f, 0.15f, 0.3f));
            SetMaterial(panel, panelMat);
        }

        // 天线
        GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "Antenna";
        antenna.transform.SetParent(satellite.transform);
        antenna.transform.localPosition = new Vector3(0, 0.3f, 0);
        antenna.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);
        Material antMat = CreateMaterial("Mat_Antenna", new Color(0.6f, 0.6f, 0.6f));
        SetMaterial(antenna, antMat);

        // 天线碟
        GameObject dish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dish.name = "Dish";
        dish.transform.SetParent(satellite.transform);
        dish.transform.localPosition = new Vector3(0, 0.48f, 0);
        dish.transform.localScale = new Vector3(0.15f, 0.05f, 0.15f);
        SetMaterial(dish, antMat);

        AddExhibitScript(satellite, name, desc, im, inter, img, pickup, 20f);
    }

    void CreatePlanetModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup)
    {
        GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planet.name = name;
        planet.transform.position = pos;
        planet.transform.localScale = Vector3.one * 0.6f;

        Color planetColor = name == "火星" ? new Color(0.8f, 0.3f, 0.15f) : new Color(0.85f, 0.75f, 0.5f);
        Material mat = CreateMaterial("Mat_" + name, planetColor);

        // 土星加光环
        if (name == "土星")
        {
            SetMaterial(planet, mat);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(planet.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(1.5f, 0.02f, 1.5f);
            Material ringMat = CreateMaterial("Mat_Ring", new Color(0.7f, 0.65f, 0.5f, 0.7f));
            SetMaterial(ring, ringMat);
        }
        else
        {
            SetMaterial(planet, mat);
        }

        // 火星加陨石坑装饰
        if (name == "火星")
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject crater = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crater.name = "Crater_" + i;
                crater.transform.SetParent(planet.transform);
                crater.transform.localPosition = Random.onUnitSphere * 0.4f;
                crater.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);
                Material craterMat = CreateMaterial("Mat_Crater", new Color(0.5f, 0.15f, 0.05f));
                SetMaterial(crater, craterMat);
            }
        }

        AddExhibitScript(planet, name, desc, im, inter, img, pickup, 10f);
    }

    void CreateSpaceStationModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup)
    {
        GameObject station = new GameObject(name);
        station.transform.position = pos;

        // 中央桁架
        GameObject truss = GameObject.CreatePrimitive(PrimitiveType.Cube);
        truss.name = "Truss";
        truss.transform.SetParent(station.transform);
        truss.transform.localPosition = Vector3.zero;
        truss.transform.localScale = new Vector3(1.2f, 0.05f, 0.05f);
        Material trussMat = CreateMaterial("Mat_Truss", new Color(0.7f, 0.7f, 0.75f));
        SetMaterial(truss, trussMat);

        // 太阳能板
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "Panel_" + (side > 0 ? "R" : "L") + "_" + i;
                panel.transform.SetParent(station.transform);
                panel.transform.localPosition = new Vector3(side * 0.3f + side * i * 0.3f, 0, 0);
                panel.transform.localScale = new Vector3(0.2f, 0.01f, 0.4f);
                Material panelMat = CreateMaterial("Mat_SSPanel", new Color(0.05f, 0.1f, 0.25f));
                SetMaterial(panel, panelMat);
            }
        }

        // 居住舱
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject module = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            module.name = "Module_" + (i > 0 ? "R" : "L");
            module.transform.SetParent(station.transform);
            module.transform.localPosition = new Vector3(i * 0.35f, 0, 0);
            module.transform.localRotation = Quaternion.Euler(0, 0, 90);
            module.transform.localScale = new Vector3(0.15f, 0.2f, 0.15f);
            Material modMat = CreateMaterial("Mat_Module", new Color(0.85f, 0.85f, 0.9f));
            SetMaterial(module, modMat);
        }

        // 中央节点舱
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        node.name = "NodeModule";
        node.transform.SetParent(station.transform);
        node.transform.localPosition = Vector3.zero;
        node.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        Material nodeMat = CreateMaterial("Mat_Node", new Color(0.8f, 0.8f, 0.85f));
        SetMaterial(node, nodeMat);

        AddExhibitScript(station, name, desc, im, inter, img, pickup, 8f);
    }

    void CreateDeepSpaceModel(string name, Vector3 pos, string desc, string im, string inter, string img, bool pickup)
    {
        GameObject probe = new GameObject(name);
        probe.transform.position = pos;

        // 主体
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(probe.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Material bodyMat = CreateMaterial("Mat_ProbeBody", new Color(0.7f, 0.7f, 0.7f));
        SetMaterial(body, bodyMat);

        // 碟形天线
        GameObject dish = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dish.name = "Dish";
        dish.transform.SetParent(probe.transform);
        dish.transform.localPosition = new Vector3(0, 0, 0.3f);
        dish.transform.localRotation = Quaternion.Euler(90, 0, 0);
        dish.transform.localScale = new Vector3(0.35f, 0.03f, 0.35f);
        Material dishMat = CreateMaterial("Mat_ProbeDish", new Color(0.85f, 0.85f, 0.85f));
        SetMaterial(dish, dishMat);

        // 天线杆
        GameObject boom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        boom.name = "Boom";
        boom.transform.SetParent(probe.transform);
        boom.transform.localPosition = new Vector3(0, 0, 0.15f);
        boom.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);
        SetMaterial(boom, dishMat);

        // RTG（核电池）
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject rtg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rtg.name = "RTG_" + (side > 0 ? "R" : "L");
            rtg.transform.SetParent(probe.transform);
            rtg.transform.localPosition = new Vector3(side * 0.25f, -0.15f, 0);
            rtg.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
            Material rtgMat = CreateMaterial("Mat_RTG", new Color(0.2f, 0.2f, 0.25f));
            SetMaterial(rtg, rtgMat);
        }

        // 金唱片（金色小方块）
        GameObject record = GameObject.CreatePrimitive(PrimitiveType.Cube);
        record.name = "GoldenRecord";
        record.transform.SetParent(probe.transform);
        record.transform.localPosition = new Vector3(0, 0.2f, 0.2f);
        record.transform.localScale = new Vector3(0.08f, 0.08f, 0.01f);
        Material recordMat = CreateMaterial("Mat_Record", new Color(1f, 0.84f, 0f));
        SetMaterial(record, recordMat);

        AddExhibitScript(probe, name, desc, im, inter, img, pickup, 15f);
    }

    void AddExhibitScript(GameObject obj, string name, string desc, string im, string inter, string img, bool pickup, float rotSpeed)
    {
        // 添加碰撞体
        SphereCollider sc = obj.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 3f;
        sc.center = Vector3.zero;

        if (pickup)
        {
            ExhibitPickup ep = obj.AddComponent<ExhibitPickup>();
            ep.exhibitName = name;
            ep.exhibitDescription = desc;
            ep.hintText = "按 E 查看详情";
            ep.interactionDistance = 3f;
            ep.autoRotateSpeed = rotSpeed;
            ep.immersionNote = im;
            ep.interactionNote = inter;
            ep.imaginationNote = img;
            ep.pickupDistance = 1.5f;
            ep.pickupScale = Vector3.one;
            ep.rotationSpeed = 100f;
            ep.minZoom = 0.5f;
            ep.maxZoom = 3f;
        }
        else
        {
            ExhibitBase eb = obj.AddComponent<ExhibitBase>();
            eb.exhibitName = name;
            eb.exhibitDescription = desc;
            eb.hintText = "按 E 查看详情";
            eb.interactionDistance = 3f;
            eb.autoRotateSpeed = rotSpeed;
            eb.immersionNote = im;
            eb.interactionNote = inter;
            eb.imaginationNote = img;
        }
    }

    #endregion

    #region 玩家

    void BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, 0, 8); // 入口位置

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);
        cc.height = 2;
        cc.radius = 0.3f;

        player.AddComponent<FirstPersonController>();

        // 相机
        GameObject camObj = new GameObject("Main Camera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();
    }

    #endregion

    #region UI系统

    void BuildUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // === 开始面板 ===
        GameObject startPanel = CreateUIPanel(canvasObj.transform, "StartPanel",
            new Vector2(0, 0), new Vector2(1920, 1080), new Color(0, 0, 0, 0.85f));

        // 标题
        CreateUIText(startPanel.transform, "StartTitle", "🚀 太空探索虚拟展览馆",
            new Vector2(0, 150), new Vector2(800, 80), 42, Color.white, true, TextAnchor.MiddleCenter);

        // 副标题
        CreateUIText(startPanel.transform, "StartSubtitle", "Space Exploration Virtual Museum",
            new Vector2(0, 70), new Vector2(600, 40), 22, new Color(0.6f, 0.7f, 1f), false, TextAnchor.MiddleCenter);

        // 开始按钮
        CreateUIButton(startPanel.transform, "StartBtn", "开始参观",
            new Vector2(0, -40), new Vector2(220, 55), Color.white,
            () => { GameManager.Instance?.StartGame(); });

        // 退出按钮
        CreateUIButton(startPanel.transform, "QuitBtn", "退出",
            new Vector2(0, -120), new Vector2(220, 55), Color.white,
            () => { GameManager.Instance?.QuitGame(); });

        // === 信息面板 ===
        GameObject infoPanel = CreateUIPanel(canvasObj.transform, "InfoPanel",
            new Vector2(0, 0), new Vector2(560, 420), new Color(0, 0, 0, 0.85f));

        Text titleText = CreateUIText(infoPanel.transform, "InfoTitle", "展品名称",
            new Vector2(0, 160), new Vector2(500, 50), 28, Color.white, true, TextAnchor.MiddleCenter);

        Text descText = CreateUIText(infoPanel.transform, "InfoDesc", "展品描述",
            new Vector2(0, 20), new Vector2(500, 160), 16, new Color(0.9f, 0.9f, 0.9f), false, TextAnchor.UpperCenter);

        // 3I标签文字（描述下方）
        Text threeIText = CreateUIText(infoPanel.transform, "Info3I", "",
            new Vector2(0, -80), new Vector2(500, 100), 14, new Color(0.5f, 0.7f, 1f), false, TextAnchor.UpperCenter);

        CreateUIButton(infoPanel.transform, "InfoCloseBtn", "关闭",
            new Vector2(0, -170), new Vector2(160, 45), Color.white,
            () => { GameManager.Instance?.HideExhibitInfo(); });

        infoPanel.SetActive(false);

        // === 交互提示 ===
        Text hintText = CreateUIText(canvasObj.transform, "HintText", "",
            new Vector2(0, -380), new Vector2(500, 40), 20, Color.white, false, TextAnchor.MiddleCenter);
        hintText.gameObject.SetActive(false);

        // === 暂停面板 ===
        GameObject pausePanel = CreateUIPanel(canvasObj.transform, "PausePanel",
            new Vector2(0, 0), new Vector2(400, 300), new Color(0, 0, 0, 0.8f));

        CreateUIText(pausePanel.transform, "PauseTitle", "暂停",
            new Vector2(0, 80), new Vector2(300, 50), 36, Color.white, true, TextAnchor.MiddleCenter);

        CreateUIButton(pausePanel.transform, "ResumeBtn", "继续",
            new Vector2(0, 0), new Vector2(180, 45), Color.white,
            () => { GameManager.Instance?.TogglePause(); });

        CreateUIButton(pausePanel.transform, "PauseQuitBtn", "退出",
            new Vector2(0, -70), new Vector2(180, 45), Color.white,
            () => { GameManager.Instance?.QuitGame(); });

        pausePanel.SetActive(false);

        // === 绑定到 GameManager ===
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

    GameObject CreateUIPanel(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = panel.AddComponent<Image>();
        img.color = color;
        return panel;
    }

    Text CreateUIText(Transform parent, string name, string content,
        Vector2 anchoredPos, Vector2 sizeDelta, int fontSize, Color color, bool bold, TextAnchor anchor)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform));
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.alignment = anchor;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return text;
    }

    void CreateUIButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 sizeDelta, Color textColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.3f, 0.5f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        // 按钮文字
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform lrt = labelObj.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.fontSize = 22;
        labelText.color = textColor;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    #endregion

    #region 音频系统

    void BuildAudio()
    {
        GameObject audioObj = new GameObject("AudioManager");
        AudioManager am = audioObj.AddComponent<AudioManager>();
        am.bgmVolume = 0.3f;
        am.ambientVolume = 0.15f;
        // 音频文件需要在Inspector中手动拖入
    }

    #endregion

    #region 区域触发器

    void BuildZoneTriggers()
    {
        CreateZone("Zone_RocketTech", "火箭技术区",
            "欢迎来到火箭技术展区。这里展示了人类进入太空的'座驾'——从长征火箭到现代运载工具，每一枚火箭都承载着人类探索宇宙的梦想。",
            new Vector3(-10, 1.5f, 0), new Vector3(5, 3, 6));

        CreateZone("Zone_PlanetExplore", "行星探索区",
            "欢迎来到行星探索展区。在这里，您可以近距离观察太阳系的壮丽行星，感受宇宙的宏大与人类的渺小。",
            new Vector3(0, 1.5f, 0), new Vector3(5, 3, 6));

        CreateZone("Zone_DeepSpace", "深空未来区",
            "欢迎来到深空未来展区。从国际空间站到深空探测器，人类正在一步步走向更遥远的星际空间。未来，就在前方。",
            new Vector3(10, 1.5f, 0), new Vector3(5, 3, 6));
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
        zt.triggerOnce = false;
    }

    #endregion

    #region 辅助方法

    Material CreateMaterial(string name, Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        return mat;
    }

    void SetMaterial(GameObject obj, Material mat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = mat;
        }
    }

    #endregion
}


