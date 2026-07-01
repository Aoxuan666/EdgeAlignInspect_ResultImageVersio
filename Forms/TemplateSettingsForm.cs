using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	public partial class TemplateSettingsForm : Form
	{
		private const int RightPanelWidth = 360;

		private readonly Bitmap _bitmap;
		private readonly TemplateEdgeInspectProcessor _processor = new TemplateEdgeInspectProcessor();
		private EdgeInspectJob _job;
		private List<PointF> _featurePoints = new List<PointF>();
		private List<TemplateEraseStroke> _eraseStrokes = new List<TemplateEraseStroke>();
		private bool _eraseMode;
		private bool _erasing;
		private bool _hasEraseCursor;
		private PointF _eraseCursor;
		private bool _suppressEvents;
		private bool _busy;
		private bool _eraseOverlayDirty;
		private DateTime _lastEraseOverlayRefresh = DateTime.MinValue;

		public EdgeInspectJob Job => _job?.DeepClone();

		public TemplateSettingsForm(Bitmap bitmap, EdgeInspectJob job)
		{
			if (bitmap == null)
			{
				throw new ArgumentNullException("bitmap");
			}
			InitializeComponent();
			btnOk.DialogResult = DialogResult.None;
			_bitmap = new Bitmap(bitmap);
			_job = (job?.DeepClone() ?? new EdgeInspectJob());
			_job.Normalize();
			_job.Match.Enabled = true;
			_job.Match.UseOuterContourOnly = true;
			if (_job.TemplateRoi.IsEmpty)
			{
				_job.TemplateRoi = CreateDefaultTemplateRoi(_bitmap);
			}
			ApplyLanguageToUi();
			_featurePoints = _job.TeachData != null && _job.TeachData.FeaturePoints != null
				? new List<PointF>(_job.TeachData.FeaturePoints)
				: new List<PointF>();
			_eraseStrokes = _job.TeachData != null && _job.TeachData.EraseStrokes != null
				? _job.TeachData.EraseStrokes.Select(x => x?.DeepClone() ?? new TemplateEraseStroke()).ToList()
				: new List<TemplateEraseStroke>();
			LoadJobToControls();
			BindEvents();
			canvas.LoadBitmap(_bitmap);
			canvas.Language = _job.Language;
			canvas.AutoClearOverlaysOnRoiEdit = false;
			canvas.SetRois(_job.TemplateRoi, default(RotRectF), default(RotRectF));
			canvas.SetSelection(CanvasRoiKind.Template, 0);
			RefreshFeatureOverlay();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			FixTemplateLayout();
			canvas.FitToWindow();
			canvas.Invalidate();
			if (_featurePoints.Count == 0)
			{
				ExtractFeaturePoints(showMessage: false);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			FixTemplateLayout();
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			_processor.Dispose();
			_bitmap.Dispose();
			base.OnFormClosed(e);
		}

		private void BindEvents()
		{
			canvas.RoiChanged += delegate
			{
				if (_suppressEvents)
				{
					return;
				}
				_job.TemplateRoi = canvas.TemplateRoi;
				ClearTeachModelKeepEraseStrokes();
				ExtractFeaturePoints(showMessage: false);
			};
			canvas.ImageMouseDown += delegate(PointF p, MouseButtons button)
			{
				if (_eraseMode && button == MouseButtons.Left)
				{
					_hasEraseCursor = true;
					_eraseCursor = p;
					_erasing = true;
					EraseAt(p);
				}
			};
			canvas.ImageMouseMove += delegate(PointF p, MouseButtons button)
			{
				if (_eraseMode)
				{
					_hasEraseCursor = true;
					_eraseCursor = p;
					if (_erasing && button == MouseButtons.Left)
					{
						EraseAt(p);
					}
					else
					{
						RefreshFeatureOverlayForErase();
					}
				}
			};
			canvas.ImageMouseUp += delegate
			{
				_erasing = false;
				if (_eraseOverlayDirty)
				{
					RefreshFeatureOverlay();
					_eraseOverlayDirty = false;
				}
			};
			btnExtract.Click += delegate
			{
				ExtractFeaturePoints(showMessage: true);
			};
			btnReset.Click += delegate
			{
				_eraseStrokes.Clear();
				ExtractFeaturePoints(showMessage: true);
			};
			btnErase.Click += delegate
			{
				SetEraseMode(!_eraseMode);
			};
			btnCreateModel.Click += delegate
			{
				CreateModelAsync();
			};
			btnTestMatch.Click += delegate
			{
				TestMatchAsync();
			};
			btnOk.Click += delegate
			{
				if (TryAcceptTemplateSettings())
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			};
			numSigma.ValueChanged += delegate { ParametersChanged(); };
			numLow.ValueChanged += delegate { ParametersChanged(); };
			numHigh.ValueChanged += delegate { ParametersChanged(); };
			numMinDistance.ValueChanged += delegate { ParametersChanged(); };
			numBins.ValueChanged += delegate { ParametersChanged(); };
			numEraseRadius.ValueChanged += delegate
			{
				PullControlsToJob();
				RefreshFeatureOverlay();
			};
			chkOuterOnly.CheckedChanged += delegate { PullControlsToJob(); };
		}

		private string T(string text)
		{
			return LocalizedText.Message(text, _job?.Language ?? InspectionLanguage.Chinese);
		}

		private void ApplyLanguageToUi()
		{
			InspectionLanguage language = _job?.Language ?? InspectionLanguage.Chinese;
			ApplyLanguageToControl(this, language);
		}

		private static void ApplyLanguageToControl(Control control, InspectionLanguage language)
		{
			if (control == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(control.Text))
			{
				control.Text = LocalizedText.Ui(control.Text, language);
			}
			foreach (Control child in control.Controls)
			{
				ApplyLanguageToControl(child, language);
			}
		}

		private void FixTemplateLayout()
		{
			if (splitMain == null || splitMain.IsDisposed || splitMain.ClientSize.Width <= 0)
			{
				return;
			}
			int desiredRight = Math.Min(RightPanelWidth, Math.Max(300, splitMain.ClientSize.Width / 2));
			int distance = Math.Max(splitMain.Panel1MinSize, splitMain.ClientSize.Width - desiredRight - splitMain.SplitterWidth);
			int maxDistance = Math.Max(splitMain.Panel1MinSize, splitMain.ClientSize.Width - splitMain.Panel2MinSize - splitMain.SplitterWidth);
			splitMain.SplitterDistance = Math.Min(distance, maxDistance);

			int margin = 10;
			int panelWidth = Math.Max(300, rightPanel.ClientSize.Width - margin * 2);
			grpContour.SetBounds(margin, margin, panelWidth, 560);
			int buttonTop = Math.Max(grpContour.Bottom + 16, rightPanel.ClientSize.Height - margin - 32);
			btnCancel.SetBounds(rightPanel.ClientSize.Width - margin - 90, buttonTop, 90, 32);
			btnOk.SetBounds(btnCancel.Left - 100, buttonTop, 90, 32);
		}

		private void LoadJobToControls()
		{
			_suppressEvents = true;
			try
			{
				chkOuterOnly.Checked = _job.Match.UseOuterContourOnly;
				SetDecimal(numSigma, _job.Match.EdgeSigma);
				SetDecimal(numLow, _job.Match.EdgeLowThreshold);
				SetDecimal(numHigh, _job.Match.EdgeHighThreshold);
				SetDecimal(numMinDistance, _job.Match.FeatureMinDistancePx);
				SetDecimal(numBins, _job.Match.FeatureAngleBins);
				SetDecimal(numEraseRadius, _job.Match.EraseRadiusPx);
			}
			finally
			{
				_suppressEvents = false;
			}
		}

		private void PullControlsToJob()
		{
			if (_suppressEvents)
			{
				return;
			}
			_job.Match.Enabled = true;
			_job.Match.UseOuterContourOnly = chkOuterOnly.Checked;
			_job.Match.EdgeSigma = (double)numSigma.Value;
			_job.Match.EdgeLowThreshold = (double)numLow.Value;
			_job.Match.EdgeHighThreshold = Math.Max((double)numLow.Value + 1.0, (double)numHigh.Value);
			_job.Match.FeatureMinDistancePx = (double)numMinDistance.Value;
			_job.Match.FeatureAngleBins = (int)numBins.Value;
			_job.Match.EraseRadiusPx = (double)numEraseRadius.Value;
			_job.TemplateRoi = canvas.TemplateRoi;
			_job.Normalize();
		}

		private bool TryAcceptTemplateSettings()
		{
			if (_busy)
			{
				return false;
			}
			try
			{
				PullControlsToJob();
				_job.TemplateRoi = canvas.TemplateRoi;
				_job.Match.Enabled = true;
				if (_job.TemplateRoi.IsEmpty)
				{
					throw new InvalidOperationException("Template ROI is empty.");
				}
				bool needsModel = _job.TeachData == null ||
					!_job.TeachData.HasTemplate ||
					_job.TeachData.ModelBytes == null ||
					_job.TeachData.ModelBytes.Length == 0;
				if (needsModel)
				{
					if (GetRealFeatureCount(_featurePoints) == 0)
					{
						_featurePoints = _processor.ExtractOuterContourPoints(_bitmap, _job);
						_featurePoints = ApplyEraseStrokes(_featurePoints);
					}
					_job.TeachData = _processor.TeachFromOuterContourPoints(_bitmap, _job, _featurePoints);
				}
				_featurePoints = new List<PointF>(_job.TeachData.FeaturePoints ?? _featurePoints ?? new List<PointF>());
				_job.TeachData.FeaturePoints = new List<PointF>(_featurePoints);
				_job.TeachData.EraseStrokes = CloneEraseStrokes(_eraseStrokes);
				_job.Normalize();
				return true;
			}
				catch (Exception ex)
			{
				lblStatus.Text = ex.Message;
				MessageBox.Show(this, ex.Message, T("模板设置"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}

		private void ParametersChanged()
		{
			if (_suppressEvents)
			{
				return;
			}
			PullControlsToJob();
			ClearTeachModelKeepEraseStrokes();
			lblStatus.Text = T("参数已修改，请重新提取外轮廓点。");
		}

		private void ClearTeachModelKeepEraseStrokes()
		{
			if (_job.TeachData == null)
			{
				_job.TeachData = new TemplateTeachData();
			}
			_job.TeachData.Clear();
			_job.TeachData.EraseStrokes = CloneEraseStrokes(_eraseStrokes);
		}

		private void ExtractFeaturePoints(bool showMessage)
		{
			try
			{
				PullControlsToJob();
				_featurePoints = _processor.ExtractOuterContourPoints(_bitmap, _job);
				_featurePoints = ApplyEraseStrokes(_featurePoints);
				ClearTeachModelKeepEraseStrokes();
				RefreshFeatureOverlay();
				lblStatus.Text = _featurePoints.Count > 0 ? T("已提取外轮廓特征点。") : T("没有提取到特征点，请调整 ROI 或阈值。");
				if (showMessage && _featurePoints.Count == 0)
				{
					MessageBox.Show(this, lblStatus.Text, T("模板设置"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			catch (Exception ex)
			{
				lblStatus.Text = ex.Message;
				MessageBox.Show(this, ex.Message, T("提取失败"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void CreateModel()
		{
			try
			{
				PullControlsToJob();
				if (_featurePoints.Count == 0)
				{
					ExtractFeaturePoints(showMessage: false);
				}
				TemplateTeachData teachData = _processor.TeachFromOuterContourPoints(_bitmap, _job, _featurePoints);
				_job.TeachData = teachData;
				_job.TeachData.EraseStrokes = CloneEraseStrokes(_eraseStrokes);
				_featurePoints = new List<PointF>(teachData.FeaturePoints ?? new List<PointF>());
				RefreshFeatureOverlay();
				lblStatus.Text = T("模型创建完成，后续匹配将使用外轮廓。");
			}
			catch (Exception ex)
			{
				lblStatus.Text = ex.Message;
				MessageBox.Show(this, ex.Message, T("创建模型失败"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private async void CreateModelAsync()
		{
			if (_busy)
			{
				return;
			}
			try
			{
				PullControlsToJob();
				SetBusy(true, T("正在创建模型..."));
				EdgeInspectJob workJob = _job.DeepClone();
				List<PointF> workPoints = new List<PointF>(_featurePoints);
				TemplateTeachData teachData = await Task.Run(delegate
				{
					if (workPoints.Count == 0)
					{
						workPoints = _processor.ExtractOuterContourPoints(_bitmap, workJob);
						workPoints = ApplyEraseStrokes(workPoints);
					}
					return _processor.TeachFromOuterContourPoints(_bitmap, workJob, workPoints);
				});
				_job.TeachData = teachData;
				_job.TeachData.EraseStrokes = CloneEraseStrokes(_eraseStrokes);
				_featurePoints = new List<PointF>(teachData.FeaturePoints ?? new List<PointF>());
				RefreshFeatureOverlay();
				lblStatus.Text = T("模型创建完成，可点击测试匹配。");
			}
			catch (Exception ex)
			{
				lblStatus.Text = ex.Message;
				MessageBox.Show(this, ex.Message, T("创建模型失败"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			finally
			{
				SetBusy(false, null);
			}
		}

		private async void TestMatchAsync()
		{
			if (_busy)
			{
				return;
			}
			try
			{
				PullControlsToJob();
				if (_job.TeachData == null || !_job.TeachData.HasTemplate)
				{
					MessageBox.Show(this, T("请先创建模型。"), T("测试匹配"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				SetBusy(true, T("正在测试匹配..."));
				EdgeInspectJob workJob = _job.DeepClone();
				TemplateMatchQuickTestResult result = await Task.Run(delegate
				{
					return _processor.TestTemplateMatch(_bitmap, workJob);
				});
				canvas.ClearRuntimeRois();
				if (result.Found)
				{
					canvas.SetRuntimeRois(result.TemplateRoiCur, Enumerable.Empty<RotRectF>(), Enumerable.Empty<RotRectF>());
					canvas.ShowRuntimeRois = true;
					canvas.ShowRois = true;
					canvas.ClearOverlays();
					AddMatchContourOverlay(result.MatchContourPoints);
					lblStatus.Text = result.Message;
				}
				else
				{
					canvas.ShowRuntimeRois = false;
					lblStatus.Text = result.Message;
					MessageBox.Show(this, result.Message, T("测试匹配"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				canvas.Invalidate();
			}
			catch (Exception ex)
			{
				lblStatus.Text = ex.Message;
				MessageBox.Show(this, ex.Message, T("测试匹配失败"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			finally
			{
				SetBusy(false, null);
			}
		}

		private void SetBusy(bool busy, string message)
		{
			_busy = busy;
			btnExtract.Enabled = !busy;
			btnReset.Enabled = !busy;
			btnErase.Enabled = !busy;
			btnCreateModel.Enabled = !busy;
			btnTestMatch.Enabled = !busy;
			btnOk.Enabled = !busy;
			btnCancel.Enabled = !busy;
			UseWaitCursor = busy;
			if (!string.IsNullOrWhiteSpace(message))
			{
				lblStatus.Text = message;
			}
		}

		private void SetEraseMode(bool enabled)
		{
			_eraseMode = enabled;
			_erasing = false;
			canvas.AllowRoiEditing = !enabled;
			btnErase.Text = enabled ? T("退出擦除") : T("擦除点");
			btnErase.BackColor = enabled ? Color.FromArgb(255, 225, 160) : SystemColors.Control;
			lblStatus.Text = enabled ? T("擦除模式：按住左键擦除外轮廓点。") : T("已退出擦除模式。");
		}

		private void EraseAt(PointF center)
		{
			double radius = Math.Max(1.0, (double)numEraseRadius.Value);
			double radius2 = radius * radius;
			_eraseStrokes.Add(new TemplateEraseStroke
			{
				Center = center,
				Radius = radius
			});
			int before = GetRealFeatureCount(_featurePoints);
			_featurePoints = EraseFeaturePoints(_featurePoints, center, radius2);
			if (_job.TeachData != null)
			{
				_job.TeachData.EraseStrokes = CloneEraseStrokes(_eraseStrokes);
			}
			if (GetRealFeatureCount(_featurePoints) != before)
			{
				ClearTeachModelKeepEraseStrokes();
				RefreshFeatureOverlayForErase();
				lblStatus.Text = T("已擦除部分特征点，请重新创建模型。");
			}
			else
			{
				RefreshFeatureOverlayForErase();
			}
		}

		private void RefreshFeatureOverlayForErase()
		{
			_eraseOverlayDirty = true;
			DateTime now = DateTime.UtcNow;
			if ((now - _lastEraseOverlayRefresh).TotalMilliseconds < 33.0)
			{
				return;
			}
			RefreshFeatureOverlay();
			_lastEraseOverlayRefresh = now;
			_eraseOverlayDirty = false;
		}

		private void RefreshFeatureOverlay()
		{
			canvas.ClearOverlays();
			foreach (PointF pt in GetFeaturePointsForDisplay(_featurePoints))
			{
				if (IsContourSeparator(pt))
				{
					continue;
				}
				canvas.OverlayPoints.Add(new SolidPoint
				{
					Center = pt,
					Radius = 2.0f,
					Color = Color.FromArgb(255, 170, 30)
				});
			}
			lblCount.Text = T("特征点") + ": " + _featurePoints.Count;
			canvas.HudText = "Points: " + GetRealFeatureCount(_featurePoints);
			lblCount.Text = T("特征点") + ": " + GetRealFeatureCount(_featurePoints);
			canvas.HudTextColor = Color.FromArgb(255, 220, 120);
			AddEraseBrushOverlay();
			canvas.Invalidate();
		}

		private void AddEraseBrushOverlay()
		{
			if (!_eraseMode || !_hasEraseCursor)
			{
				return;
			}
			double radius = Math.Max(1.0, (double)numEraseRadius.Value);
			List<PointF> points = new List<PointF>(65);
			for (int i = 0; i <= 64; i++)
			{
				double a = i * Math.PI * 2.0 / 64.0;
				points.Add(new PointF((float)(_eraseCursor.X + Math.Cos(a) * radius), (float)(_eraseCursor.Y + Math.Sin(a) * radius)));
			}
			canvas.OverlayLines.Add(new ColoredPolyline
			{
				Points = points,
				Color = Color.FromArgb(255, 80, 180, 255),
				Width = 2f,
				Arrow = false
			});
		}

		private void AddMatchContourOverlay(IList<PointF> points)
		{
			if (points == null || points.Count == 0)
			{
				return;
			}
			List<PointF> segment = new List<PointF>();
			Action flushSegment = delegate
			{
				if (segment.Count >= 2)
				{
					canvas.OverlayLines.Add(new ColoredPolyline
					{
						Points = new List<PointF>(segment),
						Color = Color.Lime,
						Width = 1.6f,
						Arrow = false
					});
				}
				segment.Clear();
			};
			foreach (PointF pt in points)
			{
				if (IsContourSeparator(pt))
				{
					flushSegment();
					continue;
				}
				segment.Add(pt);
				canvas.OverlayPoints.Add(new SolidPoint
				{
					Center = pt,
					Radius = 2.2f,
					Color = Color.Lime
				});
			}
			flushSegment();
			canvas.HudText = "Match: " + GetRealFeatureCount(points) + " pts";
			canvas.HudTextColor = Color.Lime;
		}
		private static IEnumerable<PointF> GetFeaturePointsForDisplay(IList<PointF> points)
		{
			if (points == null || points.Count == 0)
			{
				yield break;
			}
			foreach (PointF pt in points)
			{
				if (IsContourSeparator(pt))
				{
					continue;
				}
				yield return pt;
			}
		}

		private static int GetRealFeatureCount(IEnumerable<PointF> points)
		{
			return points == null ? 0 : points.Count(p => !IsContourSeparator(p));
		}

		private List<PointF> ApplyEraseStrokes(IEnumerable<PointF> points)
		{
			List<PointF> result = points == null ? new List<PointF>() : new List<PointF>(points);
			foreach (TemplateEraseStroke stroke in _eraseStrokes)
			{
				if (stroke == null || stroke.Radius <= 0.0)
				{
					continue;
				}
				result = EraseFeaturePoints(result, stroke.Center, stroke.Radius * stroke.Radius);
			}
			return result;
		}

		private static List<TemplateEraseStroke> CloneEraseStrokes(IEnumerable<TemplateEraseStroke> strokes)
		{
			return strokes == null
				? new List<TemplateEraseStroke>()
				: strokes.Select(x => x?.DeepClone() ?? new TemplateEraseStroke()).ToList();
		}

		private static List<PointF> EraseFeaturePoints(IEnumerable<PointF> points, PointF center, double radius2)
		{
			List<PointF> result = new List<PointF>();
			if (points == null)
			{
				return result;
			}
			bool lastWasSeparator = true;
			foreach (PointF pt in points)
			{
				if (IsContourSeparator(pt))
				{
					if (!lastWasSeparator && result.Count > 0)
					{
						result.Add(pt);
						lastWasSeparator = true;
					}
					continue;
				}
				if (DistanceSquared(pt, center) <= radius2)
				{
					if (!lastWasSeparator && result.Count > 0)
					{
						result.Add(new PointF(float.NaN, float.NaN));
						lastWasSeparator = true;
					}
					continue;
				}
				result.Add(pt);
				lastWasSeparator = false;
			}
			return CompactFeaturePoints(result);
		}

		private static List<PointF> CompactFeaturePoints(List<PointF> points)
		{
			List<PointF> result = new List<PointF>();
			if (points == null)
			{
				return result;
			}
			bool lastWasSeparator = true;
			foreach (PointF pt in points)
			{
				if (IsContourSeparator(pt))
				{
					if (!lastWasSeparator && result.Count > 0)
					{
						result.Add(pt);
						lastWasSeparator = true;
					}
					continue;
				}
				result.Add(pt);
				lastWasSeparator = false;
			}
			while (result.Count > 0 && IsContourSeparator(result[result.Count - 1]))
			{
				result.RemoveAt(result.Count - 1);
			}
			return result;
		}

		private static bool IsContourSeparator(PointF p)
		{
			return float.IsNaN(p.X) || float.IsNaN(p.Y);
		}

		private static RotRectF CreateDefaultTemplateRoi(Bitmap bmp)
		{
			float w = Math.Max(40f, bmp.Width * 0.25f);
			float h = Math.Max(40f, bmp.Height * 0.25f);
			return RotRectF.FromAxisAligned(new RectangleF((bmp.Width - w) * 0.5f, (bmp.Height - h) * 0.5f, w, h));
		}

		private static void SetDecimal(NumericUpDown num, double value)
		{
			decimal v = (decimal)value;
			if (v < num.Minimum)
			{
				v = num.Minimum;
			}
			else if (v > num.Maximum)
			{
				v = num.Maximum;
			}
			num.Value = v;
		}

		private static double DistanceSquared(PointF a, PointF b)
		{
			double dx = a.X - b.X;
			double dy = a.Y - b.Y;
			return dx * dx + dy * dy;
		}
	}
}
