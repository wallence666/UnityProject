using System.Collections.Generic;
using UnityEngine;

namespace Supercyan.FreeSample
{
    public class SimpleSampleCharacterControl : MonoBehaviour
    {
        private enum ControlMode
        {
            /// <summary>
            /// Up moves the character forward/back, left/right strafes sideways, and rotation syncs to the camera. (Used for FPP)
            /// </summary>
            Tank, // 用于同步摄像机转向的模式
            /// <summary>
            /// Character freely moves in the chosen direction from the perspective of the camera
            /// </summary>
            Direct
        }

        [SerializeField] private float m_moveSpeed = 2f;
        [SerializeField] private float m_turnSpeed = 200f; // 实际 FPP 模式中不再使用
        [SerializeField] private float m_jumpForce = 4f;

        [SerializeField] private Animator m_animator = null;
        [SerializeField] private Rigidbody m_rigidBody = null;

        // 【注意：我们默认使用 Tank 模式来实现 FPS 控制】
        [SerializeField] private ControlMode m_controlMode = ControlMode.Tank; 

        // 【新增：用于解决 OnMouseDown 交互问题中可能出现的光标锁定引用错误】
        private bool m_cursorIsLocked = true; 
        
        private float m_currentV = 0f;
        private float m_currentH = 0f;

        private readonly float m_interpolation = 10f;
        private readonly float m_walkScale = 0.33f;
        private readonly float m_backwardsWalkScale = 0.16f;
        private readonly float m_backwardRunScale = 0.66f;

        private bool m_wasGrounded;
        private Vector3 m_currentDirection = Vector3.zero;

        private float m_jumpTimeStamp = 0f;
        private float m_minJumpInterval = 0.25f;
        private bool m_jumpInput = false;

        private bool m_isGrounded;

        private List<Collider> m_collisions = new List<Collider>();

        private void Awake()
        {
            // 【修复：确保获取正确的组件】
            if (!m_animator) { m_animator = gameObject.GetComponent<Animator>(); }
            if (!m_rigidBody) { m_rigidBody = gameObject.GetComponent<Rigidbody>(); }
        }

        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint[] contactPoints = collision.contacts;
            for (int i = 0; i < contactPoints.Length; i++)
            {
                if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
                {
                    if (!m_collisions.Contains(collision.collider))
                    {
                        m_collisions.Add(collision.collider);
                    }
                    m_isGrounded = true;
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            ContactPoint[] contactPoints = collision.contacts;
            bool validSurfaceNormal = false;
            for (int i = 0; i < contactPoints.Length; i++)
            {
                if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
                {
                    validSurfaceNormal = true; break;
                }
            }

            if (validSurfaceNormal)
            {
                m_isGrounded = true;
                if (!m_collisions.Contains(collision.collider))
                {
                    m_collisions.Add(collision.collider);
                }
            }
            else
            {
                if (m_collisions.Contains(collision.collider))
                {
                    m_collisions.Remove(collision.collider);
                }
                if (m_collisions.Count == 0) { m_isGrounded = false; }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }

        private void Update()
        {
            if (!m_jumpInput && Input.GetKey(KeyCode.Space))
            {
                m_jumpInput = true;
            }

            // 【添加：光标锁定逻辑，解决之前可能的编译错误】
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void FixedUpdate()
        {
            m_animator.SetBool("Grounded", m_isGrounded);

            switch (m_controlMode)
            {
                case ControlMode.Direct:
                    DirectUpdate();
                    break;

                case ControlMode.Tank:
                    TankUpdate(); // 调用修改后的同步逻辑
                    break;

                default:
                    Debug.LogError("Unsupported state");
                    break;
            }

            m_wasGrounded = m_isGrounded;
            m_jumpInput = false;
        }

        // 【修改后的 TankUpdate：实现人物转向同步摄像机】
        private void TankUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal"); // A/D 用于侧向移动 (Strafe)

            bool walk = Input.GetKey(KeyCode.LeftShift);

            // 速度缩放逻辑... (保持不变)
            if (v < 0)
            {
                if (walk) { v *= m_backwardsWalkScale; }
                else { v *= m_backwardRunScale; }
            }
            else if (walk)
            {
                v *= m_walkScale;
            }
            
            // 侧移速度缩放
            if (walk)
            {
                h *= m_walkScale;
            }

            m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
            m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

            // 1. 计算 FPP 移动向量 (前/后 + 左/右侧移)
            Vector3 moveVector = transform.forward * m_currentV + transform.right * m_currentH;
            
            // 2. 使用 Rigidbody 施加力来移动
            Vector3 currentVelocity = m_rigidBody.velocity;
            Vector3 targetVelocity = new Vector3(moveVector.x * m_moveSpeed, currentVelocity.y, moveVector.z * m_moveSpeed);
            Vector3 velocityChange = (targetVelocity - currentVelocity);
            m_rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);


            // 【关键修改点 1：移除原 Tank 模式的旋转】
            // transform.Rotate(0, m_currentH * m_turnSpeed * Time.deltaTime, 0); // 原本是左右转向，现在移除

            // 【关键修改点 2：人物转向同步摄像机】
            // 强制人物的水平旋转与主摄像机的水平旋转同步
            Transform cameraTransform = Camera.main.transform;
            Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            transform.rotation = targetRotation;


            m_animator.SetFloat("MoveSpeed", moveVector.magnitude); 

            JumpingAndLanding();
        }

        // DirectUpdate 保持不变 (原版 TPA 逻辑)
        private void DirectUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            Transform camera = Camera.main.transform;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                v *= m_walkScale;
                h *= m_walkScale;
            }

            m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
            m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

            Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

            float directionLength = direction.magnitude;
            direction.y = 0;
            direction = direction.normalized * directionLength;

            if (direction != Vector3.zero)
            {
                m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

                transform.rotation = Quaternion.LookRotation(m_currentDirection);
                transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

                m_animator.SetFloat("MoveSpeed", direction.magnitude);
            }

            JumpingAndLanding();
        }

        // JumpingAndLanding 函数修正（修复末尾被截断的代码）
        private void JumpingAndLanding()
        {
            bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

            if (jumpCooldownOver && m_isGrounded && m_jumpInput)
            {
                m_jumpTimeStamp = Time.time;
                // 【修复：使用 Rigidbody AddForce 实现跳跃】
                if (m_rigidBody != null)
                {
                    m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
                }
            }

            if (!m_wasGrounded && m_isGrounded)
            {
                m_animator.SetTrigger("Land");
            }
        }
    }
}