using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BedTemperatureController : MonoBehaviour
{
    [Header("温度设置")]
    [SerializeField] private float currentTemperature = 26f;
    [SerializeField] private float minTemperature = 20f;
    [SerializeField] private float maxTemperature = 35f;
    
    [Header("颜色渐变")]
    [SerializeField] private Gradient temperatureGradient;
    
    [Header("床铺子模型")]
    [SerializeField] private Renderer[] bedRenderers;
    
    [Header("UI显示 - TextMeshPro (推荐)")]
    [SerializeField] private TMP_Text temperatureTMPText;
    
    [Header("UI显示 - 普通UI文本")]
    [SerializeField] private Text temperatureText;
    
    [Header("显示设置")]
    [SerializeField] private string temperatureFormat = "00.0°C";
    [SerializeField] private bool showUnit = true;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0.1f, 0); // 床铺上方0.1米
    
    [Header("UI位置设置")]
    [SerializeField] private bool autoPositionUI = true; // 自动计算UI位置
    [SerializeField] private Transform uiAnchor; // 手动指定UI锚点
    [SerializeField] private float uiScale = 0.002f; // UI缩放
    [SerializeField] private bool faceCamera = true; // UI是否始终面向摄像机
    
    [Header("覆盖层设置")]
    [SerializeField] private bool useEmission = false;
    [SerializeField] private float emissionIntensity = 1.2f;
    [SerializeField] private float overlayOpacity = 0.4f;
    [SerializeField] private Color overlayTint = Color.white;
    [SerializeField] private string colorPropertyName = "_Color";
    
    private MaterialPropertyBlock propertyBlock;
    private Camera mainCamera;
    private Vector3 fixedUIPosition; // 固定的UI位置
    private Transform uiCanvasTransform; // UI Canvas的Transform

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        mainCamera = Camera.main;
        
        if (bedRenderers == null || bedRenderers.Length == 0)
        {
            bedRenderers = GetComponentsInChildren<Renderer>();
        }
        
        if (temperatureGradient == null || temperatureGradient.colorKeys.Length == 0)
        {
            InitializeDefaultGradient();
        }
        
        // 如果没有UI，自动创建一个
        if (temperatureText == null && temperatureTMPText == null)
        {
            CreateUI();
        }
        else
        {
            // 获取现有的UI Canvas
            SetupExistingUI();
        }
        
        // 计算并固定UI位置
        CalculateFixedUIPosition();
        
        // 更新初始显示
        UpdateTemperatureDisplay();
    }

    void Update()
    {
        // 如果启用了面向摄像机，让UI始终面向摄像机
        if (faceCamera && mainCamera != null && uiCanvasTransform != null)
        {
            UpdateUIOrientation();
        }
    }

    void InitializeDefaultGradient()
    {
        temperatureGradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        
        colorKeys[0].color = new Color(0.2f, 0.5f, 1.0f, 1.0f);
        colorKeys[0].time = 0f;
        
        colorKeys[1].color = new Color(0.4f, 0.7f, 1.0f, 1.0f);
        colorKeys[1].time = 0.25f;
        
        colorKeys[2].color = new Color(0.3f, 0.9f, 0.3f, 1.0f);
        colorKeys[2].time = 0.5f;
        
        colorKeys[3].color = new Color(1.0f, 0.8f, 0.2f, 1.0f);
        colorKeys[3].time = 0.75f;
        
        colorKeys[4].color = new Color(1.0f, 0.3f, 0.2f, 1.0f);
        colorKeys[4].time = 1f;
        
        alphaKeys[0].alpha = 0.4f;
        alphaKeys[0].time = 0f;
        
        alphaKeys[1].alpha = 0.4f;
        alphaKeys[1].time = 0.5f;
        
        alphaKeys[2].alpha = 0.4f;
        alphaKeys[2].time = 1f;
        
        temperatureGradient.SetKeys(colorKeys, alphaKeys);
    }

    void CreateUI()
    {
        // 计算UI位置
        Vector3 uiPosition = CalculateUIPosition();
        fixedUIPosition = uiPosition; // 保存固定位置
        
        #if TMP_PRESENT
        CreateTextMeshProUI(uiPosition);
        #else
        CreateStandardUI(uiPosition);
        #endif
    }

    void SetupExistingUI()
    {
        // 获取现有UI的Canvas
        Canvas canvas = null;
        
        if (temperatureTMPText != null)
        {
            canvas = temperatureTMPText.GetComponentInParent<Canvas>();
        }
        else if (temperatureText != null)
        {
            canvas = temperatureText.GetComponentInParent<Canvas>();
        }
        
        if (canvas != null)
        {
            uiCanvasTransform = canvas.transform;
        }
        
        // 计算并固定UI位置
        CalculateFixedUIPosition();
    }

    void CalculateFixedUIPosition()
    {
        if (uiAnchor != null)
        {
            // 使用手动指定的锚点
            fixedUIPosition = uiAnchor.position;
        }
        else if (autoPositionUI)
        {
            // 自动计算UI位置（只在开始时计算一次）
            Vector3 uiPosition = CalculateUIPosition();
            fixedUIPosition = uiPosition;
        }
        else
        {
            // 使用当前UI位置
            if (uiCanvasTransform != null)
            {
                fixedUIPosition = uiCanvasTransform.position;
            }
            else
            {
                fixedUIPosition = transform.position + uiOffset;
            }
        }
        
        // 设置UI位置
        if (uiCanvasTransform != null)
        {
            uiCanvasTransform.position = fixedUIPosition;
        }
    }

    Vector3 CalculateUIPosition()
    {
        if (uiAnchor != null)
        {
            return uiAnchor.position;
        }
        
        // 计算床铺所有Renderer的边界框
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;
        
        foreach (Renderer renderer in bedRenderers)
        {
            if (renderer != null)
            {
                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }
        }
        
        
        return transform.position + uiOffset;
    }

    #if TMP_PRESENT
    void CreateTextMeshProUI(Vector3 position)
    {
        GameObject canvasGO = new GameObject("TemperatureCanvas");
        canvasGO.transform.position = position;
        uiCanvasTransform = canvasGO.transform;
        
        // 设置Canvas
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        // 创建文本
        GameObject textGO = new GameObject("TemperatureText");
        textGO.transform.SetParent(canvasGO.transform);
        textGO.transform.localPosition = Vector3.zero;
        textGO.transform.localScale = Vector3.one; // 使用Vector3.one，缩放由父级控制
        
        RectTransform rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);
        rectTransform.anchoredPosition = Vector2.zero;
        
        temperatureTMPText = textGO.AddComponent<TMP_Text>();
        temperatureTMPText.text = currentTemperature.ToString("F1") + "°C";
        temperatureTMPText.fontSize = 20;
        temperatureTMPText.alignment = TMPro.TextAlignmentOptions.Center;
        temperatureTMPText.enableAutoSizing = true;
        temperatureTMPText.fontSizeMin = 12;
        temperatureTMPText.fontSizeMax = 32;
        
        // 设置Canvas缩放
        canvasGO.transform.localScale = new Vector3(uiScale, uiScale, uiScale);
    }
    #endif

    void CreateStandardUI(Vector3 position)
    {
        GameObject canvasGO = new GameObject("TemperatureCanvas");
        canvasGO.transform.position = position;
        uiCanvasTransform = canvasGO.transform;
        
        // 设置Canvas
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        // 创建文本
        GameObject textGO = new GameObject("TemperatureText");
        textGO.transform.SetParent(canvasGO.transform);
        textGO.transform.localPosition = Vector3.zero;
        textGO.transform.localScale = Vector3.one;
        
        RectTransform rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);
        rectTransform.anchoredPosition = Vector2.zero;
        
        temperatureText = textGO.AddComponent<Text>();
        temperatureText.text = currentTemperature.ToString("F1") + "°C";
        temperatureText.fontSize = 14;
        temperatureText.alignment = TextAnchor.MiddleCenter;
        temperatureText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        // 设置Canvas缩放
        canvasGO.transform.localScale = new Vector3(uiScale, uiScale, uiScale);
    }

    void UpdateUIOrientation()
    {
        if (uiCanvasTransform != null && mainCamera != null)
        {
            // 让UI始终面向摄像机，但保持上下方向正确
            uiCanvasTransform.LookAt(uiCanvasTransform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void SetTemperature(float temp)
    {
        currentTemperature = Mathf.Clamp(temp, minTemperature, maxTemperature);
        UpdateTemperatureDisplay();
    }

    public void IncreaseTemperature(float amount = 0.5f)
    {
        SetTemperature(currentTemperature + amount);
    }

    public void DecreaseTemperature(float amount = 0.5f)
    {
        SetTemperature(currentTemperature - amount);
    }

    public float GetCurrentTemperature()
    {
        return currentTemperature;
    }
    
    public void SetOverlayOpacity(float opacity)
    {
        overlayOpacity = Mathf.Clamp01(opacity);
        UpdateTemperatureDisplay();
    }

    void UpdateTemperatureDisplay()
    {
        float normalizedTemp = Mathf.InverseLerp(minTemperature, maxTemperature, currentTemperature);
        Color temperatureColor = temperatureGradient.Evaluate(normalizedTemp);
        
        UpdateBedColor(temperatureColor);
        UpdateUIText();
        
        // 注意：这里不更新UI位置，位置是固定的
    }

    void UpdateBedColor(Color baseColor)
    {
        if (bedRenderers == null || bedRenderers.Length == 0) return;

        Color finalColor = baseColor;
        finalColor.a *= overlayOpacity;
        finalColor.r *= overlayTint.r;
        finalColor.g *= overlayTint.g;
        finalColor.b *= overlayTint.b;

        foreach (Renderer bedRenderer in bedRenderers)
        {
            if (bedRenderer == null) continue;
            
            bedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorPropertyName, finalColor);
            
            if (useEmission)
            {
                Color emissionColor = finalColor * emissionIntensity;
                emissionColor.a = 1f;
                propertyBlock.SetColor("_EmissionColor", emissionColor);
                
                if (bedRenderer.material != null)
                {
                    bedRenderer.material.EnableKeyword("_EMISSION");
                }
            }
            
            bedRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    void UpdateUIText()
    {
        string displayText = currentTemperature.ToString(temperatureFormat.Replace("°C", ""));
        if (showUnit)
        {
            displayText += "°C";
        }
        
        float normalizedTemp = Mathf.InverseLerp(minTemperature, maxTemperature, currentTemperature);
        Color textColor = temperatureGradient.Evaluate(normalizedTemp);
        textColor.a = 1f;
        
        if (temperatureTMPText != null)
        {
            temperatureTMPText.text = displayText;
            temperatureTMPText.color = textColor;
        }
        else if (temperatureText != null)
        {
            temperatureText.text = displayText;
            temperatureText.color = textColor;
        }
    }

    /// <summary>
    /// 更新UI位置（如果需要）
    /// </summary>
    public void UpdateUIPosition()
    {
        if (uiCanvasTransform != null)
        {
            CalculateFixedUIPosition();
            uiCanvasTransform.position = fixedUIPosition;
        }
    }

    /// <summary>
    /// 设置UI缩放
    /// </summary>
    public void SetUIScale(float scale)
    {
        uiScale = scale;
        
        if (uiCanvasTransform != null)
        {
            uiCanvasTransform.localScale = new Vector3(uiScale, uiScale, uiScale);
        }
    }

    #region 调试和编辑器方法
    void OnDrawGizmosSelected()
    {
        // 在编辑器中显示UI位置
        Gizmos.color = Color.yellow;
        Vector3 uiPos = CalculateUIPosition();
        Gizmos.DrawWireSphere(uiPos, 0.05f);
        Gizmos.DrawLine(transform.position, uiPos);
        
        // 显示UI边界框
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(uiPos, new Vector3(0.2f, 0.05f, 0.01f));
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateTemperatureDisplay();
        }
    }
    
    [ContextMenu("自动收集床铺子模型")]
    void CollectBedRenderersInEditor()
    {
        bedRenderers = GetComponentsInChildren<Renderer>();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("删除现有UI并创建新的")]
    void RecreateUI()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("只能在编辑模式下使用此功能");
            return;
        }
        
        // 删除现有的UI
        if (temperatureTMPText != null)
        {
            Canvas canvas = temperatureTMPText.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            temperatureTMPText = null;
        }
        
        if (temperatureText != null)
        {
            Canvas canvas = temperatureText.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                DestroyImmediate(canvas.gameObject);
            }
            temperatureText = null;
        }
        
        // 创建新的UI
        CreateUI();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("固定UI位置到当前位置")]
    void FixUIPositionAtCurrent()
    {
        if (uiCanvasTransform != null)
        {
            fixedUIPosition = uiCanvasTransform.position;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    
    [ContextMenu("创建UI锚点")]
    void CreateUIAnchor()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("只能在编辑模式下使用此功能");
            return;
        }
        
        // 创建UI锚点
        GameObject anchorGO = new GameObject("UIAnchor");
        anchorGO.transform.SetParent(transform);
        
        // 计算床铺边界框中心上方位置
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;
        
        foreach (Renderer renderer in bedRenderers)
        {
            if (renderer != null)
            {
                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }
        }
        
        
        anchorGO.transform.position = transform.position + uiOffset;
        
        // 设置锚点
        uiAnchor = anchorGO.transform;
        UnityEditor.EditorUtility.SetDirty(this);
    }
    #endif
    #endregion
}