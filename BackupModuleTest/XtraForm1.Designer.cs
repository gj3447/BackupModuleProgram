namespace BackupModuleTest
{
    partial class XtraForm1
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
            listBoxControl1 = new DevExpress.XtraEditors.ListBoxControl();
            ((System.ComponentModel.ISupportInitialize)listBoxControl1).BeginInit();
            SuspendLayout();
            // 
            // listBoxControl1
            // 
            listBoxControl1.Location = new System.Drawing.Point(12, 2);
            listBoxControl1.Name = "listBoxControl1";
            listBoxControl1.Size = new System.Drawing.Size(756, 1080);
            listBoxControl1.TabIndex = 0;
            // 
            // XtraForm1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(780, 1094);
            Controls.Add(listBoxControl1);
            Name = "XtraForm1";
            Text = "XtraForm1";
            ((System.ComponentModel.ISupportInitialize)listBoxControl1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.ListBoxControl listBoxControl1;
    }
}