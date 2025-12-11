using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // 添加命名空间以使用协程

/// <summary>
/// 物体温度控制器 - 适用于床、沙发等需要显示温度的物体
/// 温度与所在房间同步
/// </summary>
public class ObjectTemperatureController : MonoBehaviour
{
    [Header("房间设置")]
    [SerializeField] private RoomType roomType = RoomType.MasterBedroom; // 物体所在的房间类型
    
    [Header("自动更新设置")]
    [SerializeField] private bool enableAutoUpdate = true; // 是否启用自动更新
    [SerializeField] private float updateInterval = 3.0f; // 更新间隔（秒）
    
    [Header("颜色渐变")]
    [SerializeField] private Gradient temperatureGradient;
    [SerializeField] private bool useCustomGradient = false;
    
    [Header("物体渲染器")]
    [SerializeField] private Renderer[] objectRenderers;
    
    [Header("UI显示")]
    [SerializeField] private bool showUI = true;
    [SerializeField] private string temperatureFormat = "F1";
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float uiScale = 0.001f;
    
    [Header("字体设置")]
    [SerializeField] private Font uiFont; // Unity内置字体
    [SerializeField] private TMP_FontAsset tmpFont; // TextMeshPro字体
    [SerializeField] private bool useTextMeshPro = true;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private bool autoAdjustTextColor = true; // 根据背景自动调整文字颜色
    
    [Header("覆盖层设置")]
    [SerializeField] private bool useEmission = true;
    [SerializeField] private float emissionIntensity = 1.5f;
    
    [Header("可见性设置")]
    [SerializeField] private bool overlayVisible = false;
    [SerializeField] private bool textVisible = true;
    
    private MaterialPropertyBlock propertyBlock;
    private Camera mainCamera;
    private RoomDataManager roomManager;
    private RoomData currentRoom;
    private float currentTemperature = 22f;
    private Canvas uiCanvas;
    private TMP_Text tmpTextComponent;
    private Text uiTextComponent;
    private Coroutine autoUpdateCoroutine; // 自动更新协程的引用
    
    // 公开的可见性属性
    public bool IsOverlayVisible => overlayVisible;
    public bool IsTextVisible => textVisible;
    public bool IsBothVisible => overlayVisible && textVisible;

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        mainCamera = Camera.main;
        
        // 获取房间管理器
        roomManager = FindObjectOfType<RoomDataManager>();
        if (roomManager == null)
        {
            Debug.LogWarning($"未找到RoomDataManager，{gameObject.name}将使用默认温度");
        }
        else
        {
            // 立即获取房间数据
            UpdateTemperatureFromRoom();
        }
        
        // 自动收集渲染器
        if (objectRenderers == null || objectRenderers.Length == 0)
        {
            objectRenderers = GetComponentsInChildren<Renderer>();
        }
        
        // 初始化渐变
        if (temperatureGradient == null || temperatureGradient.colorKeys.Length == 0 || !useCustomGradient)
        {
            InitializeDefaultGradient();
        }
        
        // 加载默认字体
        LoadDefaultFonts();
        
        // 创建UI
        if (showUI)
        {
            CreateUI();
        }
        
        // 订阅温度变化事件
        RoomEvents.OnTemperatureChanged += OnRoomTemperatureChanged;
        
        // 初始化温度显示
        UpdateTemperatureDisplay();
        
        // 启动自动更新协程
        if (enableAutoUpdate)
        {
            StartAutoUpdate();
        }
        
