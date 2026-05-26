using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 一次边缘对位检测任务的完整配置。
	/// </summary>
	/// <remarks>
	/// Job 保存模板 ROI、基准 ROI、检测 ROI、卡尺参数、模板示教数据、像素分辨率和外部公差等信息。
	/// 上位机应保存配置完成后的 Job，并在后续自动检测时原样传入 SDK。
	/// </remarks>
	public sealed class EdgeInspectJob
	{
		/// <summary>线基准 ROI 数量上限；当前实现不做业务层数量限制。</summary>
		public const int MaxBaseRoiCount = int.MaxValue;

		/// <summary>模板匹配使用的参考 ROI；仅在 <see cref="Match"/> 启用时需要。</summary>
		public RotRectF TemplateRoi { get; set; }

		/// <summary>线基准 ROI 列表，每个 ROI 会拟合一条基准线。</summary>
		public List<BaseRoiItem> BaseRois { get; set; } = new List<BaseRoiItem>();

		/// <summary>圆基准 ROI 列表，每组由两个圆组成，两个圆心连线作为基准线。</summary>
		public List<CircleBaseRoiItem> CircleBaseRois { get; set; } = new List<CircleBaseRoiItem>();

		/// <summary>单圆基准点 ROI 列表，绑定后会取拟合圆心做点到自拟合切割边的整体距离判定。</summary>
		public List<CirclePointRoiItem> CirclePointRois { get; set; } = new List<CirclePointRoiItem>();

		/// <summary>检测 ROI 列表，每个 ROI 产生独立的边缘点和缺陷判定结果。</summary>
		public List<DetectRoiItem> DetectItems { get; set; } = new List<DetectRoiItem>();

		/// <summary>兼容旧版单检测 ROI 的全局基准线判定开关。</summary>
		public bool UseReferenceLine { get; set; } = true;

		/// <summary>模板匹配参数。</summary>
		public TemplateMatchParameters Match { get; set; } = new TemplateMatchParameters();

		/// <summary>线基准 ROI 默认卡尺参数。</summary>
		public CaliperParameters BaseCaliper { get; set; } = CreateDefaultBaseCaliper();

		/// <summary>圆基准 ROI 默认卡尺参数。</summary>
		public CaliperParameters CircleCaliper { get; set; } = CreateDefaultCircleCaliper();

		/// <summary>检测 ROI 默认卡尺参数。</summary>
		public CaliperParameters DetectCaliper { get; set; } = CreateDefaultDetectCaliper();

		/// <summary>全局缺陷检测模式。</summary>
		public DefectDetectMode DetectMode { get; set; } = DefectDetectMode.Both;

		/// <summary>模板示教后保存的 HALCON 模型数据。</summary>
		public TemplateTeachData TeachData { get; set; } = new TemplateTeachData();

		/// <summary>是否使用 SDK 运行时传入的外部毛刺公差。</summary>
		public bool UseExternalBurrTolerance { get; set; } = false;

		/// <summary>SDK 运行时传入的外部毛刺公差。</summary>
		public double ExternalBurrTolerance { get; set; } = 0.0;

		/// <summary>SDK 运行时传入的外部凹陷公差，单位为毫米。</summary>
		public double ExternalDentTolerance { get; set; } = 0.0;

		/// <summary>SDK 运行时传入的外部超边公差，单位为毫米。</summary>
		public double ExternalOverEdgeTolerance { get; set; } = 0.0;

		/// <summary>SDK 运行时传入的外部漏铜公差，单位为毫米。</summary>
		public double ExternalCopperLeakTolerance { get; set; } = 0.0;

		/// <summary>X 方向单像素代表的物理尺寸。</summary>
		public double PixelResolutionX { get; set; } = 1.0;

		/// <summary>Y 方向单像素代表的物理尺寸。</summary>
		public double PixelResolutionY { get; set; } = 1.0;

		/// <summary>兼容旧版接口的第一个线基准 ROI。</summary>
		public RotRectF BaseRoi
		{
			get
			{
				return (BaseRois != null && BaseRois.Count > 0) ? BaseRois[0].Roi : default(RotRectF);
			}
			set
			{
				EnsureBaseRoi(0).Roi = value;
			}
		}

		/// <summary>兼容旧版接口的第一个检测 ROI。</summary>
		public RotRectF DetectRoi
		{
			get
			{
				return (DetectItems != null && DetectItems.Count > 0) ? DetectItems[0].Roi : default(RotRectF);
			}
			set
			{
				EnsureDetectItem(0).Roi = value;
			}
		}

		/// <summary>兼容旧版接口的第一个检测 ROI 名义距离，单位为像素。</summary>
		public double NominalDistancePx
		{
			get
			{
				return (DetectItems != null && DetectItems.Count > 0) ? DetectItems[0].NominalDistancePx : 0.0;
			}
			set
			{
				EnsureDetectItem(0).NominalDistancePx = value;
			}
		}

		/// <summary>兼容旧版接口的第一个检测 ROI 毛刺公差，单位为像素。</summary>
		public double BurrTolerancePx
		{
			get
			{
				return (DetectItems != null && DetectItems.Count > 0) ? DetectItems[0].BurrTolerancePx : 2.0;
			}
			set
			{
				EnsureDetectItem(0).BurrTolerancePx = value;
			}
		}

		/// <summary>兼容旧版接口的第一个检测 ROI 凹陷公差，单位为像素。</summary>
		public double DentTolerancePx
		{
			get
			{
				return (DetectItems != null && DetectItems.Count > 0) ? DetectItems[0].DentTolerancePx : 2.0;
			}
			set
			{
				EnsureDetectItem(0).DentTolerancePx = value;
			}
		}

		/// <summary>当前配置是否具备模板示教的基本条件。</summary>
		public bool IsReadyForTeach => !Match.Enabled || !TemplateRoi.IsEmpty;

		/// <summary>是否存在需要绑定基准线的启用检测 ROI。</summary>
		public bool RequiresAnyBaseRoi => DetectItems != null && DetectItems.Any((DetectRoiItem x) => x != null && x.Enabled && !x.Roi.IsEmpty && x.UseReferenceLine && x.ReferenceBaseKind != ReferenceBaseKind.CirclePoint);

		/// <summary>是否存在使用自身拟合线判定的启用检测 ROI。</summary>
		public bool HasAnySelfJudgeDetect => DetectItems != null && DetectItems.Any((DetectRoiItem x) => x != null && x.Enabled && !x.Roi.IsEmpty && (!x.UseReferenceLine || x.ReferenceBaseKind == ReferenceBaseKind.CirclePoint));

		/// <summary>当前配置是否具备运行检测的基本条件。</summary>
		public bool IsReadyForInspect
		{
			get
			{
				bool flag = !Match.Enabled || !TemplateRoi.IsEmpty;
				bool flag2 = DetectItems != null && DetectItems.Any((DetectRoiItem x) => x != null && x.Enabled && !x.Roi.IsEmpty);
				bool flag3 = (BaseRois != null && BaseRois.Any((BaseRoiItem x) => x != null && !x.Roi.IsEmpty)) || (CircleBaseRois != null && CircleBaseRois.Any((CircleBaseRoiItem x) => x != null && !x.Circle1.IsEmpty && !x.Circle2.IsEmpty));
				bool flag4 = CirclePointRois != null && CirclePointRois.Any((CirclePointRoiItem x) => x != null && !x.Circle.IsEmpty);
				if (!flag || !flag2)
				{
					return false;
				}
				if (RequiresAnyBaseRoi && !flag3)
				{
					return false;
				}
				if (DetectItems.Any((DetectRoiItem x) => x != null && x.Enabled && !x.Roi.IsEmpty && x.ReferenceBaseKind == ReferenceBaseKind.CirclePoint) && !flag4)
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>创建线基准 ROI 的默认卡尺参数。</summary>
		public static CaliperParameters CreateDefaultBaseCaliper()
		{
			return new CaliperParameters
			{
				NumMeasures = 30,
				MeasureLength = 30.0,
				MeasureWidth = 5.0,
				Sigma = 1.0,
				Threshold = 20.0,
				SearchOutward = 0.0,
				MeasureInterpolation = "bicubic",
				MeasureSelect = "first",
				Transition = "negative"
			};
		}

		/// <summary>创建检测 ROI 的默认卡尺参数。</summary>
		public static CaliperParameters CreateDefaultDetectCaliper()
		{
			return new CaliperParameters
			{
				NumMeasures = 40,
				MeasureLength = 25.0,
				MeasureWidth = 3.0,
				Sigma = 1.0,
				Threshold = 15.0,
				SearchOutward = 0.0,
				MeasureInterpolation = "bicubic",
				MeasureSelect = "first",
				Transition = "negative"
			};
		}

		/// <summary>创建圆基准 ROI 的默认卡尺参数。</summary>
		public static CaliperParameters CreateDefaultCircleCaliper()
		{
			return new CaliperParameters
			{
				NumMeasures = 64,
				MeasureLength = 12.0,
				MeasureWidth = 4.0,
				Sigma = 1.0,
				Threshold = 20.0,
				SearchOutward = 6.0,
				MeasureInterpolation = "bicubic",
				MeasureSelect = "first",
				Transition = "all"
			};
		}

		/// <summary>
		/// 确保指定索引的线基准 ROI 存在，并返回该对象。
		/// </summary>
		public BaseRoiItem EnsureBaseRoi(int index)
		{
			if (index < 0)
			{
				index = 0;
			}
			if (index >= int.MaxValue)
			{
				index = 2147483646;
			}
			if (BaseRois == null)
			{
				BaseRois = new List<BaseRoiItem>();
			}
			while (BaseRois.Count <= index)
			{
				int num = BaseRois.Count + 1;
				BaseRois.Add(new BaseRoiItem
				{
					Name = $"基准{num}",
					Caliper = (BaseCaliper?.DeepClone() ?? CreateDefaultBaseCaliper())
				});
			}
			return BaseRois[index];
		}

		/// <summary>
		/// 确保指定索引的检测 ROI 存在，并返回该对象。
		/// </summary>
		public DetectRoiItem EnsureDetectItem(int index)
		{
			if (index < 0)
			{
				index = 0;
			}
			if (DetectItems == null)
			{
				DetectItems = new List<DetectRoiItem>();
			}
			while (DetectItems.Count <= index)
			{
				int num = DetectItems.Count + 1;
				DetectItems.Add(new DetectRoiItem
				{
					Name = $"检测{num}",
					UseReferenceLine = UseReferenceLine,
					BaseRoiIndex = 0,
					BaseRoiId = ((BaseRois != null && BaseRois.Count > 0) ? (BaseRois[0]?.Id ?? "") : ""),
					ReferenceBaseKind = ((BaseRois == null || BaseRois.Count <= 0) ? ReferenceBaseKind.CirclePair : ReferenceBaseKind.LineRoi),
					CircleBaseRoiId = ((CircleBaseRois != null && CircleBaseRois.Count > 0) ? (CircleBaseRois[0]?.Id ?? "") : ""),
					CirclePointRoiId = ((CirclePointRois != null && CirclePointRois.Count > 0) ? (CirclePointRois[0]?.Id ?? "") : ""),
					Caliper = (DetectCaliper?.DeepClone() ?? CreateDefaultDetectCaliper())
				});
			}
			return DetectItems[index];
		}

		/// <summary>
		/// 根据基准 ROI ID 解析当前列表中的索引，ID 不存在时回退到指定索引。
		/// </summary>
		public int ResolveBaseRoiIndex(string baseRoiId, int fallbackIndex)
		{
			if (BaseRois == null || BaseRois.Count <= 0)
			{
				return 0;
			}
			if (!string.IsNullOrWhiteSpace(baseRoiId))
			{
				for (int i = 0; i < BaseRois.Count; i++)
				{
					BaseRoiItem baseRoiItem = BaseRois[i];
					if (baseRoiItem != null && string.Equals(baseRoiItem.Id, baseRoiId, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
			}
			if (fallbackIndex < 0)
			{
				fallbackIndex = 0;
			}
			if (fallbackIndex >= BaseRois.Count)
			{
				fallbackIndex = BaseRois.Count - 1;
			}
			return fallbackIndex;
		}

		/// <summary>
		/// 根据圆基准 ROI ID 解析当前列表中的索引，ID 不存在时回退到指定索引。
		/// </summary>
		public int ResolveCircleBaseRoiIndex(string circleBaseRoiId, int fallbackIndex)
		{
			if (CircleBaseRois == null || CircleBaseRois.Count <= 0)
			{
				return 0;
			}
			if (!string.IsNullOrWhiteSpace(circleBaseRoiId))
			{
				for (int i = 0; i < CircleBaseRois.Count; i++)
				{
					CircleBaseRoiItem circleBaseRoiItem = CircleBaseRois[i];
					if (circleBaseRoiItem != null && string.Equals(circleBaseRoiItem.Id, circleBaseRoiId, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
			}
			if (fallbackIndex < 0)
			{
				fallbackIndex = 0;
			}
			if (fallbackIndex >= CircleBaseRois.Count)
			{
				fallbackIndex = CircleBaseRois.Count - 1;
			}
			return fallbackIndex;
		}

		/// <summary>
		/// 根据圆点基准 ROI ID 解析当前列表中的索引，ID 不存在时回退到指定索引。
		/// </summary>
		public int ResolveCirclePointRoiIndex(string circlePointRoiId, int fallbackIndex)
		{
			if (CirclePointRois == null || CirclePointRois.Count <= 0)
			{
				return 0;
			}
			if (!string.IsNullOrWhiteSpace(circlePointRoiId))
			{
				for (int i = 0; i < CirclePointRois.Count; i++)
				{
					CirclePointRoiItem circlePointRoiItem = CirclePointRois[i];
					if (circlePointRoiItem != null && string.Equals(circlePointRoiItem.Id, circlePointRoiId, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
			}
			if (fallbackIndex < 0)
			{
				fallbackIndex = 0;
			}
			if (fallbackIndex >= CirclePointRois.Count)
			{
				fallbackIndex = CirclePointRois.Count - 1;
			}
			return fallbackIndex;
		}

		/// <summary>
		/// 归一化检测 ROI 的基准绑定，使索引和 ID 与当前基准列表保持一致；绑定圆点基准时会转为自拟合线判定。
		/// </summary>
		public void NormalizeDetectBinding(DetectRoiItem item)
		{
			if (item == null)
			{
				return;
			}
			if (item.ReferenceBaseKind == ReferenceBaseKind.CirclePoint)
			{
				item.UseReferenceLine = false;
				if (CirclePointRois == null || CirclePointRois.Count <= 0)
				{
					item.CirclePointRoiIndex = 0;
					item.CirclePointRoiId = "";
				}
				else
				{
					int index = (item.CirclePointRoiIndex = ResolveCirclePointRoiIndex(item.CirclePointRoiId, item.CirclePointRoiIndex));
					item.CirclePointRoiId = CirclePointRois[index]?.Id ?? "";
				}
			}
			else if (item.ReferenceBaseKind == ReferenceBaseKind.CirclePair)
			{
				if (CircleBaseRois == null || CircleBaseRois.Count <= 0)
				{
					item.CircleBaseRoiIndex = 0;
					item.CircleBaseRoiId = "";
				}
				else
				{
					int index = (item.CircleBaseRoiIndex = ResolveCircleBaseRoiIndex(item.CircleBaseRoiId, item.CircleBaseRoiIndex));
					item.CircleBaseRoiId = CircleBaseRois[index]?.Id ?? "";
				}
			}
			else if (BaseRois == null || BaseRois.Count <= 0)
			{
				item.BaseRoiIndex = 0;
				item.BaseRoiId = "";
			}
			else
			{
				int index2 = (item.BaseRoiIndex = ResolveBaseRoiIndex(item.BaseRoiId, item.BaseRoiIndex));
				item.BaseRoiId = BaseRois[index2]?.Id ?? "";
			}
		}

		/// <summary>取得指定线基准 ROI 的卡尺参数；未单独配置时返回全局默认参数。</summary>
		public CaliperParameters ResolveBaseCaliper(int index)
		{
			if (BaseRois != null && index >= 0 && index < BaseRois.Count && BaseRois[index] != null && BaseRois[index].Caliper != null)
			{
				return BaseRois[index].Caliper;
			}
			return BaseCaliper ?? CreateDefaultBaseCaliper();
		}

		/// <summary>取得指定检测 ROI 的卡尺参数；未单独配置时返回全局默认参数。</summary>
		public CaliperParameters ResolveDetectCaliper(int index)
		{
			if (DetectItems != null && index >= 0 && index < DetectItems.Count && DetectItems[index] != null && DetectItems[index].Caliper != null)
			{
				return DetectItems[index].Caliper;
			}
			return DetectCaliper ?? CreateDefaultDetectCaliper();
		}

		private static void NormalizeCaliper(CaliperParameters cp, CaliperParameters fallback)
		{
			if (cp != null)
			{
				CaliperParameters caliperParameters = fallback ?? new CaliperParameters();
				if (cp.NumMeasures < 1)
				{
					cp.NumMeasures = ((caliperParameters.NumMeasures <= 0) ? 1 : caliperParameters.NumMeasures);
				}
				if (cp.MeasureLength <= 0.0)
				{
					cp.MeasureLength = ((caliperParameters.MeasureLength > 0.0) ? caliperParameters.MeasureLength : 1.0);
				}
				if (cp.MeasureWidth <= 0.0)
				{
					cp.MeasureWidth = ((caliperParameters.MeasureWidth > 0.0) ? caliperParameters.MeasureWidth : 1.0);
				}
				if (cp.Sigma <= 0.0)
				{
					cp.Sigma = ((caliperParameters.Sigma > 0.0) ? caliperParameters.Sigma : 1.0);
				}
			if (cp.Threshold <= 0.0)
			{
				cp.Threshold = ((caliperParameters.Threshold > 0.0) ? caliperParameters.Threshold : 1.0);
			}
			if (cp.SearchOutward < 0.0)
			{
				cp.SearchOutward = ((caliperParameters.SearchOutward > 0.0) ? caliperParameters.SearchOutward : 0.0);
			}
			if (string.IsNullOrWhiteSpace(cp.MeasureInterpolation))
				{
					cp.MeasureInterpolation = (string.IsNullOrWhiteSpace(caliperParameters.MeasureInterpolation) ? "bicubic" : caliperParameters.MeasureInterpolation);
				}
				if (string.IsNullOrWhiteSpace(cp.MeasureSelect))
				{
					cp.MeasureSelect = (string.IsNullOrWhiteSpace(caliperParameters.MeasureSelect) ? "first" : caliperParameters.MeasureSelect);
				}
				if (string.IsNullOrWhiteSpace(cp.Transition))
				{
					cp.Transition = (string.IsNullOrWhiteSpace(caliperParameters.Transition) ? "negative" : caliperParameters.Transition);
				}
			}
		}

		/// <summary>
		/// 修正空集合、空参数、非法数值和失效 ROI，并补齐默认卡尺参数。
		/// </summary>
		/// <remarks>
		/// 该方法会移除空 ROI，适合运行检测前使用；如果需要保留编辑态草稿 ROI，应避免直接调用。
		/// </remarks>
		public void Normalize()
		{
			if (BaseRois == null)
			{
				BaseRois = new List<BaseRoiItem>();
			}
			if (DetectItems == null)
			{
				DetectItems = new List<DetectRoiItem>();
			}
			if (Match == null)
			{
				Match = new TemplateMatchParameters();
			}
			if (BaseCaliper == null)
			{
				BaseCaliper = CreateDefaultBaseCaliper();
			}
			if (CircleBaseRois == null)
			{
				CircleBaseRois = new List<CircleBaseRoiItem>();
			}
			if (CirclePointRois == null)
			{
				CirclePointRois = new List<CirclePointRoiItem>();
			}
			if (CircleCaliper == null)
			{
				CircleCaliper = CreateDefaultCircleCaliper();
			}
			if (DetectCaliper == null)
			{
				DetectCaliper = CreateDefaultDetectCaliper();
			}
			NormalizeCaliper(BaseCaliper, CreateDefaultBaseCaliper());
			NormalizeCaliper(CircleCaliper, CreateDefaultCircleCaliper());
			NormalizeCaliper(DetectCaliper, CreateDefaultDetectCaliper());
			if (TeachData == null)
			{
				TeachData = new TemplateTeachData();
			}
			if (ExternalBurrTolerance < 0.0)
			{
				ExternalBurrTolerance = 0.0;
			}
			if (ExternalDentTolerance < 0.0)
			{
				ExternalDentTolerance = 0.0;
			}
			if (ExternalOverEdgeTolerance < 0.0)
			{
				ExternalOverEdgeTolerance = 0.0;
			}
			if (ExternalCopperLeakTolerance < 0.0)
			{
				ExternalCopperLeakTolerance = 0.0;
			}
			if (PixelResolutionX <= 0.0 && PixelResolutionY > 0.0)
			{
				PixelResolutionX = PixelResolutionY;
			}
			if (PixelResolutionY <= 0.0 && PixelResolutionX > 0.0)
			{
				PixelResolutionY = PixelResolutionX;
			}
			if (PixelResolutionX <= 0.0)
			{
				PixelResolutionX = 1.0;
			}
			if (PixelResolutionY <= 0.0)
			{
				PixelResolutionY = 1.0;
			}
			BaseRois = BaseRois.Where((BaseRoiItem x) => x != null && !x.Roi.IsEmpty).Select(delegate(BaseRoiItem x, int i)
			{
				BaseRoiItem baseRoiItem = x.DeepClone() ?? new BaseRoiItem();
				if (string.IsNullOrWhiteSpace(baseRoiItem.Id))
				{
					baseRoiItem.Id = Guid.NewGuid().ToString("N");
				}
				baseRoiItem.Name = $"基准{i + 1}";
				if (baseRoiItem.Caliper == null)
				{
					baseRoiItem.Caliper = BaseCaliper.DeepClone();
				}
				NormalizeCaliper(baseRoiItem.Caliper, BaseCaliper);
				return baseRoiItem;
			}).ToList();
			CircleBaseRois = CircleBaseRois.Where((CircleBaseRoiItem x) => x != null && !x.Circle1.IsEmpty && !x.Circle2.IsEmpty).Select(delegate(CircleBaseRoiItem x, int i)
			{
				CircleBaseRoiItem circleBaseRoiItem = x.DeepClone() ?? new CircleBaseRoiItem();
				if (string.IsNullOrWhiteSpace(circleBaseRoiItem.Id))
				{
					circleBaseRoiItem.Id = Guid.NewGuid().ToString("N");
				}
				circleBaseRoiItem.Name = $"圆基准{i + 1}";
				if (circleBaseRoiItem.Caliper == null)
				{
					circleBaseRoiItem.Caliper = CircleCaliper.DeepClone();
				}
				NormalizeCaliper(circleBaseRoiItem.Caliper, CircleCaliper);
				return circleBaseRoiItem;
			}).ToList();
			CirclePointRois = CirclePointRois.Where((CirclePointRoiItem x) => x != null && !x.Circle.IsEmpty).Select(delegate(CirclePointRoiItem x, int i)
			{
				CirclePointRoiItem circlePointRoiItem = x.DeepClone() ?? new CirclePointRoiItem();
				if (string.IsNullOrWhiteSpace(circlePointRoiItem.Id))
				{
					circlePointRoiItem.Id = Guid.NewGuid().ToString("N");
				}
				circlePointRoiItem.Name = $"圆点基准{i + 1}";
				if (circlePointRoiItem.Caliper == null)
				{
					circlePointRoiItem.Caliper = CircleCaliper.DeepClone();
				}
				NormalizeCaliper(circlePointRoiItem.Caliper, CircleCaliper);
				return circlePointRoiItem;
			}).ToList();
			DetectItems = DetectItems.Where((DetectRoiItem x) => x != null && !x.Roi.IsEmpty).Select(delegate(DetectRoiItem x, int i)
			{
				DetectRoiItem detectRoiItem = x.DeepClone() ?? new DetectRoiItem();
				if (string.IsNullOrWhiteSpace(detectRoiItem.Id))
				{
					detectRoiItem.Id = Guid.NewGuid().ToString("N");
				}
				detectRoiItem.Name = $"检测{i + 1}";
				if (i == 0 && DetectItems.Count == 1)
				{
					UseReferenceLine = detectRoiItem.UseReferenceLine;
				}
				NormalizeDetectBinding(detectRoiItem);
				if (detectRoiItem.Caliper == null)
				{
					detectRoiItem.Caliper = DetectCaliper.DeepClone();
				}
				NormalizeCaliper(detectRoiItem.Caliper, DetectCaliper);
				return detectRoiItem;
			}).ToList();
		}
	}
}
