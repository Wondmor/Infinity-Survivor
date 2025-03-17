# 高性能对象池系统

这是一个高性能、零GC的对象池系统，用于优化游戏中频繁创建和销毁对象的性能问题。该系统实现了三级缓存架构和智能容量控制，可以有效减少内存碎片和GC Alloc。

## 特性

- **三级缓存架构**：根据对象的生命周期和使用频率，将对象分为三个缓存层级
  - **L1**：场景动态对象（子弹等），随场景卸载销毁
  - **L2**：全局共享对象（UI等），游戏进程生命周期
  - **L3**：大型资源（BOSS模型），按需加载/卸载
- **智能容量控制**：根据使用情况自动扩容和收缩对象池
- **零GC保障**：预生成对象，避免运行时实例化引发的GC Alloc
- **自动回收机制**：支持基于时间和可见性的自动回收
- **碎片整理**：自动整理内存碎片，保持高频对象内存连续性
- **可扩展接口**：支持自定义池化对象行为

## 使用方法

### 基本用法

```csharp
// 获取对象池管理器
ObjectPoolManager poolManager = ObjectPoolManager.Instance;

// 从池中获取对象
GameObject obj = poolManager.Get(prefab, position, rotation);

// 释放对象回池
poolManager.Release(obj);
```

### 创建对象池

```csharp
// 创建对象池配置
ObjectPoolManager.PoolConfig config = new ObjectPoolManager.PoolConfig
{
    PoolId = "BulletPool",
    Prefab = bulletPrefab,
    InitialSize = 20,
    MaxSize = 100,
    ExpansionRate = 0.2f,
    FragmentationThreshold = 0.15f,
    MaxLifetime = 5f,
    Level = ObjectPoolManager.CacheLevel.L1,
    PrewarmOnLoad = true
};

// 创建对象池
ObjectPool bulletPool = ObjectPoolManager.Instance.CreatePool(config);
```

### 自定义池化对象行为

实现 `IPoolable` 接口，自定义对象的池化行为：

```csharp
public class CustomPoolableObject : MonoBehaviour, IPoolable
{
    // 对象从池中获取时调用
    public void OnPoolGet()
    {
        // 初始化对象
    }
    
    // 对象释放回池时调用
    public void OnPoolRelease()
    {
        // 重置对象状态
    }
}
```

### 手动释放对象

```csharp
// 获取池化组件
PooledObject pooledObj = obj.GetComponent<PooledObject>();
if (pooledObj != null)
{
    // 释放对象回池
    pooledObj.Release();
}
```

### 自动回收

```csharp
// 获取池化组件
PooledObject pooledObj = obj.GetComponent<PooledObject>();
if (pooledObj != null)
{
    // 5秒后自动回收
    pooledObj.StartAutoRelease(5f);
}
```

## 性能优化

- **预热对象池**：在游戏启动或场景加载时预热对象池，避免运行时创建对象引发的卡顿
- **合理设置初始大小**：根据预期使用量设置对象池的初始大小，避免频繁扩容
- **使用适当的缓存层级**：根据对象的生命周期选择合适的缓存层级，避免内存泄漏
- **定期清理**：在场景切换或关卡结束时清理不需要的对象池，释放内存

## 示例

查看 `Assets/Scripts/Pool/Examples` 目录下的示例脚本，了解如何使用对象池系统：

- `PoolExample.cs`：展示如何创建和使用对象池
- `PoolableBullet.cs`：展示如何实现 `IPoolable` 接口，自定义池化对象行为

## 注意事项

- 对象池中的对象应该是无状态的，或者能够完全重置状态
- 避免在池化对象中使用 `Destroy` 方法，应该使用 `Release` 方法释放对象回池
- 对于大型对象或复杂对象，应该使用 `L3` 缓存层级，并设置合理的最大大小
- 对象池管理器会在场景切换时自动清理 `L1` 层级的对象池，但不会清理 `L2` 和 `L3` 层级的对象池，需要手动清理 