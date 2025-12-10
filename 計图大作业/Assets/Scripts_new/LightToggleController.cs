using UnityEngine;

public class LightToggleController : MonoBehaviour
{
    [Header("目标光源")]
    [Tooltip("将场景中要控制的 Light 组件（例如 Point Light）拖放到这里。")]
    public Light targetLight; 

    // 私有变量，用于跟踪光源的开启/关闭状态
    private bool isLightOn = true; // 默认光源开始时是开启的 (取决于场景设置)

    void Start()
    {
        // 初始化时，确保脚本中的状态与 Light 组件的当前状态一致
        if (targetLight != null)
        {
            isLightOn = targetLight.enabled;
        }
        else
        {
            Debug.LogError("LightToggleController：目标光源未设置！请在 Inspector 中拖放 Light 组件。");
        }
    }

    // 这是一个 Unity 特定的函数，当用户点击带有 Collider 的物体时触发
    void OnMouseDown()
    {
        // 检查目标光源是否已设置
        if (targetLight != null)
        {
            ToggleLight();
        }
    }

    /// <summary>
    /// 切换光源的开关状态，控制 Light 组件的 enabled 属性。
    /// </summary>
    private void ToggleLight()
    {
        // 切换布尔值
        isLightOn = !isLightOn;

        // 设置光源组件的 enabled 属性
        targetLight.enabled = isLightOn;

        // 【可选：日志提示】
        // Debug.Log("光源状态切换到：" + (isLightOn ? "开启" : "关闭"));
    }
}