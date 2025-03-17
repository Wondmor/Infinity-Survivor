using UnityEngine;
using System.Collections;

namespace TrianCatStudio
{
    /// <summary>
    /// 玩家射击状态
    /// </summary>
    public class FireState : PlayerBaseState
    {
        private float fireTimer = 0f;
        private float fireAnimationDuration = 0.2f; // 开火动画持续时间
        private float fireRate = 0.2f; // 射击频率，从 Player 获取
        private bool hasSpawnedBullet = false; // 是否已经生成子弹
        private bool isContinuousFiring = false; // 是否处于持续开火状态
        
        public FireState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.UpperBody; // 设置为上半身层
        }

        public override void OnEnter()
        {
            Debug.Log("FireState.OnEnter: 进入开火状态");
            
            // 重置状态
            fireTimer = 0f;
            hasSpawnedBullet = false;
            isContinuousFiring = true; // 默认进入持续开火状态
            
            // 从 Player 获取射击频率
            fireRate = manager.Player.FireRate;
            
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetFiringState(true);
                manager.Player.AnimController.TriggerFire();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorBool("IsFiring", true);
                SetAnimatorTrigger("Fire");
            }
            
            // 生成子弹
            SpawnBullet();
        }

        public override void Update(float deltaTime)
        {
            // 更新开火计时器
            fireTimer += deltaTime;
            
            // 如果处于持续开火状态，且按住开火键，则持续生成子弹
            if (isContinuousFiring && manager.Player.InputManager.IsFirePressed)
            {
                // 检查是否达到射击频率
                if (fireTimer >= fireRate)
                {
                    // 重置计时器
                    fireTimer = 0f;
                    hasSpawnedBullet = false;
                    
                    // 生成子弹
                    SpawnBullet();
                    
                    // 触发开火动画
                    if (manager.Player.AnimController != null)
                    {
                        manager.Player.AnimController.TriggerFire();
                    }
                    else
                    {
                        SetAnimatorTrigger("Fire");
                    }
                }
            }
            else if (!isContinuousFiring)
            {
                // 如果不是持续开火状态，且开火动画播放完毕，退出状态
                if (fireTimer >= fireAnimationDuration)
                {
                    // 通知状态机退出开火状态
                    manager.ExitFireState();
                }
            }
        }
        
        public override void OnExit()
        {
            Debug.Log("FireState.OnExit: 退出开火状态");
            
            // 重置开火状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetFiringState(false);
            }
            else
            {
                SetAnimatorBool("IsFiring", false);
            }
            
            // 重置状态
            isContinuousFiring = false;
        }
        
        // 停止持续开火
        public void StopFiring()
        {
            Debug.Log("FireState.StopFiring: 停止持续开火");
            isContinuousFiring = false;
        }
        
        private void SpawnBullet()
        {
            // 避免重复生成子弹
            if (hasSpawnedBullet)
                return;
                
            hasSpawnedBullet = true;
            
            // 检查是否有子弹预制体
            if (manager.Player.BulletPrefab == null)
            {
                Debug.LogWarning("FireState.SpawnBullet: 子弹预制体为空");
                return;
            }
            
            // 获取射击方向
            Vector3 fireDirection;
            Transform firePoint = manager.Player.FirePoint;
            
            // 如果没有指定发射点，使用玩家位置
            if (firePoint == null)
            {
                firePoint = manager.Player.transform;
            }
            
            // 始终使用相机方向作为射击方向
            if (manager.Player.CameraPivot != null)
            {
                fireDirection = manager.Player.CameraPivot.forward;
            }
            else
            {
                // 如果没有相机，使用玩家前方作为备用
                fireDirection = manager.Player.transform.forward;
                Debug.LogWarning("FireState.SpawnBullet: 相机为空，使用玩家前方作为射击方向");
            }
            
            // 生成子弹
            GameObject bullet = Object.Instantiate(
                manager.Player.BulletPrefab, 
                firePoint.position, 
                Quaternion.LookRotation(fireDirection)
            );
            
            // 设置子弹速度和发射者
            PlayerBullet playerBullet = bullet.GetComponent<PlayerBullet>();
            if (playerBullet != null)
            {
                // 使用带发射者参数的初始化方法
                playerBullet.Initialize(fireDirection, manager.Player.BulletSpeed, manager.Player.gameObject);
                // 设置伤害
                playerBullet.SetDamage(manager.Player.BulletDamage, DamageType.Physical);
                Debug.Log($"FireState.SpawnBullet: 生成玩家子弹，方向={fireDirection}, 速度={manager.Player.BulletSpeed}, 发射者={manager.Player.gameObject.name}");
                
                // 可以在这里设置玩家子弹的特殊属性，例如穿透
                // playerBullet.SetPierceProperties(true, 2, 0.2f);
            }
            else
            {
                // 尝试获取基类BulletBase
                BulletBase bulletBase = bullet.GetComponent<BulletBase>();
                if (bulletBase != null)
                {
                    bulletBase.Initialize(fireDirection, manager.Player.BulletSpeed, manager.Player.gameObject);
                    // 设置伤害
                    bulletBase.SetDamage(manager.Player.BulletDamage, DamageType.Physical);
                    Debug.Log($"FireState.SpawnBullet: 生成基础子弹，方向={fireDirection}, 速度={manager.Player.BulletSpeed}, 发射者={manager.Player.gameObject.name}");
                }
                else
                {
                    // 移除对旧Bullet类的引用，直接使用Rigidbody
                    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                    if (bulletRb != null)
                    {
                        bulletRb.velocity = fireDirection * manager.Player.BulletSpeed;
                        Debug.Log($"FireState.SpawnBullet: 生成子弹（使用Rigidbody），方向={fireDirection}, 速度={manager.Player.BulletSpeed}");
                    }
                    else
                    {
                        Debug.LogWarning("FireState.SpawnBullet: 子弹没有任何可用的组件");
                    }
                }
            }
            
            // 销毁子弹（如果没有其他脚本控制）
            if (manager.Player.BulletLifetime > 0)
            {
                Object.Destroy(bullet, manager.Player.BulletLifetime);
            }
        }
    }
} 