namespace Spectrograph
{
    partial class Spectrograph
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
            this.components = new System.ComponentModel.Container();
            this.OscilloscopePictureBox = new System.Windows.Forms.PictureBox();
            this.LineSpectrumPictureBox = new System.Windows.Forms.PictureBox();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.SpectrogramPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.OscilloscopePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LineSpectrumPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpectrogramPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // OscilloscopePictureBox
            // 
            this.OscilloscopePictureBox.Location = new System.Drawing.Point(12, 12);
            this.OscilloscopePictureBox.Name = "OscilloscopePictureBox";
            this.OscilloscopePictureBox.Size = new System.Drawing.Size(720, 256);
            this.OscilloscopePictureBox.TabIndex = 0;
            this.OscilloscopePictureBox.TabStop = false;
            // 
            // LineSpectrumPictureBox
            // 
            this.LineSpectrumPictureBox.Location = new System.Drawing.Point(12, 274);
            this.LineSpectrumPictureBox.Name = "LineSpectrumPictureBox";
            this.LineSpectrumPictureBox.Size = new System.Drawing.Size(720, 256);
            this.LineSpectrumPictureBox.TabIndex = 1;
            this.LineSpectrumPictureBox.TabStop = false;
            // 
            // UpdateTimer
            // 
            this.UpdateTimer.Interval = 1;
            this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // SpectrogramPictureBox
            // 
            this.SpectrogramPictureBox.Location = new System.Drawing.Point(12, 536);
            this.SpectrogramPictureBox.Name = "SpectrogramPictureBox";
            this.SpectrogramPictureBox.Size = new System.Drawing.Size(720, 256);
            this.SpectrogramPictureBox.TabIndex = 2;
            this.SpectrogramPictureBox.TabStop = false;
            // 
            // Spectrograph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(745, 802);
            this.Controls.Add(this.SpectrogramPictureBox);
            this.Controls.Add(this.LineSpectrumPictureBox);
            this.Controls.Add(this.OscilloscopePictureBox);
            this.Name = "Spectrograph";
            this.Text = "Spectrograph";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            ((System.ComponentModel.ISupportInitialize)(this.OscilloscopePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LineSpectrumPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpectrogramPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox OscilloscopePictureBox;
        private System.Windows.Forms.PictureBox LineSpectrumPictureBox;
        private System.Windows.Forms.Timer UpdateTimer;
        private System.Windows.Forms.PictureBox SpectrogramPictureBox;
    }
}