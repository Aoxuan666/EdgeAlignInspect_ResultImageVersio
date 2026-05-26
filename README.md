# EdgeAlignInspect Refactor

本目录是按职责拆分后的 WinForms + HALCON 视觉项目。

目录：

- `Forms`：窗体与 Designer。
- `Controls`：图像画布与 ROI 显示/编辑控件。
- `Geometry`：旋转矩形几何结构。
- `Models`：任务参数、ROI 参数、卡尺参数、检测结果模型。
- `Processing`：HALCON 模板匹配、卡尺测边、线拟合和缺陷判定处理器。
- `Sdk`：上位机调用入口。

构建：

```powershell
dotnet build
```

HALCON 引用来自：

`C:\Users\智茂软件研发部\Desktop\EdgeAlignInspect\packages\HalconDotNet.19.11.0\lib\netcoreapp3.0\halcondotnet.dll`
