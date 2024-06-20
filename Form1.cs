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
using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System.IO;
using System.Data.OleDb;
using System.Numerics;
using DuckDBExample.ModuleClass;
using System.Windows.Documents;
using System.Data.Common;
using static DuckDB.NET.Native.NativeMethods;


namespace DuckDBExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowTableData();

        }


        private void LoadBasic()
        {
            DuckDBConnection duckDBConnection = new DuckDBConnection("Data Source=D:\\file.db");
            duckDBConnection.Open();

            DuckDBCommand command = duckDBConnection.CreateCommand();
            //command.CommandText = "CREATE TABLE weather (city VARCHAR, temp_lo INTEGER, temp_hi INTEGER, prcp REAL, date DATE);";
            //var executeNonQuery = command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO weather VALUES ('Tbilisi', 41, 55, 0.1, '2020-04-02');";
            var executeNonQuery = command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO weather VALUES ('San Francisco', 46, 50, 0.25, '1994-11-27');";
            executeNonQuery = command.ExecuteNonQuery();

            command.CommandText = "Select count(*) from weather";
            var count = command.ExecuteScalar();


            //MessageBox.Show("Count :" + count);


            var command1 = duckDBConnection.CreateCommand();
            // command1.CommandText = "SELECT city, (temp_hi + temp_lo) / 2 AS temp_avg, date FROM weather;";
            command1.CommandText = "SELECT city,temp_hi , temp_lo, date FROM weather;";

            var reader = command1.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Columns.Add("City");
            dt.Columns.Add("temp_lo");
            dt.Columns.Add("temp_hi");
            dt.Columns.Add("Date");

            DataRow dr;
            while (reader.Read())
            {
                //MessageBox.Show("City:"+ reader.GetString(0) +"; Average Temperature:"+ reader.GetDouble(1) + "; Date: "+ reader.GetDateTime(2));
                dr = dt.NewRow();
                dr[0] = reader.GetString(0);
                dr[1] = reader.GetDouble(1);
                dr[2] = reader.GetDouble(2);
                dr[3] = reader.GetDateTime(3);
                dt.Rows.Add(dr);

            }

            dataGridView1.DataSource = dt;


        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLoadBasicData_Click(object sender, EventArgs e)
        {
            LoadBasic();
        }

        private void btnCSVData_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();           
            ofd.Filter = SelectExcelSheet();
            ofd.ShowDialog();
            


            string excelPath = ofd.FileName;
            string connectionString = "";
            string fileExtension = Path.GetExtension(excelPath);
           
            connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excelPath + ";" + "Extended Properties='Excel 12.0 Xml;HDR=NO;IMEX=1'";

            DataTable sheets = GetSchemaTable(connectionString);

            DuckDBConnection duckDBConnection = new DuckDBConnection("Data Source=D:\\Tempalte.db");
            duckDBConnection.Open();

            string sql = string.Empty;
            string sql1 = string.Empty;
            DataSet ds1 = new DataSet();
            DataSet ds = new DataSet();

            foreach (DataRow dr in sheets.Rows)
            { 
               
               
                string WorkSheetName = dr["TABLE_NAME"].ToString().Trim();

                if (WorkSheetName == "Deductor$"|| WorkSheetName == "Challan$" || WorkSheetName == "Salary$" || WorkSheetName == "Deductee$")
                {
                    sql = "SELECT * FROM [" + WorkSheetName + "]";
                    ds.Clear();

                    OleDbDataAdapter data = new OleDbDataAdapter(sql, connectionString);
                    data.Fill(ds);

                    DataTable dt1 = ds.Tables[0];
                    string columnNames = string.Empty;

                    foreach (DataColumn col in dt1.Columns)
                    {
                        columnNames += col + " VARCHAR,";

                    }
                    columnNames = columnNames.Substring(0, columnNames.Length - 1);
                    WorkSheetName = WorkSheetName.Replace("$", "");
                   
                    DuckDBCommand command = duckDBConnection.CreateCommand();

                    string str = " CREATE TABLE " + WorkSheetName + "(" + columnNames + ")";
                    command.CommandText = str;


                    try
                    {
                        var executeNonQuery = command.ExecuteNonQuery();

                    }
                    catch { }


                    foreach (DataRow dr1 in dt1.Rows)
                    {

                        using (var appender = duckDBConnection.CreateAppender(WorkSheetName))
                        {
                            var row = appender.CreateRow();
                            for (var i = 0; i < dt1.Columns.Count; i++)
                            {
                                row.AppendValue(dr1.ItemArray[i].ToString());
                            }
                            row.EndRow();
                        }
                    


                    }

                }

            }
            MessageBox.Show(" Process complete.");

        }


        private void ShowTableData()
        {
            //    DuckDBConnection duckDBConnection = new DuckDBConnection("Data Source=D:\\Tempalte.db");
            //    duckDBConnection.Open();


            //    for (int i = 0; i <= tables.Rows.Count - 1; i++)
            //    { comboBox1.Items.Add(tables .Rows [i].ItemArray [0]); }


            comboBox1.Items.Add("Challan");
            comboBox1.Items.Add("Deductor");
            comboBox1.Items.Add("Salary");
            comboBox1.Items.Add("Deductee");
        }

        public string SelectExcelSheet()
        {
            //Logic to validate which type of Excelsheet is allowed and which is not.
            string typeOfExcelAllowed;
            typeOfExcelAllowed = "Excel File (*.xls)|*.xls|Excel File (*.xlsx*)|*.xlsx*";                     
            
            return typeOfExcelAllowed;
        }

        static DataTable GetSchemaTable(string connectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                DataTable schemaTable = connection.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables,
                    new object[] { null, null, null, "TABLE" });
                return schemaTable;
            }
        }

        private void notinuse()
        {
            //    DuckDBConnection duckDBConnection = new DuckDBConnection();
            //   // duckDBConnection.Open();

            //    DuckDBCommand command = duckDBConnection.CreateCommand();
            //    command.CommandText = "SELECT * FROM read_csv('flights.csv', header = false, delim = '|')";
            //    duckDBConnection.Open();
            //    var reader = command.ExecuteReader();

            //    DataTable dt = new DataTable();
            //    dt.Columns.Add("FlightDate");
            //    dt.Columns.Add("UniqueCarrier");
            //    dt.Columns.Add("OriginCityName");
            //    dt.Columns.Add("DestCityName");

            //    DataRow dr;

            //    while (reader.Read())
            //    {
            //        //MessageBox.Show("City:"+ reader.GetString(0) +"; Average Temperature:"+ reader.GetDouble(1) + "; Date: "+ reader.GetDateTime(2));
            //        dr = dt.NewRow();
            //        dr[0] = reader.GetDateTime(0);
            //        dr[1] = reader.GetString(1);
            //        dr[2] = reader.GetString(2);
            //        dr[3] = reader.GetString(3);
            //        dt.Rows.Add(dr);



            //    }

            //    dataGridView1.DataSource = dt;


            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (File.("D:\\Tempalte.db"))
            //{
            //    File.Delete("file.db");
            //}

            DuckDBConnection duckDBConnection = new DuckDBConnection("Data Source=D:\\Tempalte.db");
            duckDBConnection.Open();

            var command1 = duckDBConnection.CreateCommand();
            command1.CommandText = "Select * From " + comboBox1.Text;

            DuckDBDataReader duckDBDataReader = command1.ExecuteReader();

            // DuckDBResult 
            //DataTable dt = duckDBDataReader.GetSchemaTable();
            DataTable dtable = new DataTable();
            // List<Challan> challans =(List<Challan>)duckDBDataReader.GetEnumerator ();
            //  DuckDBListEntry duckDBList = duckDBDataReader.l
            // var challans = duckDBDataReader.GetValues(List<Challan> Challans );
            for (int j = 0; j <= duckDBDataReader .FieldCount  - 1; j++)
            {
                dtable.Columns.Add(duckDBDataReader.GetName (j));
            }

            while (duckDBDataReader.Read())
            {

                DataRow dr1 = dtable.NewRow();
                for (int i = 0; i <= dtable.Columns.Count - 1; i++)
                {
                    dr1[i] = duckDBDataReader.GetValue(i);

                }
                dtable.Rows.Add(dr1);
            }
            duckDBConnection.Close();
            dataGridView1.DataSource = dtable;

          

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DuckDBConnection duckDBConnection = new DuckDBConnection("Data Source=D:\\Tempalte.db");
            duckDBConnection.Open();

            var command1 = duckDBConnection.CreateCommand();
            // command1.CommandText = "update challan set F3=5000 where f1='1' ";
            command1.CommandText = "delete from Deductee  where f4='Mode' ";
            command1.ExecuteNonQuery();
            MessageBox.Show("updated successfully");
            duckDBConnection.Close();
            duckDBConnection = null;
        }

        private void btnjoin_Click(object sender, EventArgs e)
        {
            DuckDBConnection duckDBConnection = new DuckDBConnection("Data Source=D:\\Tempalte.db");
                      
            duckDBConnection.Open();

            var command1 = duckDBConnection.CreateCommand();
            command1.CommandText = "Select Deductee.* From Deductee join Challan on Challan.F12=Deductee.F16 where Deductee.F16='06/02/2024'";

            DuckDBDataReader duckDBDataReader = command1.ExecuteReader();
            DataTable dt = duckDBDataReader.GetSchemaTable();
            DataTable dtable = new DataTable();
           // IQueryable dt1 = duckDBDataReader.AsQueryable();
           
            for (int j = 0; j <= dt.Rows.Count - 1; j++)
            {
                dtable.Columns.Add(dt.Rows[j].ItemArray[0].ToString());

            }
          
            while (duckDBDataReader.Read())
            {
                //<List<dt1>> = duckDBDataReader.GetEnumerator();
                DataRow dr1 = dtable.NewRow();
                for (int i = 0; i <= dtable.Columns.Count - 1; i++)
                {
                    dr1[i] = duckDBDataReader.GetValue(i);

                }
                dtable.Rows.Add(dr1);
            }
            duckDBConnection.Close();
            dataGridView1.DataSource = dtable;
            
        }
    }
}
