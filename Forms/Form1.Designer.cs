namespace EdgeAlignInspect
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.SplitContainer splitMain;
        private EdgeAlignInspect.HalconCanvas canvas;
        private System.Windows.Forms.Panel rightPanel;

        // 操作区
        private System.Windows.Forms.GroupBox grpOps;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnAddBaseRoi;
        private System.Windows.Forms.Button btnAddCircleBaseRoi;
        private System.Windows.Forms.Button btnAddDetectRoi;
        private System.Windows.Forms.Button btnDeleteRoi;
        private System.Windows.Forms.Button btnSaveTeach;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label lblRoiHint;

        // 参数区
        private System.Windows.Forms.GroupBox grpParams;

        // 当前检测ROI参数
        private System.Windows.Forms.CheckBox chkDetectEnabled;
        private System.Windows.Forms.NumericUpDown numNominal;
        private System.Windows.Forms.ComboBox cboAngleRef;
        private System.Windows.Forms.ComboBox cboDetectBindBase;

        // 全局参数
        private System.Windows.Forms.ComboBox cboDetectMode;
        private System.Windows.Forms.CheckBox chkUseReferenceLine;

        private System.Windows.Forms.ComboBox cboBasePol;
        private System.Windows.Forms.ComboBox cboDetPol;

        private System.Windows.Forms.NumericUpDown numBaseMeasures;
        private System.Windows.Forms.NumericUpDown numBaseSigma;
        private System.Windows.Forms.NumericUpDown numBaseTh;
        private System.Windows.Forms.NumericUpDown numBaseOutward;

        private System.Windows.Forms.NumericUpDown numDetMeasures;
        private System.Windows.Forms.NumericUpDown numDetSigma;
        private System.Windows.Forms.NumericUpDown numDetTh;

        // 模板匹配参数
        private System.Windows.Forms.CheckBox chkEnableMatch;
        private System.Windows.Forms.NumericUpDown numMatchMinScore;
        private System.Windows.Forms.NumericUpDown numMatchAngleStart;
        private System.Windows.Forms.NumericUpDown numMatchAngleExtent;

        // 结果区
        private System.Windows.Forms.Panel cardResult;
        private System.Windows.Forms.Label lblResultTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblResultTip;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.canvas = new EdgeAlignInspect.HalconCanvas();
            this.rightPanel = new System.Windows.Forms.Panel();

            this.grpOps = new System.Windows.Forms.GroupBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnAddBaseRoi = new System.Windows.Forms.Button();
            this.btnAddCircleBaseRoi = new System.Windows.Forms.Button();
            this.btnAddDetectRoi = new System.Windows.Forms.Button();
            this.btnDeleteRoi = new System.Windows.Forms.Button();
            this.btnSaveTeach = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lblRoiHint = new System.Windows.Forms.Label();

            this.grpParams = new System.Windows.Forms.GroupBox();

            this.chkDetectEnabled = new System.Windows.Forms.CheckBox();
            this.numNominal = new System.Windows.Forms.NumericUpDown();
            this.cboAngleRef = new System.Windows.Forms.ComboBox();
            this.cboDetectBindBase = new System.Windows.Forms.ComboBox();

            this.cboDetectMode = new System.Windows.Forms.ComboBox();
            this.chkUseReferenceLine = new System.Windows.Forms.CheckBox();

            this.cboBasePol = new System.Windows.Forms.ComboBox();
            this.cboDetPol = new System.Windows.Forms.ComboBox();

            this.numBaseMeasures = new System.Windows.Forms.NumericUpDown();
            this.numBaseSigma = new System.Windows.Forms.NumericUpDown();
            this.numBaseTh = new System.Windows.Forms.NumericUpDown();
            this.numBaseOutward = new System.Windows.Forms.NumericUpDown();

            this.numDetMeasures = new System.Windows.Forms.NumericUpDown();
            this.numDetSigma = new System.Windows.Forms.NumericUpDown();
            this.numDetTh = new System.Windows.Forms.NumericUpDown();

            this.chkEnableMatch = new System.Windows.Forms.CheckBox();
            this.numMatchMinScore = new System.Windows.Forms.NumericUpDown();
            this.numMatchAngleStart = new System.Windows.Forms.NumericUpDown();
            this.numMatchAngleExtent = new System.Windows.Forms.NumericUpDown();

            this.cardResult = new System.Windows.Forms.Panel();
            this.lblResultTitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblResultTip = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();

            this.rightPanel.SuspendLayout();
            this.grpOps.SuspendLayout();
            this.grpParams.SuspendLayout();
            this.cardResult.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.numNominal)).BeginInit();

            ((System.ComponentModel.ISupportInitialize)(this.numBaseMeasures)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBaseSigma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBaseTh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBaseOutward)).BeginInit();

            ((System.ComponentModel.ISupportInitialize)(this.numDetMeasures)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDetSigma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDetTh)).BeginInit();

            ((System.ComponentModel.ISupportInitialize)(this.numMatchMinScore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMatchAngleStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMatchAngleExtent)).BeginInit();

            this.SuspendLayout();

            // Form1
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.ClientSize = new System.Drawing.Size(1320, 780);
            this.MinimumSize = new System.Drawing.Size(1160, 700);
            this.Text = "智茂检测";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            // splitMain
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitMain.SplitterWidth = 6;
            // 右侧宽度由 Form1.cs 的 FixRightWidth() 动态控制

            // canvas
            this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvas.BackColor = System.Drawing.Color.Black;
            this.splitMain.Panel1.Controls.Add(this.canvas);

            // rightPanel
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.BackColor = System.Drawing.Color.FromArgb(247, 248, 250);
            this.rightPanel.Padding = new System.Windows.Forms.Padding(10);
            this.rightPanel.AutoScroll = true;
            this.rightPanel.Controls.Add(this.grpOps);
            this.rightPanel.Controls.Add(this.grpParams);
            this.rightPanel.Controls.Add(this.cardResult);
            this.splitMain.Panel2.Controls.Add(this.rightPanel);

            // grpOps
            this.grpOps.Text = "操作";
            this.grpOps.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.grpOps.SetBounds(8, 8, 476, 164);
            this.grpOps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));

            SetupButton(this.btnLoad, "加载图片", 14, 24, 96, 30, false);
            SetupButton(this.btnDeleteRoi, "删除选中ROI", 118, 24, 118, 30, false);
            SetupButton(this.btnRun, "运行", 376, 24, 96, 30, true);

            SetupButton(this.btnAddBaseRoi, "添加线基准", 14, 60, 142, 30, false);
            SetupButton(this.btnAddCircleBaseRoi, "添加圆基准", 166, 60, 142, 30, false);
            SetupButton(this.btnAddDetectRoi, "添加检测ROI", 318, 60, 154, 30, false);

            SetupButton(this.btnSaveTeach, "保存(示教)", 14, 96, 222, 30, false);
            SetupButton(this.btnConfirm, "确认配置", 250, 96, 222, 30, false);

            this.lblRoiHint.Text = "直接点击左侧图像中的 ROI 进行选中、拖动、旋转和缩放；毛刺公差由上位机传入。";
            this.lblRoiHint.Font = new System.Drawing.Font("微软雅黑", 8.5F, System.Drawing.FontStyle.Regular);
            this.lblRoiHint.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this.lblRoiHint.AutoSize = false;
            this.lblRoiHint.SetBounds(14, 132, 458, 22);
            this.lblRoiHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));

            this.grpOps.Controls.Add(this.btnLoad);
            this.grpOps.Controls.Add(this.btnDeleteRoi);
            this.grpOps.Controls.Add(this.btnRun);
            this.grpOps.Controls.Add(this.btnAddBaseRoi);
            this.grpOps.Controls.Add(this.btnAddCircleBaseRoi);
            this.grpOps.Controls.Add(this.btnAddDetectRoi);
            this.grpOps.Controls.Add(this.btnSaveTeach);
            this.grpOps.Controls.Add(this.btnConfirm);
            this.grpOps.Controls.Add(this.lblRoiHint);

            // grpParams
            this.grpParams.Text = "参数";
            this.grpParams.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.grpParams.SetBounds(8, 180, 476, 470);
            this.grpParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));

            int h = 28;
            int gap = 8;
            int y = 20;

            // 当前检测ROI参数
            AddSubTitle(this.grpParams, "当前选中检测ROI参数", 14, y, 446);
            y += 26;

            this.chkDetectEnabled.Text = "启用当前检测ROI";
            this.chkDetectEnabled.AutoSize = false;
            this.chkDetectEnabled.SetBounds(100, y, 160, h);
            this.grpParams.Controls.Add(this.chkDetectEnabled);
            y += h + gap;

            AddLabel(this.grpParams, "关联基准ROI", 14, y, 78, h);
            SetupCombo(this.cboDetectBindBase, 100, y, 150, h, 180);
            this.grpParams.Controls.Add(this.cboDetectBindBase);

            AddLabel(this.grpParams, "角度参考", 260, y, 58, h);
            SetupAngleRefCombo(this.cboAngleRef, 322, y, 138, h);
            this.grpParams.Controls.Add(this.cboAngleRef);
            y += h + gap;

            AddLabel(this.grpParams, "标准距离", 14, y, 78, h);
            SetupNum(this.numNominal, -10000, 10000, 2, 0, 100, y, 150, h);
            this.grpParams.Controls.Add(this.numNominal);

            y += h + 12;

            int leftX = 14;
            int leftW = 216;
            int rightX = 244;
            int rightW = 216;
            int secY = y;

            // 左列 - 全局判定参数
            AddSubTitle(this.grpParams, "全局判定参数", leftX, secY, leftW);
            int ly = secY + 26;

            this.chkUseReferenceLine.Text = "使用基准线判定";
            this.chkUseReferenceLine.AutoSize = false;
            this.chkUseReferenceLine.SetBounds(leftX + 82, ly, 128, h);
            this.grpParams.Controls.Add(this.chkUseReferenceLine);
            ly += h + gap;

            AddLabel(this.grpParams, "检测模式", leftX, ly, 72, h);
            SetupDetectModeCombo(this.cboDetectMode, leftX + 82, ly, 128, h);
            this.grpParams.Controls.Add(this.cboDetectMode);
            ly += h + 12;

            // 左列 - 模板匹配
            AddSubTitle(this.grpParams, "模板匹配", leftX, ly, leftW);
            ly += 26;

            this.chkEnableMatch.Text = "启用模板匹配";
            this.chkEnableMatch.Checked = true;
            this.chkEnableMatch.AutoSize = false;
            this.chkEnableMatch.SetBounds(leftX + 82, ly, 128, h);
            this.grpParams.Controls.Add(this.chkEnableMatch);
            ly += h + gap;

            AddLabel(this.grpParams, "最小分数", leftX, ly, 72, h);
            SetupNum(this.numMatchMinScore, 0.00m, 1.00m, 2, 0.50m, leftX + 82, ly, 128, h);
            this.grpParams.Controls.Add(this.numMatchMinScore);
            ly += h + gap;

            AddLabel(this.grpParams, "角度起始", leftX, ly, 72, h);
            SetupNum(this.numMatchAngleStart, -3.14m, 3.14m, 3, -0.300m, leftX + 82, ly, 128, h);
            this.grpParams.Controls.Add(this.numMatchAngleStart);
            ly += h + gap;

            AddLabel(this.grpParams, "角度范围", leftX, ly, 72, h);
            SetupNum(this.numMatchAngleExtent, 0.00m, 6.28m, 3, 0.600m, leftX + 82, ly, 128, h);
            this.grpParams.Controls.Add(this.numMatchAngleExtent);

            // 右列 - 找边极性
            AddSubTitle(this.grpParams, "找边极性", rightX, secY, rightW);
            int ry = secY + 26;

            AddLabel(this.grpParams, "基准极性", rightX, ry, 72, h);
            SetupPolCombo(this.cboBasePol, rightX + 82, ry, 128, h);
            this.grpParams.Controls.Add(this.cboBasePol);
            ry += h + gap;

            AddLabel(this.grpParams, "检测极性", rightX, ry, 72, h);
            SetupPolCombo(this.cboDetPol, rightX + 82, ry, 128, h);
            this.grpParams.Controls.Add(this.cboDetPol);
            ry += h + 12;

            // 右列 - 卡尺参数
            AddSubTitle(this.grpParams, "卡尺参数", rightX, ry, rightW);
            ry += 26;

            AddSmallHeader(this.grpParams, "基准", rightX + 64, ry, 56);
            AddSmallHeader(this.grpParams, "检测", rightX + 140, ry, 56);
            ry += 20;

            AddMiniLabel(this.grpParams, "点数", rightX, ry, 36);
            SetupNum(this.numBaseMeasures, 1, 500, 0, 30, rightX + 44, ry, 74, h);
            this.grpParams.Controls.Add(this.numBaseMeasures);
            SetupNum(this.numDetMeasures, 1, 500, 0, 40, rightX + 128, ry, 74, h);
            this.grpParams.Controls.Add(this.numDetMeasures);
            ry += h + gap;

            AddMiniLabel(this.grpParams, "Sigma", rightX, ry, 36);
            SetupNum(this.numBaseSigma, 0.1m, 50, 1, 1.0m, rightX + 44, ry, 74, h);
            this.grpParams.Controls.Add(this.numBaseSigma);
            SetupNum(this.numDetSigma, 0.1m, 50, 1, 1.0m, rightX + 128, ry, 74, h);
            this.grpParams.Controls.Add(this.numDetSigma);
            ry += h + gap;

            AddMiniLabel(this.grpParams, "阈值", rightX, ry, 36);
            SetupNum(this.numBaseTh, 1, 255, 0, 20, rightX + 44, ry, 74, h);
            this.grpParams.Controls.Add(this.numBaseTh);
            SetupNum(this.numDetTh, 1, 255, 0, 15, rightX + 128, ry, 74, h);
            this.grpParams.Controls.Add(this.numDetTh);
            ry += h + gap;

            AddMiniLabel(this.grpParams, "圆外扩", rightX, ry, 42);
            SetupNum(this.numBaseOutward, 0, 2000, 1, 6.0m, rightX + 44, ry, 74, h);
            this.grpParams.Controls.Add(this.numBaseOutward);

            // cardResult
            this.cardResult.SetBounds(8, 658, 476, 110);
            this.cardResult.BackColor = System.Drawing.Color.White;
            this.cardResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));

            this.lblResultTitle.Text = "检测结果";
            this.lblResultTitle.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lblResultTitle.ForeColor = System.Drawing.Color.FromArgb(55, 55, 55);
            this.lblResultTitle.SetBounds(12, 8, 100, 20);

            this.lblStatus.Text = "READY";
            this.lblStatus.Font = new System.Drawing.Font("Arial", 22F, System.Drawing.FontStyle.Bold);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.BackColor = System.Drawing.Color.LightGray;
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.SetBounds(12, 30, 110, 66);

            this.lblSummary.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this.lblSummary.AutoSize = false;
            this.lblSummary.SetBounds(134, 30, 328, 40);

            this.lblResultTip.Text = "运行后会在左侧图像显示线、点和偏差标注";
            this.lblResultTip.Font = new System.Drawing.Font("微软雅黑", 8.5F);
            this.lblResultTip.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblResultTip.AutoSize = false;
            this.lblResultTip.SetBounds(134, 74, 328, 22);

            this.cardResult.Controls.Add(this.lblResultTitle);
            this.cardResult.Controls.Add(this.lblStatus);
            this.cardResult.Controls.Add(this.lblSummary);
            this.cardResult.Controls.Add(this.lblResultTip);

            // 加入主窗体
            this.Controls.Add(this.splitMain);

            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);

            this.rightPanel.ResumeLayout(false);
            this.grpOps.ResumeLayout(false);
            this.grpParams.ResumeLayout(false);
            this.cardResult.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.numNominal)).EndInit();

            ((System.ComponentModel.ISupportInitialize)(this.numBaseMeasures)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBaseSigma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBaseTh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBaseOutward)).EndInit();

            ((System.ComponentModel.ISupportInitialize)(this.numDetMeasures)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDetSigma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDetTh)).EndInit();

            ((System.ComponentModel.ISupportInitialize)(this.numMatchMinScore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMatchAngleStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMatchAngleExtent)).EndInit();

            this.ResumeLayout(false);
        }

        private static void SetupButton(System.Windows.Forms.Button b, string text, int x, int y, int w, int h, bool primary)
        {
            b.Text = text;
            b.SetBounds(x, y, w, h);
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
            b.BackColor = primary ? System.Drawing.Color.FromArgb(60, 110, 245) : System.Drawing.Color.White;
            b.ForeColor = primary ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(40, 40, 40);
            b.Cursor = System.Windows.Forms.Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        private static void AddLabel(System.Windows.Forms.Control parent, string text, int x, int y, int w, int h)
        {
            var l = new System.Windows.Forms.Label
            {
                Text = text,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular),
                AutoSize = false
            };
            l.SetBounds(x, y + 1, w, h);
            parent.Controls.Add(l);
        }

        private static void AddMiniLabel(System.Windows.Forms.Control parent, string text, int x, int y, int w)
        {
            var l = new System.Windows.Forms.Label
            {
                Text = text,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Font = new System.Drawing.Font("微软雅黑", 8.5F, System.Drawing.FontStyle.Regular),
                AutoSize = false
            };
            l.SetBounds(x, y + 2, w, 20);
            parent.Controls.Add(l);
        }

        private static void AddSubTitle(System.Windows.Forms.Control parent, string text, int x, int y, int width)
        {
            var l = new System.Windows.Forms.Label
            {
                Text = text,
                Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                AutoSize = false
            };
            l.SetBounds(x, y, width, 18);
            parent.Controls.Add(l);

            var line = new System.Windows.Forms.Label
            {
                BackColor = System.Drawing.Color.FromArgb(225, 225, 225),
                AutoSize = false
            };
            line.SetBounds(x, y + 18, width, 1);
            parent.Controls.Add(line);
        }

        private static void AddSmallHeader(System.Windows.Forms.Control parent, string text, int x, int y, int w)
        {
            var l = new System.Windows.Forms.Label
            {
                Text = text,
                Font = new System.Drawing.Font("微软雅黑", 8.5F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(80, 80, 80),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                AutoSize = false
            };
            l.SetBounds(x, y, w, 16);
            parent.Controls.Add(l);
        }

        private static void SetupNum(System.Windows.Forms.NumericUpDown n, decimal min, decimal max, int decimals, decimal val,
            int x, int y, int w, int h)
        {
            n.Minimum = min;
            n.Maximum = max;
            n.DecimalPlaces = decimals;
            if (val < min) val = min;
            if (val > max) val = max;
            n.Value = val;
            n.SetBounds(x, y, w, h);
            n.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
        }

        private static void SetupCombo(System.Windows.Forms.ComboBox c, int x, int y, int w, int h, int dropDownWidth)
        {
            c.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            c.SetBounds(x, y, w, h);
            c.DropDownWidth = dropDownWidth;
        }

        private static void SetupPolCombo(System.Windows.Forms.ComboBox c, int x, int y, int w, int h)
        {
            c.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            c.Items.Clear();
            c.Items.AddRange(new object[] { "白找黑", "黑找白", "任意" });
            c.SelectedIndex = 0;
            c.SetBounds(x, y, w, h);
            c.DropDownWidth = 140;
        }

        private static void SetupDetectModeCombo(System.Windows.Forms.ComboBox c, int x, int y, int w, int h)
        {
            c.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            c.Items.Clear();
            c.Items.AddRange(new object[] { "两者都检测", "只检测毛刺", "只检测凹陷" });
            c.SelectedIndex = 0;
            c.SetBounds(x, y, w, h);
            c.DropDownWidth = 180;
        }

        private static void SetupAngleRefCombo(System.Windows.Forms.ComboBox c, int x, int y, int w, int h)
        {
            c.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            c.Items.Clear();
            c.Items.AddRange(new object[] { "平行于关联基准线", "水平", "竖直" });
            c.SelectedIndex = 0;
            c.SetBounds(x, y, w, h);
            c.DropDownWidth = 220;
        }
    }
}
