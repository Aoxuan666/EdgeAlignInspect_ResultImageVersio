using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using HalconDotNet;

namespace EdgeAlignInspect
{
	/// <summary>
	/// 基于 HALCON 模板匹配和卡尺测量的核心检测处理器。
	/// </summary>
	/// <remarks>
	/// 处理器负责模板示教、运行时 ROI 坐标变换、线/圆基准拟合、检测 ROI 找边以及毛刺/凹陷判定。
	/// </remarks>
	public sealed class TemplateEdgeInspectProcessor : IDisposable
	{
		private struct PtD
		{
			public double X;

			public double Y;

			/// <summary>创建双精度图像点。</summary>
			/// <param name="x">列坐标。</param>
			/// <param name="y">行坐标。</param>
			public PtD(double x, double y)
			{
				X = x;
				Y = y;
			}
		}

		private sealed class SignedLine
		{
			private readonly PointF _p1;

			public double NRow { get; }

			public double NCol { get; }

			/// <summary>创建带方向法向量的直线。</summary>
			/// <param name="p1">直线上的参考点。</param>
			/// <param name="nRow">行方向法向量分量。</param>
			/// <param name="nCol">列方向法向量分量。</param>
			private SignedLine(PointF p1, double nRow, double nCol)
			{
				_p1 = p1;
				double num = Math.Sqrt(nRow * nRow + nCol * nCol);
				if (num < 1E-09)
				{
					num = 1.0;
				}
				NRow = nRow / num;
				NCol = nCol / num;
			}

			/// <summary>根据线段生成有符号距离直线。</summary>
			/// <param name="a">线段起点。</param>
			/// <param name="b">线段终点。</param>
			/// <returns>用于距离计算的有符号直线。</returns>
			public static SignedLine FromLine(PointF a, PointF b)
			{
				double num = b.X - a.X;
				double num2 = b.Y - a.Y;
				double num3 = Math.Sqrt(num * num + num2 * num2);
				if (num3 < 1E-09)
				{
					num3 = 1.0;
				}
				num /= num3;
				num2 /= num3;
				return new SignedLine(a, num, 0.0 - num2);
			}

			/// <summary>计算点到当前有符号线的距离。</summary>
			/// <param name="p">待计算的图像点。</param>
			/// <returns>带方向的距离，单位为像素。</returns>
			public double SignedDistance(PointF p)
			{
				return (double)(p.Y - _p1.Y) * NRow + (double)(p.X - _p1.X) * NCol;
			}
		}

		/// <summary>
		/// 根据参考图和模板 ROI 生成可持久化的 HALCON 模板数据。
		/// </summary>
		/// <param name="refBmp">参考图像。</param>
		/// <param name="job">包含模板 ROI 和模板匹配参数的任务配置。</param>
		/// <returns>模板示教数据；未启用模板匹配时返回空模板数据。</returns>
		public TemplateTeachData Teach(Bitmap refBmp, EdgeInspectJob job)
		{
			if (refBmp == null)
			{
				throw new ArgumentNullException("refBmp");
			}
			if (job == null)
			{
				throw new ArgumentNullException("job");
			}
			job = job.DeepClone();
			job.Normalize();
			TemplateTeachData templateTeachData = new TemplateTeachData();
			if (!job.Match.Enabled)
			{
				return templateTeachData;
			}
			if (job.TemplateRoi.IsEmpty)
			{
				throw new InvalidOperationException("未设置模板ROI。");
			}
			HImage hImage = null;
			GCHandle grayHandle = default(GCHandle);
			HObject rect = null;
			HObject imageReduced = null;
			HTuple modelID = null;
			try
			{
				hImage = BitmapToGrayHImage(refBmp, out var _, out var _, out var _, out grayHandle);
				GenRectangle2(job.TemplateRoi, out rect);
				HOperatorSet.ReduceDomain(hImage, rect, out imageReduced);
				HOperatorSet.CreateShapeModel(imageReduced, job.Match.NumLevels, job.Match.AngleStart, job.Match.AngleExtent, "auto", "auto", "use_polarity", 20, 10, out modelID);
				HOperatorSet.FindShapeModel(hImage, modelID, job.Match.AngleStart, job.Match.AngleExtent, job.Match.MinScore, 1, 0.0, "least_squares", 0, 0.95, out var row, out var column, out var angle, out var score);
				if (score.Length > 0)
				{
					templateTeachData.RefRow = row[0].D;
					templateTeachData.RefCol = column[0].D;
					templateTeachData.RefAngle = angle[0].D;
				}
				else
				{
					templateTeachData.RefRow = job.TemplateRoi.Center.Y;
					templateTeachData.RefCol = job.TemplateRoi.Center.X;
					templateTeachData.RefAngle = job.TemplateRoi.AngleRad;
				}
				templateTeachData.HasTemplate = true;
				templateTeachData.ModelBytes = ExportShapeModelToBytes(modelID);
				return templateTeachData;
			}
			finally
			{
				if (modelID != null && modelID.Length > 0)
				{
					try
					{
						HOperatorSet.ClearShapeModel(modelID);
					}
					catch
					{
					}
				}
				imageReduced?.Dispose();
				rect?.Dispose();
				hImage?.Dispose();
				if (grayHandle.IsAllocated)
				{
					grayHandle.Free();
				}
			}
		}

