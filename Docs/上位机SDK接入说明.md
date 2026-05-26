# 上位机 SDK 接入说明

本文档用于说明上位机如何接入 `EdgeAlignInspect` 检测 SDK，包括配置流程、运行接口、结果序列化方式，以及上位机需要重点读取的结果字段。

## 1. 引用文件

上位机工程需要引用以下文件：

- `EdgeAlignInspect.exe` 或 `EdgeAlignInspect.dll`
- `halcondotnet.dll`

如果按 DLL 方式集成，建议把 `halcondotnet.dll` 和主程序放在同一输出目录，避免运行时找不到 HALCON 依赖。

项目目标框架为 `.NET Framework 4.7.2`，上位机建议使用同版本或更高兼容版本。

## 2. SDK 入口

SDK 入口类为：

```csharp
EdgeAlignInspect.TemplateEdgeInspectSdk
```

主要提供两个方法：

```csharp
EdgeInspectJob OpenSetupDialog(Bitmap image, EdgeInspectJob currentJob)
```

用于打开参数配置窗口。配置完成并确认后，会返回新的 `EdgeInspectJob`。上位机需要保存这个 Job，后续检测时直接传入。

```csharp
EdgeInspectResult RunInspection(
    Bitmap image,
    EdgeInspectJob job,
    double acceptedTolerance,
    double pixelResolutionX,
    double pixelResolutionY)
```

用于执行一次检测。

参数说明：

- `image`：当前待检测图像。
- `job`：已经配置并示教完成的检测任务。
- `acceptedTolerance`：上位机传入的允许公差，单位为物理单位。
- `pixelResolutionX`：X 方向单像素代表的物理尺寸。
- `pixelResolutionY`：Y 方向单像素代表的物理尺寸。

## 3. 标准调用方式

上位机第一次配置时调用：

```csharp
using EdgeAlignInspect;
using System.Drawing;

TemplateEdgeInspectSdk sdk = new TemplateEdgeInspectSdk();

using (Bitmap image = new Bitmap("sample.bmp"))
{
    EdgeInspectJob job = sdk.OpenSetupDialog(image, null);

    if (job != null)
    {
        // 上位机需要把 job 保存起来，后续检测复用
    }
}
```

正常运行检测时调用：

```csharp
using EdgeAlignInspect;
using System.Drawing;

TemplateEdgeInspectSdk sdk = new TemplateEdgeInspectSdk();

using (Bitmap image = new Bitmap("run.bmp"))
{
    EdgeInspectResult result = sdk.RunInspection(
        image,
        job,
        acceptedTolerance: 0.05,
        pixelResolutionX: 0.01,
        pixelResolutionY: 0.01);

    if (result.Success)
    {
        // OK
    }
    else
    {
        // NG，直接读取 result.NgReasons 或 result.NgReasonText
    }
}
```

## 4. 序列化说明

`EdgeInspectJob`、`EdgeInspectResult`、`DetectRoiInspectResult`、`EdgePointResult` 等结果模型都带有 `[Serializable]`，可以用于 .NET 二进制序列化或其他对象持久化方式。

推荐上位机保存配置时序列化 `EdgeInspectJob`，不要每次重新配置 ROI。

示例：

```csharp
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static void SaveJob(string path, EdgeInspectJob job)
{
    using (FileStream fs = File.Create(path))
    {
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fs, job);
    }
}

public static EdgeInspectJob LoadJob(string path)
{
    using (FileStream fs = File.OpenRead(path))
    {
        BinaryFormatter formatter = new BinaryFormatter();
        return (EdgeInspectJob)formatter.Deserialize(fs);
    }
}
```

说明：

- `EdgeInspectJob` 里包含 ROI、卡尺参数、模板示教数据等运行所需配置。
- `EdgeInspectResult` 可以序列化保存检测记录。
- 如果上位机要转 JSON，建议只转关键结果字段，不建议直接把完整对象全部转 JSON，因为完整结果中包含点集、ROI、拟合线等较多结构数据。

## 5. 总结果字段

上位机最常用的是 `EdgeInspectResult`。

建议优先读取以下字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `Success` | `bool` | 本次检测是否 OK。`false` 表示 NG 或运行失败。 |
| `Message` | `string` | 本次检测的总说明。NG 时会包含失败原因和统计信息。 |
| `NgReasons` | `NgReason` | 本次 NG 原因，可同时包含多种原因。 |
| `NgReasonText` | `string` | NG 原因中文文本，例如 `毛刺+超边`。 |
| `BurrCount` | `int` | 毛刺点数量。 |
| `DentCount` | `int` | 凹陷点数量。 |
| `OverEdgeCount` | `int` | 整体超边数量。 |
| `CopperLeakCount` | `int` | 整体漏铜数量。 |
| `DetectResults` | `List<DetectRoiInspectResult>` | 每个检测 ROI 的详细结果。 |
| `FailedItems` | `List<string>` | 失败项文字列表，适合直接显示。 |
| `TemplateMatchOk` | `bool` | 模板匹配是否成功。 |
| `TemplateMatchScore` | `double` | 模板匹配分数。 |
| `PixelResolutionX` | `double` | 本次检测使用的 X 向像素解析度。 |
| `PixelResolutionY` | `double` | 本次检测使用的 Y 向像素解析度。 |

判断 NG 类型建议使用：

