using UnityEngine;
using System.Collections;

namespace TrianCatStudio
{
    /// <summary>
    /// 掉落物品行为 - 定义掉落物品的交互和效果
    /// </summary>
    public class DropItem : MonoBehaviour
    {
        [Header("基本设置")]
        [SerializeField] private string itemId;          // 物品ID
        [SerializeField] private int count = 1;          // 物品数量
        [SerializeField] private int itemLevel = 1;      // 物品等级
        [SerializeField] private float lifeTime = 30f;   // 物品存在时间
        [SerializeField] private bool destroyOnPickup = true;  // 拾取后销毁
        
        [Header("物理设置")]
        [SerializeField] private float attractSpeed = 10f;  // 吸引速度
        [SerializeField] private float attractDistance = 3f;  // 吸引距离
        [SerializeField] private float groundCheckOffset = 0.1f;  // 地面检测偏移
        
        [Header("视觉效果")]
        [SerializeField] private GameObject pickupEffect;  // 拾取效果
        [SerializeField] private GameObject highlightEffect;  // 高亮效果
        [SerializeField] private float rotationSpeed = 50f;  // 旋转速度
        [SerializeField] private float floatHeight = 0.2f;  // 浮动高度
        [SerializeField] private float floatSpeed = 1f;  // 浮动速度
        
        [Header("音效")]
        [SerializeField] private AudioClip pickupSound;  // 拾取音效
        [SerializeField] private float pickupVolume = 0.5f;  // 拾取音量
        
        // 实例变量
        private bool isAttracting = false;          // 是否正在吸引
        private Transform targetPlayer = null;      // 目标玩家
        private Vector3 originalPosition;           // 原始位置
        private float dropTime;                     // 掉落时间
        private bool isPickable = false;            // 是否可拾取
        private Rigidbody rb;                       // 刚体组件
        private Collider itemCollider;              // 碰撞体组件
        private Renderer itemRenderer;              // 渲染器组件
        private AudioSource audioSource;            // 音频源组件
        
        // 属性
        public string ItemId => itemId;
        public int Count => count;
        public int ItemLevel => itemLevel;
        
        private void Awake()
        {
            // 获取组件
            rb = GetComponent<Rigidbody>();
            itemCollider = GetComponent<Collider>();
            itemRenderer = GetComponentInChildren<Renderer>();
            audioSource = GetComponent<AudioSource>();
            
            // 如果没有音频源，添加一个
            if (audioSource == null && pickupSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D音效
                audioSource.volume = pickupVolume;
            }
            
            // 存储原始位置
            originalPosition = transform.position;
            
            // 记录掉落时间
            dropTime = Time.time;
            
            // 掉落后短暂延迟才能拾取
            StartCoroutine(EnablePickup());
        }
        
        private void OnEnable()
        {
            // 更新掉落时间
            dropTime = Time.time;
        }
        
        private void Update()
        {
            // 检查生命周期
            if (Time.time - dropTime > lifeTime)
            {
                // 如果超过生命周期，开始闪烁然后销毁
                StartCoroutine(FadeOutAndDestroy());
                return;
            }
            
            // 视觉效果 - 旋转和浮动
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            // 只有在地面上才浮动
            if (IsGrounded())
            {
                // 使用正弦函数实现浮动效果
                float yOffset = Mathf.Sin((Time.time - dropTime) * floatSpeed) * floatHeight;
                transform.position = new Vector3(
                    transform.position.x,
                    originalPosition.y + yOffset,
                    transform.position.z
                );
            }
            else if (rb != null && !rb.isKinematic)
            {
                // 更新原始位置（当物品着地时）
                originalPosition = transform.position;
            }
            
            // 吸引效果
            if (isAttracting && targetPlayer != null)
            {
                AttractToPlayer();
            }
            else
            {
                // 检查附近是否有玩家
                CheckForNearbyPlayer();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // 检查是否是玩家碰撞
            if (isPickable && other.CompareTag("Player"))
            {
                // 尝试拾取物品
                TryPickup(other.transform);
            }
        }
        
        /// <summary>
        /// 设置物品数据
        /// </summary>
        public void SetItemData(string id, int itemCount, int level)
        {
            itemId = id;
            count = itemCount;
            itemLevel = level;
            
            // 更新视觉表现
            UpdateVisuals();
        }
        
        /// <summary>
        /// 更新物品的视觉表现
        /// </summary>
        private void UpdateVisuals()
        {
            // 这里可以根据itemId和level更新物品的外观
            // 例如，更改材质、模型等
            
            // 简单示例：根据物品ID更改颜色
            if (itemRenderer != null)
            {
                Material mat = itemRenderer.material;
                
                // 根据物品ID设置颜色
                switch (itemId)
                {
                    case "health_potion":
                        mat.color = new Color(1f, 0.2f, 0.2f);  // 红色
                        break;
                        
                    case "mana_potion":
                        mat.color = new Color(0.2f, 0.2f, 1f);  // 蓝色
                        break;
                        
                    case "wood":
                        mat.color = new Color(0.6f, 0.4f, 0.2f);  // 棕色
                        break;
                        
                    case "stone":
                        mat.color = new Color(0.6f, 0.6f, 0.6f);  // 灰色
                        break;
                        
                    case "sword":
                        mat.color = new Color(0.8f, 0.8f, 0.8f);  // 银色
                        break;
                        
                    case "shield":
                        mat.color = new Color(0.2f, 0.6f, 0.2f);  // 绿色
                        break;
                        
                    default:
                        mat.color = new Color(1f, 1f, 1f);  // 白色
                        break;
                }
                
                // 根据等级调整亮度
                float levelBonus = Mathf.Min(itemLevel * 0.1f, 0.5f);
                mat.color = new Color(
                    Mathf.Min(mat.color.r + levelBonus, 1f),
                    Mathf.Min(mat.color.g + levelBonus, 1f),
                    Mathf.Min(mat.color.b + levelBonus, 1f)
                );
            }
        }
        
        /// <summary>
        /// 检查是否在地面上
        /// </summary>
        private bool IsGrounded()
        {
            if (rb == null || rb.isKinematic)
                return true;
                
            // 简单的地面检测
            return Physics.Raycast(
                transform.position,
                Vector3.down,
                itemCollider?.bounds.extents.y + groundCheckOffset ?? 0.5f,
                LayerMask.GetMask("Ground")
            );
        }
        
        /// <summary>
        /// 检查附近是否有玩家
        /// </summary>
        private void CheckForNearbyPlayer()
        {
            if (!isPickable)
                return;
                
            // 获取场景中的玩家对象
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                
                // 如果玩家在吸引范围内，开始吸引
                if (distance <= attractDistance)
                {
                    isAttracting = true;
                    targetPlayer = player.transform;
                    
                    // 启用高亮效果
                    EnableHighlight(true);
                    
                    // 关闭物理效果
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.velocity = Vector3.zero;
                    }
                }
            }
        }
        
