namespace PictureFrame
{
    partial class Form1
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
            this.toolStrip1 = new GUIToolbar();
            this.pictureBox1 = new GUIPictureBox();
            this.statusTextBox1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            //this.toolStrip1.Size = new System.Drawing.Size(600, 64);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1"; // relevant?
            this.toolStrip1.AutoSize = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.DoDrawGrid = false;
            this.pictureBox1.Location = new System.Drawing.Point(0, toolStrip1.Size.Height); // note: assumes toolstrip's size is set in its constructer, which it currently is
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(800, 375);
            this.pictureBox1.state = null;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // statusTextBox1
            // 
            this.statusTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right))); // am I using this right?
            this.statusTextBox1.Location = new System.Drawing.Point(0, pictureBox1.Location.Y + pictureBox1.Size.Height); // right under PictureBox
            this.statusTextBox1.Name = "statusTextBox1";
            this.statusTextBox1.Size = new System.Drawing.Size(800, 25); // TODO: make it a bit bigger. Changing size here doesn't help. // font size seems to directly change text box size
            this.statusTextBox1.TabIndex = 6; // relevant?
            this.statusTextBox1.Text = "statusTextBox1";
            //this.statusTextBox1.ReadOnly = true;
            this.statusTextBox1.Font = new System.Drawing.Font(this.statusTextBox1.Font.FontFamily, 14); // DEBUGGING: original font was 8.25
            //this.statusTextBox1.BackColor = System.Drawing.Color.Red; // DEBUGGING: make the textbox more visible...remove later
            //this.statusTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None; // this cuts off text...
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, this.statusTextBox1.Location.Y + this.statusTextBox1.Size.Height); // make sure text box has enough room
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.statusTextBox1);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(200, this.toolStrip1.Height + this.statusTextBox1.Size.Height + 100); // at least 100 pixels of image, otherwise hard to tell something is there...
            this.Name = "Form1";
            this.Text = "Form1";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GUIPictureBox pictureBox1;
        private GUIToolbar toolStrip1;
        private System.Windows.Forms.Label statusTextBox1;
        // the following replaced by fields in GUIToolbar
        //private System.Windows.Forms.ToolStripButton toolStripButton1;
        //private System.Windows.Forms.ToolStripButton toolStripButton2;
        //private System.Windows.Forms.ToolStripButton toolStripButton3;
        //private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        //private System.Windows.Forms.ToolStripButton toolStripButton4;
    }
}

