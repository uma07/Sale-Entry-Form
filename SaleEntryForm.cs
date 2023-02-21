using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace SALE_Entry_Form
{
    public partial class SaleEntryForm : Form
    {
        public SaleEntryForm()
        {
            InitializeComponent();
        }
        private int item_num = 1;
        DataTable dt = new DataTable();

        private void DataGridCreation()
        {
            dt.Columns.Add("ItemNo", Type.GetType("System.Int32"));
            dt.Columns.Add("ItemName", Type.GetType("System.String"));
            dt.Columns.Add("QTY", Type.GetType("System.Double"));
            dt.Columns.Add("Tax %", Type.GetType("System.String"));
            dt.Columns.Add("Price", Type.GetType("System.Double"));
            dt.Columns.Add("Total Amt", Type.GetType("System.Double"));
            dt.Columns.Add("Tax Amt", Type.GetType("System.Double"));
            dataGridView1.DataSource = dt;
            dataGridView1.AllowUserToAddRows = false;
        }

        // When SALE Entry form is loaded, we load the data grid with columns
        private void SaleEntryForm_Load(object sender, EventArgs e)
        {
            DataGridCreation();
            textBox1.Text = GetBillNo().ToString();
            textBox1.ReadOnly = true;
            textBox2.Text = item_num.ToString();
            textBox1.ReadOnly = true;
        }

        // Add Button transfers the user entered data into datagrid
        private void button2_Click(object sender, EventArgs e)
        {
            double dPrice, dTax, dTotalAmt, dTaxAmt, dQty;
            dPrice = textBox6.Text == ""? 0.0 : Convert.ToDouble(textBox6.Text);
            dTax = comboBox1.Text == "" ? 0.0 : Convert.ToDouble(comboBox1.Text);
            dQty = textBox5.Text == "" ? 0.0 : Convert.ToDouble(textBox5.Text);
            dTotalAmt = dPrice * dQty;
            dTaxAmt = (dPrice * dTax * dQty) / 100;
            dt.Rows.Add(textBox2.Text, textBox4.Text, dQty.ToString(), dTax.ToString(), dPrice.ToString(), dTotalAmt.ToString(), dTaxAmt.ToString());
            dataGridView1.DataSource= dt;

            // Clear item details
            textBox2.Clear();
            textBox4.Clear();
            textBox5.Clear();
            comboBox1.SelectedIndex= -1;
            textBox6.Clear();
            item_num++;
            textBox2.Text = item_num.ToString();
        }

        // Save Button saves the datagrid data and customer data into database
        private void button1_Click(object sender, EventArgs e)
        {
            //1. Address of SQL Server and Database
            string strConnection = "Data Source=localhost\\SQLExpress;Initial Catalog=SalesDB;Integrated Security=True";

            //2. Establish Connection
            SqlConnection con = new SqlConnection(strConnection);

            //3. Open Connection
            con.Open();

            //4. Prepare and Execute Query
            double dTotalTax = 0.0, dTotal = 0.0;
            
            int iBillNo = Convert.ToInt32(textBox1.Text);
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                int iItemNo = Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value);
                string strItemName = dataGridView1.Rows[i].Cells[1].Value.ToString();
                double dQty = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);
                int iTax = Convert.ToInt32(dataGridView1.Rows[i].Cells[3].Value);
                double dPrice = Convert.ToDouble(dataGridView1.Rows[i].Cells[4].Value);
                dTotal += Convert.ToDouble(dataGridView1.Rows[i].Cells[5].Value);
                dTotalTax += Convert.ToDouble(dataGridView1.Rows[i].Cells[6].Value);

                SqlCommand cmd1 = new SqlCommand("SpInsertSaleDetail", con);
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.Parameters.AddWithValue("@Billno", iBillNo);
                cmd1.Parameters.AddWithValue("@ItemNo", iItemNo);
                cmd1.Parameters.AddWithValue("@ItemName", strItemName);
                cmd1.Parameters.AddWithValue("@QTY", dQty);
                cmd1.Parameters.AddWithValue("@Tax", iTax);
                cmd1.Parameters.AddWithValue("@Price", dPrice);
                cmd1.ExecuteNonQuery();
            }
            string strCustomer = textBox3.Text;
            DateTime date = dateTimePicker1.Value.Date;
            SqlCommand cmd2 = new SqlCommand("SpInsertSaleMaster", con);
            cmd2.CommandType = CommandType.StoredProcedure;
            cmd2.Parameters.AddWithValue("@Billno", iBillNo);
            cmd2.Parameters.AddWithValue("@Customer", strCustomer);
            cmd2.Parameters.AddWithValue("@Date", date);
            cmd2.Parameters.AddWithValue("@Tax", dTotalTax);
            cmd2.Parameters.AddWithValue("@Total", dTotal);
            cmd2.ExecuteNonQuery();

            //5. Close Connection
            con.Close();

            //Clear datagrid & customer details
            dt.Clear();
            dataGridView1.Refresh();
            textBox1.Clear();
            textBox3.Clear();
            dateTimePicker1.ResetText();
            item_num = 1;
            textBox1.Text = GetBillNo().ToString();
            textBox2.Text = item_num.ToString();
        }

        // Report button shows a new form to display sales records between 2 chosen dates
        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            SalesRecordDisplayReportForm salesRecordDisplayReportForm = new SalesRecordDisplayReportForm();
            salesRecordDisplayReportForm.ShowDialog();
            if(!salesRecordDisplayReportForm.IsHandleCreated)
            {
                this.Show();
                item_num = 1;
                textBox1.Text = GetBillNo().ToString();
                textBox2.Text = item_num.ToString();
            }
        }

        private int GetBillNo()
        {
            //1. Address of SQL Server and Database
            string strConnection = "Data Source=localhost\\SQLExpress;Initial Catalog=SalesDB;Integrated Security=True";

            //2. Establish Connection
            SqlConnection con = new SqlConnection(strConnection);

            //3. Open Connection
            con.Open();

            //4. Prepare and Execute Query
            SqlCommand cmd = new SqlCommand("GetLastBillNo", con);
            int bill = 0;
            var reader = cmd.ExecuteReader();
            if (reader.Read())
                bill = Convert.ToInt32(reader[0].ToString());

            //Close the connection
            con.Close();

            return bill+1;
        }
    }
}
