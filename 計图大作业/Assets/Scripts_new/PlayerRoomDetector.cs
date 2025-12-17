using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// 玩家房间检测器示例
/// </summary>
public class PlayerRoomDetector : MonoBehaviour
{
    private RoomData currentRoom;
    private RoomDataManager roomManager;
    
    private void Start()
    {
        roomManager = FindObjectOfType<RoomDataManager>();
        if (roomManager == null)
        {
            Debug.LogError("未找到RoomDataManager");
        }
    }
    
    private void Update()
    {
        DetectCurrentRoom();
    }
    
    private void DetectCurrentRoom()
    {
        if (roomManager == null) return;
        
        var newRoom = roomManager.GetRoomAtPosition(transform.position);
        
        if (newRoom != currentRoom)
        {
            if (currentRoom != null)
            {
                RoomEvents.TriggerRoomExited(currentRoom.RoomType);
            }
            
            currentRoom = newRoom;
            
            if (currentRoom != null)
            {
                RoomEvents.TriggerRoomEntered(currentRoom.RoomType);
                Debug.Log($"进入房间: {currentRoom.RoomType}, 温度: {currentRoom.Temperature}°C");
            }
            else
            {
                Debug.Log("离开房间区域");
            }
        }
    }
}

