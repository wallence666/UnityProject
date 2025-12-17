using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 空调覆盖层控制器 - 显示空调开关状态和设置温度
/// 开启时会更新房间温度到RoomDataManager
/// </summary>
public class AirConditionerController : MonoBehaviour
{
    [Header("房间设置")]
    [SerializeField] private RoomType roomType = RoomType.MasterBedroom; // 空调所在的房间类型
    
    [Header("空调设置")]
    [SerializeField] private bool isOn = false; // 空调开关状态
    [SerializeField] private float targetTemperature = 25.5f; // 设置温度
    [SerializeField] private float halfLifeTime = 10.0f; // 使用半衰期方法平衡内外温度
    
    [Header("自动控制设置")]
    [SerializeField] private bool enableAutoControl = true; // 是否启用自动控制
    [SerializeField] private float autoTurnOnDelay = 60.0f; // 进入房间后自动开启延迟（秒）
    [SerializeField] private bool autoTurnOffOnExit = true; // 离开房间时是否自动关闭
    
    [Header("UI显示")]
    [SerializeField] private bool showUI = true;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float uiScale = 0.001f;
    
    [Header("温度更新")]
    [SerializeField] private float temperatureUpdateInterval = 1.0f; // 温度更新间隔（秒）
    
    [Header("字体设置")]
    [SerializeField] private Font uiFont; // Unity内置字体
    [SerializeField] private TMP_FontAsset tmpFont; // TextMeshPro字体
    [SerializeField] private bool useTextMeshPro = true;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;
    
    [Header("可见性设置")]
    [SerializeField] private bool textVisible = true;
    
    private Camera mainCamera;
    private RoomDataManager roomManager;
    private Canvas uiCanvas;
    private TMP_Text tmpTextComponent;
    private Text uiTextComponent;
    private Coroutine temperatureUpdateCoroutine;
    private Coroutine autoTurnOnCoroutine;
    
    // 房间状态追踪
    private bool isRoomOccupied = false;
    private float roomEntryTime = 0f;
    
    // 空调状态事件
    public System.Action<AirConditionerController, bool> OnAirConditionerStateChanged; // 空调状态变化事件
    
    // 公开属性
    public bool IsOn => isOn;
    public bool IsTextVisible => textVisible;
    public float TargetTemperature => targetTemperature;
    public RoomType Room => roomType;
    public bool EnableAutoControl => enableAutoControl;
    public float AutoTurnOnDelay => autoTurnOnDelay;
    public bool AutoTurnOffOnExit => autoTurnOffOnExit;

    void Start()
    {
        mainCamera = Camera.main;
        
        // 获取房间管理器
        roomManager = FindObjectOfType<RoomDataManager>();
        if (roomManager == null)
        {
            Debug.LogWarning($"未找到RoomDataManager，{gameObject.name}将无法更新房间温度");
        }
        
        // 创建UI
        if (showUI)
        {
            CreateUI();
        }
        
        // 如果空调开启，启动温度更新
        if (isOn)
        {
            StartTemperatureUpdate();
        }
        
        // 初始化显示
        UpdateUIDisplay();
        
        // 注册到OverlayController
        RegisterToOverlayController();
        
        // 注册房间事件监听
        RegisterRoomEvents();
    }
    
    /// <summary>
    /// 注册房间事件监听
    /// </summary>
    private void RegisterRoomEvents()
    {
        // 假设有房间事件管理器
		RoomEvents.OnRoomEntered += HandleRoomEntered;
		RoomEvents.OnRoomExited += HandleRoomExited;
    }
    /// <summary>
    /// 注册到OverlayController
    /// </summary>
    private void RegisterToOverlayController()
    {
        OverlayController overlayController = FindObjectOfType<OverlayController>();
        if (overlayController != null)
        {
            overlayController.RegisterAirConditioner(this);
        }
        else
        {
            Debug.LogWarning($"未找到OverlayController，{gameObject.name}将独立控制可见性");
        }
    }
    
    void Update()
    {
        // 面向摄像机
        if (textVisible && uiCanvas != null && mainCamera != null)
        {
            UpdateUIDisplay();
            UpdateUIOrientation();
        }
        
        // 调试用：模拟房间事件
        #if UNITY_EDITOR
        HandleDebugInput();
        #endif
    }
    
