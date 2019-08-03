using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using RecommenderSystems.Controller;
using RecommenderSystems.Model;

namespace RecommenderSystems
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox_pathFile.Text = @"D:\LearningC#\page\Recommender System\App\RecommenderSystems_GitHub\RecommenderSystems\RecommenderSystems\Data\movie.data";
        }

        private void button_browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "data files (*.data)|*.data",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_pathFile.Text = openFileDialog1.FileName;
            }
        }


        private ResultModel user_predict(string pathFile)
        {
            ResultModel result = new ResultModel();
            //load Data
            Matrix<double> data = DelimitedReader.Read<double>(pathFile, false, "\t", false, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            result.raw_data = data;

            int n_users = data.Column(0).Distinct().Count();
            int n_items = data.Column(1).Distinct().Count();

            double test_split = 0.1;
            double train_split = 1 - test_split;
            int train_row = (int)(data.RowCount * train_split);
            int train_col = data.ColumnCount;
            int test_row = data.RowCount - train_row;
            int test_col = train_col;
            Matrix<double> train_data = DenseMatrix.Create(train_row, train_col, 0);
            Matrix<double> test_data = DenseMatrix.Create(test_row, test_col, 0);
            train_data = data.SubMatrix(0, train_row, 0, train_col);
            test_data = data.SubMatrix(train_row, test_row, 0, test_col);

            // Create two user-item matrices, one for training and another for testing
            Matrix<double> train_data_matrix = DenseMatrix.Create(n_users, n_items, 0);
            for (int line = 0; line < train_data.RowCount; line++)
            {
                int userId = (int)(train_data[line, 0]);
                int itemId = (int)(train_data[line, 1] - 1);
                train_data_matrix[userId, itemId] = train_data[line, 2];
            }

            Matrix<double> test_data_matrix = DenseMatrix.Create(n_users, n_items, 0);
            for (int line = 0; line < test_data.RowCount; line++)
            {
                int userId = (int)(test_data[line, 0]);
                int itemId = (int)(test_data[line, 1] - 1);
                test_data_matrix[userId, itemId] = test_data[line, 2];
            }
            result.train_data = train_data_matrix;
            result.test_data = test_data_matrix;

            // consine similarity calculate
            Matrix<double> user_similarity = Recommender_System.cosin(train_data_matrix);

            // predict
            Matrix<double> user_prediction = Recommender_System.predict(train_data_matrix, user_similarity, "user");
            result.predic_data = user_prediction;
            // rmse error
            double user_rmse = RMSE.calRMSE(user_prediction, test_data_matrix);
            result.rmse = user_rmse;
            return result;
        }

        private ResultModel item_predict(string pathFile)
        {
            ResultModel result = new ResultModel();
            // load data
            Matrix<double> data = DelimitedReader.Read<double>(pathFile, false, "\t", false, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            result.raw_data = data;

            int n_users = data.Column(0).Distinct().Count();
            int n_items = data.Column(1).Distinct().Count();

            double test_split = 0.1;
            double train_split = 1 - test_split;
            int train_row = (int)(data.RowCount * train_split);
            int train_col = data.ColumnCount;
            int test_row = data.RowCount - train_row;
            int test_col = train_col;
            Matrix<double> train_data = DenseMatrix.Create(train_row, train_col, 0);
            Matrix<double> test_data = DenseMatrix.Create(test_row, test_col, 0);
            train_data = data.SubMatrix(0, train_row, 0, train_col);
            test_data = data.SubMatrix(train_row, test_row, 0, test_col);

            // Create two user-item matrices, one for training and another for testing
            Matrix<double> train_data_matrix = DenseMatrix.Create(n_users, n_items, 0);
            for (int line = 0; line < train_data.RowCount; line++)
            {
                int userId = (int)(train_data[line, 0]);
                int itemId = (int)(train_data[line, 1] - 1);
                train_data_matrix[userId, itemId] = train_data[line, 2];
            }

            Matrix<double> test_data_matrix = DenseMatrix.Create(n_users, n_items, 0);
            for (int line = 0; line < test_data.RowCount; line++)
            {
                int userId = (int)(test_data[line, 0]);
                int itemId = (int)(test_data[line, 1] - 1);
                test_data_matrix[userId, itemId] = test_data[line, 2];
            }
            result.train_data = train_data_matrix;
            result.test_data = test_data_matrix;
            
            // consine similarity calculate
            Matrix<double> item_similarity = Recommender_System.cosin(train_data_matrix.Transpose());

            // predict
            Matrix<double> item_prediction = Recommender_System.predict(train_data_matrix, item_similarity, "item");
            result.predic_data = item_prediction;

            // rmse error
            double item_rmse = RMSE.calRMSE(item_prediction, test_data_matrix);
            result.rmse = item_rmse;

            return result;
        }

        private bool covertMatrixToDataTable(Matrix<double> matrix,string[] label_Columns, out DataTable dataTable, string table_name = "Table")
        {
            dataTable = new DataTable();
            dataTable.TableName = table_name;
            if(matrix != null)
            {
                int rowCount = matrix.RowCount;
                int colCount = matrix.ColumnCount;
                if(colCount == label_Columns.Length)
                {
                    //add Column
                    for(int colIndex=0; colIndex<colCount;colIndex++)
                    {
                        dataTable.Columns.Add(label_Columns[colIndex]);
                    }

                    for(int rowIndex=0;rowIndex<rowCount;rowIndex++)
                    {
                        DataRow row = dataTable.NewRow();
                        for (int colIndex = 0; colIndex < colCount; colIndex++)
                        {
                            row[label_Columns[colIndex]] = matrix[rowIndex, colIndex];
                        }
                        dataTable.Rows.Add(row);
                    }
                    return true;
                }
            }
            return false;
        }

        private void button_run_Click(object sender, EventArgs e)
        {
            ResultModel result = user_predict(textBox_pathFile.Text);

            //show raw data on DataGridView
            {
                DataTable rawData;
                if (covertMatrixToDataTable(result.raw_data, new string[] { "UserID", "movieID", "rating" }, out rawData, "RawData"))
                {
                    dataGridView_rawData.DataSource = rawData;
                }
            }

            //show train data on DataGridView
            {
                DataTable train_data;
                string[] colName = new string[result.train_data.ColumnCount];
                for (int colIndex = 0; colIndex < colName.Length; colIndex++)
                {
                    colName[colIndex] = string.Format("movieID{0}", colIndex.ToString());
                }
                if (covertMatrixToDataTable(result.train_data, colName, out train_data, "TrainData"))
                {
                    dataGridView_trainData.DataSource = train_data;
                }
            }

            //show predic data on DataGridView
            {
                DataTable predic_data;
                string[] colName = new string[result.predic_data.ColumnCount];
                for (int colIndex = 0; colIndex < colName.Length; colIndex++)
                {
                    colName[colIndex] = string.Format("movieID{0}", colIndex.ToString());
                }
                if (covertMatrixToDataTable(result.predic_data, colName, out predic_data, "PredicData"))
                {
                    dataGridView_predictData.DataSource = predic_data;
                }
            }

            //show Root Mean Square Error (RMSE)
            MessageBox.Show(String.Format("RMSE = {0}", result.rmse));
        }


    }
}
