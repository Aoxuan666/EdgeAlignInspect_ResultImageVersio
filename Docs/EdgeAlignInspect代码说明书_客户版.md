# EdgeAlignInspect 代码说明书

## 一、项目概述

`EdgeAlignInspect` 是一套基于 WinForms 与 HALCON 的边缘对位检测程序，主要用于图像中基准 ROI 和检测 ROI 的配置、示教、运行检测以及检测结果回传。

本项目支持线基准、圆基准、检测 ROI、模板匹配、卡尺找边、缺陷判定和上位机 SDK 调用。客户可通过配置界面完成参数示教，也可以由上位机直接调用 SDK 完成自动检测。

项目按功能职责拆分目录，核心代码分为界面层、图像交互层、几何模型层、检测参数层、算法处理层和 SDK 对接层。

## 二、目录结构说明

### 1. Forms

`Forms` 目录为界面层代码。

`Form1.cs` 是主要参数配置窗口，负责加载图像、添加 ROI、配置参数、运行检测、显示检测结果以及返回示教后的 Job 参数。

`Form2.cs` 是 SDK 调用演示窗口，用于模拟客户上位机调用流程，展示如何打开配置窗口、传入图像和 Job，并接收配置结果。

### 2. Controls

`Controls` 目录为图像显示与 ROI 编辑控件。

`HalconCanvas.cs` 是图像画布控件，负责图像显示、缩放、平移、ROI 绘制、ROI 选中、拖动、旋转和运行结果叠加显示。

其他文件用于描述画布中的 ROI 类型、选中状态、圆基准组合、点和折线等显示对象。

### 3. Geometry

`Geometry` 目录为基础几何结构。

`RotRectF.cs` 表示旋转矩形 ROI，用于线基准、检测 ROI 和模板 ROI。

`CircleRoiF.cs` 表示圆形 ROI，用于圆基准检测。

### 4. Models

`Models` 目录为业务模型和参数模型。

主要包括：

- `EdgeInspectJob`：完整检测任务参数。
- `CaliperParameters`：卡尺找边参数。
- `TemplateMatchParameters`：模板匹配参数。
- `BaseRoiItem`：线基准 ROI。
- `CircleBaseRoiItem`：圆基准 ROI。
- `DetectRoiItem`：检测 ROI。
- `EdgeInspectResult`：检测总结果。
- `BaseRoiInspectResult`：线基准结果。
- `CircleBaseRoiInspectResult`：圆基准结果。
- `DetectRoiInspectResult`：单个检测 ROI 结果。

### 5. Processing

`Processing` 目录为核心算法处理层。

`TemplateEdgeInspectProcessor.cs` 负责模板匹配、ROI 坐标变换、线基准拟合、圆基准拟合、检测 ROI 找边、毛刺/凹陷判定和结果汇总。

### 6. Sdk

`Sdk` 目录为上位机调用入口。

`TemplateEdgeInspectSdk.cs` 提供两个主要方法：

- `OpenSetupDialog`：打开参数配置窗口。
- `RunInspection`：上位机传入图像、Job 和判定参数后直接运行检测。

### 7. Lib

`Lib` 目录存放 HALCON .NET 运行库引用。

项目当前引用 `Lib\halcondotnet.dll`。工程按 64 位运行配置，客户现场需要保证 HALCON 运行环境和 DLL 位数一致。

## 三、主要运行流程

### 1. 参数配置流程

客户进入配置界面后，流程如下：

1. 加载待检测图片。
2. 根据需要启用或关闭模板匹配。
3. 添加线基准 ROI 或圆基准 ROI。
4. 添加检测 ROI。
5. 点击左侧图像中的 ROI，配置当前 ROI 对应的卡尺参数。
6. 点击“保存(示教)”保存当前 Job。
7. 点击“运行”可在当前图像上验证检测效果。
8. 点击“确认配置”将 Job 返回给调用方。

### 2. 上位机调用流程

上位机通常分为两步调用。

第一步为示教配置：

```csharp
TemplateEdgeInspectSdk sdk = new TemplateEdgeInspectSdk();
EdgeInspectJob job = sdk.OpenSetupDialog(image, currentJob);
```

当客户点击“确认配置”时，返回配置后的 `EdgeInspectJob`。如果客户取消窗口，则返回 `null`。