		/// <summary>
		/// 在当前图像上执行完整检测流程。
		/// </summary>
		/// <param name="curBmp">当前待检测图像。</param>
		/// <param name="job">检测任务配置，方法内部会先归一化参数。</param>
		/// <returns>包含模板匹配、基准拟合、检测 ROI 和缺陷统计的检测结果。</returns>
		public EdgeInspectResult Inspect(Bitmap curBmp, EdgeInspectJob job)
		{
			if (curBmp == null)
			{
				throw new ArgumentNullException("curBmp");
			}
			if (job == null)
			{
				throw new ArgumentNullException("job");
			}
			job = job.DeepClone();
			job.Normalize();
			bool requiresAnyBaseRoi = job.RequiresAnyBaseRoi;
			bool hasAnySelfJudgeDetect = job.HasAnySelfJudgeDetect;
			if (!job.IsReadyForInspect)
			{
				throw new InvalidOperationException(requiresAnyBaseRoi ? "需要：至少一个检测ROI；其中使用基准线判定的检测ROI还需要至少一个基准ROI。" : "需要：至少一个检测ROI。");
			}
			if (job.Match.Enabled && (job.TeachData == null || !job.TeachData.HasTemplate || job.TeachData.ModelBytes == null || job.TeachData.ModelBytes.Length == 0))
			{
				throw new InvalidOperationException("Job 中未包含有效模板数据。");
			}
			bool flag = job.UseExternalBurrTolerance && job.ExternalBurrTolerance >= 0.0 && job.PixelResolutionX > 0.0 && job.PixelResolutionY > 0.0;
			EdgeInspectResult edgeInspectResult = new EdgeInspectResult
			{
				DetectMode = job.DetectMode,
				TemplateMatchEnabled = job.Match.Enabled,
				TemplateMatchOk = !job.Match.Enabled,
				TemplateRoiCur = job.TemplateRoi,
				UseReferenceLine = (requiresAnyBaseRoi && !hasAnySelfJudgeDetect),
				HasReferenceLineItems = requiresAnyBaseRoi,
				HasSelfFitLineItems = hasAnySelfJudgeDetect,
				UseExternalBurrTolerance = flag,
				ExternalBurrTolerance = job.ExternalBurrTolerance,
				ExternalDentTolerance = job.ExternalDentTolerance,
				ExternalOverEdgeTolerance = job.ExternalOverEdgeTolerance,
				ExternalCopperLeakTolerance = job.ExternalCopperLeakTolerance,
				PixelResolutionX = job.PixelResolutionX,
				PixelResolutionY = job.PixelResolutionY
			};
			HImage hImage = null;
			GCHandle grayHandle = default(GCHandle);
			HTuple hTuple = null;
			try
			{
				hImage = BitmapToGrayHImage(curBmp, out var _, out var w, out var h, out grayHandle);
				HTuple homMat2D = null;
				bool flag2 = false;
				if (job.Match.Enabled)
				{
					hTuple = ImportShapeModelFromBytes(job.TeachData.ModelBytes);
					HOperatorSet.FindShapeModel(hImage, hTuple, job.Match.AngleStart, job.Match.AngleExtent, job.Match.MinScore, 1, 0.0, "least_squares", 0, 0.95, out var row, out var column, out var angle, out var score);
					if (score.Length < 1)
					{
						edgeInspectResult.TemplateMatchOk = false;
						edgeInspectResult.Success = false;
						edgeInspectResult.NgReasons |= NgReason.TemplateMatchFailed;
						edgeInspectResult.FailedItems.Add("模板匹配失败");
						edgeInspectResult.Message = "模板匹配失败";
						return edgeInspectResult;
					}
					edgeInspectResult.TemplateMatchOk = true;
					edgeInspectResult.TemplateMatchScore = score[0].D;
					HOperatorSet.VectorAngleToRigid(job.TeachData.RefRow, job.TeachData.RefCol, job.TeachData.RefAngle, row[0], column[0], angle[0], out homMat2D);
					flag2 = true;
					edgeInspectResult.TemplateRoiCur = TransformRotRect(job.TemplateRoi, homMat2D);
				}
				Dictionary<int, EdgeLineFit> dictionary = new Dictionary<int, EdgeLineFit>();
				Dictionary<int, EdgeLineFit> dictionary2 = new Dictionary<int, EdgeLineFit>();
				Dictionary<int, EdgeCircleFit> dictionary3 = new Dictionary<int, EdgeCircleFit>();
				for (int i = 0; i < job.BaseRois.Count; i++)
				{
					BaseRoiItem baseRoiItem = job.BaseRois[i] ?? new BaseRoiItem
					{
						Name = $"基准{i + 1}"
					};
					RotRectF rotRectF = (flag2 ? TransformRotRect(baseRoiItem.Roi, homMat2D) : baseRoiItem.Roi);
					CaliperParameters caliperParameters = (baseRoiItem.Caliper ?? job.ResolveBaseCaliper(i) ?? EdgeInspectJob.CreateDefaultBaseCaliper()).DeepClone();
					BaseRoiInspectResult baseRoiInspectResult = new BaseRoiInspectResult
					{
						Index = i,
						Name = (string.IsNullOrWhiteSpace(baseRoiItem.Name) ? $"基准{i + 1}" : baseRoiItem.Name),
						RoiCur = rotRectF,
						CaliperUsed = caliperParameters.DeepClone()
					};
					if (rotRectF.IsEmpty)
					{
						baseRoiInspectResult.Success = false;
						baseRoiInspectResult.Message = "基准ROI为空";
					}
					else
					{
						baseRoiInspectResult.Line = FitLineInRotRoi(hImage, w, h, rotRectF, caliperParameters);
						baseRoiInspectResult.Success = baseRoiInspectResult.Line != null && baseRoiInspectResult.Line.Success;
						baseRoiInspectResult.Message = (baseRoiInspectResult.Success ? "OK" : AppendFailureDetail("基准线拟合失败", baseRoiInspectResult.Line));
						if (baseRoiInspectResult.Success)
						{
							dictionary[i] = baseRoiInspectResult.Line;
						}
					}
					edgeInspectResult.BaseResults.Add(baseRoiInspectResult);
				}
				for (int j = 0; j < job.CircleBaseRois.Count; j++)
				{
					CircleBaseRoiItem circleBaseRoiItem = job.CircleBaseRois[j] ?? new CircleBaseRoiItem
					{
						Name = $"圆基准{j + 1}"
					};
					CircleRoiF circleRoiF = (flag2 ? TransformCircleRoi(circleBaseRoiItem.Circle1, homMat2D) : circleBaseRoiItem.Circle1);
					CircleRoiF circleRoiF2 = (flag2 ? TransformCircleRoi(circleBaseRoiItem.Circle2, homMat2D) : circleBaseRoiItem.Circle2);
					CaliperParameters caliperParameters2 = (circleBaseRoiItem.Caliper ?? job.CircleCaliper ?? EdgeInspectJob.CreateDefaultCircleCaliper()).DeepClone();
					CircleBaseRoiInspectResult circleBaseRoiInspectResult = new CircleBaseRoiInspectResult
					{
						Index = j,
						Name = (string.IsNullOrWhiteSpace(circleBaseRoiItem.Name) ? $"圆基准{j + 1}" : circleBaseRoiItem.Name),
						Circle1RoiCur = circleRoiF,
						Circle2RoiCur = circleRoiF2,
						CaliperUsed = caliperParameters2.DeepClone()
					};
					if (circleRoiF.IsEmpty || circleRoiF2.IsEmpty)
					{
						circleBaseRoiInspectResult.Success = false;
						circleBaseRoiInspectResult.Message = "圆基准ROI为空";
					}
					else
					{
						circleBaseRoiInspectResult.Circle1 = FitCircleInRoi(hImage, w, h, circleRoiF, caliperParameters2);
						circleBaseRoiInspectResult.Circle2 = FitCircleInRoi(hImage, w, h, circleRoiF2, caliperParameters2);
						if (circleBaseRoiInspectResult.Circle1.Success && circleBaseRoiInspectResult.Circle2.Success)
						{
							circleBaseRoiInspectResult.Line = LineFromCircleCenters(circleBaseRoiInspectResult.Circle1.Center, circleBaseRoiInspectResult.Circle2.Center, w, h);
							circleBaseRoiInspectResult.Success = circleBaseRoiInspectResult.Line.Success;
							circleBaseRoiInspectResult.Message = (circleBaseRoiInspectResult.Success ? "OK" : "圆心连线无效");
							if (circleBaseRoiInspectResult.Success)
							{
								dictionary2[j] = circleBaseRoiInspectResult.Line;
							}
						}
						else
						{
							circleBaseRoiInspectResult.Success = false;
							circleBaseRoiInspectResult.Message = "圆拟合失败：" + (circleBaseRoiInspectResult.Circle1.Success ? "" : ("圆1 " + circleBaseRoiInspectResult.Circle1.Message + " ")) + (circleBaseRoiInspectResult.Circle2.Success ? "" : ("圆2 " + circleBaseRoiInspectResult.Circle2.Message));
						}
					}
					edgeInspectResult.CircleBaseResults.Add(circleBaseRoiInspectResult);
				}
				for (int p = 0; p < job.CirclePointRois.Count; p++)
				{
					CirclePointRoiItem circlePointRoiItem = job.CirclePointRois[p] ?? new CirclePointRoiItem
					{
						Name = $"圆点基准{p + 1}"
					};
					CircleRoiF circleRoiF3 = (flag2 ? TransformCircleRoi(circlePointRoiItem.Circle, homMat2D) : circlePointRoiItem.Circle);
					CaliperParameters caliperParameters4 = (circlePointRoiItem.Caliper ?? job.CircleCaliper ?? EdgeInspectJob.CreateDefaultCircleCaliper()).DeepClone();
					CirclePointRoiInspectResult circlePointRoiInspectResult = new CirclePointRoiInspectResult
					{
						Index = p,
						Name = (string.IsNullOrWhiteSpace(circlePointRoiItem.Name) ? $"圆点基准{p + 1}" : circlePointRoiItem.Name),
						CircleRoiCur = circleRoiF3,
						CaliperUsed = caliperParameters4.DeepClone()
					};
					if (circleRoiF3.IsEmpty)
					{
						circlePointRoiInspectResult.Success = false;
						circlePointRoiInspectResult.Message = "圆点基准ROI为空";
					}
					else
					{
						circlePointRoiInspectResult.Circle = FitCircleInRoi(hImage, w, h, circleRoiF3, caliperParameters4);
						circlePointRoiInspectResult.Success = circlePointRoiInspectResult.Circle != null && circlePointRoiInspectResult.Circle.Success;
						circlePointRoiInspectResult.Message = (circlePointRoiInspectResult.Success ? "OK" : AppendFailureDetail("圆点基准拟合失败", circlePointRoiInspectResult.Circle));
						if (circlePointRoiInspectResult.Success)
						{
							dictionary3[p] = circlePointRoiInspectResult.Circle;
						}
					}
					edgeInspectResult.CirclePointResults.Add(circlePointRoiInspectResult);
				}
				if (edgeInspectResult.BaseResults.Count > 0)
				{
					edgeInspectResult.BaseRoiCur = edgeInspectResult.BaseResults[0].RoiCur;
					edgeInspectResult.BaseLine = edgeInspectResult.BaseResults[0].Line ?? new EdgeLineFit();
				}
				int num = 0;
				List<double> list = new List<double>();
				List<double> list2 = new List<double>();
				for (int k = 0; k < job.DetectItems.Count; k++)
				{
					DetectRoiItem detectRoiItem = job.DetectItems[k] ?? new DetectRoiItem
					{
						Name = $"检测{k + 1}"
					};
					if (!detectRoiItem.Enabled)
					{
						continue;
					}
					num++;
					RotRectF rotRectF2 = (flag2 ? TransformRotRect(detectRoiItem.Roi, homMat2D) : detectRoiItem.Roi);
					CaliperParameters caliperParameters3 = (detectRoiItem.Caliper ?? job.ResolveDetectCaliper(k) ?? EdgeInspectJob.CreateDefaultDetectCaliper()).DeepClone();
					int num2 = job.ResolveBaseRoiIndex(detectRoiItem.BaseRoiId, detectRoiItem.BaseRoiIndex);
					int num3 = job.ResolveCircleBaseRoiIndex(detectRoiItem.CircleBaseRoiId, detectRoiItem.CircleBaseRoiIndex);
					int num4 = job.ResolveCirclePointRoiIndex(detectRoiItem.CirclePointRoiId, detectRoiItem.CirclePointRoiIndex);
					DetectRoiInspectResult detectRoiInspectResult = new DetectRoiInspectResult
					{
						Index = k,
						Name = (string.IsNullOrWhiteSpace(detectRoiItem.Name) ? $"检测{k + 1}" : detectRoiItem.Name),
						Enabled = true,
						RoiCur = rotRectF2,
						UseReferenceLine = detectRoiItem.UseReferenceLine,
						ReferenceBaseKind = detectRoiItem.ReferenceBaseKind,
						BaseRoiIndex = num2,
						CircleBaseRoiIndex = num3,
						CirclePointRoiIndex = num4,
						AngleReference = detectRoiItem.AngleReference,
						NominalDistancePx = detectRoiItem.NominalDistancePx,
						ReferencePointCur = rotRectF2.Center,
						BurrTolerancePx = detectRoiItem.BurrTolerancePx,
						DentTolerancePx = detectRoiItem.DentTolerancePx,
						DetectMode = job.DetectMode,
						CaliperUsed = caliperParameters3.DeepClone(),
						UseExternalBurrTolerance = flag,
						ExternalBurrTolerance = job.ExternalBurrTolerance,
						ExternalDentTolerance = job.ExternalDentTolerance,
						ExternalOverEdgeTolerance = job.ExternalOverEdgeTolerance,
						ExternalCopperLeakTolerance = job.ExternalCopperLeakTolerance,
						PixelResolutionX = job.PixelResolutionX,
						PixelResolutionY = job.PixelResolutionY
					};
					if (rotRectF2.IsEmpty)
					{
						AddDetectFailure(edgeInspectResult, detectRoiInspectResult, "检测ROI为空");
						continue;
					}
					detectRoiInspectResult.FittedLine = FitLineInRotRoi(hImage, w, h, rotRectF2, caliperParameters3);
					if (detectRoiInspectResult.FittedLine == null || !detectRoiInspectResult.FittedLine.Success || detectRoiInspectResult.FittedLine.MeasurePoints.Count == 0)
					{
						AddDetectFailure(edgeInspectResult, detectRoiInspectResult, AppendFailureDetail("检测ROI拟合失败（检查：ROI/卡尺参数/极性）", detectRoiInspectResult.FittedLine));
						continue;
					}
					detectRoiInspectResult.FittedAngleRad = detectRoiInspectResult.FittedLine.AngleRad;
					EdgeLineFit overallReferenceLine = null;
					PointF? overallMeasurePoint = null;
					if (detectRoiItem.ReferenceBaseKind == ReferenceBaseKind.CirclePoint)
					{
						detectRoiInspectResult.UseReferenceLine = false;
						EdgeCircleFit value3;
						if (!dictionary3.TryGetValue(num4, out value3) || value3 == null || !value3.Success)
						{
							AddDetectFailure(edgeInspectResult, detectRoiInspectResult, $"关联圆点基准无效（圆点基准索引={num4 + 1}）");
							continue;
						}
						detectRoiInspectResult.CirclePointRoiIndex = num4;
						detectRoiInspectResult.ReferencePointCur = value3.Center;
						detectRoiInspectResult.JudgeLine = CopyLine(detectRoiInspectResult.FittedLine);
						overallReferenceLine = detectRoiInspectResult.FittedLine;
						overallMeasurePoint = value3.Center;
					}
					else if (detectRoiItem.UseReferenceLine)
					{
						EdgeLineFit value = null;
						if (detectRoiItem.ReferenceBaseKind == ReferenceBaseKind.CirclePair)
						{
							dictionary2.TryGetValue(num3, out value);
							detectRoiInspectResult.CircleBaseRoiIndex = num3;
						}
						else
						{
							dictionary.TryGetValue(num2, out value);
						}
						if (value == null || !value.Success)
						{
							string text = ((detectRoiItem.ReferenceBaseKind == ReferenceBaseKind.CirclePair) ? $"圆基准索引={detectRoiInspectResult.CircleBaseRoiIndex + 1}" : $"基准索引={num2 + 1}");
							AddDetectFailure(edgeInspectResult, detectRoiInspectResult, "关联基准线无效（" + text + "）");
							continue;
						}
						overallReferenceLine = value;
						detectRoiInspectResult.JudgeLine = CopyLine(detectRoiInspectResult.FittedLine);
					}
					else
					{
						detectRoiInspectResult.JudgeLine = CopyLine(detectRoiInspectResult.FittedLine);
					}
					ComputeAngleReference(detectRoiInspectResult, dictionary, dictionary2);
					EvaluateDetectItem(detectRoiInspectResult, rotRectF2, job.DetectMode, useReferenceLine: false);
					if (overallReferenceLine != null && overallReferenceLine.Success)
					{
						if (overallMeasurePoint.HasValue)
						{
							EvaluateOverallDistance(detectRoiInspectResult, overallReferenceLine, overallMeasurePoint.Value);
						}
						else
						{
							EvaluateOverallDistanceFromMeasurePoints(detectRoiInspectResult, overallReferenceLine);
						}
					}
					if (!detectRoiInspectResult.Success)
					{
						edgeInspectResult.FailedItems.Add(detectRoiInspectResult.Name + ": " + detectRoiInspectResult.Message);
					}
					edgeInspectResult.NgReasons |= detectRoiInspectResult.NgReasons;
					edgeInspectResult.BurrCount += detectRoiInspectResult.BurrCount;
					edgeInspectResult.DentCount += detectRoiInspectResult.DentCount;
					if (detectRoiInspectResult.IsOverEdge)
					{
						edgeInspectResult.OverEdgeCount++;
					}
					if (detectRoiInspectResult.IsCopperLeak)
					{
						edgeInspectResult.CopperLeakCount++;
					}
					edgeInspectResult.Points.AddRange(detectRoiInspectResult.Points);
					list.AddRange(detectRoiInspectResult.Points.Select((EdgePointResult p) => p.SignedDistancePx));
					list2.AddRange(detectRoiInspectResult.Points.Select((EdgePointResult p) => p.DeltaPx));
					edgeInspectResult.MaxPositiveDeltaPx = Math.Max(edgeInspectResult.MaxPositiveDeltaPx, detectRoiInspectResult.MaxPositiveDeltaPx);
					edgeInspectResult.MaxPositiveDeltaValue = Math.Max(edgeInspectResult.MaxPositiveDeltaValue, detectRoiInspectResult.MaxPositiveDeltaValue);
					edgeInspectResult.DetectResults.Add(detectRoiInspectResult);
				}
				if (edgeInspectResult.DetectResults.Count > 0)
				{
					DetectRoiInspectResult detectRoiInspectResult2 = edgeInspectResult.DetectResults[0];
					edgeInspectResult.DetectRoiCur = detectRoiInspectResult2.RoiCur;
					edgeInspectResult.Nominal = detectRoiInspectResult2.NominalDistancePx;
					edgeInspectResult.BurrTolerance = detectRoiInspectResult2.BurrTolerancePx;
					edgeInspectResult.DentTolerance = detectRoiInspectResult2.DentTolerancePx;
				}
				if (list.Count > 0)
				{
					edgeInspectResult.SignedMin = list.Min();
					edgeInspectResult.SignedMax = list.Max();
					edgeInspectResult.SignedMean = list.Average();
				}
				if (list2.Count > 0)
				{
					edgeInspectResult.DeltaMin = list2.Min();
					edgeInspectResult.DeltaMax = list2.Max();
					edgeInspectResult.DeltaMean = list2.Average();
				}
				bool flag3 = num > 0 && edgeInspectResult.DetectResults.All((DetectRoiInspectResult x) => x.Success);
				edgeInspectResult.Success = edgeInspectResult.TemplateMatchOk && flag3;
				if (!edgeInspectResult.Success && edgeInspectResult.NgReasons == NgReason.None)
				{
					edgeInspectResult.NgReasons = NgReason.DetectRoiFailed;
				}
				string text2 = GetDetectModeText(job.DetectMode);
				string judgeModeText = GetJudgeModeText(edgeInspectResult.HasReferenceLineItems, edgeInspectResult.HasSelfFitLineItems);
				string text3 = (flag ? $" | 外部允差(mm)：毛刺={job.ExternalBurrTolerance:F4} 凹陷={job.ExternalDentTolerance:F4} 超边={job.ExternalOverEdgeTolerance:F4} 漏铜={job.ExternalCopperLeakTolerance:F4} | 解析度X={job.PixelResolutionX:F6}mm/px | 解析度Y={job.PixelResolutionY:F6}mm/px | 最大偏差={edgeInspectResult.MaxPositiveDeltaValue:F4}mm" : $" | 本地毛刺允差={edgeInspectResult.BurrTolerance:F2}px | 本地凹陷允差={edgeInspectResult.DentTolerance:F2}px | 最大偏差={edgeInspectResult.MaxPositiveDeltaPx:F2}px");
				string text4 = ((edgeInspectResult.FailedItems.Count > 0) ? (" | " + string.Join("；", edgeInspectResult.FailedItems.Take(6))) : "");
				edgeInspectResult.Message = (edgeInspectResult.Success ? $"OK | 模式={text2} | 判定={judgeModeText} | 检测ROI={num}{text3} | 局部Δ(min/max/mean)={edgeInspectResult.DeltaMin:F2}/{edgeInspectResult.DeltaMax:F2}/{edgeInspectResult.DeltaMean:F2}px" : $"NG | 原因={edgeInspectResult.NgReasonText} | 模式={text2} | 判定={judgeModeText} | 检测ROI={num} | 失败项={edgeInspectResult.DetectResults.Count((DetectRoiInspectResult x) => !x.Success)} | 毛刺={edgeInspectResult.BurrCount} 凹陷={edgeInspectResult.DentCount} 超边={edgeInspectResult.OverEdgeCount} 漏铜={edgeInspectResult.CopperLeakCount}{text3} | 局部Δ(min/max/mean)={edgeInspectResult.DeltaMin:F2}/{edgeInspectResult.DeltaMax:F2}/{edgeInspectResult.DeltaMean:F2}px{text4}");
				return edgeInspectResult;
			}
			finally
			{
				if (hTuple != null && hTuple.Length > 0)
				{
					try
					{
						HOperatorSet.ClearShapeModel(hTuple);
					}
					catch
					{
					}
				}
				hImage?.Dispose();
				if (grayHandle.IsAllocated)
				{
					grayHandle.Free();
				}
			}
		}