        // 注册到OverlayController
        RegisterToOverlayController();
    }
    
    /// <summary>
    /// 注册到OverlayController
    /// </summary>
    private void RegisterToOverlayController()
    {
        OverlayController overlayController = FindObjectOfType<OverlayController>();
        if (overlayController != null)
        {
            overlayController.RegisterController(this);
        }
        else
        {
            Debug.LogWarning($"未找到OverlayController，{gameObject.name}将独立控制可见性");
        }
    }
    
    void Update()
    {
        // 注意：移除了按键检测，现在由OverlayController统一管理
        
        // 面向摄像机
        if (textVisible && uiCanvas != null && mainCamera != null)
        {
            UpdateUIOrientation();
        }
    }
    
    /// <summary>
    /// 启动自动更新协程
    /// </summary>
    private void StartAutoUpdate()
    {
        if (autoUpdateCoroutine != null)
        {
            StopCoroutine(autoUpdateCoroutine);
        }
        
        autoUpdateCoroutine = StartCoroutine(AutoUpdateTemperature());
    }
    
    /// <summary>
    /// 停止自动更新协程
    /// </summary>
    private void StopAutoUpdate()
    {
        if (autoUpdateCoroutine != null)
        {
            StopCoroutine(autoUpdateCoroutine);
            autoUpdateCoroutine = null;
        }
    }
    
    /// <summary>
    /// 自动更新温度的协程
    /// </summary>
    private IEnumerator AutoUpdateTemperature()
    {
        while (enableAutoUpdate)
        {
            // 等待指定的时间间隔
            yield return new WaitForSeconds(updateInterval);
            
            // 强制更新温度
            ForceUpdateTemperature();
        }
    }
    
    /// <summary>
    /// 设置自动更新
    /// </summary>
    public void SetAutoUpdate(bool enable, float interval = 5.0f)
    {
        enableAutoUpdate = enable;
        updateInterval = interval;
        
        if (enableAutoUpdate)
        {
            StartAutoUpdate();
        }
        else
        {
            StopAutoUpdate();
        }
    }
    
    /// <summary>
    /// 加载默认字体
    /// </summary>
    void LoadDefaultFonts()
    {
        // 加载Unity默认字体
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (uiFont == null)
            {
                Debug.LogWarning("无法加载Arial字体，将使用系统默认字体");
            }
        }
        
        // 如果使用TextMeshPro但未指定字体，尝试加载默认TMP字体
        if (useTextMeshPro && tmpFont == null)
        {
            #if TMP_PRESENT
            // 尝试加载TMP默认字体
            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont != null)
            {
                tmpFont = defaultFont;
            }
            else
            {
                // 如果默认路径没有，尝试获取TMP设置中的默认字体
                if (TMPro.TMP_Settings.defaultFontAsset != null)
                {
                    tmpFont = TMPro.TMP_Settings.defaultFontAsset;
                }
                else
                {
                    Debug.LogWarning("无法加载TextMeshPro默认字体，将使用普通UI Text");
                    useTextMeshPro = false;
                }
            }
            #endif
        }
    }
    
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
        // 取消订阅事件
        RoomEvents.OnTemperatureChanged -= OnRoomTemperatureChanged;
        
        // 停止自动更新协程
        StopAutoUpdate();
        
        // 从OverlayController注销
        UnregisterFromOverlayController();
    }
    
    /// <summary>
    /// 从OverlayController注销
    /// </summary>
    private void UnregisterFromOverlayController()
    {
        OverlayController overlayController = FindObjectOfType<OverlayController>();
        if (overlayController != null)
        {
            overlayController.UnregisterController(this);
        }
    }
    
    /// <summary>
    /// 房间温度变化事件处理
    /// </summary>
    private void OnRoomTemperatureChanged(RoomType changedRoomType, float newTemperature)
    {
        if (changedRoomType == roomType)
        {
            currentTemperature = newTemperature;
            UpdateTemperatureDisplay();
        }
    }
    
    /// <summary>
    /// 从房间管理器获取温度
    /// </summary>
    private void UpdateTemperatureFromRoom()
    {
        if (roomManager != null)
        {
            currentRoom = roomManager[roomType];
            if (currentRoom != null)
            {
                currentTemperature = currentRoom.Temperature;
            }
            else
            {
                Debug.LogWarning($"未找到房间类型为 {roomType} 的房间数据，使用默认温度22°C");
                currentTemperature = 22f;
            }
        }
        else
        {
            currentTemperature = 22f;
        }
    }
    
    /// <summary>
    /// 初始化默认渐变
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
    /// 创建UI
    /// </summary>
    void CreateUI()
    {
        // 计算UI位置
        Vector3 uiPosition = transform.position + uiOffset;
        
        // 创建Canvas
        GameObject canvasGO = new GameObject($"{gameObject.name}_TemperatureCanvas");
        canvasGO.transform.position = uiPosition;
        canvasGO.transform.rotation = Quaternion.identity;
        
        // 设置Canvas
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.WorldSpace;
        
        // 设置Canvas尺寸
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(500, 100);
        
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
        GameObject textGO = new GameObject("TemperatureText");
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
        UpdateUIText();
    }
    
    /// <summary>
    /// 创建TextMeshPro文本
    /// </summary>
    void CreateTMPText(GameObject textGO)
    {
        #if TMP_PRESENT
        tmpTextComponent = textGO.AddComponent<TMP_Text>();
        tmpTextComponent.text = FormatTemperature(currentTemperature);
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
        uiTextComponent.text = FormatTemperature(currentTemperature);
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
    /// 格式化温度显示
    /// </summary>
    string FormatTemperature(float temp)
    {
        return temp.ToString(temperatureFormat) + "°C";
    }
    
    /// <summary>
    /// 切换可见性（由OverlayController调用）
    /// </summary>
    public void ToggleVisibility()
    {
        overlayVisible = !overlayVisible;
        textVisible = !textVisible;
        
        UpdateTemperatureDisplay();
        
        if (uiCanvas != null)
        {
            uiCanvas.enabled = textVisible;
        }
    }
    
    /// <summary>
    /// 设置覆盖层可见性
    /// </summary>
    public void SetOverlayVisible(bool visible)
    {
        overlayVisible = visible;
        UpdateTemperatureDisplay();
    }
    
    /// <summary>
    /// 设置文本可见性
    /// </summary>
    public void SetTextVisible(bool visible)
    {
        textVisible = visible;
        if (uiCanvas != null)
        {
            uiCanvas.enabled = visible;
        }
    }
    
    /// <summary>
    /// 同时设置覆盖层和文本可见性
    /// </summary>
    public void SetBothVisible(bool visible)
    {
        overlayVisible = visible;
        textVisible = visible;
        
        UpdateTemperatureDisplay();
        
        if (uiCanvas != null)
        {
            uiCanvas.enabled = visible;
        }
    }
    
    /// <summary>
    /// 强制更新温度显示
    /// </summary>
    public void ForceUpdateTemperature()
    {
        UpdateTemperatureFromRoom();
        UpdateTemperatureDisplay();
    }
    
    /// <summary>
    /// 立即更新一次温度显示（不调用自动更新）
    /// </summary>
    public void UpdateTemperatureDisplayImmediate()
    {
        UpdateTemperatureFromRoom();
        UpdateTemperatureDisplay();
    }
    
    /// <summary>
    /// 更新温度显示
    /// </summary>
    void UpdateTemperatureDisplay()
    {
        // 计算归一化温度用于颜色渐变
        float minTemp = 15f;
        float maxTemp = 30f;
        float normalizedTemp = Mathf.InverseLerp(minTemp, maxTemp, currentTemperature);
        normalizedTemp = Mathf.Clamp01(normalizedTemp);
        
        Color temperatureColor = temperatureGradient.Evaluate(normalizedTemp);
        
        // 更新物体颜色
        UpdateObjectColor(temperatureColor);
        
        // 更新UI文本
        UpdateUIText();
    }
    
    /// <summary>
    /// 更新物体颜色
    /// </summary>
    void UpdateObjectColor(Color temperatureColor)
    {
        if (objectRenderers == null || objectRenderers.Length == 0) return;

        foreach (Renderer objRenderer in objectRenderers)
        {
            if (objRenderer == null) continue;
            
            objRenderer.GetPropertyBlock(propertyBlock);
            
            if (overlayVisible)
            {
                if (useEmission)
                {
                    Color emissionColor = temperatureColor * emissionIntensity;
                    propertyBlock.SetColor("_EmissionColor", emissionColor);
                    
                    if (objRenderer.material != null)
                    {
                        // 启用自发光
                        objRenderer.material.EnableKeyword("_EMISSION");
                        objRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    }
                }
            }
            else
            {
                if (useEmission && objRenderer.material != null)
                {
                    // 关闭自发光
                    objRenderer.material.DisableKeyword("_EMISSION");
                    propertyBlock.SetColor("_EmissionColor", Color.black);
                }
            }
            
            objRenderer.SetPropertyBlock(propertyBlock);
        }
    }
    
    /// <summary>
    /// 更新UI文本
    /// </summary>
    void UpdateUIText()
    {
        if (!showUI || uiCanvas == null) return;
        
        string displayText = FormatTemperature(currentTemperature);
        
        // 计算文本颜色
        Color finalTextColor = textColor;
        
        if (autoAdjustTextColor)
        {
            // 根据温度计算合适的文本颜色
            float minTemp = 15f;
            float maxTemp = 30f;
            float normalizedTemp = Mathf.InverseLerp(minTemp, maxTemp, currentTemperature);
            normalizedTemp = Mathf.Clamp01(normalizedTemp);
            
            Color temperatureColor = temperatureGradient.Evaluate(normalizedTemp);
            
            // 计算亮度
            float brightness = 0.299f * temperatureColor.r + 0.587f * temperatureColor.g + 0.114f * temperatureColor.b;
            
            // 根据亮度选择文字颜色
            if (brightness > 0.5f)
            {
                finalTextColor = Color.black; // 背景亮，用黑色文字
            }
            else
            {
                finalTextColor = Color.white; // 背景暗，用白色文字
            }
        }
        
        // 更新文本内容和颜色
        if (tmpTextComponent != null)
        {
            tmpTextComponent.text = displayText;
            tmpTextComponent.color = finalTextColor;
        }
        else if (uiTextComponent != null)
        {
            uiTextComponent.text = displayText;
            uiTextComponent.color = finalTextColor;
        }
        else
        {
            Debug.LogWarning("文本组件未找到，重新创建");
            RecreateUI();
        }
    }
    
    /// <summary>
    /// 重新创建UI
    /// </summary>
    void RecreateUI()
    {
        // 删除旧的UI
        if (uiCanvas != null)
        {
            Destroy(uiCanvas.gameObject);
        }
        
        // 创建新的UI
        CreateUI();
        UpdateUIText();
    }
    
    /// <summary>
    /// 获取当前温度
    /// </summary>
    public float GetCurrentTemperature()
    {
        return currentTemperature;
    }
    
    /// <summary>
    /// 获取所在房间
    /// </summary>
    public RoomData GetCurrentRoom()
    {
        return currentRoom;
    }
    
    /// <summary>
    /// 获取房间类型
    /// </summary>
    public RoomType GetRoomType()
    {
        return roomType;
    }
    
    /// <summary>
    /// 修复UI
    /// </summary>
    public void FixUI()
    {
        if (uiCanvas == null)
        {
            RecreateUI();
            return;
        }
        
        // 重新计算UI位置
        Vector3 uiPosition = transform.position + uiOffset;
        uiCanvas.transform.position = uiPosition;
        
        // 重置缩放
        uiCanvas.transform.localScale = new Vector3(uiScale, uiScale, uiScale);
    }
    
    /// <summary>
    /// 更改字体
    /// </summary>
    public void ChangeFont(Font newFont)
    {
        uiFont = newFont;
        if (uiTextComponent != null && uiFont != null)
        {
            uiTextComponent.font = uiFont;
        }
    }
    
    /// <summary>
    /// 更改TMP字体
    /// </summary>
    public void ChangeTMPFont(TMP_FontAsset newFont)
    {
        tmpFont = newFont;
        #if TMP_PRESENT
        if (tmpTextComponent != null && tmpFont != null)
        {
            tmpTextComponent.font = tmpFont;
        }
        #endif
    }
    
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 在编辑器中显示UI位置
        Gizmos.color = Color.yellow;
        Vector3 uiPos = transform.position + uiOffset;
        Gizmos.DrawWireSphere(uiPos, 0.05f);
        Gizmos.DrawLine(transform.position, uiPos);
    }
    
    void OnValidate()
    {
        // 在编辑器中实时预览
        if (!Application.isPlaying)
        {
            // 更新默认渐变
            if (temperatureGradient == null || temperatureGradient.colorKeys.Length == 0 || !useCustomGradient)
            {
                InitializeDefaultGradient();
            }
        }
    }
    
    [ContextMenu("自动收集渲染器")]
    void CollectRenderersInEditor()
    {
        objectRenderers = GetComponentsInChildren<Renderer>();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("重置为默认渐变")]
    void ResetToDefaultGradient()
    {
        useCustomGradient = false;
        InitializeDefaultGradient();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("修复/重新创建UI")]
    void FixUIInEditor()
    {
        if (Application.isPlaying)
        {
            RecreateUI();
        }
        else
        {
            Debug.LogWarning("只能在运行模式下修复UI");
        }
    }
    
    [ContextMenu("检查UI组件")]
    void CheckUIComponentsInEditor()
    {
        if (Application.isPlaying && uiCanvas != null)
        {
            Debug.Log($"Canvas: {uiCanvas.name}, 启用: {uiCanvas.enabled}");
            
            if (tmpTextComponent != null)
            {
                Debug.Log($"TMP Text: {tmpTextComponent.text}, 字体: {tmpTextComponent.font?.name}");
            }
            else if (uiTextComponent != null)
            {
                Debug.Log($"UI Text: {uiTextComponent.text}, 字体: {uiTextComponent.font?.name}");
            }
            else
            {
                Debug.LogWarning("未找到文本组件");
                
                // 尝试查找子对象中的文本组件
                var foundTMP = uiCanvas.GetComponentInChildren<TMP_Text>();
                if (foundTMP != null)
                {
                    Debug.Log($"找到TMP Text子对象: {foundTMP.text}, 字体: {foundTMP.font?.name}");
                }
                
                var foundText = uiCanvas.GetComponentInChildren<Text>();
                if (foundText != null)
                {
                    Debug.Log($"找到UI Text子对象: {foundText.text}, 字体: {foundText.font?.name}");
                }
            }
        }
        else
        {
            Debug.Log("UI Canvas为空或不在运行模式");
        }
    }
    
    [ContextMenu("测试温度显示")]
    void TestTemperatureDisplayInEditor()
    {
        if (Application.isPlaying)
        {
            currentTemperature = Random.Range(15f, 30f);
            UpdateTemperatureDisplay();
            Debug.Log($"测试温度: {currentTemperature}°C");
        }
    }
    
    [ContextMenu("切换使用TextMeshPro")]
    void ToggleTextMeshProInEditor()
    {
        useTextMeshPro = !useTextMeshPro;
        if (Application.isPlaying)
        {
            RecreateUI();
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("加载默认字体")]
    void LoadDefaultFontsInEditor()
    {
        LoadDefaultFonts();
        if (Application.isPlaying)
        {
            if (tmpTextComponent != null && tmpFont != null)
            {
                tmpTextComponent.font = tmpFont;
            }
            if (uiTextComponent != null && uiFont != null)
            {
                uiTextComponent.font = uiFont;
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("手动触发温度更新")]
    void TriggerTemperatureUpdateInEditor()
    {
        if (Application.isPlaying)
        {
            ForceUpdateTemperature();
            Debug.Log($"手动触发温度更新: {currentTemperature}°C");
        }
    }
    
    [ContextMenu("开始自动更新")]
    void StartAutoUpdateInEditor()
    {
        if (Application.isPlaying)
        {
            enableAutoUpdate = true;
            StartAutoUpdate();
            Debug.Log($"开始自动更新，间隔: {updateInterval}秒");
        }
    }
    
    [ContextMenu("停止自动更新")]
    void StopAutoUpdateInEditor()
    {
        if (Application.isPlaying)
        {
            enableAutoUpdate = false;
            StopAutoUpdate();
            Debug.Log("停止自动更新");
        }
    }
    
    [ContextMenu("切换可见性")]
    void ToggleVisibilityInEditor()
    {
        if (Application.isPlaying)
        {
            ToggleVisibility();
            Debug.Log($"切换可见性: 覆盖层={overlayVisible}, 文本={textVisible}");
        }
    }
    #endif
}