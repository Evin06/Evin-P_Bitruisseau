﻿namespace MédiaPlayer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            MusicList = new TreeView();
            vScrollBar1 = new VScrollBar();
            button1 = new Button();
            listBox1 = new ListBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ButtonShadow;
            pictureBox1.Location = new Point(53, 40);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(183, 362);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20F);
            label1.Location = new Point(302, 20);
            label1.Name = "label1";
            label1.Size = new Size(194, 37);
            label1.TabIndex = 1;
            label1.Text = "MEDIA PLAYER";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15F);
            label2.Location = new Point(84, 122);
            label2.Name = "label2";
            label2.Size = new Size(112, 28);
            label2.TabIndex = 2;
            label2.Text = "Mon média";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F);
            label3.Location = new Point(84, 182);
            label3.Name = "label3";
            label3.Size = new Size(119, 28);
            label3.TabIndex = 3;
            label3.Text = "Autre média";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 15F);
            label4.Location = new Point(84, 248);
            label4.Name = "label4";
            label4.Size = new Size(86, 28);
            label4.TabIndex = 4;
            label4.Text = "réglages";
            // 
            // MusicList
            // 
            MusicList.Location = new Point(271, 113);
            MusicList.Name = "MusicList";
            MusicList.Size = new Size(369, 214);
            MusicList.TabIndex = 5;

            // 
            // vScrollBar1
            // 
            vScrollBar1.Location = new Point(660, 145);
            vScrollBar1.Name = "vScrollBar1";
            vScrollBar1.Size = new Size(17, 80);
            vScrollBar1.TabIndex = 6;
            // 
            // button1
            // 
            button1.Location = new Point(669, 379);
            button1.Name = "button1";
            button1.Size = new Size(107, 46);
            button1.TabIndex = 7;
            button1.Text = "Send Request";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(302, 140);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(316, 169);
            listBox1.TabIndex = 8;
 
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(listBox1);
            Controls.Add(button1);
            Controls.Add(vScrollBar1);
            Controls.Add(MusicList);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TreeView MusicList;
        private VScrollBar vScrollBar1;
        private Button button1;
        private ListBox listBox1;
    }
}