    void OnMouseDown()
    {
        TogglePower();
        StopAutoTurnOnCoroutine();
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// 处理调试输入
    /// </summary>
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SimulateRoomEntered(roomType);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SimulateRoomExited(roomType);
        }
    }
    
    /// <summary>
    /// 模拟房间进入
    /// </summary>
    private void SimulateRoomEntered(RoomType room)
    {
        if (room == roomType)
        {
            HandleRoomEntered(room);
            Debug.Log($"模拟进入房间: {room}");
        }
    }
    
    /// <summary>
    /// 模拟房间退出
    /// </summary>
    private void SimulateRoomExited(RoomType room)
    {
        if (room == roomType)
        {
            HandleRoomExited(room);
            Debug.Log($"模拟离开房间: {room}");
        }
    }
    #endif
    
    /// <summary>
    /// 更新UI朝向
    /// </summary>
    void UpdateUIOrientation()
    {
        if (uiCanvas == null || mainCamera == null) return;
        
        // 让UI始终面向摄像机
        uiCanvas.transform.LookAt(
            uiCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up
        );
    }
    
    void OnDestroy()
    {
        // 停止所有协程
        StopTemperatureUpdate();
        StopAutoTurnOnCoroutine();
        
        // 从OverlayController注销
        UnregisterFromOverlayController();
        
        // 注销房间事件监听
        UnregisterRoomEvents();
    }
    
    /// <summary>
    /// 注销房间事件监听
    /// </summary>
    private void UnregisterRoomEvents()
    {
		RoomEvents.OnRoomEntered -= HandleRoomEntered;
		RoomEvents.OnRoomExited -= HandleRoomExited;
    }
    
    /// <summary>
    /// 从OverlayController注销
    /// </summary>
    private void UnregisterFromOverlayController()
    {
        OverlayController overlayController = FindObjectOfType<OverlayController>();
        if (overlayController != null)
        {
            overlayController.UnregisterAirConditioner(this);
        }
    }
    
    /// <summary>
    /// 处理房间进入事件
    /// </summary>
    private void HandleRoomEntered(RoomType enteredRoom)
    {
        if (!enableAutoControl || enteredRoom != roomType) return;
        
        Debug.Log($"房间进入: {enteredRoom}, 空调所在房间: {roomType}");
        
        isRoomOccupied = true;
        roomEntryTime = Time.time;
        
        // 如果房间已经有人，并且空调是关闭的，启动自动开启计时器
        if (!isOn)
        {
            StartAutoTurnOnCoroutine();
        }
        
        // 更新UI显示，提示自动控制状态
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 处理房间退出事件
    /// </summary>
    private void HandleRoomExited(RoomType exitedRoom)
    {
        if (!enableAutoControl || exitedRoom != roomType) return;
        
        Debug.Log($"房间退出: {exitedRoom}, 空调所在房间: {roomType}");
        
        isRoomOccupied = false;
        
        // 停止自动开启计时器
        StopAutoTurnOnCoroutine();
        
        // 如果启用了离开时自动关闭功能，并且空调是开启的，关闭空调
        if (autoTurnOffOnExit && isOn)
        {
            SetPower(false);
            Debug.Log($"房间无人，自动关闭空调: {roomType}");
        }
        
        // 更新UI显示
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 启动自动开启协程
    /// </summary>
    private void StartAutoTurnOnCoroutine()
    {
        if (autoTurnOnCoroutine != null)
        {
            StopCoroutine(autoTurnOnCoroutine);
        }
        
        autoTurnOnCoroutine = StartCoroutine(AutoTurnOnCoroutine());
    }
    
    /// <summary>
    /// 停止自动开启协程
    /// </summary>
    private void StopAutoTurnOnCoroutine()
    {
        if (autoTurnOnCoroutine != null)
        {
            StopCoroutine(autoTurnOnCoroutine);
            autoTurnOnCoroutine = null;
        }
    }
    
    /// <summary>
    /// 自动开启协程
    /// </summary>
    private IEnumerator AutoTurnOnCoroutine()
    {
        Debug.Log($"开始自动开启计时: {autoTurnOnDelay}秒后开启空调");
        
        yield return new WaitForSeconds(autoTurnOnDelay);
        
        // 计时结束后检查房间是否仍然有人
        if (isRoomOccupied && !isOn)
        {
            Debug.Log($"房间持续占用超过{autoTurnOnDelay}秒，自动开启空调");
            SetPower(true);
        }
        
        autoTurnOnCoroutine = null;
    }
    
    /// <summary>
    /// 创建UI
    /// </summary>
    void CreateUI()
    {
        // 计算UI位置
        Vector3 uiPosition = transform.position + uiOffset;
        
        // 创建Canvas
        GameObject canvasGO = new GameObject($"{gameObject.name}_AirConditionerCanvas");
        canvasGO.transform.position = uiPosition;
        canvasGO.transform.rotation = Quaternion.identity;
        
        // 设置Canvas
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.WorldSpace;
        
        // 设置Canvas尺寸
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(300, 100);
        
        // 添加Canvas Scaler
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        scaler.referencePixelsPerUnit = 100;
        
        // 设置Canvas缩放
        canvasGO.transform.localScale = new Vector3(uiScale, uiScale, uiScale);
        
        // 创建背景板
        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(canvasGO.transform);
        backgroundGO.transform.localPosition = Vector3.zero;
        backgroundGO.transform.localScale = Vector3.one;
        
        // 设置背景板RectTransform
        RectTransform bgRect = backgroundGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // 添加背景Image
        Image bgImage = backgroundGO.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f);
        
        // 创建文本
        GameObject textGO = new GameObject("AirConditionerText");
        textGO.transform.SetParent(canvasGO.transform);
        textGO.transform.localPosition = Vector3.zero;
        textGO.transform.localScale = Vector3.one;
        
        // 设置文本RectTransform
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // 创建文本组件
        if (useTextMeshPro)
        {
            #if TMP_PRESENT
            CreateTMPText(textGO);
            #else
            Debug.LogWarning("TextMeshPro不可用，使用普通UI Text");
            CreateUIText(textGO);
            #endif
        }
        else
        {
            CreateUIText(textGO);
        }
        
        // 设置初始可见性
        uiCanvas.enabled = textVisible;
        
        // 立即更新文本
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 创建TextMeshPro文本
    /// </summary>
    void CreateTMPText(GameObject textGO)
    {
        #if TMP_PRESENT
        tmpTextComponent = textGO.AddComponent<TMP_Text>();
        tmpTextComponent.fontSize = fontSize;
        tmpTextComponent.alignment = TMPro.TextAlignmentOptions.Center;
        tmpTextComponent.color = textColor;
        tmpTextComponent.fontStyle = TMPro.FontStyles.Bold;
        
        // 设置字体
        if (tmpFont != null)
        {
            tmpTextComponent.font = tmpFont;
        }
        
        // 确保文本正确渲染
        tmpTextComponent.enableWordWrapping = false;
        tmpTextComponent.overflowMode = TMPro.TextOverflowModes.Overflow;
        tmpTextComponent.raycastTarget = false;
        #endif
    }
    
    /// <summary>
    /// 创建普通UI文本
    /// </summary>
    void CreateUIText(GameObject textGO)
    {
        uiTextComponent = textGO.AddComponent<Text>();
        uiTextComponent.text = "test";
        uiTextComponent.fontSize = fontSize;
        uiTextComponent.alignment = TextAnchor.MiddleCenter;
        uiTextComponent.color = textColor;
        uiTextComponent.fontStyle = FontStyle.Bold;
        
        // 设置字体
        if (uiFont != null)
        {
            uiTextComponent.font = uiFont;
        }
        
        // 确保文本正确渲染
        uiTextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiTextComponent.verticalOverflow = VerticalWrapMode.Overflow;
        uiTextComponent.raycastTarget = false;
    }
    
    /// <summary>
    /// 启动温度更新协程
    /// </summary>
    private void StartTemperatureUpdate()
    {
        if (temperatureUpdateCoroutine != null)
        {
            StopCoroutine(temperatureUpdateCoroutine);
        }
        
        temperatureUpdateCoroutine = StartCoroutine(UpdateTemperatureCoroutine());
    }
    
    /// <summary>
    /// 停止温度更新协程
    /// </summary>
    private void StopTemperatureUpdate()
    {
        if (temperatureUpdateCoroutine != null)
        {
            StopCoroutine(temperatureUpdateCoroutine);
            temperatureUpdateCoroutine = null;
        }
    }
    
    /// <summary>
    /// 温度更新协程
    /// </summary>
    private IEnumerator UpdateTemperatureCoroutine()
    {
        while (isOn)
        {
            // 等待指定的时间间隔
            yield return new WaitForSeconds(temperatureUpdateInterval);
            
            // 更新房间温度
            UpdateRoomTemperature();
        }
    }
    
    /// <summary>
    /// 更新房间温度
    /// </summary>
    public void UpdateRoomTemperature()
    {
        if (roomManager != null && isOn)
        {
            roomManager.UpdateRoomTemperature(roomType,targetTemperature + Mathf.Pow(0.5f,temperatureUpdateInterval/halfLifeTime) * (roomManager[roomType].Temperature- targetTemperature));
            // roomManager.UpdateRoomTemperature(roomType, targetTemperature);
        }
    }
    
    /// <summary>
    /// 切换空调开关状态
    /// </summary>
    public void TogglePower()
    {
        SetPower(!isOn);
    }
    
    /// <summary>
    /// 设置空调开关状态
    /// </summary>
    public void SetPower(bool powerOn)
    {
        if (isOn == powerOn) return;
        
        isOn = powerOn;
        
        if (isOn)
        {
            roomManager[roomType].tempLock++;
            UpdateRoomTemperature();
            StartTemperatureUpdate();
            Debug.Log($"空调已开启: {roomType}");
        }
        else
        {
            roomManager[roomType].tempLock--;
            if(roomManager[roomType].tempLock<0) roomManager[roomType].tempLock=0;
            StopTemperatureUpdate();
            Debug.Log($"空调已关闭: {roomType}");
        }
        
        // 触发状态变化事件
        OnAirConditionerStateChanged?.Invoke(this, isOn);
        
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 设置目标温度
    /// </summary>
    public void SetTargetTemperature(float temperature)
    {
        targetTemperature = temperature;
        
        // 如果空调开启，立即更新房间温度
        if (isOn)
        {
            UpdateRoomTemperature();
        }
        
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 调整目标温度（增加或减少）
    /// </summary>
    public void AdjustTargetTemperature(float delta)
    {
        targetTemperature += delta;
        
        // 限制温度范围（可根据需要调整）
        targetTemperature = Mathf.Clamp(targetTemperature, 16f, 30f);
        
        // 如果空调开启，立即更新房间温度
        if (isOn)
        {
            UpdateRoomTemperature();
        }
        
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 更新UI显示
    /// </summary>
    public void UpdateUIDisplay()
    {
        if (!showUI || uiCanvas == null || (tmpTextComponent == null && uiTextComponent == null)) return;
        
        string statusText = isOn ? "开启" : "关闭";
        string statusColor = isOn ? "green" : "red";
        
        // 添加自动控制状态信息
        string autoControlInfo = "";
        if (enableAutoControl)
        {
            if (isRoomOccupied && !isOn)
            {
                float remainingTime = autoTurnOnDelay - (Time.time - roomEntryTime);
                if (remainingTime > 0 && autoTurnOnCoroutine != null)
                {
                    autoControlInfo = $"\n<color=yellow>自动开启倒计时: {Mathf.Ceil(remainingTime)}秒</color>";
                }
            }
            else if (isRoomOccupied)
            {
                autoControlInfo = "\n<color=cyan>房间有人</color>";
            }
        }
        
        string displayText = $"空调: <color={statusColor}>{statusText}</color>\n" +
                            $"设置温度: {targetTemperature:F1}°C" +
                            autoControlInfo;
        
        if (useTextMeshPro)
            tmpTextComponent.text = displayText;
        else 
            uiTextComponent.text = displayText;
    }
    
    /// <summary>
    /// 切换可见性（由OverlayController调用）
    /// </summary>
    public void ToggleVisibility()
    {
        textVisible = !textVisible;
        
        UpdateUIDisplay();

        if (uiCanvas != null)
        {
            uiCanvas.enabled = textVisible;
        }
    }
    
    /// <summary>
    /// 设置文本可见性
    /// </summary>
    public void SetTextVisible(bool visible)
    {
        textVisible = visible;
        UpdateUIDisplay();
        if (uiCanvas != null)
        {
            uiCanvas.enabled = visible;
        }
    }
    
    /// <summary>
    /// 设置自动控制参数
    /// </summary>
    public void SetAutoControlParameters(bool enable, float delay, bool autoTurnOff)
    {
        enableAutoControl = enable;
        autoTurnOnDelay = delay;
        autoTurnOffOnExit = autoTurnOff;
        
        // 如果关闭自动控制，停止相关协程
        if (!enableAutoControl)
        {
            StopAutoTurnOnCoroutine();
            isRoomOccupied = false;
        }
        
        UpdateUIDisplay();
    }
    
    /// <summary>
    /// 强制更新显示
    /// </summary>
    public void ForceUpdateDisplay()
    {
        UpdateUIDisplay();
        
        // 如果空调开启，更新房间温度
        if (isOn)
        {
            UpdateRoomTemperature();
        }
    }
    
    /// <summary>
    /// 修复UI位置
    /// </summary>
    public void FixUI()
    {
        if (uiCanvas == null)
        {
            if (showUI) CreateUI();
            return;
        }
        
        // 重新计算UI位置
        Vector3 uiPosition = transform.position + uiOffset;
        uiCanvas.transform.position = uiPosition;
        
        // 重置缩放
        uiCanvas.transform.localScale = new Vector3(uiScale, uiScale, uiScale);
        
        UpdateUIDisplay();
    }
    
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 在编辑器中显示UI位置
        Gizmos.color = Color.cyan;
        Vector3 uiPos = transform.position + uiOffset;
        Gizmos.DrawWireSphere(uiPos, 0.05f);
        Gizmos.DrawLine(transform.position, uiPos);
    }
    
    [ContextMenu("切换空调开关")]
    void TogglePowerInEditor()
    {
        if (Application.isPlaying)
        {
            TogglePower();
            Debug.Log($"空调开关: {isOn}");
        }
    }
    
    [ContextMenu("增加温度")]
    void IncreaseTemperatureInEditor()
    {
        if (Application.isPlaying)
        {
            AdjustTargetTemperature(1.0f);
            Debug.Log($"设置温度: {targetTemperature}°C");
        }
    }
    
    [ContextMenu("减少温度")]
    void DecreaseTemperatureInEditor()
    {
        if (Application.isPlaying)
        {
            AdjustTargetTemperature(-1.0f);
            Debug.Log($"设置温度: {targetTemperature}°C");
        }
    }
    
    [ContextMenu("修复UI")]
    void FixUIInEditor()
    {
        if (Application.isPlaying)
        {
            FixUI();
        }
    }
    
    [ContextMenu("强制更新房间温度")]
    void ForceUpdateRoomTemperatureInEditor()
    {
        if (Application.isPlaying)
        {
            UpdateRoomTemperature();
            Debug.Log($"更新房间温度: {roomType} -> {targetTemperature}°C");
        }
    }
    
    [ContextMenu("模拟房间进入")]
    void SimulateRoomEntryInEditor()
    {
        if (Application.isPlaying)
        {
            HandleRoomEntered(roomType);
            Debug.Log($"模拟进入房间: {roomType}");
        }
    }
    
    [ContextMenu("模拟房间退出")]
    void SimulateRoomExitInEditor()
    {
        if (Application.isPlaying)
        {
            HandleRoomExited(roomType);
            Debug.Log($"模拟离开房间: {roomType}");
        }
    }
    
    [ContextMenu("启用/禁用自动控制")]
    void ToggleAutoControlInEditor()
    {
        if (Application.isPlaying)
        {
            enableAutoControl = !enableAutoControl;
            Debug.Log($"自动控制: {enableAutoControl}");
            UpdateUIDisplay();
        }
    }
    #endif
}