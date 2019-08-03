using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystems.Controller
{
    /// <summary>
    /// this class for calculate root mean square error
    /// </summary>
    class RMSE
    {
        /// <summary>
        /// root mean square error.
        /// </summary>
        /// <param name="prediction"></param>
        /// <param name="ground_truth"></param>
        /// <returns></returns>
        public static double calRMSE(Matrix<double> prediction, Matrix<double> ground_truth)
        {
            List<int[]> list_index = findNonZero(ground_truth);
            List<double> list_predic = getValueFromListIndex(prediction, list_index);
            List<double> list_ground_truth = getValueFromListIndex(ground_truth, list_index);
            return Math.Sqrt(mean_squared_error(list_predic, list_ground_truth));

        }

        public static double mean_squared_error(List<double> list1, List<double> list2)
        {
            double sum = 0;
            int count = list1.Count;
            if (count > list2.Count)
            {
                count = list2.Count;
            }

            for (int index = 0; index < count; index++)
            {
                double delta = list1[index] - list2[index];
                if (double.IsNaN(delta))
                {
                    count--;
                }
                else
                {
                    sum += (delta * delta);
                }

            }
            return sum / count;
        }

        public static List<double> getValueFromListIndex(Matrix<double> matrix, List<int[]> list_index)
        {
            List<double> list_value = new List<double>();
            foreach (int[] index in list_index)
            {
                list_value.Add(matrix[index[0], index[1]]);
            }
            return list_value;
        }

        public static List<int[]> findNonZero(Matrix<double> matrix)
        {
            List<int[]> list_index = new List<int[]>();
            for (int row = 0; row < matrix.RowCount; row++)
            {
                for (int col = 0; col < matrix.ColumnCount; col++)
                {
                    if (matrix[row, col] != 0)
                    {
                        list_index.Add(new int[] { row, col });
                    }
                }
            }
            return list_index;
        }
    }
}
