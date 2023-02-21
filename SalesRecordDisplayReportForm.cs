using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SALE_Entry_Form
{
    public partial class SalesRecordDisplayReportForm : Form
    {
        public SalesRecordDisplayReportForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //1. Address of SQL Server and Database
            string strConnection = "Data Source=localhost\\SQLExpress;Initial Catalog=SalesDB;Integrated Security=True";

            //2. Establish Connection
            SqlConnection con = new SqlConnection(strConnection);

            //3. Open Connection
            con.Open();

            //4. Prepare and Execute Query
            DateTime dtFromDate, dtToDate;
            dtFromDate = dateTimePicker1.Value.Date;
            dtToDate = dateTimePicker2.Value.Date;
            SqlCommand cmd = new SqlCommand("spGetSalesRecord", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@fromDate", dtFromDate);
            cmd.Parameters.AddWithValue("@toDate", dtToDate);

            //SqlDataReader is stored in datatable to fill the table in crystal report
            SalesReport sr = new SalesReport();
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            sr.SetDataSource(dt);
            crystalReportViewer1.ReportSource = sr;

            //5. Close Connection
            con.Close();
        }
    }
}
