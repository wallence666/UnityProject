using UnityEngine;
using System.Collections.Generic;

public class TemperatureLayerRenderer : MonoBehaviour
{
    public RoomDataManager roomManager;  // 拖入你的 RoomDataManager 空物体
    public Material temperatureMaterial; // 一个简单的 Unlit/Color 材质
    private List<GameObject> tempPlanes = new List<GameObject>();

    void Start()
    {
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

        // 清理旧平面
        foreach (var p in tempPlanes)
        {
            Destroy(p);
        }
        tempPlanes.Clear();

        // 遍历每个房间
        List<RoomData> rooms = roomManager.GetAllRooms();
        foreach (var room in rooms)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);

            plane.name = $"TempPlane_{room.RoomType}";
            plane.transform.SetParent(transform);

            // 平面大小 = 房间 XZ 大小
            Vector3 size = room.Bounds.size;
            plane.transform.localScale = new Vector3(size.x, size.z, 1);

            // 平面位置 = 房间中心的顶部（你们是平面图）
            plane.transform.position = new Vector3(
                room.Bounds.center.x,
                room.Bounds.center.y + room.Bounds.size.y + 0.1f, // 避免穿模
                room.Bounds.center.z
            );

            // 朝下（因为 Quad 默认面向正 Z）
            plane.transform.rotation = Quaternion.Euler(90, 0, 0);

            // 赋予材质和颜色
            Renderer renderer = plane.GetComponent<Renderer>();
            renderer.material = new Material(temperatureMaterial);
            renderer.material.color = GetTemperatureColor(room.Temperature);

            tempPlanes.Add(plane);
        }
    }

    /// <summary>
    /// 温度映射颜色
    /// </summary>
    Color GetTemperatureColor(float temp)
    {
        if (temp < 15) return Color.blue;  
        if (temp > 25) return Color.red;
        return Color.green;
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