```csharp
bool hasBurr = (result.NgReasons & NgReason.Burr) == NgReason.Burr;
bool hasDent = (result.NgReasons & NgReason.Dent) == NgReason.Dent;
bool hasOverEdge = (result.NgReasons & NgReason.OverEdge) == NgReason.OverEdge;
bool hasCopperLeak = (result.NgReasons & NgReason.CopperLeak) == NgReason.CopperLeak;
```

## 6. NG 原因枚举

`NgReason` 是可组合枚举，可能同时出现多个原因。

| 枚举值 | 数值 | 说明 |
| --- | ---: | --- |
| `None` | 0 | 无 NG 原因。 |
| `Burr` | 1 | 毛刺。 |
| `Dent` | 2 | 凹陷。 |
| `OverEdge` | 4 | 超边，实际距离大于标准距离并超过公差。 |
| `CopperLeak` | 8 | 漏铜，实际距离小于标准距离并超过公差。 |
| `TemplateMatchFailed` | 16 | 模板匹配失败。 |
| `BaseRoiFailed` | 32 | 基准 ROI 拟合或关联失败。 |
| `DetectRoiFailed` | 64 | 检测 ROI 找边或拟合失败。 |
| `ParameterInvalid` | 128 | 输入参数无效。 |
| `AlgorithmException` | 256 | 算法运行异常。 |

## 7. 单个 ROI 结果字段

每个检测 ROI 的结果在：

```csharp
result.DetectResults
```

建议上位机重点读取：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `Index` | `int` | ROI 序号。 |
| `Name` | `string` | ROI 名称。 |
| `Success` | `bool` | 当前 ROI 是否 OK。 |
| `Message` | `string` | 当前 ROI 的判定说明。 |
| `NgReasons` | `NgReason` | 当前 ROI 的 NG 原因。 |
| `NgReasonText` | `string` | 当前 ROI 的 NG 原因中文文本。 |
| `BurrCount` | `int` | 当前 ROI 毛刺点数量。 |
| `DentCount` | `int` | 当前 ROI 凹陷点数量。 |
| `IsOverEdge` | `bool` | 当前 ROI 是否整体超边。 |
| `IsCopperLeak` | `bool` | 当前 ROI 是否整体漏铜。 |
| `HasOverallDistance` | `bool` | 当前 ROI 是否执行了整体距离判定。 |
| `OverallDistanceValue` | `double` | 整体实际距离，物理单位。 |
| `OverallDeltaValue` | `double` | 整体距离相对标准距离的偏差，物理单位。 |
| `Points` | `List<EdgePointResult>` | 当前 ROI 内每个检测点的详细结果。 |

示例：

```csharp
foreach (DetectRoiInspectResult roi in result.DetectResults)
{
    if (!roi.Success)
    {
        string roiName = roi.Name;
        string reason = roi.NgReasonText;
        int burrCount = roi.BurrCount;
        int dentCount = roi.DentCount;
        bool overEdge = roi.IsOverEdge;
        bool copperLeak = roi.IsCopperLeak;
    }
}
```

## 8. 单点结果字段

如果上位机需要追溯到具体检测点，可以读取：

```csharp
roi.Points
```

常用字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `Index` | `int` | 点序号。 |
| `Point` | `PointF` | 点坐标。 |
| `IsBurr` | `bool` | 该点是否毛刺。 |
| `IsDent` | `bool` | 该点是否凹陷。 |
| `NgReasons` | `NgReason` | 该点 NG 原因。 |
| `NgReasonText` | `string` | 该点 NG 原因中文文本。 |
| `DeltaPx` | `double` | 点偏差，像素单位。 |
| `DeltaValue` | `double` | 点偏差，物理单位。 |
| `SignedDistancePx` | `double` | 带方向偏差，像素单位。 |
| `SignedDistanceValue` | `double` | 带方向偏差，物理单位。 |

示例：

```csharp
foreach (EdgePointResult point in roi.Points)
{
    if (point.IsBurr || point.IsDent)
    {
        PointF imagePoint = point.Point;
        string reason = point.NgReasonText;
        double delta = point.DeltaValue;
    }
}
```

## 9. 推荐上位机判定逻辑

上位机建议按以下顺序处理结果：

1. 先判断 `result.Success`。
2. 如果 `Success == false`，读取 `result.NgReasonText` 作为总 NG 原因。
3. 如果需要定位具体 ROI，遍历 `result.DetectResults`，读取每个 ROI 的 `NgReasonText`。
4. 如果需要定位具体点，遍历 `roi.Points`，读取 `IsBurr`、`IsDent`、`NgReasonText` 和 `DeltaValue`。

最简处理方式：

```csharp
if (!result.Success)
{
    Alarm(result.NgReasonText);
}
```

详细处理方式：

```csharp
if (!result.Success)
{
    foreach (DetectRoiInspectResult roi in result.DetectResults)
    {
        if (!roi.Success)
        {
            Log($"{roi.Name}: {roi.NgReasonText}, {roi.Message}");
        }
    }
}
```

## 10. 注意事项

- `RunInspection` 内部会复制输入图像，不会直接修改上位机传入的 `Bitmap`。
- `RunInspection` 参数错误时不会向外抛异常，会返回 `Success=false`，并设置 `NgReasons=ParameterInvalid`。
- 算法异常时会返回 `Success=false`，并设置 `NgReasons=AlgorithmException`。
- 毛刺和凹陷属于局部点判定。
- 超边和漏铜属于整体距离判定。
- 一个结果可能同时包含多种 NG 原因，例如 `毛刺+超边`。
- 如果只做简单报警，直接使用 `NgReasonText` 即可。
