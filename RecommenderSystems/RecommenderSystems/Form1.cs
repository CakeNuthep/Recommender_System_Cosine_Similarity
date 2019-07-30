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


namespace RecommenderSystems
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //Matrix<double> A = DenseMatrix.OfArray(new double[,] {
            //{0.01,0.01,0.01,0.01},
            //{0.01,0.02,0,0.04},
            //{0.04,0.03,0.02,0.01}});
            //Matrix<double> sum = DenseMatrix.Create(1, A.ColumnCount, 0);
            //sum.SetRow(0, A.ColumnAbsoluteSums());
            //Console.WriteLine(sum.Transpose());

            //load data
            //Matrix<double> data = DelimitedReader.Read<double>(@"D:\MachineLernning\recommender systems\console\RecommenderSystemsConsole\RecommenderSystemsConsole\Data\u.data", false, "\t", false, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            Matrix<double> data = DelimitedReader.Read<double>(@"D:\MachineLernning\recommender systems\RecommenderSystems\RecommenderSystems\Data\movie.data", false, "\t", false, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            Console.WriteLine("data = {0}", data);


            int n_users = data.Column(0).Distinct().Count();
            int n_items = data.Column(1).Distinct().Count();
            Console.WriteLine("Num Users = {0}", n_users);
            Console.WriteLine("Num Items = {0}", n_items);

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
            Console.WriteLine();
            Console.WriteLine("Train Data = {0}", train_data);
            Console.WriteLine();
            Console.WriteLine("Test Data = {0}", test_data);
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
            Console.WriteLine();
            Console.WriteLine("Train Data Matrix = {0}", train_data_matrix);

            //train_data_matrix.Row(0).DotProduct()
            Matrix<double> user_similarity = Recommender_System.cosin(train_data_matrix);
            Matrix<double> item_similarity = Recommender_System.cosin(train_data_matrix.Transpose());
            Console.WriteLine();
            Console.WriteLine("user_similarity = {0}", user_similarity);

            Console.WriteLine();
            Console.WriteLine("item_similarity = {0}", item_similarity);


            Matrix<double> item_prediction = Recommender_System.predict(train_data_matrix, item_similarity, "item");
            Matrix<double> user_prediction = Recommender_System.predict(train_data_matrix, user_similarity, "user");

            Console.WriteLine();
            Console.WriteLine("user_predic = {0}", user_prediction);

            Console.WriteLine();
            Console.WriteLine("item_predic = {0}", item_prediction);

            double user_rmse = Recommender_System.rmse(user_prediction, test_data_matrix);
            double item_rmse = Recommender_System.rmse(item_prediction, test_data_matrix);
            Console.WriteLine("User-based CF RMSE: " + user_rmse.ToString());
            Console.WriteLine("Item-based CF RMSE: " + item_rmse.ToString());
        }
    }
}
