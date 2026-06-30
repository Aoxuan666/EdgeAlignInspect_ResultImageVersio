using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	public class Form2 : Form
	{
		private static string _imagePath;

		private EdgeInspectJob _job = null;

		private readonly TemplateEdgeInspectSdk _sdk = new TemplateEdgeInspectSdk();

		private IContainer components = null;

		private Button button1;

		private Button button2;

		public Form2()
		{
			InitializeComponent();
			button1.Text = "选择图片";
			button2.Text = "模拟调用SDK";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "选择测试图片";
				openFileDialog.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
				openFileDialog.Multiselect = false;
				openFileDialog.CheckFileExists = true;
				openFileDialog.CheckPathExists = true;
				if (openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					_imagePath = openFileDialog.FileName;
					Text = "已选择: " + Path.GetFileName(_imagePath);
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_imagePath))
			{
				MessageBox.Show(this, "请先点击“选择图片”。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			try
			{
				using (Bitmap bitmap = new Bitmap(_imagePath))
				{
					if (_job == null)
					{
						_job = BuildMockJob(bitmap);
					}
					EdgeInspectJob edgeInspectJob = _sdk.OpenSetupDialog(bitmap, _job, CreateToleranceOptions());
					if (edgeInspectJob == null)
					{
						MessageBox.Show(this, "用户取消了参数设置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}
					_job = edgeInspectJob.DeepClone();
					MessageBox.Show(this, "已成功返回 Job。\n再次点击“模拟调用SDK”会继续回显上次修改后的参数和 ROI。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "调用 SDK 失败：\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private EdgeInspectJob BuildMockJob(Bitmap bmp)
		{
			int num = bmp.Width;
			int num2 = bmp.Height;
			EdgeInspectJob edgeInspectJob = new EdgeInspectJob
			{
				UseReferenceLine = true,
				DetectMode = DefectDetectMode.Both,
				Match = new TemplateMatchParameters
				{
					Enabled = true,
					NumLevels = 4,
					AngleStart = -0.25,
					AngleExtent = 0.5,
					MinScore = 0.65
				},
				BaseCaliper = new CaliperParameters
				{
					NumMeasures = 26,
					MeasureLength = 28.0,
					MeasureWidth = 5.0,
					Sigma = 1.5,
					Threshold = 18.0,
					MeasureInterpolation = "bicubic",
					MeasureSelect = "first",
					Transition = "negative"
				},
				DetectCaliper = new CaliperParameters
				{
					NumMeasures = 36,
					MeasureLength = 22.0,
					MeasureWidth = 4.0,
					Sigma = 1.2,
					Threshold = 12.0,
					MeasureInterpolation = "bicubic",
					MeasureSelect = "first",
					Transition = "negative"
				},
				TeachData = new TemplateTeachData()
			};
			edgeInspectJob.TemplateRoi = RotRectF.FromAxisAligned(new RectangleF((float)num * 0.1f, (float)num2 * 0.1f, (float)num * 0.18f, (float)num2 * 0.18f));
			edgeInspectJob.BaseRois.Add(new BaseRoiItem
			{
				Name = "基准1",
				Roi = RotRectF.FromAxisAligned(new RectangleF((float)num * 0.55f, (float)num2 * 0.18f, (float)num * 0.12f, (float)num2 * 0.22f)),
				Caliper = edgeInspectJob.BaseCaliper.DeepClone()
			});
			edgeInspectJob.BaseRois.Add(new BaseRoiItem
			{
				Name = "基准2",
				Roi = RotRectF.FromAxisAligned(new RectangleF((float)num * 0.7f, (float)num2 * 0.38f, (float)num * 0.12f, (float)num2 * 0.22f)),
				Caliper = new CaliperParameters
				{
					NumMeasures = 28,
					MeasureLength = 26.0,
					MeasureWidth = 4.0,
					Sigma = 1.0,
					Threshold = 16.0,
					MeasureInterpolation = "bicubic",
					MeasureSelect = "first",
					Transition = "positive"
				}
			});
			edgeInspectJob.DetectItems.Add(new DetectRoiItem
			{
				Name = "检测1",
				Enabled = true,
				BaseRoiIndex = 0,
				AngleReference = DetectAngleReferenceMode.ParallelToBase,
				NominalDistancePx = 18.5,
				BurrTolerancePx = 1.8,
				DentTolerancePx = 2.6,
				Roi = RotRectF.FromAxisAligned(new RectangleF((float)num * 0.08f, (float)num2 * 0.55f, (float)num * 0.14f, (float)num2 * 0.28f)),
				Caliper = edgeInspectJob.DetectCaliper.DeepClone()
			});
			edgeInspectJob.DetectItems.Add(new DetectRoiItem
			{
				Name = "检测2",
				Enabled = true,
				BaseRoiIndex = 1,
				AngleReference = DetectAngleReferenceMode.ParallelToBase,
				NominalDistancePx = 16.0,
				BurrTolerancePx = 2.0,
				DentTolerancePx = 2.2,
				Roi = RotRectF.FromAxisAligned(new RectangleF((float)num * 0.3f, (float)num2 * 0.55f, (float)num * 0.14f, (float)num2 * 0.26f)),
				Caliper = new CaliperParameters
				{
					NumMeasures = 40,
					MeasureLength = 24.0,
					MeasureWidth = 3.0,
					Sigma = 1.0,
					Threshold = 10.0,
					MeasureInterpolation = "bicubic",
					MeasureSelect = "first",
					Transition = "positive"
				}
			});
			edgeInspectJob.DetectItems.Add(new DetectRoiItem
			{
				Name = "检测3",
				Enabled = true,
				BaseRoiIndex = 0,
				AngleReference = DetectAngleReferenceMode.Horizontal,
				NominalDistancePx = 0.0,
				BurrTolerancePx = 1.5,
				DentTolerancePx = 1.5,
				Roi = RotRectF.FromAxisAligned(new RectangleF((float)num * 0.5f, (float)num2 * 0.58f, (float)num * 0.14f, (float)num2 * 0.24f)),
				Caliper = new CaliperParameters
				{
					NumMeasures = 32,
					MeasureLength = 20.0,
					MeasureWidth = 4.0,
					Sigma = 1.3,
					Threshold = 14.0,
					MeasureInterpolation = "bicubic",
					MeasureSelect = "first",
					Transition = "all"
				}
			});
			edgeInspectJob.Normalize();
			return edgeInspectJob;
		}

		private static EdgeInspectionToleranceOptions CreateToleranceOptions()
		{
			return new EdgeInspectionToleranceOptions
			{
				BurrToleranceMm = 0.05,
				DentToleranceMm = 0.05,
				OverEdgeToleranceMm = 0.05,
				CopperLeakToleranceMm = 0.05,
				PixelResolutionX = 0.01,
				PixelResolutionY = 0.01
			};
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			base.SuspendLayout();
			this.button1.Location = new System.Drawing.Point(204, 177);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(button1_Click);
			this.button2.Location = new System.Drawing.Point(393, 177);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 0;
			this.button2.Text = "button2";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(button2_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 15f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(800, 450);
			base.Controls.Add(this.button2);
			base.Controls.Add(this.button1);
			base.Name = "Form2";
			this.Text = "Form2";
			base.ResumeLayout(false);
		}
	}
}
