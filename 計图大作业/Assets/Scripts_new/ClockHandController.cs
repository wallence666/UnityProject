using UnityEngine;

public class ClockHandController : MonoBehaviour
{
    [Header("时钟指针模型")]
    public Transform hourHand;   // 时针
    public Transform minuteHand; // 分针
    public Transform secondHand; // 秒针

    private Vector3 initialRotation; // 用于存储时钟的初始旋转

    void Start()
    {
        if (hourHand != null)
        {
            initialRotation = hourHand.localEulerAngles; // 记录初始旋转，以便复位
        }
        
        // 订阅时间管理器的小时变化事件
        TimeOfDayManager.OnHourChanged += UpdateClockHands;

        // 在游戏结束或禁用时，确保取消订阅
        // TimeOfDayManager.OnHourChanged -= UpdateClockHands;
    }

    /// <summary>
    /// 当 TimeOfDayManager 触发 OnHourChanged 事件时调用
    /// </summary>
    private void UpdateClockHands(float currentHours)
    {
        // currentHours 范围是 0.0 到 24.0

        // 计算旋转角度：时钟旋转是顺时针，Unity 默认旋转是逆时针。
        // 时针：24 小时旋转 360 度，每小时旋转 15 度 (360/24)
        float hourRotation = currentHours * 15f; 
        
        // 分针：60 分钟旋转 360 度，每小时旋转 360 度
        float minuteRotation = currentHours * 360f; 

        // 秒针：60 秒旋转 360 度 (如果需要秒针的话)
        // float secondRotation = (currentHours * 3600) % 60 * 6f; // 复杂计算，如果不需要可忽略

        // -----------------------------------------------------

        if (hourHand != null)
        {
            // 时针：应用旋转（假设时针在局部 Z 轴旋转）
            // 注意：减号用于将 Unity 的逆时针旋转转换为顺时针（时钟）
            hourHand.localRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, initialRotation.z - hourRotation);
        }
        
        if (minuteHand != null)
        {
            // 分针：只看小数部分，相当于 0-60 分钟的旋转
            // 这里的 currentHours * 360f 已经包含了小时到分钟的累积旋转
            minuteHand.localRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, initialRotation.z - (currentHours % 1f) * 360f);
        }
        
        // 如果你的时钟指针模型是垂直于 Y 轴的，你需要调整旋转轴（例如 `Vector3.up`）
        // 示例：transform.Rotate(Vector3.up, -hourRotation, Space.Self); 
    }
}