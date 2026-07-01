using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	public sealed class  HalconCanvas : Control
	{
		private enum EditMode
		{
			None,
			Move,
			ResizeLongPos,
			ResizeLongNeg,
			ResizeShortPos,
			ResizeShortNeg,
			ResizeCircle,
			Rotate
		}

		private enum EdgeHandleKind
		{
			LongPos,
			LongNeg,
			ShortPos,
			ShortNeg
		}

		private Bitmap _bmp;

		private float _zoom = 1f;

		private PointF _pan = new PointF(0f, 0f);

		private bool _panning;

		private Point _lastMouse;

		private readonly List<RotRectF> _baseRois = new List<RotRectF>();

		private readonly List<CircleBaseRoiPair> _circleBaseRois = new List<CircleBaseRoiPair>();

		private readonly List<CircleRoiF> _circlePointRois = new List<CircleRoiF>();

		private readonly List<RotRectF> _detectRois = new List<RotRectF>();

		private readonly List<RotRectF> _runtimeBaseRois = new List<RotRectF>();

		private readonly List<CircleBaseRoiPair> _runtimeCircleBaseRois = new List<CircleBaseRoiPair>();

		private readonly List<CircleRoiF> _runtimeCirclePointRois = new List<CircleRoiF>();

		private readonly List<RotRectF> _runtimeDetectRois = new List<RotRectF>();

		private bool _showRois = true;

		private bool _showTemplateRoi = true;

		private bool _showRuntimeRois = false;

		private EditMode _edit = EditMode.None;

		private Point _startMouseScr;

		private RotRectF _startRoi;

		private CircleRoiF _startCircleRoi;

		private bool _roiDirtyDuringEdit = false;

		private const float HandleScreenSize = 10f;

		private const float RotateHandleScreenRadius = 6f;

		private const float MinHalfLen = 3f;

		public RotRectF TemplateRoi { get; private set; } = default(RotRectF);

		public RotRectF RuntimeTemplateRoi { get; private set; } = default(RotRectF);

		public List<ColoredPolyline> OverlayLines { get; } = new List<ColoredPolyline>();

		public List<SolidPoint> OverlayPoints { get; } = new List<SolidPoint>();

		public string HudText { get; set; } = "";

		public Color HudTextColor { get; set; } = Color.White;

		public bool AutoClearOverlaysOnRoiEdit { get; set; } = true;

		public bool AllowRoiEditing { get; set; } = true;

		public InspectionLanguage Language { get; set; } = InspectionLanguage.Chinese;

		public event Action<PointF, MouseButtons> ImageMouseDown;

		public event Action<PointF, MouseButtons> ImageMouseMove;

		public event Action<PointF, MouseButtons> ImageMouseUp;

		public bool ShowRois
		{
			get
			{
				return _showRois;
			}
			set
			{
				if (_showRois != value)
				{
					_showRois = value;
					Invalidate();
				}
			}
		}

		public bool ShowTemplateRoi
		{
			get
			{
				return _showTemplateRoi;
			}
			set
			{
				if (_showTemplateRoi != value)
				{
					_showTemplateRoi = value;
					if (!_showTemplateRoi && SelectedRoi.Kind == CanvasRoiKind.Template)
					{
						SetSelection(CanvasRoiKind.None, -1);
					}
					Invalidate();
				}
			}
		}

		public bool ShowRuntimeRois
		{
			get
			{
				return _showRuntimeRois;
			}
			set
			{
				if (_showRuntimeRois != value)
				{
					_showRuntimeRois = value;
					Invalidate();
				}
			}
		}

		public CanvasRoiSelection SelectedRoi { get; private set; } = CanvasRoiSelection.None;

		public IReadOnlyList<RotRectF> BaseRois => _baseRois;

		public IReadOnlyList<CircleBaseRoiPair> CircleBaseRois => _circleBaseRois;

		public IReadOnlyList<CircleRoiF> CirclePointRois => _circlePointRois;

		public IReadOnlyList<RotRectF> DetectRois => _detectRois;

		public IReadOnlyList<RotRectF> RuntimeBaseRois => _runtimeBaseRois;

		public IReadOnlyList<CircleBaseRoiPair> RuntimeCircleBaseRois => _runtimeCircleBaseRois;

		public IReadOnlyList<CircleRoiF> RuntimeCirclePointRois => _runtimeCirclePointRois;

		public IReadOnlyList<RotRectF> RuntimeDetectRois => _runtimeDetectRois;

		public RotRectF BaseRoi => (_baseRois.Count > 0) ? _baseRois[0] : default(RotRectF);

		public RotRectF DetectRoi => (_detectRois.Count > 0) ? _detectRois[0] : default(RotRectF);

		public event Action RoiChanged;

		public event Action SelectionChanged;

		public HalconCanvas()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			BackColor = Color.Black;
			TabStop = true;
			base.MouseWheel += delegate(object s, MouseEventArgs e)
			{
				if (_bmp != null)
				{
					float num = ((e.Delta > 0) ? 1.15f : 0.86956525f);
					float zoom = Math.Max(0.02f, Math.Min(50f, _zoom * num));
					PointF p = ScreenToImage(e.Location);
					_zoom = zoom;
					PointF pointF = ImageToScreen(p);
					_pan = new PointF(_pan.X + (float)e.X - pointF.X, _pan.Y + (float)e.Y - pointF.Y);
					Invalidate();
				}
			};
		}

		protected override bool IsInputKey(Keys keyData)
		{
			Keys keyCode = keyData & Keys.KeyCode;
			if (keyCode == Keys.Left || keyCode == Keys.Right || keyCode == Keys.Up || keyCode == Keys.Down)
			{
				return true;
			}
			return base.IsInputKey(keyData);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			Keys keyCode = keyData & Keys.KeyCode;
			if (keyCode == Keys.Left || keyCode == Keys.Right || keyCode == Keys.Up || keyCode == Keys.Down)
			{
				float step = ((keyData & Keys.Shift) == Keys.Shift) ? 10f : 1f;
				float dx = 0f;
				float dy = 0f;
				switch (keyCode)
				{
				case Keys.Left:
					dx = 0f - step;
					break;
				case Keys.Right:
					dx = step;
					break;
				case Keys.Up:
					dy = 0f - step;
					break;
				case Keys.Down:
					dy = step;
					break;
				}
				if (MoveSelectedRoiBy(dx, dy))
				{
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void PrepareForEditRoiDisplay()
		{
			_showRuntimeRois = false;
			_showRois = true;
		}

		public void LoadBitmap(Bitmap bmp)
		{
			_bmp = bmp;
			ClearOverlays();
			ClearHud();
			ClearRuntimeRois();
			ShowRuntimeRois = false;
			ShowRois = true;
			FitToWindow();
			Invalidate();
		}

		public void FitToWindow()
		{
			if (_bmp != null && base.Width > 0 && base.Height > 0)
			{
				float val = Math.Min((float)base.Width / (float)_bmp.Width, (float)base.Height / (float)_bmp.Height) * 0.95f;
				_zoom = Math.Max(0.02f, val);
				_pan = new PointF(((float)base.Width - (float)_bmp.Width * _zoom) / 2f, ((float)base.Height - (float)_bmp.Height * _zoom) / 2f);
			}
		}

		public void ClearOverlays()
		{
			OverlayLines.Clear();
			OverlayPoints.Clear();
		}

		public void ClearHud()
		{
			HudText = "";
		}

		public void SetRois(RotRectF tpl, RotRectF baseRoi, RotRectF detectRoi)
		{
			PrepareForEditRoiDisplay();
			TemplateRoi = tpl;
			_baseRois.Clear();
			_circleBaseRois.Clear();
			_circlePointRois.Clear();
			_detectRois.Clear();
			if (!baseRoi.IsEmpty)
			{
				_baseRois.Add(baseRoi);
			}
			if (!detectRoi.IsEmpty)
			{
				_detectRois.Add(detectRoi);
			}
			NormalizeSelectionAfterRoiChange();
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void SetTemplateRoi(RotRectF roi)
		{
			PrepareForEditRoiDisplay();
			TemplateRoi = roi;
			NormalizeSelectionAfterRoiChange();
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void SetBaseRois(IEnumerable<RotRectF> rois)
		{
			PrepareForEditRoiDisplay();
			_baseRois.Clear();
			if (rois != null)
			{
				_baseRois.AddRange(rois);
			}
			NormalizeSelectionAfterRoiChange();
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void SetDetectRois(IEnumerable<RotRectF> rois)
		{
			PrepareForEditRoiDisplay();
			_detectRois.Clear();
			if (rois != null)
			{
				_detectRois.AddRange(rois);
			}
			NormalizeSelectionAfterRoiChange();
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void SetAllRois(RotRectF tpl, IEnumerable<RotRectF> baseRois, IEnumerable<RotRectF> detectRois)
		{
			SetAllRois(tpl, baseRois, null, null, detectRois);
		}

		public void SetAllRois(RotRectF tpl, IEnumerable<RotRectF> baseRois, IEnumerable<CircleBaseRoiPair> circleBaseRois, IEnumerable<RotRectF> detectRois)
		{
			SetAllRois(tpl, baseRois, circleBaseRois, null, detectRois);
		}

		public void SetAllRois(RotRectF tpl, IEnumerable<RotRectF> baseRois, IEnumerable<CircleBaseRoiPair> circleBaseRois, IEnumerable<CircleRoiF> circlePointRois, IEnumerable<RotRectF> detectRois)
		{
			PrepareForEditRoiDisplay();
			TemplateRoi = tpl;
			_baseRois.Clear();
			_circleBaseRois.Clear();
			_circlePointRois.Clear();
			_detectRois.Clear();
			if (baseRois != null)
			{
				_baseRois.AddRange(baseRois);
			}
			if (circleBaseRois != null)
			{
				_circleBaseRois.AddRange(circleBaseRois);
			}
			if (circlePointRois != null)
			{
				_circlePointRois.AddRange(circlePointRois);
			}
			if (detectRois != null)
			{
				_detectRois.AddRange(detectRois);
			}
			NormalizeSelectionAfterRoiChange();
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void AddBaseRoi(RotRectF roi, bool selectNew = true)
		{
			PrepareForEditRoiDisplay();
			_baseRois.Add(roi);
			if (selectNew)
			{
				SetSelection(CanvasRoiKind.Base, _baseRois.Count - 1);
			}
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void AddCircleBaseRoi(CircleBaseRoiPair pair, bool selectNew = true)
		{
			PrepareForEditRoiDisplay();
			_circleBaseRois.Add(pair);
			if (selectNew)
			{
				SetSelection(CanvasRoiKind.CircleBase1, _circleBaseRois.Count - 1);
			}
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void AddCirclePointRoi(CircleRoiF roi, bool selectNew = true)
		{
			PrepareForEditRoiDisplay();
			_circlePointRois.Add(roi);
			if (selectNew)
			{
				SetSelection(CanvasRoiKind.CirclePoint, _circlePointRois.Count - 1);
			}
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public void AddDetectRoi(RotRectF roi, bool selectNew = true)
		{
			PrepareForEditRoiDisplay();
			_detectRois.Add(roi);
			if (selectNew)
			{
				SetSelection(CanvasRoiKind.Detect, _detectRois.Count - 1);
			}
			this.RoiChanged?.Invoke();
			Invalidate();
		}

		public bool RemoveSelectedRoi()
		{
			PrepareForEditRoiDisplay();
			if (!SelectedRoi.IsValid)
			{
				return false;
			}
			if (SelectedRoi.Kind == CanvasRoiKind.Template)
			{
				return false;
			}
			if (SelectedRoi.Kind == CanvasRoiKind.Base)
			{
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _baseRois.Count)
				{
					return false;
				}
				int index = SelectedRoi.Index;
				_baseRois.RemoveAt(index);
				if (_baseRois.Count == 0)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				else
				{
					SetSelection(CanvasRoiKind.Base, Math.Min(index, _baseRois.Count - 1));
				}
				this.RoiChanged?.Invoke();
				Invalidate();
				return true;
			}
			if (SelectedRoi.Kind == CanvasRoiKind.Detect)
			{
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _detectRois.Count)
				{
					return false;
				}
				int index2 = SelectedRoi.Index;
				_detectRois.RemoveAt(index2);
				if (_detectRois.Count == 0)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				else
				{
					SetSelection(CanvasRoiKind.Detect, Math.Min(index2, _detectRois.Count - 1));
				}
				this.RoiChanged?.Invoke();
				Invalidate();
				return true;
			}
			if (SelectedRoi.Kind == CanvasRoiKind.CircleBase1 || SelectedRoi.Kind == CanvasRoiKind.CircleBase2)
			{
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _circleBaseRois.Count)
				{
					return false;
				}
				int index3 = SelectedRoi.Index;
				_circleBaseRois.RemoveAt(index3);
				if (_circleBaseRois.Count == 0)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				else
				{
					SetSelection(CanvasRoiKind.CircleBase1, Math.Min(index3, _circleBaseRois.Count - 1));
				}
				this.RoiChanged?.Invoke();
				Invalidate();
				return true;
			}
			if (SelectedRoi.Kind == CanvasRoiKind.CirclePoint)
			{
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _circlePointRois.Count)
				{
					return false;
				}
				int index4 = SelectedRoi.Index;
				_circlePointRois.RemoveAt(index4);
				if (_circlePointRois.Count == 0)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				else
				{
					SetSelection(CanvasRoiKind.CirclePoint, Math.Min(index4, _circlePointRois.Count - 1));
				}
				this.RoiChanged?.Invoke();
				Invalidate();
				return true;
			}
			return false;
		}

		public void SetSelection(CanvasRoiKind kind, int index)
		{
			CanvasRoiSelection selectedRoi;
			switch (kind)
			{
			case CanvasRoiKind.Template:
				selectedRoi = (TemplateRoi.IsEmpty || !ShowTemplateRoi ? CanvasRoiSelection.None : new CanvasRoiSelection
				{
					Kind = CanvasRoiKind.Template,
					Index = 0
				});
				break;
			case CanvasRoiKind.Base:
				selectedRoi = ((index >= 0 && index < _baseRois.Count) ? new CanvasRoiSelection
				{
					Kind = CanvasRoiKind.Base,
					Index = index
				} : CanvasRoiSelection.None);
				break;
			case CanvasRoiKind.CircleBase1:
			case CanvasRoiKind.CircleBase2:
				selectedRoi = ((index >= 0 && index < _circleBaseRois.Count) ? new CanvasRoiSelection
				{
					Kind = kind,
					Index = index
				} : CanvasRoiSelection.None);
				break;
			case CanvasRoiKind.CirclePoint:
				selectedRoi = ((index >= 0 && index < _circlePointRois.Count) ? new CanvasRoiSelection
				{
					Kind = kind,
					Index = index
				} : CanvasRoiSelection.None);
				break;
			case CanvasRoiKind.Detect:
				selectedRoi = ((index >= 0 && index < _detectRois.Count) ? new CanvasRoiSelection
				{
					Kind = CanvasRoiKind.Detect,
					Index = index
				} : CanvasRoiSelection.None);
				break;
			default:
				selectedRoi = CanvasRoiSelection.None;
				break;
			}
			bool flag = selectedRoi.Kind != SelectedRoi.Kind || selectedRoi.Index != SelectedRoi.Index;
			SelectedRoi = selectedRoi;
			if (flag)
			{
				this.SelectionChanged?.Invoke();
			}
			Invalidate();
		}

		public RotRectF GetSelectedRoiOrDefault()
		{
			if (!SelectedRoi.IsValid)
			{
				return default(RotRectF);
			}
			return GetRoiBySelection(SelectedRoi);
		}

		public bool UpdateSelectedRoi(RotRectF roi)
		{
			PrepareForEditRoiDisplay();
			if (!SelectedRoi.IsValid)
			{
				return false;
			}
			SetRoiBySelection(SelectedRoi, roi);
			this.RoiChanged?.Invoke();
			Invalidate();
			return true;
		}

		public void SetRuntimeRois(RotRectF tpl, RotRectF baseRoi, RotRectF detectRoi)
		{
			RuntimeTemplateRoi = tpl;
			_runtimeBaseRois.Clear();
			_runtimeDetectRois.Clear();
			if (!baseRoi.IsEmpty)
			{
				_runtimeBaseRois.Add(baseRoi);
			}
			if (!detectRoi.IsEmpty)
			{
				_runtimeDetectRois.Add(detectRoi);
			}
			Invalidate();
		}

		public void SetRuntimeRois(RotRectF tpl, IEnumerable<RotRectF> baseRois, IEnumerable<RotRectF> detectRois)
		{
			SetRuntimeRois(tpl, baseRois, null, null, detectRois);
		}

		public void SetRuntimeRois(RotRectF tpl, IEnumerable<RotRectF> baseRois, IEnumerable<CircleBaseRoiPair> circleBaseRois, IEnumerable<RotRectF> detectRois)
		{
			SetRuntimeRois(tpl, baseRois, circleBaseRois, null, detectRois);
		}

		public void SetRuntimeRois(RotRectF tpl, IEnumerable<RotRectF> baseRois, IEnumerable<CircleBaseRoiPair> circleBaseRois, IEnumerable<CircleRoiF> circlePointRois, IEnumerable<RotRectF> detectRois)
		{
			RuntimeTemplateRoi = tpl;
			_runtimeBaseRois.Clear();
			_runtimeCircleBaseRois.Clear();
			_runtimeCirclePointRois.Clear();
			_runtimeDetectRois.Clear();
			if (baseRois != null)
			{
				_runtimeBaseRois.AddRange(baseRois);
			}
			if (circleBaseRois != null)
			{
				_runtimeCircleBaseRois.AddRange(circleBaseRois);
			}
			if (circlePointRois != null)
			{
				_runtimeCirclePointRois.AddRange(circlePointRois);
			}
			if (detectRois != null)
			{
				_runtimeDetectRois.AddRange(detectRois);
			}
			Invalidate();
		}

		public void ClearRuntimeRois()
		{
			RuntimeTemplateRoi = default(RotRectF);
			_runtimeBaseRois.Clear();
			_runtimeCircleBaseRois.Clear();
			_runtimeCirclePointRois.Clear();
			_runtimeDetectRois.Clear();
			Invalidate();
		}

		private PointF ScreenToImage(Point p)
		{
			return new PointF(((float)p.X - _pan.X) / _zoom, ((float)p.Y - _pan.Y) / _zoom);
		}

		public bool TryScreenToImage(Point p, out PointF imagePoint)
		{
			if (_bmp == null)
			{
				imagePoint = default(PointF);
				return false;
			}
			imagePoint = ScreenToImage(p);
			return true;
		}

		private PointF ImageToScreen(PointF p)
		{
			return new PointF(p.X * _zoom + _pan.X, p.Y * _zoom + _pan.Y);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			graphics.Clear(BackColor);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (_bmp == null)
			{
				TextRenderer.DrawText(graphics, LocalizedText.Ui("请加载图片", Language), Font, base.ClientRectangle, Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				return;
			}
			graphics.DrawImage(_bmp, _pan.X, _pan.Y, (float)_bmp.Width * _zoom, (float)_bmp.Height * _zoom);
			if (ShowRois)
			{
				DrawRoi(graphics, TemplateRoi, LocalizedText.Ui("模板", Language), Color.Red, SelectedRoi.Kind == CanvasRoiKind.Template, drawArrow: false, dashed: false, SelectedRoi.Kind == CanvasRoiKind.Template);
				for (int i = 0; i < _baseRois.Count; i++)
				{
					DrawRoi(graphics, _baseRois[i], LocalizedText.Ui($"基准{i + 1}", Language), (i == 0) ? Color.Yellow : Color.Orange, SelectedRoi.Kind == CanvasRoiKind.Base && SelectedRoi.Index == i, drawArrow: true, dashed: false, SelectedRoi.Kind == CanvasRoiKind.Base && SelectedRoi.Index == i);
				}
				for (int j = 0; j < _circleBaseRois.Count; j++)
				{
					CircleBaseRoiPair circleBaseRoiPair = _circleBaseRois[j];
					DrawCircleRoi(graphics, circleBaseRoiPair.Circle1, LocalizedText.Ui($"圆基准{j + 1}-1", Language), Color.Cyan, SelectedRoi.Kind == CanvasRoiKind.CircleBase1 && SelectedRoi.Index == j, SelectedRoi.Kind == CanvasRoiKind.CircleBase1 && SelectedRoi.Index == j);
					DrawCircleRoi(graphics, circleBaseRoiPair.Circle2, LocalizedText.Ui($"圆基准{j + 1}-2", Language), Color.Cyan, SelectedRoi.Kind == CanvasRoiKind.CircleBase2 && SelectedRoi.Index == j, SelectedRoi.Kind == CanvasRoiKind.CircleBase2 && SelectedRoi.Index == j);
					if (!circleBaseRoiPair.Circle1.IsEmpty && !circleBaseRoiPair.Circle2.IsEmpty)
					{
						using (Pen pen = new Pen(Color.Cyan, 2f))
						{
							graphics.DrawLine(pen, ImageToScreen(circleBaseRoiPair.Circle1.Center), ImageToScreen(circleBaseRoiPair.Circle2.Center));
						}
					}
				}
				for (int k = 0; k < _detectRois.Count; k++)
				{
					DrawRoi(graphics, _detectRois[k], LocalizedText.Ui($"检测{k + 1}", Language), Color.Lime, SelectedRoi.Kind == CanvasRoiKind.Detect && SelectedRoi.Index == k, drawArrow: true, dashed: false, SelectedRoi.Kind == CanvasRoiKind.Detect && SelectedRoi.Index == k);
				}
				for (int l = 0; l < _circlePointRois.Count; l++)
				{
					DrawCircleRoi(graphics, _circlePointRois[l], LocalizedText.Ui($"圆点基准{l + 1}", Language), Color.DeepSkyBlue, SelectedRoi.Kind == CanvasRoiKind.CirclePoint && SelectedRoi.Index == l, SelectedRoi.Kind == CanvasRoiKind.CirclePoint && SelectedRoi.Index == l);
				}
			}
			if (ShowRuntimeRois)
			{
				DrawRoi(graphics, RuntimeTemplateRoi, LocalizedText.Ui("模板(运行)", Language), Color.FromArgb(230, 255, 120, 120), selected: false, drawArrow: false, dashed: true, showHandles: false);
				for (int m = 0; m < _runtimeBaseRois.Count; m++)
				{
					DrawRoi(graphics, _runtimeBaseRois[m], LocalizedText.Ui($"基准{m + 1}(运行)", Language), (m == 0) ? Color.FromArgb(230, 0, 255, 255) : Color.FromArgb(230, 255, 180, 0), selected: false, drawArrow: true, dashed: true, showHandles: false);
				}
				for (int n = 0; n < _runtimeCircleBaseRois.Count; n++)
				{
					CircleBaseRoiPair circleBaseRoiPair2 = _runtimeCircleBaseRois[n];
					DrawCircleRoi(graphics, circleBaseRoiPair2.Circle1, LocalizedText.Ui($"圆基准{n + 1}-1(运行)", Language), Color.FromArgb(230, 0, 255, 255), selected: false, showHandles: false);
					DrawCircleRoi(graphics, circleBaseRoiPair2.Circle2, LocalizedText.Ui($"圆基准{n + 1}-2(运行)", Language), Color.FromArgb(230, 0, 255, 255), selected: false, showHandles: false);
					if (!circleBaseRoiPair2.Circle1.IsEmpty && !circleBaseRoiPair2.Circle2.IsEmpty)
					{
						using (Pen pen2 = new Pen(Color.FromArgb(230, 0, 255, 255), 2f))
						{
							pen2.DashStyle = DashStyle.Dash;
							graphics.DrawLine(pen2, ImageToScreen(circleBaseRoiPair2.Circle1.Center), ImageToScreen(circleBaseRoiPair2.Circle2.Center));
						}
					}
				}
				for (int num = 0; num < _runtimeDetectRois.Count; num++)
				{
					DrawRoi(graphics, _runtimeDetectRois[num], LocalizedText.Ui($"检测{num + 1}(运行)", Language), Color.FromArgb(230, 255, 0, 255), selected: false, drawArrow: true, dashed: true, showHandles: false);
				}
				for (int num2 = 0; num2 < _runtimeCirclePointRois.Count; num2++)
				{
					DrawCircleRoi(graphics, _runtimeCirclePointRois[num2], LocalizedText.Ui($"圆点基准{num2 + 1}(运行)", Language), Color.FromArgb(230, 0, 190, 255), selected: false, showHandles: false);
				}
			}
			foreach (ColoredPolyline overlayLine in OverlayLines)
			{
				if (overlayLine.Points == null || overlayLine.Points.Count < 2)
				{
					continue;
				}
				using (Pen pen3 = new Pen(overlayLine.Color, overlayLine.Width))
				{
					if (overlayLine.Arrow)
					{
						pen3.EndCap = LineCap.ArrowAnchor;
						pen3.CustomEndCap = new AdjustableArrowCap(4f, 4f);
					}
					graphics.DrawLines(pen3, overlayLine.Points.Select(ImageToScreen).ToArray());
				}
			}
			foreach (SolidPoint overlayPoint in OverlayPoints)
			{
				PointF pointF = ImageToScreen(overlayPoint.Center);
				float num3 = Math.Max(2f, overlayPoint.Radius);
				using (SolidBrush brush = new SolidBrush(overlayPoint.Color))
				{
					graphics.FillEllipse(brush, pointF.X - num3, pointF.Y - num3, num3 * 2f, num3 * 2f);
				}
				if (!string.IsNullOrEmpty(overlayPoint.Label))
				{
					Point pt = new Point((int)(pointF.X + 6f), (int)(pointF.Y + 6f));
					TextRenderer.DrawText(graphics, overlayPoint.Label, Font, pt, overlayPoint.Color);
				}
			}
			if (!string.IsNullOrWhiteSpace(HudText))
			{
				DrawHud(graphics, HudText, HudTextColor, 10, 10);
			}
		}

		private void DrawHud(Graphics g, string text, Color color, int x, int y)
		{
			string[] array = text.Replace("\r\n", "\n").Split('\n');
			int num = 6;
			int num2 = 0;
			int num3 = 0;
			string[] array2 = array;
			string[] array3 = array2;
			foreach (string text2 in array3)
			{
				Size size = TextRenderer.MeasureText(text2, Font);
				num2 = Math.Max(num2, size.Width);
				num3 += size.Height;
			}
			Rectangle rect = new Rectangle(x, y, num2 + num * 2, num3 + num * 2);
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
			{
				g.FillRectangle(brush, rect);
			}
			int num4 = y + num;
			string[] array4 = array;
			string[] array5 = array4;
			foreach (string text3 in array5)
			{
				TextRenderer.DrawText(g, text3, Font, new Point(x + num, num4), color);
				num4 += TextRenderer.MeasureText(text3, Font).Height;
			}
		}

		private void DrawRoi(Graphics g, RotRectF roi, string label, Color color, bool selected, bool drawArrow, bool dashed, bool showHandles)
		{
			if (roi.IsEmpty)
			{
				return;
			}
			if (!ShowTemplateRoi && !drawArrow && IsTemplateRoiColor(color, dashed))
			{
				return;
			}
			PointF[] corners = roi.GetCorners();
			PointF[] array = corners.Select(ImageToScreen).ToArray();
			using (Pen pen = new Pen(color, selected ? 3 : 2))
			{
				if (dashed)
				{
					pen.DashStyle = DashStyle.Dash;
				}
				g.DrawPolygon(pen, array);
			}
			PointF pointF = array[2];
			DrawLabel(g, new Point((int)pointF.X + 4, (int)pointF.Y + 4), label, color);
			if (drawArrow)
			{
				DrawRoiArrow(g, roi, color, dashed);
			}
			if (selected && showHandles)
			{
				DrawEdgeHandle(g, GetEdgeHandleScreen(roi, EdgeHandleKind.LongPos));
				DrawEdgeHandle(g, GetEdgeHandleScreen(roi, EdgeHandleKind.LongNeg));
				DrawEdgeHandle(g, GetEdgeHandleScreen(roi, EdgeHandleKind.ShortPos));
				DrawEdgeHandle(g, GetEdgeHandleScreen(roi, EdgeHandleKind.ShortNeg));
				Rectangle rotateHandleScreen = GetRotateHandleScreen(roi);
				using (SolidBrush brush = new SolidBrush(Color.White))
				{
					g.FillEllipse(brush, rotateHandleScreen);
				}
				g.DrawEllipse(Pens.Black, rotateHandleScreen);
			}
		}

		private static bool IsTemplateRoiColor(Color color, bool dashed)
		{
			return (!dashed && color.ToArgb() == Color.Red.ToArgb()) ||
				(dashed && color.ToArgb() == Color.FromArgb(230, 255, 120, 120).ToArgb());
		}

		private void DrawEdgeHandle(Graphics g, Rectangle rc)
		{
			g.FillRectangle(Brushes.White, rc);
			g.DrawRectangle(Pens.Black, rc);
		}

		private void DrawCircleRoi(Graphics g, CircleRoiF roi, string label, Color color, bool selected, bool showHandles)
		{
			if (!roi.IsEmpty)
			{
				PointF pointF = ImageToScreen(roi.Center);
				float num = roi.Radius * _zoom;
				using (Pen pen = new Pen(color, selected ? 3 : 2))
				{
					g.DrawEllipse(pen, pointF.X - num, pointF.Y - num, num * 2f, num * 2f);
				}
				DrawLabel(g, new Point((int)(pointF.X + num + 4f), (int)(pointF.Y + 4f)), label, color);
				using (SolidBrush brush = new SolidBrush(color))
				{
					g.FillEllipse(brush, pointF.X - 3f, pointF.Y - 3f, 6f, 6f);
				}
				if (selected && showHandles)
				{
					DrawEdgeHandle(g, GetCircleRadiusHandleScreen(roi));
				}
			}
		}

		private void DrawLabel(Graphics g, Point pt, string text, Color color)
		{
			Size size = TextRenderer.MeasureText(text, Font);
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
			{
				g.FillRectangle(brush, new Rectangle(pt, new Size(size.Width + 6, size.Height + 2)));
			}
			TextRenderer.DrawText(g, text, Font, new Rectangle(pt.X + 3, pt.Y + 1, size.Width, size.Height), color, TextFormatFlags.Default);
		}

		private void DrawRoiArrow(Graphics g, RotRectF roi, Color color, bool dashed)
		{
			float len = Math.Max(18f, Math.Min(roi.HalfLen1, roi.HalfLen2) * 0.9f);
			PointF pt = ImageToScreen(new PointF(roi.Cx, roi.Cy));
			PointF arrowEnd = roi.GetArrowEnd(len);
			PointF pt2 = ImageToScreen(arrowEnd);
			using (Pen pen = new Pen(color, 2f))
			{
				if (dashed)
				{
					pen.DashStyle = DashStyle.Dash;
				}
				pen.EndCap = LineCap.ArrowAnchor;
				pen.CustomEndCap = new AdjustableArrowCap(5f, 5f);
				g.DrawLine(pen, pt, pt2);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			Focus();
			if (_bmp == null)
			{
				return;
			}
			PointF pt = ScreenToImage(e.Location);
			this.ImageMouseDown?.Invoke(pt, e.Button);
			if (!AllowRoiEditing)
			{
				return;
			}
			if (e.Button == MouseButtons.Middle)
			{
				_panning = true;
				_lastMouse = e.Location;
				Cursor = Cursors.Hand;
			}
			else if (ShowRuntimeRois && e.Button == MouseButtons.Left)
			{
				ShowRuntimeRois = false;
				ShowRois = true;
			}
			else
			{
				if (!ShowRois || e.Button != MouseButtons.Left)
				{
					return;
				}
				_roiDirtyDuringEdit = false;
				if (HitSelectCurrentRoi(e.Location))
				{
					return;
				}
				for (int num = _detectRois.Count - 1; num >= 0; num--)
				{
					if (HitSelectRoi(CanvasRoiKind.Detect, num, _detectRois[num], e.Location))
					{
						return;
					}
				}
				for (int num2 = _baseRois.Count - 1; num2 >= 0; num2--)
				{
					if (HitSelectRoi(CanvasRoiKind.Base, num2, _baseRois[num2], e.Location))
					{
						return;
					}
				}
				for (int num3 = _circleBaseRois.Count - 1; num3 >= 0; num3--)
				{
					if (HitSelectCircleRoi(CanvasRoiKind.CircleBase2, num3, _circleBaseRois[num3].Circle2, e.Location) || HitSelectCircleRoi(CanvasRoiKind.CircleBase1, num3, _circleBaseRois[num3].Circle1, e.Location))
					{
						return;
					}
				}
				for (int num4 = _circlePointRois.Count - 1; num4 >= 0; num4--)
				{
					if (HitSelectCircleRoi(CanvasRoiKind.CirclePoint, num4, _circlePointRois[num4], e.Location))
					{
						return;
					}
				}
				if (!ShowTemplateRoi || !HitSelectRoi(CanvasRoiKind.Template, 0, TemplateRoi, e.Location))
				{
					SetSelection(CanvasRoiKind.None, -1);
					_edit = EditMode.None;
					Invalidate();
				}
			}
		}

		private bool HitSelectCurrentRoi(Point mouse)
		{
			if (!SelectedRoi.IsValid)
			{
				return false;
			}
			CanvasRoiSelection selectedRoi = SelectedRoi;
			if (selectedRoi.Kind == CanvasRoiKind.CircleBase1 || selectedRoi.Kind == CanvasRoiKind.CircleBase2 || selectedRoi.Kind == CanvasRoiKind.CirclePoint)
			{
				CircleRoiF circleBySelection = GetCircleBySelection(selectedRoi);
				return HitSelectCircleRoi(selectedRoi.Kind, selectedRoi.Index, circleBySelection, mouse);
			}
			RotRectF roiBySelection = GetRoiBySelection(selectedRoi);
			return HitSelectRoi(selectedRoi.Kind, selectedRoi.Index, roiBySelection, mouse);
		}

		private bool HitSelectRoi(CanvasRoiKind kind, int index, RotRectF roi, Point mouse)
		{
			if (roi.IsEmpty)
			{
				return false;
			}
			bool flag = GetRotateHandleScreen(roi).Contains(mouse);
			bool flag2 = GetEdgeHandleScreen(roi, EdgeHandleKind.LongPos).Contains(mouse);
			bool flag3 = GetEdgeHandleScreen(roi, EdgeHandleKind.LongNeg).Contains(mouse);
			bool flag4 = GetEdgeHandleScreen(roi, EdgeHandleKind.ShortPos).Contains(mouse);
			bool flag5 = GetEdgeHandleScreen(roi, EdgeHandleKind.ShortNeg).Contains(mouse);
			bool flag6 = HitRoiBody(roi, mouse);
			if (!flag && !flag2 && !flag3 && !flag4 && !flag5 && !flag6)
			{
				return false;
			}
			if (AutoClearOverlaysOnRoiEdit)
			{
				ClearOverlays();
				ClearHud();
			}
			SetSelection(kind, index);
			_startMouseScr = mouse;
			_startRoi = roi;
			_roiDirtyDuringEdit = false;
			if (flag)
			{
				_edit = EditMode.Rotate;
			}
			else if (flag2)
			{
				_edit = EditMode.ResizeLongPos;
			}
			else if (flag3)
			{
				_edit = EditMode.ResizeLongNeg;
			}
			else if (flag4)
			{
				_edit = EditMode.ResizeShortPos;
			}
			else if (flag5)
			{
				_edit = EditMode.ResizeShortNeg;
			}
			else
			{
				_edit = EditMode.Move;
			}
			Invalidate();
			return true;
		}

		private bool HitSelectCircleRoi(CanvasRoiKind kind, int index, CircleRoiF roi, Point mouse)
		{
			if (roi.IsEmpty)
			{
				return false;
			}
			bool flag = GetCircleRadiusHandleScreen(roi).Contains(mouse);
			bool flag2 = HitCircleBody(roi, mouse);
			if (!flag && !flag2)
			{
				return false;
			}
			if (AutoClearOverlaysOnRoiEdit)
			{
				ClearOverlays();
				ClearHud();
			}
			SetSelection(kind, index);
			_startMouseScr = mouse;
			_startCircleRoi = roi;
			_roiDirtyDuringEdit = false;
			_edit = ((!flag) ? EditMode.Move : EditMode.ResizeCircle);
			Invalidate();
			return true;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_bmp == null)
			{
				return;
			}
			PointF pt = ScreenToImage(e.Location);
			this.ImageMouseMove?.Invoke(pt, e.Button);
			if (_panning)
			{
				_pan.X += e.X - _lastMouse.X;
				_pan.Y += e.Y - _lastMouse.Y;
				_lastMouse = e.Location;
				Invalidate();
			}
			else
			{
				if (!ShowRois || !SelectedRoi.IsValid || _edit == EditMode.None)
				{
					return;
				}
				if (AutoClearOverlaysOnRoiEdit && (OverlayLines.Count > 0 || OverlayPoints.Count > 0 || !string.IsNullOrEmpty(HudText)))
				{
					ClearOverlays();
					ClearHud();
				}
				PointF curImg = ScreenToImage(e.Location);
				PointF pointF = ScreenToImage(_startMouseScr);
				RotRectF result = _startRoi;
				if (SelectedRoi.Kind == CanvasRoiKind.CircleBase1 || SelectedRoi.Kind == CanvasRoiKind.CircleBase2 || SelectedRoi.Kind == CanvasRoiKind.CirclePoint)
				{
					CircleRoiF circleBySelection = GetCircleBySelection(SelectedRoi);
					CircleRoiF circleRoiF = _startCircleRoi;
					if (_edit == EditMode.Move)
					{
						float num = curImg.X - pointF.X;
						float num2 = curImg.Y - pointF.Y;
						circleRoiF.Cx += num;
						circleRoiF.Cy += num2;
						circleRoiF = ClampCircleToImage(circleRoiF);
					}
					else if (_edit == EditMode.ResizeCircle)
					{
						float num3 = curImg.X - circleRoiF.Cx;
						float num4 = curImg.Y - circleRoiF.Cy;
						circleRoiF.Radius = Math.Max(3f, (float)Math.Sqrt(num3 * num3 + num4 * num4));
						circleRoiF = ClampCircleToImage(circleRoiF);
					}
					if (!CircleApproximatelyEquals(circleBySelection, circleRoiF))
					{
						SetCircleBySelection(SelectedRoi, circleRoiF);
						_roiDirtyDuringEdit = true;
						Invalidate();
					}
					return;
				}
				if (_edit == EditMode.Move)
				{
					float num5 = curImg.X - pointF.X;
					float num6 = curImg.Y - pointF.Y;
					result.Cx += num5;
					result.Cy += num6;
					result = ClampRoiToImage(result);
				}
				else if (_edit == EditMode.Rotate)
				{
					float num7 = pointF.X - result.Cx;
					float num8 = pointF.Y - result.Cy;
					float num9 = curImg.X - result.Cx;
					float num10 = curImg.Y - result.Cy;
					float num11 = (float)Math.Atan2(num8, num7);
					float num12 = (float)Math.Atan2(num10, num9);
					float num13 = num12 - num11;
					result.Phi = RotRectF.NormalizeAngle(_startRoi.Phi + num13);
				}
				else
				{
					ApplyEdgeResize(_startRoi, curImg, _edit, out result);
					result = ClampRoiToImage(result);
				}
				RotRectF roiBySelection = GetRoiBySelection(SelectedRoi);
				if (!RoiApproximatelyEquals(roiBySelection, result))
				{
					SetRoiBySelection(SelectedRoi, result);
					_roiDirtyDuringEdit = true;
					Invalidate();
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (_bmp != null)
			{
				PointF pt = ScreenToImage(e.Location);
				this.ImageMouseUp?.Invoke(pt, e.Button);
			}
			if (_panning)
			{
				_panning = false;
				Cursor = Cursors.Default;
				return;
			}
			bool flag = _edit != EditMode.None && _roiDirtyDuringEdit;
			_edit = EditMode.None;
			_roiDirtyDuringEdit = false;
			if (flag)
			{
				this.RoiChanged?.Invoke();
			}
		}

		private Rectangle GetEdgeHandleScreen(RotRectF roi, EdgeHandleKind kind)
		{
			PointF p;
			switch (kind)
			{
			case EdgeHandleKind.LongPos:
				p = roi.GetEdgeMidLongPos();
				break;
			case EdgeHandleKind.LongNeg:
				p = roi.GetEdgeMidLongNeg();
				break;
			case EdgeHandleKind.ShortPos:
				p = roi.GetEdgeMidShortPos();
				break;
			default:
				p = roi.GetEdgeMidShortNeg();
				break;
			}
			PointF pointF = ImageToScreen(p);
			int num = 10;
			return new Rectangle((int)pointF.X - num / 2, (int)pointF.Y - num / 2, num, num);
		}

		private Rectangle GetRotateHandleScreen(RotRectF roi)
		{
			PointF rotateCorner = roi.GetRotateCorner();
			PointF pointF = ImageToScreen(rotateCorner);
			int num = 12;
			return new Rectangle((int)pointF.X - num / 2, (int)pointF.Y - num / 2, num, num);
		}

		private Rectangle GetCircleRadiusHandleScreen(CircleRoiF roi)
		{
			PointF pointF = ImageToScreen(new PointF(roi.Center.X + roi.Radius, roi.Center.Y));
			int num = 10;
			return new Rectangle((int)pointF.X - num / 2, (int)pointF.Y - num / 2, num, num);
		}

		private bool HitRoiBody(RotRectF roi, Point pScreen)
		{
			if (roi.IsEmpty)
			{
				return false;
			}
			PointF pointF = ScreenToImage(pScreen);
			float num = pointF.X - roi.Cx;
			float num2 = pointF.Y - roi.Cy;
			float num3 = (float)Math.Cos(0f - roi.Phi);
			float num4 = (float)Math.Sin(0f - roi.Phi);
			float value = num * num3 - num2 * num4;
			float value2 = num * num4 + num2 * num3;
			return Math.Abs(value) <= roi.HalfLen1 && Math.Abs(value2) <= roi.HalfLen2;
		}

		private bool HitCircleBody(CircleRoiF roi, Point pScreen)
		{
			if (roi.IsEmpty)
			{
				return false;
			}
			PointF pointF = ScreenToImage(pScreen);
			float num = pointF.X - roi.Center.X;
			float num2 = pointF.Y - roi.Center.Y;
			float num3 = (float)Math.Sqrt(num * num + num2 * num2);
			return num3 <= roi.Radius;
		}

		private void ApplyEdgeResize(RotRectF startRoi, PointF curImg, EditMode mode, out RotRectF result)
		{
			result = startRoi;
			float num = curImg.X - startRoi.Cx;
			float num2 = curImg.Y - startRoi.Cy;
			float num3 = (float)Math.Cos(0f - startRoi.Phi);
			float num4 = (float)Math.Sin(0f - startRoi.Phi);
			float val = num * num3 - num2 * num4;
			float val2 = num * num4 + num2 * num3;
			PointF axisU = startRoi.GetAxisU();
			PointF axisV = startRoi.GetAxisV();
			switch (mode)
			{
			case EditMode.ResizeLongPos:
			{
				float num12 = 0f - startRoi.HalfLen1;
				float num13 = Math.Max(num12 + 6f, val);
				result.HalfLen1 = (num13 - num12) * 0.5f;
				float num14 = (num13 + num12) * 0.5f;
				result.Cx = startRoi.Cx + axisU.X * num14;
				result.Cy = startRoi.Cy + axisU.Y * num14;
				break;
			}
			case EditMode.ResizeLongNeg:
			{
				float halfLen2 = startRoi.HalfLen1;
				float num10 = Math.Min(halfLen2 - 6f, val);
				result.HalfLen1 = (halfLen2 - num10) * 0.5f;
				float num11 = (halfLen2 + num10) * 0.5f;
				result.Cx = startRoi.Cx + axisU.X * num11;
				result.Cy = startRoi.Cy + axisU.Y * num11;
				break;
			}
			case EditMode.ResizeShortPos:
			{
				float num7 = 0f - startRoi.HalfLen2;
				float num8 = Math.Max(num7 + 6f, val2);
				result.HalfLen2 = (num8 - num7) * 0.5f;
				float num9 = (num8 + num7) * 0.5f;
				result.Cx = startRoi.Cx + axisV.X * num9;
				result.Cy = startRoi.Cy + axisV.Y * num9;
				break;
			}
			case EditMode.ResizeShortNeg:
			{
				float halfLen = startRoi.HalfLen2;
				float num5 = Math.Min(halfLen - 6f, val2);
				result.HalfLen2 = (halfLen - num5) * 0.5f;
				float num6 = (halfLen + num5) * 0.5f;
				result.Cx = startRoi.Cx + axisV.X * num6;
				result.Cy = startRoi.Cy + axisV.Y * num6;
				break;
			}
			}
			result.HalfLen1 = Math.Max(3f, result.HalfLen1);
			result.HalfLen2 = Math.Max(3f, result.HalfLen2);
		}

		private RotRectF ClampRoiToImage(RotRectF rr)
		{
			if (_bmp == null)
			{
				return rr;
			}
			RectangleF rectangleF = rr.BoundsAABB();
			if (rectangleF.IsEmpty)
			{
				return rr;
			}
			float num = _bmp.Width - 1;
			float num2 = _bmp.Height - 1;
			float num3 = 0f;
			float num4 = 0f;
			if (rectangleF.Left < 0f)
			{
				num3 = 0f - rectangleF.Left;
			}
			if (rectangleF.Top < 0f)
			{
				num4 = 0f - rectangleF.Top;
			}
			if (rectangleF.Right > num)
			{
				num3 = num - rectangleF.Right;
			}
			if (rectangleF.Bottom > num2)
			{
				num4 = num2 - rectangleF.Bottom;
			}
			rr.Cx += num3;
			rr.Cy += num4;
			return rr;
		}

		private bool MoveSelectedRoiBy(float dx, float dy)
		{
			if (!AllowRoiEditing || !ShowRois || !SelectedRoi.IsValid || _edit != EditMode.None)
			{
				return false;
			}
			if (dx == 0f && dy == 0f)
			{
				return false;
			}
			if (AutoClearOverlaysOnRoiEdit && (OverlayLines.Count > 0 || OverlayPoints.Count > 0 || !string.IsNullOrEmpty(HudText)))
			{
				ClearOverlays();
				ClearHud();
			}
			if (SelectedRoi.Kind == CanvasRoiKind.CircleBase1 || SelectedRoi.Kind == CanvasRoiKind.CircleBase2 || SelectedRoi.Kind == CanvasRoiKind.CirclePoint)
			{
				CircleRoiF circle = GetCircleBySelection(SelectedRoi);
				if (circle.IsEmpty)
				{
					return false;
				}
				CircleRoiF movedCircle = circle;
				movedCircle.Cx += dx;
				movedCircle.Cy += dy;
				movedCircle = ClampCircleToImage(movedCircle);
				if (CircleApproximatelyEquals(circle, movedCircle))
				{
					return false;
				}
				SetCircleBySelection(SelectedRoi, movedCircle);
				this.RoiChanged?.Invoke();
				Invalidate();
				return true;
			}
			RotRectF roi = GetRoiBySelection(SelectedRoi);
			if (roi.IsEmpty)
			{
				return false;
			}
			RotRectF movedRoi = roi;
			movedRoi.Cx += dx;
			movedRoi.Cy += dy;
			movedRoi = ClampRoiToImage(movedRoi);
			if (RoiApproximatelyEquals(roi, movedRoi))
			{
				return false;
			}
			SetRoiBySelection(SelectedRoi, movedRoi);
			this.RoiChanged?.Invoke();
			Invalidate();
			return true;
		}

		private RotRectF GetRoiBySelection(CanvasRoiSelection sel)
		{
			switch (sel.Kind)
			{
			case CanvasRoiKind.Template:
				return TemplateRoi;
			case CanvasRoiKind.Base:
				if (sel.Index >= 0 && sel.Index < _baseRois.Count)
				{
					return _baseRois[sel.Index];
				}
				break;
			case CanvasRoiKind.Detect:
				if (sel.Index >= 0 && sel.Index < _detectRois.Count)
				{
					return _detectRois[sel.Index];
				}
				break;
			}
			return default(RotRectF);
		}

		private CircleRoiF GetCircleBySelection(CanvasRoiSelection sel)
		{
			if (sel.Kind == CanvasRoiKind.CirclePoint)
			{
				return (sel.Index >= 0 && sel.Index < _circlePointRois.Count) ? _circlePointRois[sel.Index] : default(CircleRoiF);
			}
			if (sel.Index < 0 || sel.Index >= _circleBaseRois.Count)
			{
				return default(CircleRoiF);
			}
			if (sel.Kind == CanvasRoiKind.CircleBase1)
			{
				return _circleBaseRois[sel.Index].Circle1;
			}
			if (sel.Kind == CanvasRoiKind.CircleBase2)
			{
				return _circleBaseRois[sel.Index].Circle2;
			}
			return default(CircleRoiF);
		}

		private void SetRoiBySelection(CanvasRoiSelection sel, RotRectF roi)
		{
			switch (sel.Kind)
			{
			case CanvasRoiKind.Template:
				TemplateRoi = roi;
				break;
			case CanvasRoiKind.Base:
				if (sel.Index >= 0 && sel.Index < _baseRois.Count)
				{
					_baseRois[sel.Index] = roi;
				}
				break;
			case CanvasRoiKind.Detect:
				if (sel.Index >= 0 && sel.Index < _detectRois.Count)
				{
					_detectRois[sel.Index] = roi;
				}
				break;
			}
		}

		private void SetCircleBySelection(CanvasRoiSelection sel, CircleRoiF roi)
		{
			if (sel.Kind == CanvasRoiKind.CirclePoint)
			{
				if (sel.Index >= 0 && sel.Index < _circlePointRois.Count)
				{
					_circlePointRois[sel.Index] = roi;
				}
			}
			else if (sel.Index >= 0 && sel.Index < _circleBaseRois.Count)
			{
				CircleBaseRoiPair value = _circleBaseRois[sel.Index];
				if (sel.Kind == CanvasRoiKind.CircleBase1)
				{
					value.Circle1 = roi;
				}
				else if (sel.Kind == CanvasRoiKind.CircleBase2)
				{
					value.Circle2 = roi;
				}
				_circleBaseRois[sel.Index] = value;
			}
		}

		private CircleRoiF ClampCircleToImage(CircleRoiF circle)
		{
			if (_bmp == null || circle.IsEmpty)
			{
				return circle;
			}
			float num = _bmp.Width - 1;
			float num2 = _bmp.Height - 1;
			circle.Radius = Math.Max(3f, Math.Min(circle.Radius, Math.Min(num, num2)));
			if (circle.Cx - circle.Radius < 0f)
			{
				circle.Cx = circle.Radius;
			}
			if (circle.Cy - circle.Radius < 0f)
			{
				circle.Cy = circle.Radius;
			}
			if (circle.Cx + circle.Radius > num)
			{
				circle.Cx = num - circle.Radius;
			}
			if (circle.Cy + circle.Radius > num2)
			{
				circle.Cy = num2 - circle.Radius;
			}
			return circle;
		}

		private void NormalizeSelectionAfterRoiChange()
		{
			if (!SelectedRoi.IsValid)
			{
				return;
			}
			switch (SelectedRoi.Kind)
			{
			case CanvasRoiKind.Template:
				if (TemplateRoi.IsEmpty || !ShowTemplateRoi)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				break;
			case CanvasRoiKind.Base:
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _baseRois.Count)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				break;
			case CanvasRoiKind.Detect:
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _detectRois.Count)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				break;
			case CanvasRoiKind.CircleBase1:
			case CanvasRoiKind.CircleBase2:
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _circleBaseRois.Count)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				break;
			case CanvasRoiKind.CirclePoint:
				if (SelectedRoi.Index < 0 || SelectedRoi.Index >= _circlePointRois.Count)
				{
					SetSelection(CanvasRoiKind.None, -1);
				}
				break;
			}
		}

		private static bool RoiApproximatelyEquals(RotRectF a, RotRectF b)
		{
			if (a.IsEmpty && b.IsEmpty)
			{
				return true;
			}
			if (a.IsEmpty != b.IsEmpty)
			{
				return false;
			}
			return Math.Abs(a.Cx - b.Cx) < 0.001f && Math.Abs(a.Cy - b.Cy) < 0.001f && Math.Abs(a.Phi - b.Phi) < 0.001f && Math.Abs(a.HalfLen1 - b.HalfLen1) < 0.001f && Math.Abs(a.HalfLen2 - b.HalfLen2) < 0.001f;
		}

		private static bool CircleApproximatelyEquals(CircleRoiF a, CircleRoiF b)
		{
			if (a.IsEmpty && b.IsEmpty)
			{
				return true;
			}
			if (a.IsEmpty != b.IsEmpty)
			{
				return false;
			}
			return Math.Abs(a.Cx - b.Cx) < 0.001f && Math.Abs(a.Cy - b.Cy) < 0.001f && Math.Abs(a.Radius - b.Radius) < 0.001f;
		}
	}
}