		public void Dispose()
		{
		}

		/// <summary>记录单个检测 ROI 的失败结果。</summary>
		/// <param name="res">总检测结果对象。</param>
		/// <param name="dr">当前检测 ROI 结果。</param>
		/// <param name="message">失败说明。</param>
		private static void AddDetectFailure(EdgeInspectResult res, DetectRoiInspectResult dr, string message)
		{
			if (dr != null)
			{
				dr.Success = false;
				dr.Message = message ?? "失败";
				dr.NgReasons |= ResolveFailureReason(dr.Message);
				res.NgReasons |= dr.NgReasons;
				res.DetectResults.Add(dr);
				string text = (string.IsNullOrWhiteSpace(dr.Name) ? $"检测{dr.Index + 1}" : dr.Name);
				res.FailedItems.Add(text + ": " + dr.Message);
			}
		}

		/// <summary>根据失败信息推断对上位机友好的 NG 原因。</summary>
		/// <param name="message">失败信息。</param>
		/// <returns>对应的 NG 原因。</returns>
		private static NgReason ResolveFailureReason(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return NgReason.DetectRoiFailed;
			}
			if (message.Contains("基准"))
			{
				return NgReason.BaseRoiFailed;
			}
			if (message.Contains("检测ROI") || message.Contains("找边") || message.Contains("拟合"))
			{
				return NgReason.DetectRoiFailed;
			}
			return NgReason.DetectRoiFailed;
		}

