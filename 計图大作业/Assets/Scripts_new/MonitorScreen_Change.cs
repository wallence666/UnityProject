using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorScreen_Change : MonoBehaviour
{
    [Header("渲染设置")]
    [SerializeField] private RenderTexture[] screenRenderTextures;  // 渲染纹理数组，数量不固定
    [SerializeField] private int currentTextureIndex = 0;  // 当前渲染纹理索引

    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 3f;  // 交互距离
    [SerializeField] private string mouseClickButton = "Fire1";  // 交互按键（默认鼠标左键）

    [Header("屏幕效果")]
    [SerializeField] private Light screenLight;  // 屏幕光源（可选）
    [SerializeField] private float lightIntensity = 1f;  // 屏幕发光强度
    [SerializeField] private Color[] screenColors;  // 屏幕发光颜色（可选）
    [SerializeField] private float clickEffectDuration = 0.2f;  // 点击效果持续时间

    private MeshRenderer screenRenderer;
    private Material screenMaterial;
    private Camera playerCamera;
    private bool isPlayerLooking = false;
    private bool isClickEffectActive = false;
    private float clickEffectTime = 0f;

    private void Start()
    {
        // 获取屏幕渲染器
        screenRenderer = GetComponent<MeshRenderer>();
        if (screenRenderer == null)
        {
            Debug.LogError($"屏幕 {gameObject.name} 缺少MeshRenderer组件");
            return;
        }

        // 获取或创建屏幕材质
        screenMaterial = screenRenderer.material;
        if (screenMaterial == null)
        {
            screenMaterial = new Material(Shader.Find("Standard"));
            screenRenderer.material = screenMaterial;
        }

        // 检查渲染纹理数组
        if (screenRenderTextures == null || screenRenderTextures.Length == 0)
        {
            Debug.LogWarning($"屏幕 {gameObject.name} 没有设置渲染纹理");
            return;
        }

        // 查找玩家相机
        playerCamera = Camera.main;

        // 自动查找屏幕光源（子物体）
        if (screenLight == null)
        {
            screenLight = GetComponentInChildren<Light>();
        }

        // 初始化光源
        if (screenLight != null)
        {
            // 如果提供了颜色数组，设置初始颜色
            if (screenColors != null && screenColors.Length > 0)
            {
                UpdateScreenLight();
            }
        }

        // 设置初始渲染纹理
        SetRenderTexture(currentTextureIndex);
    }

    private void Update()
    {
        if (playerCamera == null) return;

        // 检测玩家是否看着屏幕
        CheckIfPlayerIsLooking();

        // 检测交互输入
        if (isPlayerLooking && Input.GetButtonDown(mouseClickButton))
        {
            OnScreenClicked();
        }

        // 更新点击效果
        UpdateClickEffect();
    }

    private void CheckIfPlayerIsLooking()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        isPlayerLooking = Physics.Raycast(ray, out hit, interactionDistance) &&
                         hit.collider.gameObject == gameObject;
    }

    private void OnScreenClicked()
    {
        // 触发点击效果
        StartClickEffect();

        // 切换到下一个渲染纹理
        SwitchToNextRenderTexture();

        Debug.Log($"屏幕被点击，切换到渲染纹理 {currentTextureIndex + 1}/{screenRenderTextures.Length}");
    }

    private void SwitchToNextRenderTexture()
    {
        // 增加纹理索引
        currentTextureIndex++;

        // 循环索引（0, 1, 2, ..., n, 0, 1, 2, ...）
        if (currentTextureIndex >= screenRenderTextures.Length)
        {
            currentTextureIndex = 0;
        }

        // 设置新渲染纹理
        SetRenderTexture(currentTextureIndex);
    }

    private void SetRenderTexture(int index)
    {
        if (screenMaterial != null &&
            screenRenderTextures != null &&
            index >= 0 && index < screenRenderTextures.Length)
        {
            // 设置渲染纹理到材质的_MainTex属性
            screenMaterial.mainTexture = screenRenderTextures[index];

            // 更新屏幕光源
            UpdateScreenLight();
        }
    }

    private void UpdateScreenLight()
    {
        if (screenLight != null)
        {
            // 如果提供了颜色数组，根据纹理索引设置颜色
            if (screenColors != null && screenColors.Length > 0)
            {
                int colorIndex = currentTextureIndex % screenColors.Length;
                screenLight.color = screenColors[colorIndex];
            }

            // 设置发光强度
            screenLight.intensity = lightIntensity;
        }
    }

    private void StartClickEffect()
    {
        isClickEffectActive = true;
        clickEffectTime = 0f;

        // 如果屏幕光源存在，短暂增强亮度
        if (screenLight != null)
        {
            screenLight.intensity = lightIntensity * 2f;
        }
    }

    private void UpdateClickEffect()
    {
        if (!isClickEffectActive) return;

        clickEffectTime += Time.deltaTime;

        if (clickEffectTime >= clickEffectDuration)
        {
            // 恢复原始亮度
            if (screenLight != null)
            {
                screenLight.intensity = lightIntensity;
            }

            isClickEffectActive = false;
        }
        else if (screenLight != null)
        {
            // 渐变恢复亮度
            float t = clickEffectTime / clickEffectDuration;
            float intensity = Mathf.Lerp(lightIntensity * 2f, lightIntensity, t);
            screenLight.intensity = intensity;
        }
    }

    // 直接切换到指定渲染纹理（可用于事件触发）
    public void SetRenderTextureByIndex(int index)
    {
        if (index >= 0 && index < screenRenderTextures.Length)
        {
            currentTextureIndex = index;
            SetRenderTexture(currentTextureIndex);
            Debug.Log($"屏幕切换到渲染纹理 {currentTextureIndex + 1}/{screenRenderTextures.Length}");
        }
    }

    // 获取当前渲染纹理索引
    public int GetCurrentTextureIndex()
    {
        return currentTextureIndex;
    }

    // 获取当前渲染纹理名称
    public string GetCurrentTextureName()
    {
        if (screenRenderTextures != null &&
            currentTextureIndex >= 0 &&
            currentTextureIndex < screenRenderTextures.Length)
        {
            return screenRenderTextures[currentTextureIndex].name;
        }
        return "无纹理";
    }

    // 获取渲染纹理数量
    public int GetTextureCount()
    {
        return screenRenderTextures != null ? screenRenderTextures.Length : 0;
    }
}