第二步为自动检测：

```csharp
EdgeInspectResult result = sdk.RunInspection(
    image,
    job,
    acceptedTolerance,
    pixelResolutionX,
    pixelResolutionY);
```

`acceptedTolerance` 为上位机传入的毛刺判定公差。`pixelResolutionX` 和 `pixelResolutionY` 为像素分辨率，用于像素值和实际尺寸之间的换算。

## 四、Job 参数说明

`EdgeInspectJob` 是本项目的核心配置对象，保存一次检测任务所需的全部信息。

主要字段如下：

- `TemplateRoi`：模板匹配 ROI。
- `BaseRois`：线基准 ROI 列表。
- `CircleBaseRois`：圆基准 ROI 列表。
- `DetectItems`：检测 ROI 列表。
- `UseReferenceLine`：是否使用基准线判定。
- `Match`：模板匹配参数。
- `BaseCaliper`：默认线基准卡尺参数。
- `CircleCaliper`：默认圆基准卡尺参数。
- `DetectCaliper`：默认检测卡尺参数。
- `DetectMode`：缺陷检测模式。
- `TeachData`：模板示教数据。
- `UseExternalBurrTolerance`：是否使用上位机传入的毛刺公差。
- `ExternalBurrTolerance`：上位机传入的毛刺公差。
- `PixelResolutionX`：X 方向像素分辨率。
- `PixelResolutionY`：Y 方向像素分辨率。

每个 ROI 都可以拥有独立卡尺参数。如果单个 ROI 没有配置独立参数，则使用 Job 中对应的默认卡尺参数。

## 五、卡尺参数说明

`CaliperParameters` 用于描述卡尺找边行为。

主要字段如下：

- `NumMeasures`：卡尺测量点数量。
- `MeasureLength`：卡尺搜索长度。
- `MeasureWidth`：卡尺宽度。
- `Sigma`：边缘平滑参数。
- `Threshold`：边缘阈值。
- `SearchOutward`：圆基准搜索外扩距离。
- `MeasureInterpolation`：测量插值方式。
- `MeasureSelect`：边缘选择方式。
- `Transition`：找边极性。

线基准和检测 ROI 主要使用点数、长度、宽度、Sigma、阈值和极性。

圆基准额外使用 `SearchOutward`。圆基准检测时，卡尺会从圆 ROI 边缘向外扩展指定距离作为搜索起点，再沿圆法向向圆心内部搜索边缘点，最后根据有效边缘点拟合圆心。

## 六、基准检测说明

### 1. 线基准

线基准 ROI 使用旋转矩形表示。运行检测时，程序会在 ROI 内使用 HALCON 卡尺沿指定方向寻找边缘点，并根据边缘点拟合基准线。

线基准适用于直线边、台阶边、产品外轮廓边等场景。

### 2. 圆基准

圆基准由两个圆 ROI 组成。每个圆 ROI 会独立拟合圆心，两个圆心连线作为最终基准线。

圆基准适用于光斑、圆孔、圆形标记点等场景。

圆基准当前采用径向卡尺搜索方式：

1. 以用户绘制的圆 ROI 为理论圆。
2. 沿圆周均匀生成多个搜索方向。
3. 每个方向从圆外扩位置开始。
4. 沿圆法向向圆心内部搜索边缘。
5. 收集有效边缘点。
6. 用有效点拟合真实圆。
7. 输出拟合圆心。

这种方式的优点是搜索方向稳定，能够避免从圆心向外搜索时误选内部亮斑或噪声边。

## 七、检测 ROI 判定说明

检测 ROI 使用旋转矩形表示。运行检测时，程序会在检测 ROI 内使用卡尺寻找边缘点，并根据配置选择判定方式。

当前支持两类判定：

- 使用关联基准线判定。
- 使用检测 ROI 自身拟合线判定。

当使用基准线判定时，检测点会和关联基准线计算偏差，并根据公差判断毛刺或凹陷。

当使用自身拟合线判定时，程序会基于检测 ROI 内的边缘点拟合自身参考线，再计算局部偏差。

检测模式由 `DefectDetectMode` 控制：

- `Both`：同时检测毛刺和凹陷。
- `BurrOnly`：只检测毛刺。
- `DentOnly`：只检测凹陷。