		/// <summary>获取判定方式显示文本。</summary>
		/// <param name="hasReferenceLineItems">是否存在绑定基准线的检测项。</param>
		/// <param name="hasSelfFitItems">是否存在自拟合判定的检测项。</param>
		/// <returns>判定方式文本。</returns>
		private static string GetJudgeModeText(bool hasReferenceLineItems, bool hasSelfFitItems)
		{
			if (hasReferenceLineItems && hasSelfFitItems)
			{
				return "混合判定";
			}
			if (hasReferenceLineItems)
			{
				return "基准线判定";
			}
			return "自拟合线判定";
		}

		/// <summary>拼接直线拟合失败详情。</summary>
		/// <param name="message">基础失败说明。</param>
		/// <param name="fit">直线拟合结果。</param>
		/// <returns>包含详情的失败说明。</returns>
		private static string AppendFailureDetail(string message, EdgeLineFit fit)
		{
			if (fit == null || string.IsNullOrWhiteSpace(fit.Message))
			{
				return message;
			}
			return message + "：" + fit.Message;
		}

		/// <summary>拼接圆拟合失败详情。</summary>
		/// <param name="message">基础失败说明。</param>
		/// <param name="fit">圆拟合结果。</param>
		/// <returns>包含详情的失败说明。</returns>
		private static string AppendFailureDetail(string message, EdgeCircleFit fit)
		{
			if (fit == null || string.IsNullOrWhiteSpace(fit.Message))
			{
				return message;
			}
			return message + "：" + fit.Message;
		}

		/// <summary>执行检测 ROI 的局部毛刺/凹陷判定。</summary>
		/// <param name="dr">当前检测 ROI 结果。</param>
		/// <param name="detRoi">运行时检测 ROI。</param>
		/// <param name="mode">缺陷检测模式。</param>
		/// <param name="useReferenceLine">保留参数；当前局部判定固定使用检测 ROI 自拟合线。</param>
		private static void EvaluateDetectItem(DetectRoiInspectResult dr, RotRectF detRoi, DefectDetectMode mode, bool useReferenceLine)
		{
			if (dr == null)
			{
				return;
			}
			if (dr.JudgeLine == null || !dr.JudgeLine.Success)
			{
				dr.Success = false;
				dr.Message = "判定线无效";
				return;
			}
			if (dr.FittedLine == null || dr.FittedLine.MeasurePoints.Count == 0)
			{
				dr.Success = false;
				dr.Message = "未取得检测点";
				return;
			}
			SignedLine signedLine = SignedLine.FromLine(dr.FittedLine.P1, dr.FittedLine.P2);
			bool flag = mode == DefectDetectMode.Both || mode == DefectDetectMode.BurrOnly;
			bool flag2 = mode == DefectDetectMode.Both || mode == DefectDetectMode.DentOnly;
			bool flag3 = dr.UseExternalBurrTolerance && dr.ExternalBurrTolerance >= 0.0 && dr.PixelResolutionX > 0.0 && dr.PixelResolutionY > 0.0;
			double num = double.PositiveInfinity;
			double num2 = double.NegativeInfinity;
			double num3 = 0.0;
			double num4 = double.PositiveInfinity;
			double num5 = double.NegativeInfinity;
			double num6 = 0.0;
			double num7 = ResolvePositiveDirectionScale(detRoi, signedLine);
			double physicalDistancePerPixel = GetPhysicalDistancePerPixel(signedLine, dr.PixelResolutionX, dr.PixelResolutionY);
			for (int i = 0; i < dr.FittedLine.MeasurePoints.Count; i++)
			{
				PointF pointF = dr.FittedLine.MeasurePoints[i];
				double num8 = signedLine.SignedDistance(pointF);
				double num9 = num8 * num7;
				double num10 = num9;
				double signedDistanceValue = num9 * physicalDistancePerPixel;
				double num11 = num10 * physicalDistancePerPixel;
				bool flag4 = ((!flag3) ? (flag && num10 < 0.0 - dr.BurrTolerancePx) : (flag && num11 < 0.0 - dr.ExternalBurrTolerance));
				bool flag5 = ((!flag3) ? (flag2 && num10 > dr.DentTolerancePx) : (flag2 && num11 > dr.ExternalDentTolerance));
				if (flag4)
				{
					dr.BurrCount++;
				}
				if (flag5)
				{
					dr.DentCount++;
				}
				NgReason pointReasons = NgReason.None;
				if (flag4)
				{
					pointReasons |= NgReason.Burr;
					dr.NgReasons |= NgReason.Burr;
				}
				if (flag5)
				{
					pointReasons |= NgReason.Dent;
					dr.NgReasons |= NgReason.Dent;
				}
				num = Math.Min(num, num9);
				num2 = Math.Max(num2, num9);
				num3 += num9;
				num4 = Math.Min(num4, num10);
				num5 = Math.Max(num5, num10);
				num6 += num10;
				if (num10 > dr.MaxPositiveDeltaPx)
				{
					dr.MaxPositiveDeltaPx = num10;
				}
				if (num11 > dr.MaxPositiveDeltaValue)
				{
					dr.MaxPositiveDeltaValue = num11;
				}
				dr.Points.Add(new EdgePointResult
				{
					DetectIndex = dr.Index,
					DetectName = dr.Name,
					Index = i,
					Point = pointF,
					SignedDistanceRawPx = num8,
					SignedDistancePx = num9,
					DeltaPx = num10,
					SignedDistanceValue = signedDistanceValue,
					DeltaValue = num11,
					IsBurr = flag4,
					IsDent = flag5,
					NgReasons = pointReasons
				});
			}
			dr.SignedMin = num;
			dr.SignedMax = num2;
			dr.SignedMean = num3 / (double)dr.FittedLine.MeasurePoints.Count;
			dr.DeltaMin = num4;
			dr.DeltaMax = num5;
			dr.DeltaMean = num6 / (double)dr.FittedLine.MeasurePoints.Count;
			bool flag6 = (flag && dr.BurrCount > 0) || (flag2 && dr.DentCount > 0);
			dr.Success = !flag6;
			object obj;
			switch (mode)
			{
			default:
				obj = "毛刺+凹陷";
				break;
			case DefectDetectMode.DentOnly:
				obj = "凹陷";
				break;
			case DefectDetectMode.BurrOnly:
				obj = "毛刺";
				break;
			}
			string text = (string)obj;
			string text2 = "自拟合线局部判定";
			string text3 = (double.IsNaN(dr.AngleDeltaDeg) ? "角差=N/A" : $"角差={dr.AngleDeltaDeg:F3}°");
			string text4 = (flag3 ? $"外部允差(mm)：毛刺={dr.ExternalBurrTolerance:F4} 凹陷={dr.ExternalDentTolerance:F4} | 解析度X={dr.PixelResolutionX:F6}mm/px | 解析度Y={dr.PixelResolutionY:F6}mm/px | 最大毛刺={dr.MaxPositiveDeltaValue:F4}mm" : $"本地毛刺允差={dr.BurrTolerancePx:F2}px | 本地凹陷允差={dr.DentTolerancePx:F2}px | 最大毛刺={dr.MaxPositiveDeltaPx:F2}px");
			dr.Message = (dr.Success ? $"OK | {text2} | 模式={text} | {text4} | {text3} | 局部Δ(min/max/mean)={dr.DeltaMin:F2}/{dr.DeltaMax:F2}/{dr.DeltaMean:F2}px" : $"NG | {text2} | 模式={text} | {text4} | 毛刺={dr.BurrCount} 凹陷={dr.DentCount} | {text3} | 局部Δ(min/max/mean)={dr.DeltaMin:F2}/{dr.DeltaMax:F2}/{dr.DeltaMean:F2}px");
		}

		/// <summary>
		/// 计算整体距离，并按标准距离判定超边或漏铜。
		/// </summary>
		/// <param name="dr">当前检测 ROI 结果。</param>
		/// <param name="referenceLine">整体距离使用的参考线。</param>
		/// <param name="measurePoint">被测点。</param>
		private static void EvaluateOverallDistance(DetectRoiInspectResult dr, EdgeLineFit referenceLine, PointF measurePoint)
		{
			EvaluateOverallDistance(dr, referenceLine, measurePoint, null);
		}

		/// <summary>
		/// 使用检测 ROI 内的实际边缘测量点计算线线整体距离，取中位距离降低局部毛刺/凹陷对整体超边漏铜的影响。
		/// </summary>
		/// <param name="dr">当前检测 ROI 结果。</param>
		/// <param name="referenceLine">整体距离使用的参考线。</param>
		private static void EvaluateOverallDistanceFromMeasurePoints(DetectRoiInspectResult dr, EdgeLineFit referenceLine)
		{
			if (dr == null || dr.FittedLine == null || dr.FittedLine.MeasurePoints == null || dr.FittedLine.MeasurePoints.Count == 0)
			{
				return;
			}
			if (referenceLine == null || !referenceLine.Success)
			{
				return;
			}
			SignedLine signedLine = SignedLine.FromLine(referenceLine.P1, referenceLine.P2);
			List<PointF> points = dr.FittedLine.MeasurePoints;
			List<Tuple<double, PointF>> samples = new List<Tuple<double, PointF>>(points.Count);
			foreach (PointF point in points)
			{
				samples.Add(Tuple.Create(Math.Abs(signedLine.SignedDistance(point)), point));
			}
			samples.Sort((a, b) => a.Item1.CompareTo(b.Item1));
			int mid = samples.Count / 2;
			double distancePx;
			PointF measurePoint;
			if (samples.Count % 2 == 0 && samples.Count > 1)
			{
				Tuple<double, PointF> a = samples[mid - 1];
				Tuple<double, PointF> b = samples[mid];
				distancePx = (a.Item1 + b.Item1) * 0.5;
				measurePoint = new PointF((a.Item2.X + b.Item2.X) * 0.5f, (a.Item2.Y + b.Item2.Y) * 0.5f);
			}
			else
			{
				distancePx = samples[mid].Item1;
				measurePoint = samples[mid].Item2;
			}
			EvaluateOverallDistance(dr, referenceLine, measurePoint, distancePx);
		}

