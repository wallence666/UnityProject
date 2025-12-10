using UnityEngine;

public class Button3DClickTrigger : MonoBehaviour
{
    [Header("目标风扇模型")]
    [Tooltip("将挂载 FanController 脚本的风扇模型拖放到这里")]
    public FanController targetFan;

    // 这是一个 Unity 特定的函数，当用户点击带有 Collider 的物体时触发
    void OnMouseDown()
    {
        // 检查目标风扇控制器是否已设置
        if (targetFan != null)
        {
            // 调用风扇控制器上的开关方法
            targetFan.ToggleFanPower();
            
            // 【可选：按钮模型视觉反馈】
            // 如果你希望按钮被按下时有视觉上的移动，可以在这里添加代码。
            // 例如： transform.position -= Vector3.up * 0.01f;
            // 并在 OnMouseUp 或一段时间后恢复位置。
        }
        else
        {
            Debug.LogError("Button3DClickTrigger：目标风扇未设置！请在 Inspector 中拖放目标风扇模型。");
        }
    }
}