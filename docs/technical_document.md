# U3D_ProfilerSDK 技术文档

## 一、背景

### 1.1 项目背景

在 Unity 游戏开发过程中，性能优化是保障用户体验的关键环节。Unity 引擎内置了 Profiler 工具，能够捕获运行时的 CPU、GPU、内存、渲染、物理等多维度性能数据，并将其保存为 `.raw` 二进制文件。然而，Unity Profiler 的原生分析界面存在以下局限：

1. **交互式限制**：仅能在 Unity Editor 中手动查看，无法批量自动化处理大量性能采集文件。
2. **数据导出不便**：原始数据以二进制存储，难以直接对接外部数据分析平台或自动化流水线。
3. **缺乏聚合统计**：单帧级别的数据需要人工逐帧比对，缺少跨帧的函数级聚合统计能力。
4. **火焰图集成缺失**：无法直接导出为 Speedscope 等通用火焰图可视化格式。

为解决上述问题，U3D_ProfilerSDK 应运而生。它是一个 Unity Editor 扩展 SDK，核心目标是将 Unity Profiler 的二进制性能数据解析为结构化数据（Protocol Buffer、CSV、JSON），支持命令行批处理模式，可集成到 CI/CD 流水线中实现自动化性能分析与回归检测。

### 1.2 应用场景

| 场景 | 说明 |
|------|------|
| **CI/CD 性能回归检测** | 在构建流水线中自动解析 `.raw` 文件，提取关键指标，与基线对比检测性能退化 |
| **批量性能数据采集** | 对大量性能采集文件进行批量解析，生成 CSV 报表供数据分析团队使用 |
| **函数级热点分析** | 通过 `funprofiler` 模式构建函数调用树，定位 CPU 热点函数 |
| **内存快照分析** | 解析 `.snap` 内存快照文件，分析内存泄漏和对象引用关系 |
| **火焰图可视化** | 导出 Speedscope JSON 格式，在 speedscope.app 中进行交互式火焰图分析 |

### 1.3 技术栈

- **Unity 版本**：推荐 Unity 2022.3.2f1，最低支持 Unity 2018.3+
- **核心依赖**：Unity Profiler API、Unity Memory Profiler（com.unity.memoryprofiler@1.1.3）
- **序列化**：Protocol Buffer（protobuf-net）、Newtonsoft.Json
- **输出格式**：Protocol Buffer 二进制、CSV（UTF-8 无 BOM）、JSON、Speedscope JSON

---

## 二、系统架构

### 2.1 整体架构

```
┌─────────────────────────────────────────────────────────────────┐
│                        入口层 (Entry Layer)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────────┐ │
│  │  Editor 窗口  │  │  Entrance.cs │  │  测试工具 (ProfilerTest)│ │
│  │  (交互模式)   │  │  (命令行模式) │  │                        │ │
│  └──────┬───────┘  └──────┬───────┘  └───────────┬────────────┘ │
├─────────┼──────────────────┼──────────────────────┼──────────────┤
│         │          调度层 (Orchestration Layer)    │              │
│  ┌──────▼──────────────────▼──────────────────────▼────────────┐ │
│  │              AnalyzeSDK (Profiler 数据分析)                  │ │
│  │              SnapSDK    (内存快照分析)                       │ │
│  └──────┬──────────────────┬───────────────────────────────────┘ │
├─────────┼──────────────────┼─────────────────────────────────────┤
│         │          解析层 (Parsing Layer)                         │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌────────────────────────┐ │
│  │ ParsingToCsv │  │ ParseFuntion │  │   MemoryProfiler       │ │
│  │ (Simple 模式)│  │ (Funprofiler │  │   (内存快照解析)        │ │
│  │              │  │  模式)       │  │                        │ │
│  └──────┬───────┘  └──────┬───────┘  └──────────┬─────────────┘ │
├─────────┼──────────────────┼─────────────────────┼───────────────┤
│         │          基础设施层 (Infrastructure)                    │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────────▼─────────────┐ │
│  │ ProfilerTool │  │  CsvFileHelper│  │    SHttpSender         │ │
│  │ (API 桥接)   │  │  (CSV 读写)   │  │    (HTTP 上报)    │ │
│  └──────────────┘  └──────────────┘  └────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 核心模块职责

| 模块 | 文件 | 职责 |
|------|------|------|
| **Entrance** | `Entrance.cs` | 命令行入口，解析 CLI 参数，分发到对应 SDK |
| **AnalyzeSDK** | `AnalyzeSDK.cs` | Profiler 数据分析主控，驱动整个解析流水线 |
| **SnapSDK** | `SnapSDK.cs` | 内存快照分析主控，加载 `.snap` 文件并构建分析数据 |
| **ParsingToCsv** | `ParsingToCsv.cs` | Simple 模式：逐帧采集 70+ 项性能指标 |
| **ParseFuntion** | `ParseFuntion.cs` | Funprofiler 模式：构建函数调用树 |
| **ParseFun** | `ParseFun.cs` | 函数级聚合统计 |
| **ProfilerTool** | `ProfilerTool.cs` | Unity Profiler API 的统一封装层 |
| **SHttpSender** | `SHttpSender.cs` | HTTP 客户端，向服务端上报解析状态 |

---

## 三、实现原理

### 3.1 数据输入

SDK 处理两类输入文件：

**`.raw` 文件（Profiler 数据）**：Unity Profiler 在运行时捕获的性能数据二进制文件，包含逐帧的 CPU、GPU、内存、渲染、物理等维度的详细数据。通过 `Profiler.AddFramesFromFile()` 加载到 Unity Profiler 系统中。

**`.snap` 文件（内存快照）**：Unity Memory Profiler 捕获的内存快照文件，包含完整的堆内存布局和对象引用关系。通过 `MemoryProfilerNoWindow.LoadedSnapshot()` 异步加载。

### 3.2 三种分析模式

#### 3.2.1 Simple 模式 — 帧级指标采集

**目标**：采集每帧的宏观性能指标，输出列式存储的结构化数据。

**实现流程**：

```
.raw 文件
  │
  ▼
