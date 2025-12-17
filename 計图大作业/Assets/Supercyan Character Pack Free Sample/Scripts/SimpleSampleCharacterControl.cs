using System.Collections.Generic;
using UnityEngine;

namespace Supercyan.FreeSample
{
    public class SimpleSampleCharacterControl : MonoBehaviour
    {
        private enum ControlMode { Tank, Direct }

        [Header("Movement Settings")]
        [SerializeField] private float m_moveSpeed = 4f;
        [SerializeField] private float m_jumpForce = 5f;
        [SerializeField] private float m_interpolation = 10f;

        [Header("References")]
        [SerializeField] private Animator m_animator = null;
        [SerializeField] private Rigidbody m_rigidBody = null;
        [SerializeField] private ControlMode m_controlMode = ControlMode.Tank;

        private float m_currentV = 0f;
        private float m_currentH = 0f;

        private readonly float m_walkScale = 0.33f;
        private readonly float m_backwardsWalkScale = 0.16f;
        private readonly float m_backwardRunScale = 0.66f;

        private bool m_wasGrounded;
        private bool m_isGrounded;
        private bool m_jumpInput = false;
        private float m_jumpTimeStamp = 0f;
        private float m_minJumpInterval = 0.25f;

        private List<Collider> m_collisions = new List<Collider>();

        private void Awake()
        {
            if (!m_animator) { m_animator = GetComponent<Animator>(); }
            if (!m_rigidBody) { m_rigidBody = GetComponent<Rigidbody>(); }
            
            if (m_rigidBody)
            {
                // [解決旋轉方案 1] 強制鎖定物理引擎導致的旋轉
                m_rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        private void Update()
        {
            if (!m_jumpInput && Input.GetKeyDown(KeyCode.Space))
            {
                m_jumpInput = true;
            }
        }

        private void FixedUpdate()
        {
            // [解決旋轉方案 2] 每一幀都將物理旋轉速度歸零，防止碰撞導致的意外轉向
            if (m_rigidBody != null)
            {
                m_rigidBody.angularVelocity = Vector3.zero;
            }

            m_animator.SetBool("Grounded", m_isGrounded);

            switch (m_controlMode)
            {
                case ControlMode.Direct:
                    DirectUpdate();
                    break;
                case ControlMode.Tank:
                    TankUpdate(); 
                    break;
            }

            m_wasGrounded = m_isGrounded;
            m_jumpInput = false;
        }

        private void TankUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");
            bool walk = Input.GetKey(KeyCode.LeftShift);

            if (v < 0) { v *= walk ? m_backwardsWalkScale : m_backwardRunScale; }
            else if (walk) { v *= m_walkScale; }
            if (walk) { h *= m_walkScale; }

            m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
            m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

            // 基於當前人物朝向計算移動方向（朝向由相機腳本控制）
            Vector3 moveDirection = transform.forward * m_currentV + transform.right * m_currentH;
            
            if (m_rigidBody != null)
            {
                // 設定水平速度，保留原始垂直速度（重力）
                Vector3 targetVelocity = new Vector3(moveDirection.x * m_moveSpeed, m_rigidBody.velocity.y, moveDirection.z * m_moveSpeed);
                m_rigidBody.velocity = targetVelocity;
            }

            m_animator.SetFloat("MoveSpeed", moveDirection.magnitude);
            JumpingAndLanding();
        }

        private void DirectUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");
            Transform camera = Camera.main.transform;

            if (Input.GetKey(KeyCode.LeftShift)) { v *= m_walkScale; h *= m_walkScale; }

            m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
            m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

            Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * m_interpolation);
                transform.position += direction * m_moveSpeed * Time.deltaTime;
                m_animator.SetFloat("MoveSpeed", direction.magnitude);
            }
            JumpingAndLanding();
        }

        private void JumpingAndLanding()
        {
            bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;
            if (jumpCooldownOver && m_isGrounded && m_jumpInput)
            {
                m_jumpTimeStamp = Time.time;
                m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            }

            if (!m_wasGrounded && m_isGrounded) { m_animator.SetTrigger("Land"); }
        }

        private void OnCollisionEnter(Collision collision) { CheckGrounded(collision); }
        private void OnCollisionStay(Collision collision) { CheckGrounded(collision); }

        private void CheckGrounded(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
                {
                    if (!m_collisions.Contains(collision.collider)) { m_collisions.Add(collision.collider); }
                    m_isGrounded = true;
                    return;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (m_collisions.Contains(collision.collider)) { m_collisions.Remove(collision.collider); }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }
}