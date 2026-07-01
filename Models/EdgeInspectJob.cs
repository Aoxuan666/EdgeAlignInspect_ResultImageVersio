using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class EdgeInspectJob
	{
		public const int MaxBaseRoiCount = int.MaxValue;

		public RotRectF TemplateRoi { get; set; }

		public List<BaseRoiItem> BaseRois { get; set; } = new List<BaseRoiItem>();

		public List<CircleBaseRoiItem> CircleBaseRois { get; set; } = new List<CircleBaseRoiItem>();

		public List<CirclePointRoiItem> CirclePointRois { get; set; } = new List<CirclePointRoiItem>();

		public List<DetectRoiItem> DetectItems { get; set; } = new List<DetectRoiItem>();

		public bool UseReferenceLine { get; set; } = true;

		public TemplateMatchParameters Match { get; set; } = new TemplateMatchParameters();

		public CaliperParameters BaseCaliper { get; set; } = CreateDefaultBaseCaliper();

		public CaliperParameters CircleCaliper { get; set; } = CreateDefaultCircleCaliper();

		public CaliperParameters DetectCaliper { get; set; } = CreateDefaultDetectCaliper();

		public DefectDetectMode DetectMode { get; set; } = DefectDetectMode.Both;

		public TemplateTeachData TeachData { get; set; } = new TemplateTeachData();

		public InspectionLanguage Language { get; set; } = InspectionLanguage.Chinese;

		public bool UseExternalBurrTolerance { get; set; } = false;

		public double ExternalBurrTolerance { get; set; } = 0.0;

		public double ExternalDentTolerance { get; set; } = 0.0;

		public double ExternalOverEdgeTolerance { get; set; } = 0.0;

		public double ExternalCopperLeakTolerance { get; set; } = 0.0;

		public double PixelResolutionX { get; set; } = 1.0;

		public double PixelResolutionY { get; set; } = 1.0;

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

		public bool IsReadyForTeach => !Match.Enabled || !TemplateRoi.IsEmpty;

		public bool RequiresAnyBaseRoi => DetectItems != null && DetectItems.Any((DetectRoiItem x) => x != null && x.Enabled && !x.Roi.IsEmpty && x.UseReferenceLine && x.ReferenceBaseKind != ReferenceBaseKind.CirclePoint);

		public bool HasAnySelfJudgeDetect => DetectItems != null && DetectItems.Any((DetectRoiItem x) => x != null && x.Enabled && !x.Roi.IsEmpty && (!x.UseReferenceLine || x.ReferenceBaseKind == ReferenceBaseKind.CirclePoint));

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
					UseTemplateTransform = true,
					Caliper = (BaseCaliper?.DeepClone() ?? CreateDefaultBaseCaliper())
				});
			}
			return BaseRois[index];
		}

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
					UseTemplateTransform = true,
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
					int num = (item.CirclePointRoiIndex = ResolveCirclePointRoiIndex(item.CirclePointRoiId, item.CirclePointRoiIndex));
					int index = num;
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
					int num = (item.CircleBaseRoiIndex = ResolveCircleBaseRoiIndex(item.CircleBaseRoiId, item.CircleBaseRoiIndex));
					int index2 = num;
					item.CircleBaseRoiId = CircleBaseRois[index2]?.Id ?? "";
				}
			}
			else if (BaseRois == null || BaseRois.Count <= 0)
			{
				item.BaseRoiIndex = 0;
				item.BaseRoiId = "";
			}
			else
			{
				int num = (item.BaseRoiIndex = ResolveBaseRoiIndex(item.BaseRoiId, item.BaseRoiIndex));
				int index3 = num;
				item.BaseRoiId = BaseRois[index3]?.Id ?? "";
			}
		}

		public CaliperParameters ResolveBaseCaliper(int index)
		{
			if (BaseRois != null && index >= 0 && index < BaseRois.Count && BaseRois[index] != null && BaseRois[index].Caliper != null)
			{
				return BaseRois[index].Caliper;
			}
			return BaseCaliper ?? CreateDefaultBaseCaliper();
		}

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