Profiler.AddFramesFromFile()
  │
  ▼
遍历帧范围 [firstFrameIndex .. lastFrameIndex]
  │
  ├── 每帧调用 ProfilerTool.ExtractAreaData()
  │     │
  │     ├── Unity 2020+ 路径：RawFrameDataView API
  │     │     GetMarkerId() → GetCounterValueAsLong()
  │     │     精确提取 ~70 项指标
  │     │
  │     └── 旧版本路径：ProfilerDriver.GetOverviewText()
  │           字符串解析提取指标（兼容性方案）
  │
  ▼
Dictionary<string, Simple>  （列式存储：指标名 → 帧值列表）
  │
  ├── CPU 指标：Rendering, Scripts, Physics, GC, UI, etc.
  ├── 渲染指标：Draw Calls, Batches, Triangles, Vertices, SetPass, etc.
  ├── 内存指标：System Used, Managed Heap, GC Alloc, Textures, Mesh, etc.
  └── 物理指标：Bodies, Colliders, Contacts, Constraints, etc.
  │
  ▼
ProtoBuf 序列化 → 二进制文件
CSV 导出 → CSV 文件
```

**存储设计**：采用列式存储（Column-oriented）而非行式存储。每个 `Simple` 对象包含一个字段名和一个 `List<float>`，存储该指标在所有帧中的值。这种设计：

- 与 Protocol Buffer 的流式编码天然契合，压缩率高
- 便于下游进行统计分析（均值、最大值、百分位等）
- 支持按指标名快速检索

#### 3.2.2 Funprofiler 模式 — 函数调用树分析

**目标**：构建每帧的函数调用树，并进行跨帧聚合统计。

**核心算法 — 基于栈的树构建**：

```
输入：ProfilerProperty 迭代器（按深度优先顺序遍历函数节点）

算法伪代码：
  stack = []
  root = new SubData("root")

  while profilerProperty.Next(true):
      currentPath = profilerProperty.GetColumn(columnName)
      currentDepth = profilerProperty.depth

      // 弹出栈中深度 >= 当前深度的节点（它们不是当前节点的父节点）
      while stack.size > currentDepth:
          stack.pop()

      // 当前节点的父节点是栈顶元素
      parent = stack.isEmpty ? root : stack.top()

      // 创建当前节点并挂载到父节点
      node = new SubData(currentPath)
      node.total = totalPercent * 100      // 整数存储，避免浮点精度问题
      node.self = selfPercent * 100
      node.calls = calls
      node.gcalloc = gcAlloc
      parent.children.add(node)

      stack.push(node)
```

**数据结构 — SubData（递归树节点）**：

```csharp
[ProtoContract]
class SubData {
    [ProtoMember(1)] string name;           // 函数名
    [ProtoMember(2)] int total;             // 总耗时占比 (×100)
    [ProtoMember(3)] int self;              // 自身耗时占比 (×100)
    [ProtoMember(4)] int calls;             // 调用次数
    [ProtoMember(5)] int gcalloc;           // GC 分配字节
    [ProtoMember(6)] int timems;            // 总耗时毫秒 (×100)
    [ProtoMember(7)] int selfms;            // 自身耗时毫秒 (×100)
    [ProtoMember(8)] List<SubData> children; // 子函数列表
}
```

**跨帧聚合统计**：

在每帧的 `SubData` 树构建完成后，`ParseFun.GetParsedFunRowjson()` 递归遍历所有帧的函数树，将同一函数路径的数据聚合到 `CaseFunRow` 中：

```
遍历每一帧的 SubData 树
  │
  ├── 对每个节点：以函数路径 (path) 为 key
  │     ├── 新路径 → 创建 CaseFunRow，添加一条 CaseFunRowInfo
  │     └── 已有路径 → 追加 CaseFunRowInfo 到列表
  │
  ▼
