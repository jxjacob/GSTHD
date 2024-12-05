using GSTHD.Properties;

namespace GSTHD
{
    partial class progresswindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(progresswindow));
            this.connectProgress = new System.Windows.Forms.ProgressBar();
            this.waitinglabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // connectProgress
            // 
            this.connectProgress.Location = new System.Drawing.Point(12, 45);
            this.connectProgress.Name = "connectProgress";
            this.connectProgress.Size = new System.Drawing.Size(335, 23);
            this.connectProgress.TabIndex = 0;
            // 
            // waitinglabel
            // 
            this.waitinglabel.AutoSize = true;
            this.waitinglabel.Location = new System.Drawing.Point(9, 9);
            this.waitinglabel.Name = "waitinglabel";
            this.waitinglabel.Size = new System.Drawing.Size(35, 13);
            this.waitinglabel.TabIndex = 1;
            this.waitinglabel.Text = "label1";
            // 
            // progresswindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 81);
            this.Controls.Add(this.waitinglabel);
            this.Controls.Add(this.connectProgress);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "progresswindow";
            this.Text = "Connecting to Emulator";
            this.Load += new System.EventHandler(this.progresswindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar connectProgress;
        private System.Windows.Forms.Label waitinglabel;
    }
}