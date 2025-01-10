namespace MédiaPlayer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            label1 = new Label();
            buttonSend = new Button();
            listBox1 = new ListBox();
            buttonMesMedias = new Button();
            buttonMediaAutres = new Button();
            buttonReglage = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ButtonShadow;
            pictureBox1.Location = new Point(30, 70);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(175, 281);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20F);
            label1.Location = new Point(262, 19);
            label1.Name = "label1";
            label1.Size = new Size(194, 37);
            label1.TabIndex = 3;
            label1.Text = "MEDIA PLAYER";

            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(262, 94);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(344, 244);
            listBox1.TabIndex = 1;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            // 
            // buttonMesMedias
            // 
            buttonMesMedias.Font = new Font("Segoe UI", 12F);
            buttonMesMedias.Location = new Point(60, 136);
            buttonMesMedias.Name = "buttonMesMedias";
            buttonMesMedias.Size = new Size(120, 30);
            buttonMesMedias.TabIndex = 4;
            buttonMesMedias.Text = "Mes Médias";
            buttonMesMedias.UseVisualStyleBackColor = true;
            buttonMesMedias.Click += buttonMesMedias_Click;
            // 
            // buttonMediaAutres
            // 
            buttonMediaAutres.Font = new Font("Segoe UI", 12F);
            buttonMediaAutres.Location = new Point(51, 192);
            buttonMediaAutres.Name = "buttonMediaAutres";
            buttonMediaAutres.Size = new Size(140, 30);
            buttonMediaAutres.TabIndex = 5;
            buttonMediaAutres.Text = "Média des autres";
            buttonMediaAutres.UseVisualStyleBackColor = true;
            buttonMediaAutres.Click += buttonMediaAutres_Click;

            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 422);
            Controls.Add(buttonReglage);
            Controls.Add(buttonMediaAutres);
            Controls.Add(buttonMesMedias);
            Controls.Add(listBox1);
            Controls.Add(buttonSend);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private PictureBox pictureBox1;
        private Label label1;
        private Button buttonSend;
        private ListBox listBox1;
        private Button buttonMesMedias;
        private Button buttonMediaAutres;
        private Button buttonReglage;
    }
}
