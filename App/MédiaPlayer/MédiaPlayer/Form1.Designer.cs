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
            button1 = new Button();
            listBox1 = new ListBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ButtonShadow;
            pictureBox1.Location = new Point(51, 70);
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
            // button1
            // 
            button1.Location = new Point(525, 375);
            button1.Name = "button1";
            button1.Size = new Size(81, 22);
            button1.TabIndex = 2;
            button1.Text = "Send Request";
            button1.Click += button1_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(262, 94);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(344, 244);
            listBox1.TabIndex = 1;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15F);
            label2.Location = new Point(81, 149);
            label2.Name = "label2";
            label2.Size = new Size(116, 28);
            label2.TabIndex = 4;
            label2.Text = "Mes Médias";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F);
            label3.Location = new Point(60, 189);
            label3.Name = "label3";
            label3.Size = new Size(160, 28);
            label3.TabIndex = 5;
            label3.Text = "Média des autres";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 15F);
            label4.Location = new Point(93, 250);
            label4.Name = "label4";
            label4.Size = new Size(82, 28);
            label4.TabIndex = 6;
            label4.Text = "Réglage";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 422);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(listBox1);
            Controls.Add(button1);
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
        private Button button1;
        private ListBox listBox1;
        private Label label2;
        private Label label3;
        private Label label4;
    }
}
