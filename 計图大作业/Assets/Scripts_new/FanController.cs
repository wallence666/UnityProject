using UnityEngine;

public class FanController : MonoBehaviour
{
    [Header("房间设置")]
    [SerializeField] private RoomType roomType = RoomType.LivingRoom; // 风扇所在的房间类型

    [Header("温度设置")]
    [SerializeField] private float targetTemperature = 30.0f; // 设置温度
    [SerializeField] private float halfLifeTime = 5.0f; // 使用半衰期方法平衡内外温度


    [Header("旋转设置")]
    [Tooltip("风扇叶片在 Y 轴上旋转的速度（度/秒）。")]
    public float rotationSpeed = 500f; 

    private RoomDataManager roomManager;

    // 私有变量，用于跟踪风扇的开启/关闭状态
    private bool isFanOn = false;

    
    void Start()
    {
        roomManager = FindObjectOfType<RoomDataManager>();
    }


    /// <summary>
    /// Update 函数每帧调用一次，用于实现不间断的旋转。
    /// </summary>
    void Update()
    {
        // 只有当 isFanOn 为 true 时，才执行旋转逻辑
        if (isFanOn)
        {
            if(roomManager[roomType].tempLock == 1)
            {
                roomManager.UpdateRoomTemperature(roomType,targetTemperature + Mathf.Pow(0.5f,Time.deltaTime/halfLifeTime) * (roomManager[roomType].Temperature- targetTemperature));
            }
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
            roomManager[roomType].tempLock++;
            Debug.Log("风扇已启动！");
            // 你可以在这里添加播放启动音效的代码
        }
        else
        {
            roomManager[roomType].tempLock--;
            if(roomManager[roomType].tempLock < 0) roomManager[roomType].tempLock = 0;
            Debug.Log("风扇已关闭！");
            // 你可以在这里添加播放关闭音效的代码
        }
    }
}