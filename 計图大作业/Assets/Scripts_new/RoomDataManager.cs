/*
RoomDataManager.cs
用于存储各房间温度及其包围盒


使用示例：

1. 在场景中创建空物体，添加RoomDataManager组件
2. 调用InitializeDefaultRooms()初始化数据
3. 在编辑器中配置或通过代码访问：

// 获取管理器
RoomDataManager manager = FindObjectOfType<RoomDataManager>();

// 获取特定房间
RoomData livingRoom = manager[RoomType.LivingRoom];

// 更新温度
manager.UpdateRoomTemperature(RoomType.Kitchen, 25.5f);

// 查找玩家所在房间
RoomData currentRoom = manager.GetRoomAtPosition(playerTransform.position);

4. 创建ScriptableObject配置：
   Assets/Create/Room System/Room Configuration
*/

using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 房间类型枚举
/// </summary>
public enum RoomType
{
    LivingRoom = 0,  // 客厅
    Kitchen = 1,     // 厨房
    Toilet = 2,      // 厕所
    MasterBedroom = 3, // 主卧
    SecondBedroom = 4  // 次卧
}

/// <summary>
/// 房间数据类 - 存储单个房间的信息
/// </summary>
[System.Serializable]
public class RoomData
{
    [SerializeField] private RoomType roomType;
    [SerializeField] private float temperature;
    [SerializeField] private Bounds bounds;
    [SerializeField] private Vector3 center;
    [SerializeField] private Vector3 size;
    
    public RoomType RoomType => roomType;
    public float Temperature
    {
        get => temperature;
        set => temperature = Mathf.Clamp(value, -50f, 100f); // 合理的温度范围
    }
    public Bounds Bounds
    {
        get => bounds;
        set
        {
            bounds = value;
            center = value.center;
            size = value.size;
        }
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public RoomData(RoomType type, float temp, Bounds roomBounds)
    {
        roomType = type;
        Temperature = temp;
        Bounds = roomBounds;
    }
    
    /// <summary>
    /// 从BoxCollider创建房间数据
    /// </summary>
    public static RoomData FromBoxCollider(RoomType type, float temp, BoxCollider collider)
    {
        return new RoomData(type, temp, collider.bounds);
    }
    
    /// <summary>
    /// 检查点是否在房间内
    /// </summary>
    public bool Contains(Vector3 point)
    {
        return bounds.Contains(point);
    }
    
    /// <summary>
    /// 获取房间体积
    /// </summary>
    public float GetVolume()
    {
        return bounds.size.x * bounds.size.y * bounds.size.z;
    }
    
    public override string ToString()
    {
        return $"{roomType}: 温度={temperature}°C, 中心={center}, 尺寸={size}";
    }
}

/// <summary>
/// 房间数据管理器 - 管理所有房间数据
/// </summary>
public class RoomDataManager : MonoBehaviour
{
    [SerializeField] private List<RoomData> rooms = new List<RoomData>();
    
    // 索引器，通过RoomType快速访问
    public RoomData this[RoomType type]
    {
        get
        {
            var room = rooms.Find(r => r.RoomType == type);
            if (room == null)
            {
                Debug.LogWarning($"未找到房间类型: {type}");
                return null;
            }
            return room;
        }
    }
    
    /// <summary>
    /// 初始化默认房间数据
    /// </summary>
    public void InitializeDefaultRooms()
    {
        if (rooms.Count > 0) return;
        
        // 创建默认的房间数据
        rooms = new List<RoomData>
        {
            new RoomData(RoomType.LivingRoom, 22f, new Bounds(new Vector3(0, 0, 0), new Vector3(5, 3, 4))),
            new RoomData(RoomType.Kitchen, 20f, new Bounds(new Vector3(6, 0, 0), new Vector3(3, 3, 3))),
            new RoomData(RoomType.Toilet, 18f, new Bounds(new Vector3(10, 0, 0), new Vector3(2, 3, 2))),
            new RoomData(RoomType.MasterBedroom, 21f, new Bounds(new Vector3(0, 0, 5), new Vector3(4, 3, 4))),
            new RoomData(RoomType.SecondBedroom, 20f, new Bounds(new Vector3(5, 0, 5), new Vector3(3, 3, 3)))
        };
    }
    
    /// <summary>
    /// 获取所有房间
    /// </summary>
    public List<RoomData> GetAllRooms()
    {
        return new List<RoomData>(rooms);
    }
    
    /// <summary>
    /// 更新房间温度
    /// </summary>
    public void UpdateRoomTemperature(RoomType type, float temperature)
    {
        var room = this[type];
        if (room != null)
        {
            room.Temperature = temperature;
        }
    }
    