Dictionary<string, CaseFunRow>
  │
  ├── key: 函数完整路径 (如 "Update()/PlayerLoop()/LateUpdate()")
  └── value: CaseFunRow
        ├── funPath: 函数路径
        ├── unityObjectName: 关联的 Unity 对象名
        └── rowInfos: List<CaseFunRowInfo>  （每帧一条记录）
              ├── total, self, calls, gc, totalms, selfms
              └── frameIndex
```

#### 3.2.3 Memory Snapshot 模式 — 内存快照分析

**目标**：解析 `.snap` 内存快照文件，输出内存概要、对象列表和全量内存数据。

**实现流程**：

```
.snap 文件
  │
  ▼
MemoryProfilerNoWindow.Init() → LoadedSnapshot(path)
  │
  ▼
异步等待加载完成（Thread.Sleep(1) 轮询 IsLoadSuccess()）
  │
  ▼
BuildAllData()
  ├── summaryJsonPath      → 内存概要 JSON（总内存、各区域占用）
  ├── unityObjectsJsonPath → Unity 对象列表 JSON
  └── allMemoryDataJsonPath→ 全量内存数据 JSON
  │
  ▼
文件校验：IsFileInUsed() + IsFileHasData()
  │
  ▼
HTTP 上报解析结果
```

### 3.3 API 桥接层 — ProfilerTool

`ProfilerTool` 是 SDK 与 Unity Profiler API 之间的适配层，封装了所有底层 API 调用。其核心设计要点：

**1. 版本兼容**：通过条件编译 `#if UNITY_2020_1_OR_NEWER` 实现多版本兼容：

```
Unity 2020+：
  RawFrameDataView    → 精确的二进制数据访问
  HierarchyFrameDataView → 层次化帧数据

Unity 2018.3 ~ 2019：
  ProfilerDriver.GetOverviewText() → 文本解析（精度较低）
  ProfilerProperty → 函数层次遍历
```

**2. 节流保护**：在获取 `ProfilerProperty` 和 `ProfilerFrameDataIterator` 时插入 `Thread.Sleep(1)`。这是因为在批处理模式下，连续快速调用 Profiler API 会导致 Unity 内部状态不一致而崩溃。1ms 的延迟足以让 Unity 内部完成状态清理。

**3. 线程模型**：所有 Profiler API 调用必须在主线程执行，SDK 通过 Unity 的批处理模式（`-batchmode`）确保这一点。

### 3.4 数据输出

SDK 支持四种输出格式：

| 格式 | 用途 | 序列化方式 |
|------|------|-----------|
| **Protocol Buffer** | 函数调用树、火焰图数据、函数聚合统计 | protobuf-net（`[ProtoContract]`/`[ProtoMember]` 标注） |
| **CSV** | 帧级性能指标，供 Excel/数据分析工具使用 | `CsvFileHelper`，UTF-8 无 BOM 编码 |
| **JSON** | 内存快照分析结果 | Newtonsoft.Json |
| **Speedscope JSON** | 火焰图可视化 | 自定义 JSON 序列化，兼容 speedscope.app 格式 |

### 3.5 结果上报机制

解析完成后，SDK 通过 `SHttpSender` 向服务端发送 HTTP GET 请求上报结果：

```
成功：GET {ServerUrl}/report?id={UUID}&result=1
失败：GET {ServerUrl}/report?id={UUID}&result=0&error={errorMsg}
```

这使得 SDK 可以作为自动化流水线中的一个节点，上游系统通过 HTTP 回调获知解析完成状态。

### 3.6 Unity 版本兼容策略

SDK 通过条件编译实现广泛的版本兼容：

```csharp
#if UNITY_2020_1_OR_NEWER
    // 使用 RawFrameDataView API，支持精确的计数器数据读取
    // 支持 MarkerId / GetCounterValueAsLong
#elif UNITY_2018_3_OR_NEWER
    // 使用 ProfilerDriver.GetOverviewText() 文本解析
    // 精度较低但兼容性好
#endif
```

关键兼容点：
- `RawFrameDataView`（Unity 2020+）：提供逐 Marker 的精确计数器值
- `HierarchyFrameDataView`（Unity 2020+）：提供层次化的帧数据视图
- `ProfilerProperty`（Unity 2018.3+）：函数层次遍历，跨版本可用