		/// <summary>写入整体距离结果并判定超边/漏铜。</summary>
		/// <param name="dr">当前检测 ROI 结果。</param>
		/// <param name="referenceLine">整体距离使用的参考线。</param>
		/// <param name="measurePoint">用于显示垂线的代表测量点。</param>
		/// <param name="measuredDistancePx">已计算出的整体距离；为空时使用代表点距离。</param>
		private static void EvaluateOverallDistance(DetectRoiInspectResult dr, EdgeLineFit referenceLine, PointF measurePoint, double? measuredDistancePx)
		{
			if (dr == null)
			{
				return;
			}
			if (referenceLine == null || !referenceLine.Success)
			{
				return;
			}
			SignedLine signedLine = SignedLine.FromLine(referenceLine.P1, referenceLine.P2);
			PointF footPoint = ProjectPointToLine(measurePoint, referenceLine.P1, referenceLine.P2);
			double signedDistance = signedLine.SignedDistance(measurePoint);
			double distancePx = measuredDistancePx.HasValue ? measuredDistancePx.Value : Math.Abs(signedDistance);
			double physicalDistancePerPixel = GetPhysicalDistancePerPixel(signedLine, dr.PixelResolutionX, dr.PixelResolutionY);
			double distanceValue = distancePx * physicalDistancePerPixel;
			double deltaPx = distancePx - dr.NominalDistancePx;
			double deltaValue = deltaPx * physicalDistancePerPixel;
			bool useExternalTolerance = dr.UseExternalBurrTolerance && dr.ExternalBurrTolerance >= 0.0 && dr.PixelResolutionX > 0.0 && dr.PixelResolutionY > 0.0;
			bool overEdge = useExternalTolerance ? deltaValue > dr.ExternalOverEdgeTolerance : deltaPx > dr.BurrTolerancePx;
			bool copperLeak = useExternalTolerance ? deltaValue < 0.0 - dr.ExternalCopperLeakTolerance : deltaPx < 0.0 - dr.DentTolerancePx;
			dr.OverallMeasurePoint = measurePoint;
			dr.OverallFootPoint = footPoint;
			dr.OverallReferenceLine = CopyLine(referenceLine);
			dr.ReferenceFootPoint = footPoint;
			dr.HasOverallDistance = true;
			dr.HasPointToLineDistance = true;
			dr.OverallDistancePx = distancePx;
			dr.OverallDistanceValue = distanceValue;
			dr.OverallDeltaPx = deltaPx;
			dr.OverallDeltaValue = deltaValue;
			dr.IsOverEdge = overEdge;
			dr.IsCopperLeak = copperLeak;
			if (overEdge)
			{
				dr.NgReasons |= NgReason.OverEdge;
			}
			if (copperLeak)
			{
				dr.NgReasons |= NgReason.CopperLeak;
			}
			dr.MaxPositiveDeltaPx = Math.Max(dr.MaxPositiveDeltaPx, deltaPx);
			dr.MaxPositiveDeltaValue = Math.Max(dr.MaxPositiveDeltaValue, deltaValue);
			if (overEdge || copperLeak)
			{
				dr.Success = false;
			}
			string text = useExternalTolerance ? $"外部允差(mm)：超边={dr.ExternalOverEdgeTolerance:F4} | 漏铜={dr.ExternalCopperLeakTolerance:F4}" : $"毛刺允差={dr.BurrTolerancePx:F2}px | 凹陷允差={dr.DentTolerancePx:F2}px";
			double nominalValue = dr.NominalDistancePx * physicalDistancePerPixel;
			string suffix = $" | 整体距离={distanceValue:F4}mm 标准={nominalValue:F4}mm Δ={deltaValue:+0.0000;-0.0000;0.0000}mm | 像素距离={distancePx:F2}px Δ={deltaPx:+0.00;-0.00;0.00}px | {text}";
			if (overEdge)
			{
				dr.Message += " | 超边NG" + suffix;
			}
			else if (copperLeak)
			{
				dr.Message += " | 漏铜NG" + suffix;
			}
			else
			{
				dr.Message += " | 整体距离OK" + suffix;
			}
		}

		/// <summary>获取检测模式显示文本。</summary>
		/// <param name="mode">检测模式。</param>
		/// <returns>模式文本。</returns>
		private static string GetDetectModeText(DefectDetectMode mode)
		{
			switch (mode)
			{
			case DefectDetectMode.BurrOnly:
				return "毛刺";
			case DefectDetectMode.DentOnly:
				return "凹陷";
			case DefectDetectMode.PointToCutEdgeDistance:
				return "点到切割边距离";
			default:
				return "毛刺+凹陷";
			}
		}

		/// <summary>计算检测线相对角度参考的夹角。</summary>
		/// <param name="dr">当前检测 ROI 结果。</param>
		/// <param name="baseLineMap">线基准拟合结果表。</param>
		/// <param name="circleBaseLineMap">圆基准拟合结果表。</param>
		private static void ComputeAngleReference(DetectRoiInspectResult dr, Dictionary<int, EdgeLineFit> baseLineMap, Dictionary<int, EdgeLineFit> circleBaseLineMap)
		{
			if (dr == null)
			{
				return;
			}
			double? num = null;
			string angleReferenceText;
			switch (dr.AngleReference)
			{
			case DetectAngleReferenceMode.Horizontal:
				num = 0.0;
				angleReferenceText = "水平";
				break;
			case DetectAngleReferenceMode.Vertical:
				num = Math.PI / 2.0;
				angleReferenceText = "竖直";
				break;
			default:
			{
				EdgeLineFit value2;
				if (dr.ReferenceBaseKind == ReferenceBaseKind.CirclePair)
				{
					if (circleBaseLineMap != null && circleBaseLineMap.TryGetValue(dr.CircleBaseRoiIndex, out var value) && value != null && value.Success)
					{
						num = value.AngleRad;
						angleReferenceText = $"平行于圆基准{dr.CircleBaseRoiIndex + 1}";
					}
					else
					{
						angleReferenceText = $"平行于圆基准{dr.CircleBaseRoiIndex + 1}(不可用)";
					}
				}
				else if (baseLineMap != null && baseLineMap.TryGetValue(dr.BaseRoiIndex, out value2) && value2 != null && value2.Success)
				{
					num = value2.AngleRad;
					angleReferenceText = $"平行于基准{dr.BaseRoiIndex + 1}";
				}
				else
				{
					angleReferenceText = $"平行于基准{dr.BaseRoiIndex + 1}(不可用)";
				}
				break;
			}
			}
			dr.AngleReferenceText = angleReferenceText;
			if (!num.HasValue)
			{
				dr.AngleDeltaRad = double.NaN;
				dr.AngleDeltaDeg = double.NaN;
			}
			else
			{
				dr.RefAngleRad = num.Value;
				dr.AngleDeltaRad = NormalizeLineAngleDelta(dr.FittedAngleRad - dr.RefAngleRad);
				dr.AngleDeltaDeg = dr.AngleDeltaRad * 180.0 / Math.PI;
			}
		}

		/// <summary>将线方向角差归一化到等价的最小夹角。</summary>
		/// <param name="delta">原始角差，单位为弧度。</param>
		/// <returns>归一化后的角差，单位为弧度。</returns>
		private static double NormalizeLineAngleDelta(double delta)
		{
			while (delta > Math.PI)
			{
				delta -= Math.PI * 2.0;
			}
			while (delta < -Math.PI)
			{
				delta += Math.PI * 2.0;
			}
			if (delta > Math.PI / 2.0)
			{
				delta -= Math.PI;
			}
			if (delta < -Math.PI / 2.0)
			{
				delta += Math.PI;
			}
			return delta;
		}

		/// <summary>确定 ROI 箭头方向与有符号线法向的一致性。</summary>
		/// <param name="rr">检测 ROI。</param>
		/// <param name="line">有符号判定线。</param>
		/// <returns>方向一致返回 1，否则返回 -1。</returns>
		private static double ResolvePositiveDirectionScale(RotRectF rr, SignedLine line)
		{
			double num = 0.0 - Math.Sin(rr.AngleRad);
			double num2 = Math.Cos(rr.AngleRad);
			double num3 = num * line.NCol + num2 * line.NRow;
			return (num3 >= 0.0) ? 1.0 : (-1.0);
		}

		/// <summary>计算沿指定法向的单像素物理尺寸。</summary>
		/// <param name="line">有符号线。</param>
		/// <param name="pixelResolutionX">X 方向像素分辨率。</param>
		/// <param name="pixelResolutionY">Y 方向像素分辨率。</param>
		/// <returns>沿线法向的单像素物理尺寸。</returns>
		private static double GetPhysicalDistancePerPixel(SignedLine line, double pixelResolutionX, double pixelResolutionY)
		{
			if (line == null)
			{
				return 1.0;
			}
			if (pixelResolutionX <= 0.0 && pixelResolutionY > 0.0)
			{
				pixelResolutionX = pixelResolutionY;
			}
			if (pixelResolutionY <= 0.0 && pixelResolutionX > 0.0)
			{
				pixelResolutionY = pixelResolutionX;
			}
			if (pixelResolutionX <= 0.0)
			{
				pixelResolutionX = 1.0;
			}
			if (pixelResolutionY <= 0.0)
			{
				pixelResolutionY = 1.0;
			}
			double num = line.NCol * pixelResolutionX;
			double num2 = line.NRow * pixelResolutionY;
			double num3 = Math.Sqrt(num * num + num2 * num2);
			return (num3 > 0.0) ? num3 : ((pixelResolutionX + pixelResolutionY) * 0.5);
		}

		/// <summary>将点投影到指定直线上。</summary>
		/// <param name="p">待投影点。</param>
		/// <param name="a">直线上的第一个点。</param>
		/// <param name="b">直线上的第二个点。</param>
		/// <returns>垂足坐标。</returns>
		private static PointF ProjectPointToLine(PointF p, PointF a, PointF b)
		{
			double num = b.X - a.X;
			double num2 = b.Y - a.Y;
			double num3 = num * num + num2 * num2;
			if (num3 <= 1E-09)
			{
				return a;
			}
			double num4 = ((p.X - a.X) * num + (p.Y - a.Y) * num2) / num3;
			return new PointF((float)(a.X + num4 * num), (float)(a.Y + num4 * num2));
		}

