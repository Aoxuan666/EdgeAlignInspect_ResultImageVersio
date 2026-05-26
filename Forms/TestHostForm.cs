using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	public sealed class TestHostForm : Form
	{
		private readonly TemplateEdgeInspectSdk _sdk = new TemplateEdgeInspectSdk();
		private Bitmap _image;
		private EdgeInspectJob _job;
		private Bitmap _resultImage;

		private Button btnLoadImage;
		private Button btnSetupJob;
		private Button btnRunWithImage;
		private Button btnSelfTest;
		private NumericUpDown numTolerance;
		private NumericUpDown numResolutionX;
		private NumericUpDown numResolutionY;
		private Label lblStatus;
		private PictureBox picResult;

		public TestHostForm()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_image?.Dispose();
				_resultImage?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			btnLoadImage = new Button();
			btnSetupJob = new Button();
			btnRunWithImage = new Button();
			btnSelfTest = new Button();
			numTolerance = new NumericUpDown();
			numResolutionX = new NumericUpDown();
			numResolutionY = new NumericUpDown();
			lblStatus = new Label();
			picResult = new PictureBox();
			((System.ComponentModel.ISupportInitialize)numTolerance).BeginInit();
			((System.ComponentModel.ISupportInitialize)numResolutionX).BeginInit();
			((System.ComponentModel.ISupportInitialize)numResolutionY).BeginInit();
			((System.ComponentModel.ISupportInitialize)picResult).BeginInit();
			SuspendLayout();

			Text = "上位机调用测试窗口";
			StartPosition = FormStartPosition.CenterScreen;
			ClientSize = new Size(1180, 760);
			MinimumSize = new Size(980, 640);

			btnLoadImage.Text = "1. 加载图片";
			btnLoadImage.SetBounds(16, 16, 120, 32);
			btnLoadImage.Click += delegate { LoadImage(); };

			btnSetupJob.Text = "2. 配置Job";
			btnSetupJob.SetBounds(148, 16, 120, 32);
			btnSetupJob.Click += delegate { SetupJob(); };

			AddLabel("公差", 286, 22, 42, 22);
			SetupNumber(numTolerance, 0m, 10000m, 4, 2m, 328, 18, 90, 28);
			AddLabel("X mm/px", 432, 22, 64, 22);
			SetupNumber(numResolutionX, 0.000001m, 10000m, 6, 1m, 496, 18, 96, 28);
			AddLabel("Y mm/px", 606, 22, 64, 22);
			SetupNumber(numResolutionY, 0.000001m, 10000m, 6, 1m, 670, 18, 96, 28);

			btnRunWithImage.Text = "3. 运行并获取效果图";
			btnRunWithImage.SetBounds(790, 16, 170, 32);
			btnRunWithImage.Click += delegate { RunWithResultImage(); };

			btnSelfTest.Text = "一键自测返回图";
			btnSelfTest.SetBounds(980, 16, 150, 32);
			btnSelfTest.Click += delegate { RunSelfTest(); };

			lblStatus.AutoSize = false;
			lblStatus.SetBounds(16, 60, 1120, 42);
			lblStatus.Text = "按顺序加载图片、配置 Job，然后运行；下方显示 SDK 返回的 ResultImage。";
			lblStatus.ForeColor = Color.FromArgb(70, 70, 70);

			picResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			picResult.BorderStyle = BorderStyle.FixedSingle;
			picResult.BackColor = Color.Black;
			picResult.SizeMode = PictureBoxSizeMode.Zoom;
			picResult.SetBounds(16, 112, 1148, 628);

			Controls.Add(btnLoadImage);
			Controls.Add(btnSetupJob);
			Controls.Add(numTolerance);
			Controls.Add(numResolutionX);
			Controls.Add(numResolutionY);
			Controls.Add(btnRunWithImage);
			Controls.Add(btnSelfTest);
			Controls.Add(lblStatus);
			Controls.Add(picResult);

			((System.ComponentModel.ISupportInitialize)numTolerance).EndInit();
			((System.ComponentModel.ISupportInitialize)numResolutionX).EndInit();
			((System.ComponentModel.ISupportInitialize)numResolutionY).EndInit();
			((System.ComponentModel.ISupportInitialize)picResult).EndInit();
			ResumeLayout(false);
		}

		private void AddLabel(string text, int x, int y, int w, int h)
		{
			Label label = new Label
			{
				Text = text,
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft
			};
			label.SetBounds(x, y, w, h);
			Controls.Add(label);
		}

		private static void SetupNumber(NumericUpDown n, decimal min, decimal max, int decimals, decimal value, int x, int y, int w, int h)
		{
			n.Minimum = min;
			n.Maximum = max;
			n.DecimalPlaces = decimals;
			n.Increment = decimals >= 4 ? 0.001m : 0.1m;
			n.Value = value;
			n.SetBounds(x, y, w, h);
		}

		private void LoadImage()
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|All Files|*.*";
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				using (Bitmap loaded = new Bitmap(dialog.FileName))
				{
					_image?.Dispose();
					_image = new Bitmap(loaded);
				}
				lblStatus.Text = "已加载图片：" + dialog.FileName;
			}
		}

		private void SetupJob()
		{
			if (_image == null)
			{
				MessageBox.Show(this, "请先加载图片。");
				return;
			}
			EdgeInspectJob newJob = _sdk.OpenSetupDialog(_image, _job);
			if (newJob == null)
			{
				lblStatus.Text = "配置窗口取消，Job 未更新。";
				return;
			}
			_job = newJob;
			lblStatus.Text = "Job 已配置。现在可以点击“运行并获取效果图”。";
		}

		private void RunWithResultImage()
		{
			if (_image == null)
			{
				MessageBox.Show(this, "请先加载图片。");
				return;
			}
			if (_job == null)
			{
				MessageBox.Show(this, "请先配置 Job。");
				return;
			}

			EdgeInspectResult result = _sdk.RunInspection(
				_image,
				_job,
				(double)numTolerance.Value,
				(double)numResolutionX.Value,
				(double)numResolutionY.Value,
				true);

			_resultImage?.Dispose();
			_resultImage = result.ResultImage == null ? null : new Bitmap(result.ResultImage);
			picResult.Image = _resultImage;

			if (_resultImage == null)
			{
				lblStatus.Text = "调用完成，但 ResultImage 为空。结果：" + result.Message;
				MessageBox.Show(this, "ResultImage 为空。返回信息：\r\n" + result.Message);
			}
			else
			{
				lblStatus.Text = $"调用完成，ResultImage 已返回。结果={(result.Success ? "OK" : "NG")}，图片尺寸={_resultImage.Width}x{_resultImage.Height}";
			}
		}

		private void RunSelfTest()
		{
			string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SelfTestOutput");
			int exitCode = SdkResultImageSelfTest.Run(outputDirectory);
			string resultPath = Path.Combine(outputDirectory, "sdk_selftest_result.png");
			string logPath = Path.Combine(outputDirectory, "sdk_selftest_log.txt");
			string log = File.Exists(logPath) ? File.ReadAllText(logPath) : "";
			if (exitCode != 0 || !File.Exists(resultPath))
			{
				lblStatus.Text = "一键自测失败：" + log;
				MessageBox.Show(this, "一键自测失败：\r\n" + log);
				return;
			}
			using (Bitmap loaded = new Bitmap(resultPath))
			{
				_resultImage?.Dispose();
				_resultImage = new Bitmap(loaded);
			}
			picResult.Image = _resultImage;
			lblStatus.Text = $"一键自测完成，SDK 已返回 ResultImage，图片尺寸={_resultImage.Width}x{_resultImage.Height}，已保存：{resultPath}";
		}
	}
}
