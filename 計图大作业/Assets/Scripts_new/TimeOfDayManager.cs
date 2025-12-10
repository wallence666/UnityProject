using UnityEngine;
using System.Collections.Generic;

public class TimeOfDayManager : MonoBehaviour
{
    [Header("时间流速控制")]
    [Tooltip("游戏时间与现实时间的比例。例如：300 = 1 秒现实时间等于 300 秒游戏时间。")]
    public float timeScale = 300f; 
    
    [Tooltip("当前游戏时间（从 0.0 到 24.0 小时）")]
    [Range(0f, 24f)]
    public float gameTime = 8.0f; // 默认从早上 8 点开始

    [Header("天空盒配置")]
    [Tooltip("定义四个时间段的开始时间（小时）和对应的天空盒材质")]
    public TimeSegment[] timeSegments;

    // 委托和事件：用于通知其他对象时间或时间段发生了变化
    public delegate void TimeChangeAction(float currentHours);
    public static event TimeChangeAction OnHourChanged; // 每当小时变化时触发 (用于时钟指针)

    public delegate void TimeSegmentAction(Material newSkybox, bool isNight);
    public static event TimeSegmentAction OnTimeSegmentChanged; // 当时间段切换时触发

    private int currentSegmentIndex = -1;
    private float lastHour = -1f;

    [System.Serializable]
    public struct TimeSegment
    {
        [Tooltip("该时间段开始的小时（0.0到24.0）")]
        public float startHour;
        [Tooltip("该时间段对应的天空盒材质")]
        public Material skyboxMaterial;
        [Tooltip("该时段是否应被视为夜晚（用于控制户外灯）")]
        public bool isNight;
    }

    void Update()
    {
        // 1. 推进游戏时间
        gameTime += Time.deltaTime * (timeScale / 3600f); // 3600f = 1 小时
        if (gameTime >= 24f)
        {
            gameTime -= 24f; // 循环到新的一天
        }

        // 2. 检查是否小时发生了变化 (用于同步时钟指针)
        int currentHourInt = Mathf.FloorToInt(gameTime);
        if (currentHourInt != Mathf.FloorToInt(lastHour))
        {
            if (OnHourChanged != null)
            {
                OnHourChanged(gameTime);
            }
        }
        lastHour = gameTime;


        // 3. 检查并切换时间段和天空盒
        CheckAndSetSkybox();
    }

    private void CheckAndSetSkybox()
    {
        if (timeSegments.Length == 0) return;

        int newSegmentIndex = -1;

        // 找到当前时间应该处于哪个时间段
        for (int i = 0; i < timeSegments.Length; i++)
        {
            // 注意：我们假设时间段是按 startHour 升序排列的
            if (gameTime >= timeSegments[i].startHour)
            {
                newSegmentIndex = i;
            }
        }

        // 检查是否需要切换到下一个时间段（当时间段循环时，newSegmentIndex 可能回到 0）
        if (newSegmentIndex != currentSegmentIndex)
        {
            currentSegmentIndex = newSegmentIndex;
            
            TimeSegment currentSegment = timeSegments[currentSegmentIndex];

            // 切换全局天空盒
            RenderSettings.skybox = currentSegment.skyboxMaterial;
            
            // 立即更新环境光照
            DynamicGI.UpdateEnvironment();

            // 触发事件通知订阅者 (如户外灯)
            if (OnTimeSegmentChanged != null)
            {
                OnTimeSegmentChanged(currentSegment.skyboxMaterial, currentSegment.isNight);
            }

            Debug.Log($"时间段切换到: {currentSegmentIndex} (开始时间: {currentSegment.startHour:F1}h)");
        }
    }
}