		/// <summary>导出 HALCON 形状模板为字节数组。</summary>
		/// <param name="modelId">HALCON 模板句柄。</param>
		/// <returns>序列化模板字节。</returns>
		private static byte[] ExportShapeModelToBytes(HTuple modelId)
		{
			if (modelId == null || modelId.Length == 0)
			{
				return null;
			}
			HTuple serializedItemHandle = null;
			try
			{
				HOperatorSet.SerializeShapeModel(modelId, out serializedItemHandle);
				HOperatorSet.GetSerializedItemPtr(serializedItemHandle, out var pointer, out var size);
				int i = size[0].I;
				if (i <= 0)
				{
					return null;
				}
				byte[] array = new byte[i];
				Marshal.Copy((IntPtr)pointer[0].L, array, 0, i);
				return array;
			}
			finally
			{
				if (serializedItemHandle != null && serializedItemHandle.Length > 0)
				{
					try
					{
						HOperatorSet.ClearSerializedItem(serializedItemHandle);
					}
					catch
					{
					}
				}
			}
		}

		/// <summary>从字节数组导入 HALCON 形状模板。</summary>
		/// <param name="bytes">序列化模板字节。</param>
		/// <returns>HALCON 模板句柄。</returns>
		private static HTuple ImportShapeModelFromBytes(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
			{
				throw new InvalidOperationException("模板字节数据为空。");
			}
			GCHandle gCHandle = default(GCHandle);
			HTuple serializedItemHandle = null;
			try
			{
				gCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
				HOperatorSet.CreateSerializedItemPtr(gCHandle.AddrOfPinnedObject(), bytes.Length, "true", out serializedItemHandle);
				HOperatorSet.DeserializeShapeModel(serializedItemHandle, out var modelID);
				return modelID;
			}
			finally
			{
				if (serializedItemHandle != null && serializedItemHandle.Length > 0)
				{
					try
					{
						HOperatorSet.ClearSerializedItem(serializedItemHandle);
					}
					catch
					{
					}
				}
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
			}
		}

		/// <summary>生成 HALCON 旋转矩形区域。</summary>
		/// <param name="rr">旋转矩形 ROI。</param>
		/// <param name="rect2">输出的 HALCON 区域对象。</param>
		private static void GenRectangle2(RotRectF rr, out HObject rect2)
		{
			HOperatorSet.GenRectangle2(out rect2, rr.Center.Y, rr.Center.X, rr.AngleRad, rr.HalfLen1, rr.HalfLen2);
		}

		/// <summary>使用仿射矩阵变换旋转矩形 ROI。</summary>
		/// <param name="rr">原始旋转矩形 ROI。</param>
		/// <param name="hom">HALCON 仿射矩阵。</param>
		/// <returns>变换后的旋转矩形 ROI。</returns>
		private static RotRectF TransformRotRect(RotRectF rr, HTuple hom)
		{
			if (rr.IsEmpty)
			{
				return rr;
			}
			HOperatorSet.AffineTransPoint2d(hom, rr.Center.Y, rr.Center.X, out var qx, out var qy);
			double num = (double)rr.Center.Y + Math.Sin(rr.AngleRad);
			double num2 = (double)rr.Center.X + Math.Cos(rr.AngleRad);
			HOperatorSet.AffineTransPoint2d(hom, num, num2, out var qx2, out var qy2);
			double num3 = Math.Atan2(qx2.D - qx.D, qy2.D - qy.D);
			return new RotRectF(new PointF((float)qy.D, (float)qx.D), RotRectF.NormalizeAngle((float)num3), rr.HalfLen1, rr.HalfLen2);
		}

		/// <summary>使用仿射矩阵变换圆 ROI 的中心。</summary>
		/// <param name="circle">原始圆 ROI。</param>
		/// <param name="hom">HALCON 仿射矩阵。</param>
		/// <returns>变换后的圆 ROI。</returns>
		private static CircleRoiF TransformCircleRoi(CircleRoiF circle, HTuple hom)
		{
			if (circle.IsEmpty)
			{
				return circle;
			}
			HOperatorSet.AffineTransPoint2d(hom, circle.Center.Y, circle.Center.X, out var qx, out var qy);
			return new CircleRoiF(new PointF((float)qy.D, (float)qx.D), circle.Radius);
		}

		/// <summary>将旋转矩形长轴转换为线段。</summary>
		/// <param name="rr">旋转矩形 ROI。</param>
		/// <param name="p1">线段起点。</param>
		/// <param name="p2">线段终点。</param>
		private static void ResolveRotRectToLine(RotRectF rr, out PtD p1, out PtD p2)
		{
			double num = Math.Cos(rr.AngleRad) * (double)rr.HalfLen1;
			double num2 = Math.Sin(rr.AngleRad) * (double)rr.HalfLen1;
			p1 = new PtD((double)rr.Center.X - num, (double)rr.Center.Y - num2);
			p2 = new PtD((double)rr.Center.X + num, (double)rr.Center.Y + num2);
		}

		/// <summary>在旋转矩形 ROI 内执行卡尺找边并拟合直线。</summary>
		/// <param name="gray">灰度图像。</param>
		/// <param name="width">图像宽度。</param>
		/// <param name="height">图像高度。</param>
		/// <param name="rr">检测或基准 ROI。</param>
		/// <param name="cp">卡尺参数。</param>
		/// <returns>直线拟合结果。</returns>
		private static EdgeLineFit FitLineInRotRoi(HImage gray, int width, int height, RotRectF rr, CaliperParameters cp)
		{
			EdgeLineFit edgeLineFit = new EdgeLineFit
			{
				Success = false
			};
			HObject rect = null;
			HObject imageReduced = null;
			HObject contours = null;
			HTuple metrologyHandle = new HTuple();
			try
			{
				CaliperParameters caliperParameters = cp?.DeepClone() ?? new CaliperParameters();
				NormalizeCaliper(caliperParameters);
				GenRectangle2(rr, out rect);
				HOperatorSet.ReduceDomain(gray, rect, out imageReduced);
				HOperatorSet.CreateMetrologyModel(out metrologyHandle);
				HOperatorSet.SetMetrologyModelImageSize(metrologyHandle, width, height);
				ResolveRotRectToLine(rr, out var p, out var p2);
				HOperatorSet.AddMetrologyObjectLineMeasure(metrologyHandle, p.Y, p.X, p2.Y, p2.X, Math.Max(1.0, caliperParameters.MeasureLength), Math.Max(1.0, caliperParameters.MeasureWidth), caliperParameters.Sigma, caliperParameters.Threshold, new HTuple(), new HTuple(), out var index);
				SetMetrologyParams(metrologyHandle, index, caliperParameters);
				HOperatorSet.ApplyMetrologyModel(imageReduced, metrologyHandle);
				HOperatorSet.GetMetrologyObjectMeasures(out contours, metrologyHandle, index, "all", out var row, out var column);
				if (row == null || row.Length < 2)
				{
					edgeLineFit.Message = "卡尺测量点不足";
					return edgeLineFit;
				}
				for (int i = 0; i < row.Length; i++)
				{
					edgeLineFit.MeasurePoints.Add(new PointF((float)column[i].D, (float)row[i].D));
				}
				if (TryFitLineStable(row, column, width, height, out var r, out var c, out var r2, out var c2, out var angle, out var error))
				{
					edgeLineFit.P1 = new PointF((float)c, (float)r);
					edgeLineFit.P2 = new PointF((float)c2, (float)r2);
					edgeLineFit.AngleRad = angle;
					edgeLineFit.Success = true;
				}
				else
				{
					edgeLineFit.Message = (string.IsNullOrWhiteSpace(error) ? "直线拟合失败" : error);
				}
				return edgeLineFit;
			}
			catch (Exception ex)
			{
				edgeLineFit.Message = ex.GetType().Name + ": " + ex.Message;
				return edgeLineFit;
			}
			finally
			{
				contours?.Dispose();
				if (metrologyHandle != null && metrologyHandle.Length > 0)
				{
					try
					{
						HOperatorSet.ClearMetrologyModel(metrologyHandle);
					}
					catch
					{
					}
				}
				imageReduced?.Dispose();
				rect?.Dispose();
			}
		}

		/// <summary>在圆 ROI 周向采样并拟合圆。</summary>
		/// <param name="gray">灰度图像。</param>
		/// <param name="width">图像宽度。</param>
		/// <param name="height">图像高度。</param>
		/// <param name="roi">圆 ROI。</param>
		/// <param name="cp">圆卡尺参数。</param>
		/// <returns>圆拟合结果。</returns>
		private static EdgeCircleFit FitCircleInRoi(HImage gray, int width, int height, CircleRoiF roi, CaliperParameters cp)
		{
			EdgeCircleFit edgeCircleFit = new EdgeCircleFit
			{
				Success = false
			};
			try
			{
				CaliperParameters caliperParameters = cp?.DeepClone() ?? EdgeInspectJob.CreateDefaultCircleCaliper();
				NormalizeCaliper(caliperParameters);
				int num = Math.Max(8, caliperParameters.NumMeasures);
				double outward = Math.Max(0.0, caliperParameters.SearchOutward);
				double inwardLength = Math.Max(1.0, caliperParameters.MeasureLength);
				for (int i = 0; i < num; i++)
				{
					double angle = Math.PI * 2.0 * (double)i / (double)num;
					double measuredRow;
					double measuredCol;
					if (TryMeasureRadialCircleEdge(gray, width, height, roi, caliperParameters, angle, outward, inwardLength, out measuredRow, out measuredCol))
					{
						edgeCircleFit.MeasurePoints.Add(new PointF((float)measuredCol, (float)measuredRow));
					}
				}
				if (edgeCircleFit.MeasurePoints.Count < 3)
				{
					edgeCircleFit.Message = "圆测量点不足（请检查圆ROI、圆外扩、阈值/极性）";
					return edgeCircleFit;
				}
				HTuple fitRows = new HTuple(edgeCircleFit.MeasurePoints.Select((PointF p) => (double)p.Y).ToArray());
				HTuple fitCols = new HTuple(edgeCircleFit.MeasurePoints.Select((PointF p) => (double)p.X).ToArray());
				double row2;
				double col;
				double radius;
				string error;
				if (TryFitCircleStable(fitRows, fitCols, out row2, out col, out radius, out error))
				{
					edgeCircleFit.Center = new PointF((float)col, (float)row2);
					edgeCircleFit.Radius = radius;
					edgeCircleFit.Success = true;
				}
				else
				{
					edgeCircleFit.Message = (string.IsNullOrWhiteSpace(error) ? "圆拟合失败" : error);
				}
				return edgeCircleFit;
			}
			catch (Exception ex)
			{
				edgeCircleFit.Message = ex.GetType().Name + ": " + ex.Message;
				return edgeCircleFit;
			}
			finally
			{
			}
		}

