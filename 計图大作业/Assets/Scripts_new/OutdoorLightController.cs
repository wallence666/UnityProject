using UnityEngine;

public class OutdoorLightController : MonoBehaviour
{
    [Header("户外灯光组件")]
    [Tooltip("房子外面的 Light 组件")]
    public Light targetLight;
    
    [Header("开灯时段控制")]
    [Tooltip("指定在哪两个小时之间开灯 (例如 20.0 到 6.0)")]
    public float turnOnHour = 20.0f; // 晚上 8 点 (20:00)
    public float turnOffHour = 6.0f;  // 早上 6 点 (6:00)

    private bool lightShouldBeOn = false;

    void Start()
    {
        if (targetLight == null)
        {
            Debug.LogError("OutdoorLightController：Target Light 未设置！");
            return;
        }

        // 订阅时间管理器的小时变化事件
        TimeOfDayManager.OnHourChanged += CheckLightStatus;
    }

    /// <summary>
    /// 当 TimeOfDayManager 触发 OnHourChanged 事件时调用
    /// </summary>
    private void CheckLightStatus(float currentHours)
    {
        // 处理跨午夜的开灯逻辑 (例如 20:00 到 6:00)
        if (turnOnHour > turnOffHour)
        {
            // 例如：20:00 < currentHours < 24:00 OR 0:00 < currentHours < 6:00
            lightShouldBeOn = (currentHours >= turnOnHour && currentHours <= 24f) || (currentHours >= 0f && currentHours < turnOffHour);
        }
        else
        {
            // 例如：8:00 到 18:00 这种不跨午夜的逻辑 (不太符合户外灯场景，但作为备选)
            lightShouldBeOn = (currentHours >= turnOnHour && currentHours < turnOffHour);
        }

        // 根据计算结果设置灯光状态
        targetLight.enabled = lightShouldBeOn;
    }
}