        /// <summary>
        /// 吸引至玩家
        /// </summary>
        private void AttractToPlayer()
        {
            // 计算方向和距离
            Vector3 direction = targetPlayer.position - transform.position;
            float distance = direction.magnitude;
            
            // 移动物品朝向玩家
            transform.Translate(direction.normalized * attractSpeed * Time.deltaTime, Space.World);
            
            // 如果非常靠近玩家，直接拾取
            if (distance < 0.5f)
            {
                TryPickup(targetPlayer);
            }
        }
        
        /// <summary>
        /// 尝试拾取物品
        /// </summary>
        private void TryPickup(Transform player)
        {
            if (!isPickable)
                return;
                
            // 调用物品管理器添加物品到玩家背包
            bool pickupSuccess = AddItemToPlayerInventory();
            
            if (pickupSuccess)
            {
                // 播放拾取效果
                PlayPickupEffect();
                
                // 播放拾取音效
                PlayPickupSound();
                
                // 如果配置为拾取后销毁，则销毁物品
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // 否则将物品禁用一段时间后重新启用
                    gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 将物品添加到玩家背包
        /// </summary>
        private bool AddItemToPlayerInventory()
        {
            // 获取玩家的背包管理器
            // 这里需要根据实际的背包系统进行适配
            // 例如：InventoryManager.Instance.AddItem(itemId, count, itemLevel);
            
            // 简单模拟添加成功
            // 在实际项目中应该根据背包是否已满等条件来判断添加成功与否
            Debug.Log($"[DropItem] 拾取物品: {itemId}, 数量: {count}, 等级: {itemLevel}");
            
            // 发送拾取事件
            // 这里可以通过事件系统通知其他系统玩家拾取了物品
            // 例如：EventManager.TriggerEvent("ItemPickup", new ItemPickupEventArgs(itemId, count, itemLevel));
            
            return true; // 假设添加总是成功的
        }
        
        /// <summary>
        /// 播放拾取效果
        /// </summary>
        private void PlayPickupEffect()
        {
            if (pickupEffect != null)
            {
                GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                
                // 销毁效果，防止内存泄漏
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    float duration = ps.main.duration + ps.main.startLifetime.constant;
                    Destroy(effect, duration);
                }
                else
                {
                    Destroy(effect, 2f); // 默认2秒后销毁
                }
            }
        }
        
        /// <summary>
        /// 播放拾取音效
        /// </summary>
        private void PlayPickupSound()
        {
            if (audioSource != null && pickupSound != null)
            {
                // 由于物品可能被销毁，所以我们在世界空间播放音效
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
            }
        }
        
        /// <summary>
        /// 启用或禁用高亮效果
        /// </summary>
        private void EnableHighlight(bool enable)
        {
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(enable);
            }
            else if (enable && itemRenderer != null)
            {
                // 如果没有专门的高亮效果，简单地增加物品亮度
                itemRenderer.material.SetFloat("_EmissionIntensity", 1.5f);
            }
        }
        
        /// <summary>
        /// 启用拾取
        /// </summary>
        private IEnumerator EnablePickup()
        {
            // 等待一小段时间才能拾取，防止物品刚掉落就被拾取
            yield return new WaitForSeconds(0.5f);
            isPickable = true;
        }
        
        /// <summary>
        /// 淡出并销毁
        /// </summary>
        private IEnumerator FadeOutAndDestroy()
        {
            if (itemRenderer == null)
            {
                Destroy(gameObject);
                yield break;
            }
            
            // 淡出效果
            float duration = 2f;
            float elapsed = 0f;
            Color originalColor = itemRenderer.material.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                
                // 设置透明度
                if (itemRenderer != null)
                {
                    Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    itemRenderer.material.color = newColor;
                }
                
                // 闪烁效果
                if (elapsed > duration * 0.5f)
                {
                    itemRenderer.enabled = !itemRenderer.enabled;
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    yield return null;
                }
            }
            
            // 销毁物品
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 获取物品掉落时长
        /// </summary>
        public float GetDropAge()
        {
            return Time.time - dropTime;
        }
        
        /// <summary>
        /// 手动拾取物品（可由外部调用）
        /// </summary>
        public void PickupItem(Transform player)
        {
            if (isPickable)
            {
                TryPickup(player);
            }
        }
    }
} 