		/// <summary>沿圆的一个径向方向测量边缘点。</summary>
		/// <param name="gray">灰度图像。</param>
		/// <param name="width">图像宽度。</param>
		/// <param name="height">图像高度。</param>
		/// <param name="roi">圆 ROI。</param>
		/// <param name="cp">圆卡尺参数。</param>
		/// <param name="angle">径向角度。</param>
		/// <param name="outward">向外搜索长度。</param>
		/// <param name="inwardLength">向内搜索长度。</param>
		/// <param name="row">输出边缘行坐标。</param>
		/// <param name="col">输出边缘列坐标。</param>
		/// <returns>是否测量到边缘点。</returns>
		private static bool TryMeasureRadialCircleEdge(HImage gray, int width, int height, CircleRoiF roi, CaliperParameters cp, double angle, double outward, double inwardLength, out double row, out double col)
		{
			row = (col = 0.0);
			HTuple measureHandle = new HTuple();
			double dirCol = Math.Cos(angle);
			double dirRow = Math.Sin(angle);
			double startRadius = Math.Max(1.0, (double)roi.Radius + outward);
			double minRadius = Math.Max(0.0, (double)roi.Radius - inwardLength);
			double searchLength = Math.Max(1.0, startRadius - minRadius);
			double midRadius = startRadius - searchLength * 0.5;
			double midRow = (double)roi.Center.Y + dirRow * midRadius;
			double midCol = (double)roi.Center.X + dirCol * midRadius;
			double phi = Math.Atan2(dirRow, dirCol);
			try
			{
				HOperatorSet.GenMeasureRectangle2(midRow, midCol, phi, searchLength * 0.5, Math.Max(0.5, cp.MeasureWidth), width, height, string.IsNullOrWhiteSpace(cp.MeasureInterpolation) ? "bicubic" : cp.MeasureInterpolation, out measureHandle);
				HTuple rowEdge;
				HTuple columnEdge;
				HTuple amplitude;
				HTuple distance;
				HOperatorSet.MeasurePos(gray, measureHandle, cp.Sigma, cp.Threshold, string.IsNullOrWhiteSpace(cp.Transition) ? "all" : cp.Transition, "all", out rowEdge, out columnEdge, out amplitude, out distance);
				return TryPickOutermostCircleEdge(roi, rowEdge, columnEdge, minRadius, startRadius, out row, out col);
			}
			catch
			{
				return false;
			}
			finally
			{
				if (measureHandle != null && measureHandle.Length > 0)
				{
					try
					{
						HOperatorSet.CloseMeasure(measureHandle);
					}
					catch
					{
					}
				}
			}
		}

		/// <summary>从径向测量候选点中选取最外侧圆边缘点。</summary>
		/// <param name="roi">圆 ROI。</param>
		/// <param name="edgeRows">候选点行坐标集合。</param>
		/// <param name="edgeCols">候选点列坐标集合。</param>
		/// <param name="minRadius">允许的最小半径。</param>
		/// <param name="maxRadius">允许的最大半径。</param>
		/// <param name="row">输出边缘行坐标。</param>
		/// <param name="col">输出边缘列坐标。</param>
		/// <returns>是否选出有效点。</returns>
		private static bool TryPickOutermostCircleEdge(CircleRoiF roi, HTuple edgeRows, HTuple edgeCols, double minRadius, double maxRadius, out double row, out double col)
		{
			row = (col = 0.0);
			if (edgeRows == null || edgeCols == null || edgeRows.Length <= 0 || edgeCols.Length <= 0)
			{
				return false;
			}
			double bestDist = double.NegativeInfinity;
			int best = -1;
			for (int i = 0; i < edgeRows.Length && i < edgeCols.Length; i++)
			{
				double dx = edgeCols[i].D - (double)roi.Center.X;
				double dy = edgeRows[i].D - (double)roi.Center.Y;
				double dist = Math.Sqrt(dx * dx + dy * dy);
				if (dist >= minRadius - 2.0 && dist <= maxRadius + 2.0 && dist > bestDist)
				{
					bestDist = dist;
					best = i;
				}
			}
			if (best < 0)
			{
				for (int j = 0; j < edgeRows.Length && j < edgeCols.Length; j++)
				{
					double dx2 = edgeCols[j].D - (double)roi.Center.X;
					double dy2 = edgeRows[j].D - (double)roi.Center.Y;
					double dist2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
					if (dist2 > bestDist)
					{
						bestDist = dist2;
						best = j;
					}
				}
			}
			if (best < 0)
			{
				return false;
			}
			row = edgeRows[best].D;
			col = edgeCols[best].D;
			return true;
		}

		/// <summary>使用 HALCON 对点集进行稳定圆拟合。</summary>
		/// <param name="rows">拟合点行坐标集合。</param>
		/// <param name="cols">拟合点列坐标集合。</param>
		/// <param name="row">输出圆心行坐标。</param>
		/// <param name="col">输出圆心列坐标。</param>
		/// <param name="radius">输出圆半径。</param>
		/// <param name="error">输出失败原因。</param>
		/// <returns>圆拟合是否成功。</returns>
		private static bool TryFitCircleStable(HTuple rows, HTuple cols, out double row, out double col, out double radius, out string error)
		{
			row = (col = (radius = 0.0));
			error = "";
			if (rows == null || cols == null || rows.Length < 3 || cols.Length < 3)
			{
				error = "拟合点数量不足";
				return false;
			}
			HObject contour = null;
			try
			{
				HOperatorSet.GenContourPolygonXld(out contour, rows, cols);
				HOperatorSet.FitCircleContourXld(contour, "algebraic", -1, 0, 0, 3, 2, out var row2, out var column, out var radius2, out var _, out var _, out var _);
				if (row2.Length <= 0 || radius2.Length <= 0 || radius2[0].D <= 0.0)
				{
					error = "Halcon 未返回有效圆";
					return false;
				}
				row = row2[0].D;
				col = column[0].D;
				radius = radius2[0].D;
				return true;
			}
			catch (Exception ex)
			{
				error = ex.GetType().Name + ": " + ex.Message;
				return false;
			}
			finally
			{
				contour?.Dispose();
			}
		}

		/// <summary>筛选靠近圆 ROI 内侧的拟合点。</summary>
		/// <param name="roi">圆 ROI。</param>
		/// <param name="rows">原始点行坐标集合。</param>
		/// <param name="cols">原始点列坐标集合。</param>
		/// <param name="fitRows">输出筛选后的行坐标集合。</param>
		/// <param name="fitCols">输出筛选后的列坐标集合。</param>
		private static void SelectInnerCirclePoints(CircleRoiF roi, HTuple rows, HTuple cols, out HTuple fitRows, out HTuple fitCols)
		{
			List<double> list = new List<double>();
			List<double> list2 = new List<double>();
			double num = Math.Max(1.0, roi.Radius);
			double num2 = Math.Max(1.0, num * 0.03);
			for (int i = 0; i < rows.Length && i < cols.Length; i++)
			{
				double num3 = cols[i].D - (double)roi.Center.X;
				double num4 = rows[i].D - (double)roi.Center.Y;
				double num5 = Math.Sqrt(num3 * num3 + num4 * num4);
				if (num5 <= num + num2)
				{
					list.Add(rows[i].D);
					list2.Add(cols[i].D);
				}
			}
			if (list.Count >= 3)
			{
				fitRows = new HTuple(list.ToArray());
				fitCols = new HTuple(list2.ToArray());
			}
			else
			{
				fitRows = rows;
				fitCols = cols;
			}
		}

