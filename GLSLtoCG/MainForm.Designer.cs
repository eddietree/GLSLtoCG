namespace GLSLtoCG
{
    partial class MainForm
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
            this.btn_drop_files = new System.Windows.Forms.Button();
            this.textbox_log = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_drop_files
            // 
            this.btn_drop_files.AllowDrop = true;
            this.btn_drop_files.AutoSize = true;
            this.btn_drop_files.Dock = System.Windows.Forms.DockStyle.Top;
            this.btn_drop_files.Location = new System.Drawing.Point(0, 0);
            this.btn_drop_files.Name = "btn_drop_files";
            this.btn_drop_files.Size = new System.Drawing.Size(284, 149);
            this.btn_drop_files.TabIndex = 0;
            this.btn_drop_files.Text = "Drop Shaders Here";
            this.btn_drop_files.UseVisualStyleBackColor = true;
            this.btn_drop_files.Click += new System.EventHandler(this.btn_drop_files_Click);
            // 
            // textbox_log
            // 
            this.textbox_log.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textbox_log.Enabled = false;
            this.textbox_log.Location = new System.Drawing.Point(0, 149);
            this.textbox_log.Multiline = true;
            this.textbox_log.Name = "textbox_log";
            this.textbox_log.Size = new System.Drawing.Size(284, 112);
            this.textbox_log.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.textbox_log);
            this.Controls.Add(this.btn_drop_files);
            this.Name = "MainForm";
            this.Text = "GLSL to CG";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_drop_files;
        private System.Windows.Forms.TextBox textbox_log;
    }
}