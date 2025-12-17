using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    [Header("光源设置")]
    [SerializeField] private List<Light> controlledLights = new List<Light>();
    [SerializeField] private bool startWithLightsOn = true; // 初始光源状态

    private bool allLightsOn = true;

    void Start()
    {
        // 设置初始状态
        allLightsOn = startWithLightsOn;
        ApplyLightsState();

        Debug.Log($"LightManager 初始化完成，初始状态: {(allLightsOn ? "所有光源开启" : "所有光源关闭")}");
    }

    /// <summary>
    /// 打开所有光源
    /// </summary>
    public void TurnOnAllLights()
    {
        allLightsOn = true;
        ApplyLightsState();
        Debug.Log("所有光源已打开");
    }

    /// <summary>
    /// 关闭所有光源
    /// </summary>
    public void TurnOffAllLights()
    {
        allLightsOn = false;
        ApplyLightsState();
        Debug.Log("所有光源已关闭");
    }

    /// <summary>
    /// 切换所有光源状态
    /// </summary>
    public void ToggleAllLights()
    {
        allLightsOn = !allLightsOn;
        ApplyLightsState();
        Debug.Log($"所有光源状态: {(allLightsOn ? "开启" : "关闭")}");
    }

    /// <summary>
    /// 应用当前状态到所有光源
    /// </summary>
    private void ApplyLightsState()
    {
        foreach (Light light in controlledLights)
        {
            if (light != null)
            {
                light.enabled = allLightsOn;
            }
        }
    }

    /// <summary>
    /// 获取当前光源状态
    /// </summary>
    public bool AreAllLightsOn()
    {
        return allLightsOn;
    }

    /// <summary>
    /// 获取管理的光源数量
    /// </summary>
    public int GetLightCount()
    {
        return controlledLights.Count;
    }

    /// <summary>
    /// 添加光源到管理器
    /// </summary>
    public void AddLight(Light light)
    {
        if (!controlledLights.Contains(light))
        {
            controlledLights.Add(light);
            // 立即应用当前状态
            if (light != null)
            {
                light.enabled = allLightsOn;
            }
            Debug.Log($"已将光源 {light.gameObject.name} 添加到 LightManager");
        }
    }

    /// <summary>
    /// 从管理器移除光源
    /// </summary>
    public void RemoveLight(Light light)
    {
        if (controlledLights.Contains(light))
        {
            controlledLights.Remove(light);
            Debug.Log($"已将光源 {light.gameObject.name} 从 LightManager 移除");
        }
    }

    /// <summary>
    /// 自动查找场景中所有光源
    /// </summary>
    [ContextMenu("查找场景中所有点光源和聚光灯")]
    public void FindAllLightsInScene()
    {
        Light[] allLights = FindObjectsOfType<Light>(true); // true 包含非激活的
        controlledLights.Clear();

        int pointLightCount = 0;
        int spotLightCount = 0;
        int otherLightCount = 0;

        foreach (Light light in allLights)
        {
            // 过滤特定类型的光源（点光源和聚光灯）
            if (light.type == LightType.Point || light.type == LightType.Spot)
            {
                controlledLights.Add(light);

                if (light.type == LightType.Point)
                    pointLightCount++;
                else if (light.type == LightType.Spot)
                    spotLightCount++;
            }
            else
            {
                otherLightCount++;
            }
        }

        Debug.Log($"找到 {controlledLights.Count} 个可控光源（{pointLightCount}个点光源，{spotLightCount}个聚光灯）");
        Debug.Log($"跳过 {otherLightCount} 个其他类型光源（方向光、区域光等）");
    }
}