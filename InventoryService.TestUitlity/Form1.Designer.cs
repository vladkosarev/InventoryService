namespace InventoryService.TestUitlity
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.InitialHold = new System.Windows.Forms.TextBox();
            this.InitialReservation = new System.Windows.Forms.TextBox();
            this.InitialQuantity = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.NewQuantity = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ResultHold = new System.Windows.Forms.TextBox();
            this.ResultReservation = new System.Windows.Forms.TextBox();
            this.ResultQuantity = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbOoperation = new System.Windows.Forms.ComboBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.InitialHold);
            this.groupBox1.Controls.Add(this.InitialReservation);
            this.groupBox1.Controls.Add(this.InitialQuantity);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(218, 147);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Initial Intentory";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(91, 118);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Gen Random";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // InitialHold
            // 
            this.InitialHold.Location = new System.Drawing.Point(91, 74);
            this.InitialHold.Name = "InitialHold";
            this.InitialHold.Size = new System.Drawing.Size(100, 20);
            this.InitialHold.TabIndex = 5;
            this.InitialHold.TextChanged += new System.EventHandler(this.InitialHold_TextChanged);
            // 
            // InitialReservation
            // 
            this.InitialReservation.Location = new System.Drawing.Point(91, 50);
            this.InitialReservation.Name = "InitialReservation";
            this.InitialReservation.Size = new System.Drawing.Size(100, 20);
            this.InitialReservation.TabIndex = 4;
            this.InitialReservation.TextChanged += new System.EventHandler(this.InitialReservation_TextChanged);
            // 
            // InitialQuantity
            // 
            this.InitialQuantity.Location = new System.Drawing.Point(91, 23);
            this.InitialQuantity.Name = "InitialQuantity";
            this.InitialQuantity.Size = new System.Drawing.Size(100, 20);
            this.InitialQuantity.TabIndex = 3;
            this.InitialQuantity.TextChanged += new System.EventHandler(this.InitialQuantity_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Hold";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Reservation";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Quantity";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnExecute);
            this.groupBox2.Controls.Add(this.NewQuantity);
            this.groupBox2.Controls.Add(this.cmbOoperation);
            this.groupBox2.Location = new System.Drawing.Point(236, 15);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(310, 144);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "New Update";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(186, 23);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(118, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "Gen Random";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // NewQuantity
            // 
            this.NewQuantity.Location = new System.Drawing.Point(71, 23);
            this.NewQuantity.Name = "NewQuantity";
            this.NewQuantity.Size = new System.Drawing.Size(109, 20);
            this.NewQuantity.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Update";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ResultHold);
            this.groupBox3.Controls.Add(this.ResultReservation);
            this.groupBox3.Controls.Add(this.ResultQuantity);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Enabled = false;
            this.groupBox3.Location = new System.Drawing.Point(13, 165);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(218, 110);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Result Inventory";
            // 
            // ResultHold
            // 
            this.ResultHold.Location = new System.Drawing.Point(91, 73);
            this.ResultHold.Name = "ResultHold";
            this.ResultHold.Size = new System.Drawing.Size(100, 20);
            this.ResultHold.TabIndex = 11;
            // 
            // ResultReservation
            // 
            this.ResultReservation.Location = new System.Drawing.Point(91, 49);
            this.ResultReservation.Name = "ResultReservation";
            this.ResultReservation.Size = new System.Drawing.Size(100, 20);
            this.ResultReservation.TabIndex = 10;
            // 
            // ResultQuantity
            // 
            this.ResultQuantity.Location = new System.Drawing.Point(91, 22);
            this.ResultQuantity.Name = "ResultQuantity";
            this.ResultQuantity.Size = new System.Drawing.Size(100, 20);
            this.ResultQuantity.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(18, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Hold";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 51);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Reservation";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Quantity";
            // 
            // cmbOoperation
            // 
            this.cmbOoperation.FormattingEnabled = true;
            this.cmbOoperation.Items.AddRange(new object[] {
            "ReadInventory",
            "Reserve",
            "UpdateQuantity",
            "UpdateQuantityAndHold",
            "PlaceHold",
            "Purchase",
            "PurchaseFromHolds"});
            this.cmbOoperation.Location = new System.Drawing.Point(22, 52);
            this.cmbOoperation.Name = "cmbOoperation";
            this.cmbOoperation.Size = new System.Drawing.Size(282, 21);
            this.cmbOoperation.TabIndex = 3;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(186, 79);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(118, 23);
            this.btnExecute.TabIndex = 4;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.richTextBox1);
            this.groupBox4.Location = new System.Drawing.Point(237, 167);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(312, 108);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Error";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.ForeColor = System.Drawing.Color.Maroon;
            this.richTextBox1.Location = new System.Drawing.Point(3, 16);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(306, 89);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 301);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox InitialHold;
        private System.Windows.Forms.TextBox InitialReservation;
        private System.Windows.Forms.TextBox InitialQuantity;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox NewQuantity;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox ResultHold;
        private System.Windows.Forms.TextBox ResultReservation;
        private System.Windows.Forms.TextBox ResultQuantity;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbOoperation;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

