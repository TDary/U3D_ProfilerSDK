# U3D_ProfilerSDK

Unity Profiler 数据解析 SDK - 用于解析和分析 Unity Profiler 的性能数据

## 项目简介

本项目是一个 Unity 编辑器扩展工具，主要用于解析 Unity Profiler 生成的性能数据文件（.raw 格式），并提供多种分析模式和数据导出功能。

### 主要功能

- **性能数据分析**：解析 Unity Profiler 捕获的性能数据
- **多种分析模式**：
  - `simple` - 简单模式，导出基础性能指标 CSV 数据
  - `funprofiler` - 函数分析模式，导出函数堆栈和调用数据
  - `deep` - 深度分析模式（待实现）
- **内存分析**：支持 Unity Memory Profiler 快照数据分析
- **数据导出**：支持导出为 CSV、Protocol Buffer、JSON 等格式
- **Speedscope 支持**：可导出 Speedscope 格式用于可视化分析

## 项目结构

```
U3D_ProfilerSDK/
├── Assets/
│   ├── Scripts/
│   │   ├── ProfilerParse/          # 核心解析模块
│   │   │   ├── Entrance.cs         # 命令行入口
│   │   │   ├── AnalyzeSDK.cs       # 分析 SDK 主类
│   │   │   ├── ParseFuntion.cs     # 函数解析逻辑
│   │   │   ├── ParseFun.cs         # 函数数据处理
│   │   │   ├── ParsingToCsv.cs     # CSV 数据解析
│   │   │   └── SnapSDK.cs          # 快照解析 SDK
│   │   ├── OProfilerData/          # 数据结构定义
│   │   │   └── ProData.cs          # Profiler 数据类
│   │   ├── DataAO/                 # 数据输出对象
│   │   ├── MemoryProfiler/         # 内存分析模块
│   │   ├── Analyzer/               # 分析器模块
│   │   └── Tools/                  # 工具类
│   └── Editor/                     # 编辑器扩展
│       ├── ProfilerTest.cs         # 测试工具窗口
│       └── ProfilerMemoryTest.cs   # 内存测试
├── MemoryCaptures/                 # 内存快照存储目录
├── Packages/                       # 自定义包
│   └── com.seasun.remote-debug-plus/
└── Library/                        # Unity 库文件
```

## 核心模块说明

### 1. ProfilerParse (性能分析解析模块)

**Entrance.cs** - 命令行入口
- 提供批处理模式的解析入口
- 支持命令行参数配置

**AnalyzeSDK.cs** - 分析 SDK 主类
- `start()` 方法接收配置参数执行分析
- 支持 `simple` 和 `funprofiler` 两种分析模式
- 解析完成后自动上报结果到服务器

**ParseFuntion.cs** - 函数解析
- `ProfilerTrees` 类：构建函数调用树
- `GetChildFunFast()`：提取函数堆栈数据
- 支持渲染线程数据分析

### 2. OProfilerData (数据结构)

定义核心数据结构：
- `BaseData` - 基础帧数据（帧率、CPU/GPU 时间）
- `FunctionData` - 函数性能数据
- `TimeLineData` - 线程时间线数据
- `OAreaData` - 性能区域数据

### 3. MemoryProfiler (内存分析模块)

基于 Unity Memory Profiler 的扩展：
- `HighLevelAPI.cs` - 高级内存快照 API
- `Crawler.cs` - 内存爬取器
- `SnapshotUtil.cs` - 快照工具

### 4. DataAO (数据输出对象)

- `Simple` - 简单性能数据
- `CaseFunRow` - 函数行数据
- `CaseFlame` - 火焰图数据
- `Speedscope` - Speedscope 格式数据

## 使用方法

### 编辑器内使用

1. 打开 Unity 编辑器
2. 通过菜单 `profiler/测试` 打开测试窗口
3. 使用内置的测试按钮进行各种分析

### 命令行批处理

```bash
# 通过命令行参数运行解析
-rawPath <path>        # 原始数据文件路径
-csvPath <path>        # CSV 输出路径
-funPath <path>        # 函数数据输出路径
-funrowPath <path>     # 函数行数据输出路径
-funnamePath <path>    # 函数名称输出路径
-analyzeType <type>    # 分析类型 (simple/funprofiler)
-UUID <uuid>           # 任务唯一标识
-ServerUrl <url>       # 结果上报服务器地址
-shieldSwitch <bool>   # 是否屏蔽特定函数
```

### 代码示例

```csharp
// 初始化 Profiler 数据
ProfilerDriver.ClearAllFrames();
Profiler.AddFramesFromFile(rawFilePath);

// 获取帧数据
int firstIndex = ProfilerDriver.firstFrameIndex;
int lastIndex = ProfilerDriver.lastFrameIndex;

// 提取单帧数据
ProfilerProperty profilerProperty = new ProfilerProperty();
ProfilerTool.GetProfilerProperty(frameIndex, threadIndex, ref profilerProperty);

// 遍历函数堆栈
while (profilerProperty.Next(true))
{
    string functionName = profilerProperty.GetColumn(HierarchyFrameDataView.columnName);
    string totalTime = profilerProperty.GetColumn(HierarchyFrameDataView.columnTotalTime);
    // ... 处理数据
}
```

## 支持的数据类型

### CPU Profiler 数据
- 函数调用次数 (Calls)
- CPU 总时间/自身时间 (Total/Self Time)
- CPU 占用百分比 (Total/Self Percent)
- GC 内存分配 (GC Memory)

### GPU Profiler 数据
- GPU 总时间 (Total GPU Time)
- GPU 占用百分比 (GPU Percent)
- Draw Calls 数量

### Memory Profiler 数据
- 托管堆内存 (Managed Heap)
- GC Handles
- 原生 Unity 对象
- 静态字段
- 内存引用关系

### 性能区域数据
- CPU
- Rendering
- Memory
- Physics
- Audio
- Network
- UI
- GPU

## 依赖项

### Unity 版本
- **Unity 2022.3.2f1** (推荐)
- 最低支持 Unity 2018.3+

### Unity Packages
- com.unity.memoryprofiler@1.1.3
- com.unity.nuget.newtonsoft-json@3.2.1
- com.seasun.remote-debug-plus (自定义包)

### 第三方库
- ProtoBuf (Protocol Buffer 序列化)
- Newtonsoft.Json (JSON 处理)

## 输出格式

### CSV 格式
适用于 Excel 等工具分析的基础性能数据

### Protocol Buffer 格式
高效的二进制序列化格式，用于：
- 函数堆栈数据
- 火焰图数据
- 函数行统计数据

### Speedscope JSON
可用于 [speedscope.app](https://www.speedscope.app/) 可视化的火焰图数据

## 注意事项

1. **内存占用**：解析大量帧数据时可能占用较多内存
2. **线程安全**：Profiler API 需要在主线程调用
3. **数据完整性**：首帧和尾帧数据可能被跳过以确保准确性
4. **Unity 版本兼容**：不同 Unity 版本的 Profiler API 有差异

## 开发历史

最近提交：
- 添加生成 CSV 数据格式功能
- 解决重新解析后旧数据追加问题
- 修复总内存数据缺失问题
- 优化代码，降低内存占用
- 修复 StreamWriter UTF8 BOM 问题

## 许可证

本项目为内部开发工具，请勿随意分发。

## 联系方式

如有问题，请联系开发团队。
