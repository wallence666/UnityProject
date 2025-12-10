using UnityEngine;

public class FanController : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("风扇叶片在 Y 轴上旋转的速度（度/秒）。")]
    public float rotationSpeed = 500f; 

    // 私有变量，用于跟踪风扇的开启/关闭状态
    private bool isFanOn = false;

    /// <summary>
    /// Update 函数每帧调用一次，用于实现不间断的旋转。
    /// </summary>
    void Update()
    {
        // 只有当 isFanOn 为 true 时，才执行旋转逻辑
        if (isFanOn)
        {
            // 围绕局部坐标系的 Y 轴旋转
            // Time.deltaTime 确保旋转速度与帧率无关，实现平滑且恒定的速度
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// 公共方法：切换风扇的开关状态。
    /// 这个方法会被 UI 按钮调用。
    /// </summary>
    public void ToggleFanPower()
    {
        // 切换布尔值：如果当前是 true 就变成 false，反之亦然
        isFanOn = !isFanOn;

        // 【可选：添加声音或提示】
        if (isFanOn)
        {
            Debug.Log("风扇已启动！");
            // 你可以在这里添加播放启动音效的代码
        }
        else
        {
            Debug.Log("风扇已关闭！");
            // 你可以在这里添加播放关闭音效的代码
        }
    }
}