## 八、模板匹配说明

模板匹配用于处理产品位置变化。

启用模板匹配后，程序会在示教图中根据 `TemplateRoi` 建立模板数据。运行检测时，程序先在当前图像中匹配模板位置，再根据匹配结果对基准 ROI 和检测 ROI 做坐标变换。

这样即使产品整体发生平移或旋转，ROI 仍能跟随产品位置进行检测。

如果客户现场图像位置稳定，也可以关闭模板匹配，直接使用固定 ROI 运行检测。

## 九、检测结果说明

`EdgeInspectResult` 为检测总结果。

主要字段如下：

- `Success`：整体检测是否成功。
- `Message`：结果说明。
- `TemplateMatchEnabled`：是否启用模板匹配。
- `TemplateMatchOk`：模板匹配是否成功。
- `TemplateMatchScore`：模板匹配分数。
- `BaseResults`：线基准检测结果。
- `CircleBaseResults`：圆基准检测结果。
- `DetectResults`：检测 ROI 结果。
- `FailedItems`：失败项说明。
- `BurrCount`：毛刺数量。
- `DentCount`：凹陷数量。
- `SignedMin`、`SignedMax`、`SignedMean`：有符号偏差统计。
- `DeltaMin`、`DeltaMax`、`DeltaMean`：绝对偏差统计。
- `MaxPositiveDeltaPx`：最大正向偏差像素值。
- `MaxPositiveDeltaValue`：最大正向偏差实际值。

当 `Success=false` 时，上位机应读取 `Message` 和 `FailedItems` 判断失败原因。

## 十、界面参数与代码字段对应关系

界面中的“点数”对应 `CaliperParameters.NumMeasures`。

界面中的“Sig”对应 `CaliperParameters.Sigma`。

界面中的“阈值”对应 `CaliperParameters.Threshold`。

界面中的“圆外扩”对应 `CaliperParameters.SearchOutward`，仅圆基准 ROI 使用。

界面中的“基准极性”和“检测极性”对应 `CaliperParameters.Transition`。

界面中的“检测模式”对应 `EdgeInspectJob.DetectMode`。

界面中的“启用模板匹配”对应 `EdgeInspectJob.Match.Enabled`。

界面中的“使用基准线判定”对应 `DetectRoiItem.UseReferenceLine`。

## 十一、部署说明

项目运行环境要求：

- Windows 系统。
- .NET Framework 4.7.2。
- 64 位运行环境。
- HALCON .NET 运行库。
- HALCON runtime 与 `halcondotnet.dll` 位数一致。

工程当前配置为：

- `TargetFrameworkVersion=v4.7.2`
- `PlatformTarget=x64`
- `Prefer32Bit=false`

由于 HALCON DLL 为 64 位运行库，项目不能改成 x86 或 Prefer32Bit，否则会出现“试图加载格式不正确的程序”等运行错误。

## 十二、客户集成注意事项

1. 配置窗口属于 WinForms 界面，应在 UI 线程或 STA 线程中调用。
2. 上位机传入的图像对象在 SDK 内部会复制，调用方可以自行管理原始图像生命周期。
3. `OpenSetupDialog` 返回 `null` 表示客户取消配置。
4. `RunInspection` 不会直接向上抛出算法异常，异常会转换为 `EdgeInspectResult.Success=false`。
5. 上位机应保存配置完成后的 `EdgeInspectJob`，后续检测直接传入该 Job。
6. 毛刺公差由上位机通过 `RunInspection` 参数传入，便于客户根据产品规格统一管理判定标准。
7. 圆基准检测效果与 ROI 半径、圆外扩、卡尺长度、阈值、Sigma 和极性共同相关，现场调试时应配合观察拟合圆和边缘点。

## 十三、代码维护说明

本项目采用分层结构组织代码。

界面交互集中在 `Forms`。

画布显示和 ROI 编辑集中在 `Controls`。

几何结构集中在 `Geometry`。

参数模型和结果模型集中在 `Models`。

核心检测算法集中在 `Processing`。

上位机接口集中在 `Sdk`。

后续如果新增检测类型，应优先扩展 `Models` 中的参数结构和结果结构，再在 `Processing` 中增加算法实现，最后由 `Forms` 增加对应配置入口，并由 `Sdk` 保持统一对外调用。