    /// <summary>
    /// 更新房间包围盒
    /// </summary>
    public void UpdateRoomBounds(RoomType type, Bounds bounds)
    {
        var room = this[type];
        if (room != null)
        {
            room.Bounds = bounds;
        }
    }
    
    /// <summary>
    /// 根据位置查找房间
    /// </summary>
    public RoomData GetRoomAtPosition(Vector3 position)
    {
        foreach (var room in rooms)
        {
            if (room.Contains(position))
            {
                return room;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 获取平均温度
    /// </summary>
    public float GetAverageTemperature()
    {
        if (rooms.Count == 0) return 0f;
        
        float sum = 0f;
        foreach (var room in rooms)
        {
            sum += room.Temperature;
        }
        return sum / rooms.Count;
    }
    
    /// <summary>
    /// 可视化房间边界（在Scene视图中）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        foreach (var room in rooms)
        {
            // 根据温度设置颜色
            Color color = GetTemperatureColor(room.Temperature);
            Gizmos.color = color;
            Gizmos.DrawWireCube(room.Bounds.center, room.Bounds.size);
            
            // 显示房间标签
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(room.Bounds.center, $"{room.RoomType}\n{room.Temperature}°C");
            #endif
        }
    }
    
    /// <summary>
    /// 根据温度获取颜色
    /// </summary>
    private Color GetTemperatureColor(float temperature)
    {
        if (temperature < 15) return Color.blue;       // 冷
        if (temperature > 25) return Color.red;        // 热
        return Color.green;                            // 舒适
    }
}

/// <summary>
/// 房间配置SO（ScriptableObject）- 用于存储配置数据
/// </summary>
[CreateAssetMenu(fileName = "RoomConfig", menuName = "Room System/Room Configuration")]
public class RoomConfiguration : ScriptableObject
{
    [System.Serializable]
    public struct RoomSetting
    {
        public RoomType type;
        public float defaultTemperature;
        public Vector3 defaultSize;
        public Vector3 defaultCenter;
    }
    
    [SerializeField] private List<RoomSetting> roomSettings = new List<RoomSetting>();
    
    public List<RoomSetting> RoomSettings => roomSettings;
    
    /// <summary>
    /// 获取默认配置的房间数据列表
    /// </summary>
    public List<RoomData> GetDefaultRoomData()
    {
        var roomDataList = new List<RoomData>();
        
        foreach (var setting in roomSettings)
        {
            var bounds = new Bounds(setting.defaultCenter, setting.defaultSize);
            roomDataList.Add(new RoomData(setting.type, setting.defaultTemperature, bounds));
        }
        
        return roomDataList;
    }
}

/// <summary>
/// 房间温度事件系统
/// </summary>
public static class RoomEvents
{
    public static event Action<RoomType, float> OnTemperatureChanged;
    public static event Action<RoomType> OnRoomEntered;
    public static event Action<RoomType> OnRoomExited;
    
    public static void TriggerTemperatureChanged(RoomType roomType, float newTemperature)
    {
        OnTemperatureChanged?.Invoke(roomType, newTemperature);
    }
    
    public static void TriggerRoomEntered(RoomType roomType)
    {
        OnRoomEntered?.Invoke(roomType);
    }
    
    public static void TriggerRoomExited(RoomType roomType)
    {
        OnRoomExited?.Invoke(roomType);
    }
}

/// <summary>
/// 玩家房间检测器示例
/// </summary>
public class PlayerRoomDetector : MonoBehaviour
{
    private RoomData currentRoom;
    private RoomDataManager roomManager;
    
    private void Start()
    {
        roomManager = FindObjectOfType<RoomDataManager>();
        if (roomManager == null)
        {
            Debug.LogError("未找到RoomDataManager");
        }
    }
    
    private void Update()
    {
        DetectCurrentRoom();
    }
    
    private void DetectCurrentRoom()
    {
        if (roomManager == null) return;
        
        var newRoom = roomManager.GetRoomAtPosition(transform.position);
        
        if (newRoom != currentRoom)
        {
            if (currentRoom != null)
            {
                RoomEvents.TriggerRoomExited(currentRoom.RoomType);
            }
            
            currentRoom = newRoom;
            
            if (currentRoom != null)
            {
                RoomEvents.TriggerRoomEntered(currentRoom.RoomType);
                Debug.Log($"进入房间: {currentRoom.RoomType}, 温度: {currentRoom.Temperature}°C");
            }
            else
            {
                Debug.Log("离开房间区域");
            }
        }
    }
}