		/// <summary>设置圆测量模型的卡尺参数。</summary>
		/// <param name="handle">HALCON 测量模型句柄。</param>
		/// <param name="idx">测量对象索引。</param>
		/// <param name="cp">卡尺参数。</param>
		private static void SetCircleMetrologyParams(HTuple handle, HTuple idx, CaliperParameters cp)
		{
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "num_measures", cp.NumMeasures);
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "measure_interpolation", cp.MeasureInterpolation);
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "measure_select", "all");
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "measure_transition", string.IsNullOrWhiteSpace(cp.Transition) ? "all" : cp.Transition);
		}

		/// <summary>根据两个圆心生成贯穿图像的基准线。</summary>
		/// <param name="c1">第一个圆心。</param>
		/// <param name="c2">第二个圆心。</param>
		/// <param name="width">图像宽度。</param>
		/// <param name="height">图像高度。</param>
		/// <returns>由两个圆心确定的直线结果。</returns>
		private static EdgeLineFit LineFromCircleCenters(PointF c1, PointF c2, int width, int height)
		{
			EdgeLineFit edgeLineFit = new EdgeLineFit
			{
				Success = false
			};
			double num = c2.X - c1.X;
			double num2 = c2.Y - c1.Y;
			double num3 = Math.Sqrt(num * num + num2 * num2);
			if (num3 < 1E-09)
			{
				edgeLineFit.Message = "两个圆心重合";
				return edgeLineFit;
			}
			double num4 = num / num3;
			double num5 = num2 / num3;
			if (ClipInfiniteLineToImage(c1.Y, c1.X, num5, num4, width, height, out var rA, out var cA, out var rB, out var cB))
			{
				edgeLineFit.P1 = new PointF((float)cA, (float)rA);
				edgeLineFit.P2 = new PointF((float)cB, (float)rB);
			}
			else
			{
				edgeLineFit.P1 = c1;
				edgeLineFit.P2 = c2;
			}
			edgeLineFit.AngleRad = Math.Atan2(num5, num4);
			edgeLineFit.Success = true;
			return edgeLineFit;
		}

		/// <summary>设置直线测量模型的卡尺参数。</summary>
		/// <param name="handle">HALCON 测量模型句柄。</param>
		/// <param name="idx">测量对象索引。</param>
		/// <param name="cp">卡尺参数。</param>
		private static void SetMetrologyParams(HTuple handle, HTuple idx, CaliperParameters cp)
		{
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "num_measures", cp.NumMeasures);
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "measure_interpolation", cp.MeasureInterpolation);
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "measure_select", string.IsNullOrWhiteSpace(cp.MeasureSelect) ? "first" : cp.MeasureSelect);
			HOperatorSet.SetMetrologyObjectParam(handle, idx, "measure_transition", string.IsNullOrWhiteSpace(cp.Transition) ? "all" : cp.Transition);
		}

		/// <summary>修正卡尺参数中的无效默认值。</summary>
		/// <param name="cp">待修正的卡尺参数。</param>
		private static void NormalizeCaliper(CaliperParameters cp)
		{
			if (cp != null)
			{
				if (cp.NumMeasures < 1)
				{
					cp.NumMeasures = 1;
				}
				if (cp.MeasureLength < 1.0)
				{
					cp.MeasureLength = 1.0;
				}
				if (cp.MeasureWidth < 1.0)
				{
					cp.MeasureWidth = 1.0;
				}
				if (cp.Sigma <= 0.0)
				{
					cp.Sigma = 1.0;
				}
			if (cp.Threshold <= 0.0)
			{
				cp.Threshold = 1.0;
			}
			if (cp.SearchOutward < 0.0)
			{
				cp.SearchOutward = 0.0;
			}
			if (string.IsNullOrWhiteSpace(cp.MeasureInterpolation))
				{
					cp.MeasureInterpolation = "bicubic";
				}
				if (string.IsNullOrWhiteSpace(cp.MeasureSelect))
				{
					cp.MeasureSelect = "first";
				}
				if (string.IsNullOrWhiteSpace(cp.Transition))
				{
					cp.Transition = "negative";
				}
			}
		}

		/// <summary>使用 HALCON 对点集进行稳定直线拟合。</summary>
		/// <param name="rows">拟合点行坐标集合。</param>
		/// <param name="cols">拟合点列坐标集合。</param>
		/// <param name="width">图像宽度。</param>
		/// <param name="height">图像高度。</param>
		/// <param name="r1">输出线段起点行坐标。</param>
		/// <param name="c1">输出线段起点列坐标。</param>
		/// <param name="r2">输出线段终点行坐标。</param>
		/// <param name="c2">输出线段终点列坐标。</param>
		/// <param name="angle">输出直线角度。</param>
		/// <param name="error">输出失败原因。</param>
		/// <returns>直线拟合是否成功。</returns>
		private static bool TryFitLineStable(HTuple rows, HTuple cols, int width, int height, out double r1, out double c1, out double r2, out double c2, out double angle, out string error)
		{
			r1 = (c1 = (r2 = (c2 = (angle = 0.0))));
			error = "";
			if (rows == null || cols == null || rows.Length < 2 || cols.Length < 2)
			{
				error = "拟合点数量不足";
				return false;
			}
			HObject contour = null;
			try
			{
				HOperatorSet.GenContourPolygonXld(out contour, rows, cols);
				HOperatorSet.FitLineContourXld(contour, "tukey", -1, 0, 5, 2, out var rowBegin, out var colBegin, out var rowEnd, out var colEnd, out var _, out var _, out var _);
				if (rowBegin.Length == 0)
				{
					error = "Halcon 未返回有效拟合线";
					return false;
				}
				double d = rowBegin[0].D;
				double d2 = colBegin[0].D;
				double d3 = rowEnd[0].D;
				double d4 = colEnd[0].D;
				double num = d3 - d;
				double num2 = d4 - d2;
				double num3 = Math.Sqrt(num * num + num2 * num2);
				if (num3 < 1E-09)
				{
					error = "拟合线长度过短";
					return false;
				}
				num /= num3;
				num2 /= num3;
				angle = Math.Atan2(num, num2);
				if (!ClipInfiniteLineToImage((d + d3) * 0.5, (d2 + d4) * 0.5, num, num2, width, height, out r1, out c1, out r2, out c2))
				{
					r1 = d;
					c1 = d2;
					r2 = d3;
					c2 = d4;
				}
				return true;
			}
			catch (Exception ex)
			{
				error = ex.GetType().Name + ": " + ex.Message;
				return false;
			}
			finally
			{
				contour?.Dispose();
			}
		}

		/// <summary>将无限长直线裁剪到图像边界内。</summary>
		/// <param name="r0">直线参考点行坐标。</param>
		/// <param name="c0">直线参考点列坐标。</param>
		/// <param name="dr">行方向单位向量分量。</param>
		/// <param name="dc">列方向单位向量分量。</param>
		/// <param name="width">图像宽度。</param>
		/// <param name="height">图像高度。</param>
		/// <param name="rA">输出裁剪端点 A 行坐标。</param>
		/// <param name="cA">输出裁剪端点 A 列坐标。</param>
		/// <param name="rB">输出裁剪端点 B 行坐标。</param>
		/// <param name="cB">输出裁剪端点 B 列坐标。</param>
		/// <returns>是否得到有效裁剪线段。</returns>
		private static bool ClipInfiniteLineToImage(double r0, double c0, double dr, double dc, int width, int height, out double rA, out double cA, out double rB, out double cB)
		{
			rA = (cA = (rB = (cB = 0.0)));
			double w = width - 1;
			double h = height - 1;
			List<Tuple<double, double>> pts = new List<Tuple<double, double>>();
			if (Math.Abs(dc) > 1E-12)
			{
				double num = (0.0 - c0) / dc;
				Add(r0 + num * dr, 0.0);
				num = (w - c0) / dc;
				Add(r0 + num * dr, w);
			}
			if (Math.Abs(dr) > 1E-12)
			{
				double num2 = (0.0 - r0) / dr;
				Add(0.0, c0 + num2 * dc);
				num2 = (h - r0) / dr;
				Add(h, c0 + num2 * dc);
			}
			if (pts.Count < 2)
			{
				return false;
			}
			double num3 = -1.0;
			int index = 0;
			int index2 = 1;
			for (int i = 0; i < pts.Count; i++)
			{
				for (int j = i + 1; j < pts.Count; j++)
				{
					double num4 = Math.Pow(pts[i].Item1 - pts[j].Item1, 2.0) + Math.Pow(pts[i].Item2 - pts[j].Item2, 2.0);
					if (num4 > num3)
					{
						num3 = num4;
						index = i;
						index2 = j;
					}
				}
			}
			rA = pts[index].Item1;
			cA = pts[index].Item2;
			rB = pts[index2].Item1;
			cB = pts[index2].Item2;
			return true;
			void Add(double num5, double num6)
			{
				if (num5 >= 0.0 && num5 <= h && num6 >= 0.0 && num6 <= w)
				{
					pts.Add(Tuple.Create(num5, num6));
				}
			}
		}

		/// <summary>复制直线拟合结果。</summary>
		/// <param name="src">源直线拟合结果。</param>
		/// <returns>复制后的直线拟合结果。</returns>
		private static EdgeLineFit CopyLine(EdgeLineFit src)
		{
			if (src == null)
			{
				return new EdgeLineFit();
			}
			EdgeLineFit edgeLineFit = new EdgeLineFit
			{
				Success = src.Success,
				P1 = src.P1,
				P2 = src.P2,
				AngleRad = src.AngleRad,
				Message = src.Message
			};
			foreach (PointF measurePoint in src.MeasurePoints)
			{
				edgeLineFit.MeasurePoints.Add(measurePoint);
			}
			return edgeLineFit;
		}

		/// <summary>将位图转换为 HALCON 灰度图。</summary>
		/// <param name="bmp">输入位图。</param>
		/// <param name="gray">输出灰度字节数组。</param>
		/// <param name="w">输出图像宽度。</param>
		/// <param name="h">输出图像高度。</param>
		/// <param name="grayHandle">输出灰度数组固定句柄。</param>
		/// <returns>HALCON 灰度图像。</returns>
		private static HImage BitmapToGrayHImage(Bitmap bmp, out byte[] gray, out int w, out int h, out GCHandle grayHandle)
		{
			w = bmp.Width;
			h = bmp.Height;
			using (Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.DrawImage(bmp, 0, 0, w, h);
				}
				Rectangle rect = new Rectangle(0, 0, w, h);
				BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				try
				{
					int stride = bitmapData.Stride;
					byte[] array = new byte[stride * h];
					Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
					gray = new byte[w * h];
					for (int i = 0; i < h; i++)
					{
						int num = i * stride;
						int num2 = i * w;
						for (int j = 0; j < w; j++)
						{
							int num3 = num + j * 3;
							gray[num2 + j] = (byte)(0.114 * (double)(int)array[num3] + 0.587 * (double)(int)array[num3 + 1] + 0.299 * (double)(int)array[num3 + 2]);
						}
					}
					grayHandle = GCHandle.Alloc(gray, GCHandleType.Pinned);
					HImage hImage = new HImage();
					hImage.GenImage1("byte", w, h, grayHandle.AddrOfPinnedObject());
					return hImage;
				}
				finally
				{
					bitmap.UnlockBits(bitmapData);
				}
			}
		}
	}
}