### 3.7 关键实现细节

**函数名清洗**：函数名中的 `/` 字符会被替换为 `&`，避免与路径分隔符冲突导致解析错误。当 Profiler 返回空函数名时，SDK 会手动遍历 ProfilerProperty 栈构建路径。

**首尾帧跳过**：解析时会跳过第一帧和最后一帧的数据，因为这两帧通常包含初始化/清理逻辑，不反映稳态性能。

**文件安全检查**：`SnapSDK` 在加载 `.snap` 文件前执行两项检查：
- `IsFileInUsed()`：文件锁检查，防止读取正在写入的文件
- `IsFileHasData()`：文件大小检查，防止读取空文件

**内存优化**：在处理大量帧数据时，SDK 采用流式处理而非一次性加载所有数据到内存，并在每帧处理完成后及时释放临时对象，降低峰值内存占用。

---

## 四、使用指南

### 4.1 命令行参数

```bash
# Simple 模式
Unity.exe -batchmode -projectPath <project> -executeMethod Entrance.EntranceParseBegin \
  -rawPath /path/to/profiler.raw \
  -csvPath /path/to/output.csv \
  -analyzeType simple \
  -UUID task-001 \
  -ServerUrl http://server:port/

# Funprofiler 模式
Unity.exe -batchmode -projectPath <project> -executeMethod Entrance.EntranceParseBegin \
  -rawPath /path/to/profiler.raw \
  -funPath /path/to/functions.bin \
  -funrowPath /path/to/funrows.bin \
  -funnamePath /path/to/funnames.bin \
  -analyzeType funprofiler \
  -UUID task-002 \
  -ServerUrl http://server:port/

# 内存快照模式
Unity.exe -batchmode -projectPath <project> -executeMethod Entrance.EntranceParseSnapBegin \
  -snapPath /path/to/memory.snap \
  -UUID task-003 \
  -ServerUrl http://server:port/
```

### 4.2 Editor 内使用

1. 打开 Unity Editor
2. 菜单栏 → `profiler/测试` 打开测试窗口
3. 配置 `.raw` 文件路径和输出路径
4. 点击对应按钮执行分析

### 4.3 输出文件说明

| 文件 | 内容 | 格式 |
|------|------|------|
| `*.csv` | 70+ 项帧级性能指标（每行一帧，每列一个指标） | CSV |
| `*.bin`（函数树） | 每帧的完整函数调用树 | Protocol Buffer |
| `*.bin`（函数行） | 跨帧聚合的函数统计 | Protocol Buffer |
| `*.bin`（函数名） | 函数路径到 Unity 对象名的映射 | Protocol Buffer |
| `*.json`（Speedscope） | 火焰图数据，可导入 speedscope.app | JSON |
| `*.json`（内存） | 内存概要、对象列表、全量内存数据 | JSON |

---

## 五、设计决策与权衡

### 5.1 列式 vs 行式存储（Simple 模式）

选择列式存储（`Dictionary<string, Simple>`）的原因：
- 性能指标的下游消费场景以"查看某个指标随时间变化的趋势"为主，列式存储天然适配
- Protocol Buffer 对重复浮点数组的编码效率高于稀疏的行式记录
- 便于增量追加：新帧数据只需 append 到各指标的列表末尾

### 5.2 整数存储百分比（×100）

`SubData` 中的 `total`、`self`、`timems`、`selfms` 均以整数存储（实际值 × 100）。原因：
- 避免浮点数在序列化过程中的精度损失
- 整数的 Protocol Buffer 编码比浮点数更紧凑
- 100 倍精度对于百分比和毫秒级耗时足够

### 5.3 Thread.Sleep(1) 节流

在批处理模式下，连续快速调用 Profiler API 会导致 Unity 内部崩溃。1ms 延迟是经验值，足以让 Unity 完成内部状态清理，同时对整体解析性能影响可控（假设 1000 帧 × 70 指标 ≈ 70000 次调用 ≈ 70 秒额外开销）。

### 5.4 HTTP 上报机制

采用简单的 HTTP GET 上报而非 WebSocket 或 gRPC，原因：
- 批处理模式下为无状态的一次性操作，无需长连接
- HTTP GET 最简单，无需额外依赖
- 服务端只需一个简单的 HTTP 接口即可接收

---

## 六、未来展望

1. **Deep 模式**：README 中提及的深度分析模式，目前标记为"待实现"
2. **更多输出格式**：支持 Chrome Trace、Firefox Profiler 等格式
3. **增量解析**：支持对新增帧的增量解析，避免重复处理已完成的帧
4. **并行化**：利用多线程加速大规模帧数据的解析
5. **可视化集成**：在 SDK 内嵌入轻量级火焰图查看器
