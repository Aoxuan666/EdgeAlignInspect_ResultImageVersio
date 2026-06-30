using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	partial class TemplateSettingsForm
	{
		private IContainer components = null;
		private SplitContainer splitMain;
		private HalconCanvas canvas;
		private Panel rightPanel;
		private GroupBox grpContour;
		private CheckBox chkOuterOnly;
		private NumericUpDown numSigma;
		private NumericUpDown numLow;
		private NumericUpDown numHigh;
		private NumericUpDown numMinDistance;
		private NumericUpDown numBins;
		private NumericUpDown numEraseRadius;
		private Label lblCount;
		private Label lblStatus;
		private Button btnExtract;
		private Button btnErase;
		private Button btnReset;
		private Button btnCreateModel;
		private Button btnTestMatch;
		private Button btnOk;
		private Button btnCancel;

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
			this.components = new Container();
			this.splitMain = new SplitContainer();
			this.canvas = new HalconCanvas();
			this.rightPanel = new Panel();
			this.grpContour = new GroupBox();
			this.chkOuterOnly = new CheckBox();
			this.numSigma = new NumericUpDown();
			this.numLow = new NumericUpDown();
			this.numHigh = new NumericUpDown();
			this.numMinDistance = new NumericUpDown();
			this.numBins = new NumericUpDown();
			this.numEraseRadius = new NumericUpDown();
			this.lblCount = new Label();
			this.lblStatus = new Label();
			this.btnExtract = new Button();
			this.btnErase = new Button();
			this.btnReset = new Button();
			this.btnCreateModel = new Button();
			this.btnTestMatch = new Button();
			this.btnOk = new Button();
			this.btnCancel = new Button();
			((ISupportInitialize)this.splitMain).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.rightPanel.SuspendLayout();
			this.grpContour.SuspendLayout();
			((ISupportInitialize)this.numSigma).BeginInit();
			((ISupportInitialize)this.numLow).BeginInit();
			((ISupportInitialize)this.numHigh).BeginInit();
			((ISupportInitialize)this.numMinDistance).BeginInit();
			((ISupportInitialize)this.numBins).BeginInit();
			((ISupportInitialize)this.numEraseRadius).BeginInit();
			base.SuspendLayout();
			base.AutoScaleMode = AutoScaleMode.Font;
			this.Font = new Font("Microsoft YaHei UI", 9f);
			base.ClientSize = new Size(1180, 720);
			this.MinimumSize = new Size(980, 620);
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "模板设置";
			this.splitMain.Dock = DockStyle.Fill;
			this.splitMain.FixedPanel = FixedPanel.Panel2;
			this.splitMain.SplitterWidth = 6;
			this.splitMain.Panel1MinSize = 320;
			this.splitMain.Panel2MinSize = 340;
			this.splitMain.Panel1.Controls.Add(this.canvas);
			this.splitMain.Panel2.Controls.Add(this.rightPanel);
			this.canvas.Dock = DockStyle.Fill;
			this.canvas.BackColor = Color.Black;
			this.rightPanel.Dock = DockStyle.Fill;
			this.rightPanel.BackColor = Color.FromArgb(247, 248, 250);
			this.rightPanel.Padding = new Padding(10);
			this.rightPanel.Controls.Add(this.grpContour);
			this.rightPanel.Controls.Add(this.btnOk);
			this.rightPanel.Controls.Add(this.btnCancel);
			this.grpContour.Text = "外轮廓模板";
			this.grpContour.Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);
			this.grpContour.SetBounds(10, 10, 330, 560);
			this.grpContour.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.chkOuterOnly.Text = "只使用外轮廓创建模板";
			this.chkOuterOnly.AutoSize = false;
			this.chkOuterOnly.SetBounds(18, 30, 250, 28);
			this.grpContour.Controls.Add(this.chkOuterOnly);
			AddLabel(this.grpContour, "边缘 Sigma", 18, 70, 92, 26);
			SetupNum(this.numSigma, 0.2m, 10m, 2, 1.2m, 122, 70, 170, 26);
			this.grpContour.Controls.Add(this.numSigma);
			AddLabel(this.grpContour, "低阈值", 18, 106, 92, 26);
			SetupNum(this.numLow, 1m, 255m, 1, 15m, 122, 106, 170, 26);
			this.grpContour.Controls.Add(this.numLow);
			AddLabel(this.grpContour, "高阈值", 18, 142, 92, 26);
			SetupNum(this.numHigh, 2m, 255m, 1, 35m, 122, 142, 170, 26);
			this.grpContour.Controls.Add(this.numHigh);
			AddLabel(this.grpContour, "点最小间距", 18, 178, 92, 26);
			SetupNum(this.numMinDistance, 1m, 80m, 1, 6m, 122, 178, 170, 26);
			this.grpContour.Controls.Add(this.numMinDistance);
			AddLabel(this.grpContour, "角度分桶", 18, 214, 92, 26);
			SetupNum(this.numBins, 24m, 1440m, 0, 360m, 122, 214, 170, 26);
			this.grpContour.Controls.Add(this.numBins);
			AddLabel(this.grpContour, "擦除半径", 18, 250, 92, 26);
			SetupNum(this.numEraseRadius, 2m, 120m, 1, 12m, 122, 250, 170, 26);
			this.grpContour.Controls.Add(this.numEraseRadius);
			this.lblCount.Text = "特征点: 0";
			this.lblCount.Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold);
			this.lblCount.ForeColor = Color.FromArgb(36, 96, 145);
			this.lblCount.SetBounds(18, 292, 274, 28);
			this.grpContour.Controls.Add(this.lblCount);
			this.btnExtract.Text = "提取外轮廓点";
			this.btnExtract.SetBounds(18, 330, 274, 34);
			this.grpContour.Controls.Add(this.btnExtract);
			this.btnErase.Text = "擦除点";
			this.btnErase.SetBounds(18, 374, 132, 34);
			this.grpContour.Controls.Add(this.btnErase);
			this.btnReset.Text = "重新提取";
			this.btnReset.SetBounds(160, 374, 132, 34);
			this.grpContour.Controls.Add(this.btnReset);
			this.btnCreateModel.Text = "创建模型";
			this.btnCreateModel.SetBounds(18, 418, 274, 36);
			this.grpContour.Controls.Add(this.btnCreateModel);
			this.btnTestMatch.Text = "测试匹配";
			this.btnTestMatch.SetBounds(18, 462, 274, 34);
			this.grpContour.Controls.Add(this.btnTestMatch);
			this.lblStatus.Text = "框选或调整模板 ROI 后提取外轮廓点。";
			this.lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
			this.lblStatus.AutoSize = false;
			this.lblStatus.SetBounds(18, 504, 292, 42);
			this.grpContour.Controls.Add(this.lblStatus);
			this.btnOk.Text = "确定";
			this.btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnOk.SetBounds(148, 664, 90, 32);
			this.btnOk.DialogResult = DialogResult.OK;
			this.btnCancel.Text = "取消";
			this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnCancel.SetBounds(248, 664, 90, 32);
			this.btnCancel.DialogResult = DialogResult.Cancel;
			base.AcceptButton = this.btnOk;
			base.CancelButton = this.btnCancel;
			base.Controls.Add(this.splitMain);
			this.splitMain.SplitterDistance = 814;
			this.splitMain.Panel1.ResumeLayout(false);
			this.splitMain.Panel2.ResumeLayout(false);
			//((ISupportInitialize)this.splitMain).EndInit();
			this.splitMain.ResumeLayout(false);
			this.rightPanel.ResumeLayout(false);
			this.grpContour.ResumeLayout(false);
			((ISupportInitialize)this.numSigma).EndInit();
			((ISupportInitialize)this.numLow).EndInit();
			((ISupportInitialize)this.numHigh).EndInit();
			((ISupportInitialize)this.numMinDistance).EndInit();
			((ISupportInitialize)this.numBins).EndInit();
			((ISupportInitialize)this.numEraseRadius).EndInit();
			base.ResumeLayout(false);
		}

		private static void AddLabel(Control parent, string text, int x, int y, int w, int h)
		{
			Label label = new Label();
			label.Text = text;
			label.AutoSize = false;
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular);
			label.SetBounds(x, y, w, h);
			parent.Controls.Add(label);
		}

		private static void SetupNum(NumericUpDown num, decimal min, decimal max, int decimals, decimal value, int x, int y, int w, int h)
		{
			num.Minimum = min;
			num.Maximum = max;
			num.DecimalPlaces = decimals;
			num.Increment = decimals == 0 ? 1m : 0.5m;
			num.Value = value;
			num.SetBounds(x, y, w, h);
		}
	}
}
