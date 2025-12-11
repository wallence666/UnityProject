using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 覆盖层控制器 - 管理所有物体的覆盖层和文本可见性
/// </summary>
public class OverlayController : MonoBehaviour
{
    [Header("全局设置")]
    [SerializeField] private KeyCode toggleKey = KeyCode.T; // 切换可见性的按键
    [SerializeField] private bool globalVisible = false; // 全局可见性状态
    
    [Header("自动发现物体")]
    [SerializeField] private bool autoFindObjects = true; // 是否自动发现场景中的物体
    [SerializeField] private float findInterval = 5f; // 自动发现间隔
    
    private List<ObjectTemperatureController> temperatureControllers = new List<ObjectTemperatureController>();
    private Coroutine autoFindCoroutine;
    
    void Start()
    {
        if (autoFindObjects)
        {
            StartAutoFind();
        }
    }
    
    void Update()
    {
        // 检测全局切换按键
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleAllVisibility();
        }
    }
    
    /// <summary>
    /// 启动自动发现协程
    /// </summary>
    private void StartAutoFind()
    {
        if (autoFindCoroutine != null)
        {
            StopCoroutine(autoFindCoroutine);
        }
        
        autoFindCoroutine = StartCoroutine(AutoFindObjects());
    }
    
    /// <summary>
    /// 自动发现场景中的温度控制器
    /// </summary>
    private System.Collections.IEnumerator AutoFindObjects()
    {
        while (autoFindObjects)
        {
            // 等待指定的时间间隔
            yield return new WaitForSeconds(findInterval);
            
            // 查找场景中的所有温度控制器
            FindAllTemperatureControllers();
        }
    }
    
    /// <summary>
    /// 查找场景中的所有温度控制器
    /// </summary>
    public void FindAllTemperatureControllers()
    {
        // 清除现有列表
        temperatureControllers.Clear();
        
        // 查找场景中的所有ObjectTemperatureController
        var controllers = FindObjectsOfType<ObjectTemperatureController>();
        
        foreach (var controller in controllers)
        {
            if (controller != null && !temperatureControllers.Contains(controller))
            {
                RegisterController(controller);
            }
        }
        
        Debug.Log($"找到 {temperatureControllers.Count} 个温度控制器");
    }
    
    /// <summary>
    /// 注册温度控制器
    /// </summary>
    public void RegisterController(ObjectTemperatureController controller)
    {
        if (controller == null) return;
        
        if (!temperatureControllers.Contains(controller))
        {
            temperatureControllers.Add(controller);
            
            // 根据全局状态设置控制器的初始可见性
            if (globalVisible)
            {
                controller.SetOverlayVisible(true);
                controller.SetTextVisible(true);
            }
            else
            {
                controller.SetOverlayVisible(false);
                controller.SetTextVisible(false);
            }
        }
    }
    
    /// <summary>
    /// 注销温度控制器
    /// </summary>
    public void UnregisterController(ObjectTemperatureController controller)
    {
        if (controller == null) return;
        
        if (temperatureControllers.Contains(controller))
        {
            temperatureControllers.Remove(controller);
        }
    }
    
    /// <summary>
    /// 切换所有物体的可见性
    /// </summary>
    public void ToggleAllVisibility()
    {
        globalVisible = !globalVisible;
        
        foreach (var controller in temperatureControllers)
        {
            if (controller != null)
            {
                controller.ToggleVisibility();
            }
        }
        
        Debug.Log($"切换所有物体可见性: {globalVisible}");
    }
    
    /// <summary>
    /// 强制显示所有物体的覆盖层和文本
    /// </summary>
    public void ForceShowAll()
    {
        globalVisible = true;
        
        foreach (var controller in temperatureControllers)
        {
            if (controller != null)
            {
                controller.SetOverlayVisible(true);
                controller.SetTextVisible(true);
            }
        }
        
        Debug.Log("强制显示所有物体的覆盖层和文本");
    }
    
    /// <summary>
    /// 强制隐藏所有物体的覆盖层和文本
    /// </summary>
    public void ForceHideAll()
    {
        globalVisible = false;
        
        foreach (var controller in temperatureControllers)
        {
            if (controller != null)
            {
                controller.SetOverlayVisible(false);
                controller.SetTextVisible(false);
            }
        }
        
        Debug.Log("强制隐藏所有物体的覆盖层和文本");
    }
    
    /// <summary>
    /// 设置指定物体的可见性
    /// </summary>
    public void SetObjectVisibility(ObjectTemperatureController controller, bool visible)
    {
        if (controller == null) return;
        
        controller.SetOverlayVisible(visible);
        controller.SetTextVisible(visible);
    }
    
    /// <summary>
    /// 设置指定房间类型的所有物体的可见性
    /// </summary>
    public void SetRoomTypeVisibility(RoomType roomType, bool visible)
    {
        int count = 0;
        
        foreach (var controller in temperatureControllers)
        {
            if (controller != null && controller.GetCurrentRoom()?.RoomType == roomType)
            {
                controller.SetOverlayVisible(visible);
                controller.SetTextVisible(visible);
                count++;
            }
        }
        
        Debug.Log($"设置 {roomType} 房间的 {count} 个物体可见性为: {visible}");
    }
    
    /// <summary>
    /// 获取当前全局可见性状态
    /// </summary>
    public bool GetGlobalVisibility()
    {
        return globalVisible;
    }
    
    /// <summary>
    /// 获取指定物体的可见性状态
    /// </summary>
    public bool GetObjectVisibility(ObjectTemperatureController controller)
    {
        if (controller == null) return false;
        
        // 这里假设覆盖层和文本的可见性是一致的
        // 如果需要分别获取，可以修改ObjectTemperatureController提供相应方法
        return controller.GetCurrentTemperature() > 0; // 这里只是一个示例，需要实际的方法
    }
    
    /// <summary>
    /// 获取所有温度控制器的数量
    /// </summary>
    public int GetControllerCount()
    {
        return temperatureControllers.Count;
    }
    
    /// <summary>
    /// 获取所有温度控制器的列表
    /// </summary>
    public List<ObjectTemperatureController> GetAllControllers()
    {
        return new List<ObjectTemperatureController>(temperatureControllers);
    }
    
    /// <summary>
    /// 获取指定房间类型的温度控制器
    /// </summary>
    public List<ObjectTemperatureController> GetControllersByRoomType(RoomType roomType)
    {
        List<ObjectTemperatureController> result = new List<ObjectTemperatureController>();
        
        foreach (var controller in temperatureControllers)
        {
            if (controller != null && controller.GetCurrentRoom()?.RoomType == roomType)
            {
                result.Add(controller);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 强制更新所有物体的温度显示
    /// </summary>
    public void ForceUpdateAllTemperatures()
    {
        foreach (var controller in temperatureControllers)
        {
            if (controller != null)
            {
                controller.ForceUpdateTemperature();
            }
        }
    }
    
    void OnDestroy()
    {
        // 停止自动发现协程
        if (autoFindCoroutine != null)
        {
            StopCoroutine(autoFindCoroutine);
            autoFindCoroutine = null;
        }
    }
    
    #if UNITY_EDITOR
    [ContextMenu("立即查找所有温度控制器")]
    void FindAllControllersInEditor()
    {
        FindAllTemperatureControllers();
    }
    
    [ContextMenu("切换所有可见性")]
    void ToggleAllInEditor()
    {
        if (Application.isPlaying)
        {
            ToggleAllVisibility();
        }
        else
        {
            Debug.LogWarning("只能在运行模式下切换可见性");
        }
    }
    
    [ContextMenu("强制显示所有")]
    void ForceShowAllInEditor()
    {
        if (Application.isPlaying)
        {
            ForceShowAll();
        }
        else
        {
            Debug.LogWarning("只能在运行模式下操作");
        }
    }
    
    [ContextMenu("强制隐藏所有")]
    void ForceHideAllInEditor()
    {
        if (Application.isPlaying)
        {
            ForceHideAll();
        }
        else
        {
            Debug.LogWarning("只能在运行模式下操作");
        }
    }
    #endif
}