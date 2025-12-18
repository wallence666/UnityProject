using UnityEngine;
using System.Collections.Generic;
using TMPro;  // 确保引用 TextMeshPro

public class TemperatureLayerRenderer : MonoBehaviour
{
    public RoomDataManager roomManager;  // 拖入你的 RoomDataManager 空物体
    public Material temperatureMaterial; // 一个简单的 Unlit/Color 材质
    public GameObject temperatureLabelPrefab;  // 标签 Prefab，展示房间温度
    private List<GameObject> tempPlanes = new List<GameObject>();  // 存储房间平面
    private List<GameObject> temperatureLabels = new List<GameObject>();  // 存储温度标签
    public Gradient temperatureGradient;  // 温度颜色渐变

    void Start()
    {
        // 初始化颜色渐变
        InitializeDefaultGradient();
        
        // 初始化温度平面
        GenerateTemperaturePlanes();
    }

    /// <summary>
    /// 每个房间生成一个平面并着色
    /// </summary>
    void GenerateTemperaturePlanes()
    {
        if (roomManager == null)
        {
            Debug.LogError("TemperatureLayerRenderer: RoomManager 未设置！");
            return;
        }

        // 清理旧平面和标签
        foreach (var p in tempPlanes)
        {
            Destroy(p);
        }
        foreach (var label in temperatureLabels)
        {
            Destroy(label);
        }
        tempPlanes.Clear();
        temperatureLabels.Clear();

        // 遍历每个房间，生成对应的温度平面
        List<RoomData> rooms = roomManager.GetAllRooms();
        foreach (var room in rooms)
        {
            // 创建温度平面
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.name = $"TempPlane_{room.RoomType}";
            plane.transform.SetParent(transform);

            // 设置平面大小
            Vector3 size = room.Bounds.size;
            plane.transform.localScale = new Vector3(size.x, size.z, 1);

            // 设置平面位置
            plane.transform.position = new Vector3(
                room.Bounds.center.x,
                room.Bounds.center.y + room.Bounds.size.y + 0.1f, // 避免穿模
                room.Bounds.center.z
            );

            // 设置平面朝向
            plane.transform.rotation = Quaternion.Euler(90, 0, 0);

            // 获取材质并应用温度颜色
            Renderer renderer = plane.GetComponent<Renderer>();
            renderer.material = new Material(temperatureMaterial);
            renderer.material.color = temperatureGradient.Evaluate(room.Temperature / 100f);  // 归一化温度

            tempPlanes.Add(plane);

            // 创建并设置温度标签
            GameObject label = Instantiate(temperatureLabelPrefab, plane.transform);
            label.transform.position = plane.transform.position + Vector3.up * 0.2f;
            label.transform.rotation = Quaternion.Euler(90, 0, 0);

            var text = label.GetComponent<TextMeshPro>();
            text.text = $"{room.RoomType}\n{room.Temperature:F1} °C";  // 显示房间类型和温度
            temperatureLabels.Add(label);
        }
    }

    /// <summary>
    /// 初始化温度渐变（低温到高温）
    /// </summary>
    void InitializeDefaultGradient()
    {
        temperatureGradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        
        // 低温区：深蓝色到浅蓝色
        colorKeys[0].color = new Color(0.1f, 0.2f, 0.8f, 1f);
        colorKeys[0].time = 0.0f;
        
        colorKeys[1].color = new Color(0.3f, 0.5f, 1.0f, 1f);
        colorKeys[1].time = 0.25f;
        
        // 舒适区：绿色
        colorKeys[2].color = new Color(0.2f, 0.8f, 0.3f, 1f);
        colorKeys[2].time = 0.5f;
        
        // 温暖区：黄色到橙色
        colorKeys[3].color = new Color(1.0f, 0.8f, 0.2f, 1f);
        colorKeys[3].time = 0.75f;
        
        // 高温区：红色
        colorKeys[4].color = new Color(1.0f, 0.3f, 0.1f, 1f);
        colorKeys[4].time = 1.0f;
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;
        
        temperatureGradient.SetKeys(colorKeys, alphaKeys);
    }

    /// <summary>
    /// 每帧更新温度平面
    /// </summary>
    void UpdateTemperaturePlanes()
    {
        // 遍历每个房间并更新对应的平面和标签
        float minTemp=20f;
        float maxTemp=40f;

        List<RoomData> rooms = roomManager.GetAllRooms();
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomData room = rooms[i];
            GameObject plane = tempPlanes[i];
            GameObject label = temperatureLabels[i];
            float normalizedTemp = Mathf.InverseLerp(minTemp, maxTemp, room.Temperature);
            normalizedTemp = Mathf.Clamp01(normalizedTemp);
            // 更新平面的颜色
            Color temperatureColor = temperatureGradient.Evaluate(normalizedTemp);  // 归一化温度
            Renderer renderer = plane.GetComponent<Renderer>();
            renderer.material.color = temperatureColor;

            // 更新标签的温度
            var text = label.GetComponent<TextMeshPro>();
            text.text = $"{room.RoomType}\n{room.Temperature:F1} °C";  // 显示房间类型和温度
        }
    }

    void Update()
    {
        // 每帧检查并更新温度平面
        UpdateTemperaturePlanes();
    }

    /// <summary>
    /// 显示/隐藏所有温度平面
    /// </summary>
    public void SetVisible(bool visible)
    {
        foreach (var p in tempPlanes)
        {
            p.SetActive(visible);
        }
    }
}
