using UnityEngine;
using System.Collections; // 必须导入，用于支持 Coroutine

public class DoorController : MonoBehaviour
{
    // 公开变量，可以在 Inspector 窗口中调整
    [Header("开门设置")]
    [Tooltip("门旋转的角度（逆时针为正值，顺时针为负值）")]
    public float openAngle = 80f; 

    [Tooltip("门旋转所需的时间（秒）")]
    public float rotationDuration = 1.0f;

    [Tooltip("开门时播放的声音（可选）")]
    public AudioClip openSound;

    // 私有变量
    private bool isOpen = false; // 跟踪门的状态
    private Quaternion initialRotation; // 门的初始旋转值
    private AudioSource audioSource; // 用于播放声音的组件

    void Start()
    {
        // 记录门模型刚开始时的旋转值
        initialRotation = transform.rotation;

        // 尝试获取 AudioSource 组件，如果不存在则添加一个
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // 通常，门的声音不应该循环播放
        audioSource.loop = false;
        // 声音源不需要 3D 效果
        audioSource.spatialBlend = 0.0f;
    }

    // 这是一个 Unity 特定的函数，当用户点击带有 Collider 的物体时触发
    void OnMouseDown()
    {
        // 检查当前是否有动画在播放，如果没有，则执行开/关门操作
        if (transform.rotation == initialRotation || transform.rotation == initialRotation * Quaternion.Euler(0, openAngle, 0))
        {
            ToggleDoor();
        }
    }

    /// <summary>
    /// 切换门的开/关状态，并启动动画协程。
    /// </summary>
    private void ToggleDoor()
    {
        isOpen = !isOpen; // 切换状态

        // 播放声音（如果设置了）
        if (openSound != null)
        {
            audioSource.clip = openSound;
            audioSource.Play();
        }
        
        // 计算目标旋转角度
        Quaternion targetRotation = isOpen 
            ? initialRotation * Quaternion.Euler(0, openAngle, 0) // 开门：初始旋转值 + Y轴旋转角度
            : initialRotation; // 关门：回到初始旋转值

        // 启动平滑旋转动画
        StartCoroutine(RotateDoor(targetRotation));
    }

    /// <summary>
    /// 使用 Coroutine 实现门模型的平滑旋转。
    /// </summary>
    /// <param name="targetRotation">门最终要达到的旋转值。</param>
    IEnumerator RotateDoor(Quaternion targetRotation)
    {
        float timeElapsed = 0;
        Quaternion startRotation = transform.rotation; // 动画开始时的旋转值

        // 在指定时间内平滑地从开始旋转值过渡到目标旋转值
        while (timeElapsed < rotationDuration)
        {
            // 使用 Quaternion.Slerp 实现平滑球面插值
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / rotationDuration);
            timeElapsed += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 确保动画结束后，旋转值精确地等于目标值，防止精度误差
        transform.rotation = targetRotation;
    }
}