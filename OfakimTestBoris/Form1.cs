using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfakimTestBoris
{
    public partial class Form1 : Form
    {
        private const string urlPattern = "http://rate-exchange-1.appspot.com/currency?from={0}&to={1}";
        public Form1()
        {
            InitializeComponent();
            on_load();
        }

        //Loading Currency table as is to grid
        public void on_load()
        {
            string startupPath = Environment.CurrentDirectory + "\\Database1.mdf";
            string connetionString;
            SqlConnection cnn;
            connetionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename="+ startupPath + ";Integrated Security=True";
            cnn = new SqlConnection(connetionString);

            cnn.Open();
            SqlConnection con = new SqlConnection(connetionString);

            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.Connection = con;
            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.CommandText = "select * from Currency";
            SqlDataAdapter sqlDataAdap = new SqlDataAdapter(sqlCmd);

            DataTable dtRecord = new DataTable();
            sqlDataAdap.Fill(dtRecord);
            dataGridView1.DataSource = dtRecord;

            cnn.Close();
        }

        //Load currency table after updating from the api
        void load_table()
        {

            string startupPath = Environment.CurrentDirectory + "\\Database1.mdf";
            string connetionString;
            SqlConnection cnn;
            connetionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename="+ startupPath + ";Integrated Security=True";
            cnn = new SqlConnection(connetionString);
            cnn.Open();

            SqlCommand command;
            SqlDataReader dataReader;
            string sql = string.Empty,
                   output = string.Empty,
                   SQL_Text = string.Empty;

            sql = "select * from Currency";
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            var dataAdapter = new SqlDataAdapter(sql, cnn);

            int count = dataReader.FieldCount;
            while (dataReader.Read())
            {
                    string id = dataReader.GetValue(0).ToString();
                    string value = dataReader.GetValue(1).ToString();
                    string[] arr = value.Split('/');

                    string url1 = string.Format(urlPattern, arr[0], arr[1]);
                    decimal exchangeRate1;
                    string res1 = string.Empty;

                    using (var wc = new WebClient())
                    {
                        var json = wc.DownloadString(url1);

                        Newtonsoft.Json.Linq.JToken token = Newtonsoft.Json.Linq.JObject.Parse(json);
                        exchangeRate1 = (decimal)token.SelectToken("rate");
                        res1 = (1 * exchangeRate1).ToString();
                    }

                    string url2 = string.Format(urlPattern, arr[1], arr[0]);
                    decimal exchangeRate2;
                    string res2 = string.Empty;

                    using (var wc = new WebClient())
                    {
                        var json = wc.DownloadString(url2);

                        Newtonsoft.Json.Linq.JToken token = Newtonsoft.Json.Linq.JObject.Parse(json);
                        exchangeRate2 = (decimal)token.SelectToken("rate");
                        res2 = (1 * exchangeRate2).ToString();
                    }

                    SQL_Text += "UPDATE Currency SET value = N'" + res1 + "/" + res2 + "', last_modify = GETDATE() WHERE Id = " + id + " ";
                
            }

            cnn.Close();
            cnn.Open();
            command = new SqlCommand(SQL_Text, cnn);
            dataReader = command.ExecuteReader();
            cnn.Close();

            cnn.Open();
            SqlConnection con = new SqlConnection(connetionString);

            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.Connection = con;
            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.CommandText = "select * from Currency";
            SqlDataAdapter sqlDataAdap = new SqlDataAdapter(sqlCmd);

            DataTable dtRecord = new DataTable();
            sqlDataAdap.Fill(dtRecord);
            dataGridView1.DataSource = dtRecord;

            cnn.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            load_table();
        }
    }
}
