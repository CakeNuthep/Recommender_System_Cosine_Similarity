﻿using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystems
{
    public class Recommender_System
    {
        public static Matrix<double> cosin(Matrix<double> data)
        {
            Matrix<double> similar_matrix = DenseMatrix.Create(data.RowCount, data.RowCount, 0);
            for (int vector1 = 0; vector1 < data.RowCount; vector1++)
            {
                for (int vector2 = vector1; vector2 < data.RowCount; vector2++)
                {
                    double dotProduct = data.Row(vector1).DotProduct(data.Row(vector2));
                    Vector<double> vectorA = data.Row(vector1);
                    Vector<double> vectorB = data.Row(vector2);
                    for (int i = 0; i < vectorA.Count; i++)
                    {
                        vectorA[i] = vectorA[i] * vectorA[i];
                    }

                    for (int i = 0; i < vectorB.Count; i++)
                    {
                        vectorB[i] = vectorB[i] * vectorB[i];
                    }
                    double sqrtVectorA = Math.Sqrt(vectorA.Sum());
                    double sqrtVectorB = Math.Sqrt(vectorB.Sum());
                    double valueSimilar = 0;
                    if (sqrtVectorA * sqrtVectorB != 0)
                    {
                        valueSimilar = dotProduct / (sqrtVectorA * sqrtVectorB);
                    }

                    similar_matrix[vector1, vector2] = valueSimilar;
                    similar_matrix[vector2, vector1] = valueSimilar;
                }
            }
            return similar_matrix;
        }


        public static double rmse(Matrix<double> prediction, Matrix<double> ground_truth)
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

        public static Matrix<double> predict(Matrix<double> ratings, Matrix<double> similarity, string type = "user")
        {
            Matrix<double> pred = DenseMatrix.Create(ratings.RowCount, ratings.ColumnCount, 0);

            if (type.ToLower() == "user")
            {
                Matrix<double> mean_user_rating = mean(ratings);
                Matrix<double> ratings_diff;
                if (sub(ratings, mean_user_rating, out ratings_diff))//ratings.Subtract(mean_user_rating);
                {
                    Matrix<double> sim_mul_ratingDiff = similarity.Multiply(ratings_diff);
                    Vector<double> vector_sum = RowAbsoluteSums(similarity);
                    Matrix<double> sum = DenseMatrix.Create(1, vector_sum.Count, 0);
                    sum.SetRow(0, vector_sum);
                    Matrix<double> devide_matrix;
                    if (devide(sim_mul_ratingDiff, sum.Transpose(), out devide_matrix))
                    {
                        add(devide_matrix, mean_user_rating, out pred);//mean_user_rating.Add(devide_matrix);
                    }
                }
            }
            else if (type.ToLower() == "item")
            {
                Matrix<double> rat_mul_sim = ratings.Multiply(similarity);
                Vector<double> vector_sum = RowAbsoluteSums(similarity);
                Matrix<double> sum = DenseMatrix.Create(1, vector_sum.Count, 0);
                sum.SetRow(0, vector_sum);
                Matrix<double> devide_matrix;
                if (devide(rat_mul_sim, sum, out devide_matrix))
                {
                    pred = devide_matrix;
                }
            }
            return pred;
        }

        static public Vector<double> RowAbsoluteSums(Matrix<double> matrix)
        {
            Vector<double> vector = DenseVector.Create(matrix.RowCount, 0);
            for (int line = 0; line < matrix.RowCount; line++)
            {
                double sum_value = 0;
                for (int col = 0; col < matrix.ColumnCount; col++)
                {
                    sum_value += Math.Abs(matrix[line, col]);
                }
                vector[line] = sum_value;
            }
            return vector;
        }

        public static bool add(Matrix<double> matrix, Matrix<double> addmatrix, out Matrix<double> result)
        {
            result = null;
            if (matrix.RowCount == addmatrix.RowCount && addmatrix.ColumnCount == 1)
            {
                result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0);
                for (int line = 0; line < matrix.RowCount; line++)
                {
                    Vector<double> vector = matrix.Row(line) + addmatrix[line, 0];
                    result.SetRow(line, vector);
                }
                return true;
            }
            else if (matrix.ColumnCount == addmatrix.ColumnCount && addmatrix.RowCount == 1)
            {
                result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0);
                for (int col = 0; col < matrix.ColumnCount; col++)
                {
                    Vector<double> vector = matrix.Column(col) + addmatrix[0, col];
                    result.SetColumn(col, vector);
                }
                return true;
            }
            return false;
        }

        public static bool sub(Matrix<double> matrix, Matrix<double> subMatrix, out Matrix<double> result)
        {
            result = null;
            if (matrix.RowCount == subMatrix.RowCount && subMatrix.ColumnCount == 1)
            {
                result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0);
                for (int line = 0; line < matrix.RowCount; line++)
                {
                    Vector<double> vector = matrix.Row(line) - subMatrix[line, 0];
                    result.SetRow(line, vector);
                }
                return true;
            }
            else if (matrix.ColumnCount == subMatrix.ColumnCount && subMatrix.RowCount == 1)
            {
                result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0);
                for (int col = 0; col < matrix.ColumnCount; col++)
                {
                    Vector<double> vector = matrix.Column(col) - subMatrix[0, col];
                    result.SetColumn(col, vector);
                }
                return true;
            }
            return false;
        }

        public static Matrix<double> mean(Matrix<double> matrix)
        {
            Matrix<double> mean_matrix = DenseMatrix.Create(matrix.RowCount, 1, 0);
            for (int line = 0; line < matrix.RowCount; line++)
            {
                mean_matrix[line, 0] = matrix.Row(line).Average();
            }
            return mean_matrix;
        }

        public static bool devide(Matrix<double> matrix, Matrix<double> matrixDevide, out Matrix<double> result)
        {
            result = null;
            if (matrix.RowCount == matrixDevide.RowCount && matrixDevide.ColumnCount == 1)
            {
                result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0);
                for (int line = 0; line < matrix.RowCount; line++)
                {
                    //Vector<double> vector = matrix.Row(line) / matrixDevide[line, 0];
                    for (int col = 0; col < matrix.ColumnCount; col++)
                    {
                        if (matrixDevide[line, 0] != 0)
                        {
                            result[line, col] = matrix[line, col] / matrixDevide[line, 0];
                        }
                    }
                    //result.SetRow(line, vector);
                }
                return true;
            }
            else if (matrix.ColumnCount == matrixDevide.ColumnCount && matrixDevide.RowCount == 1)
            {
                result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0);
                for (int col = 0; col < matrix.ColumnCount; col++)
                {

                    //Vector<double> vector = matrix.Column(col) / matrixDevide[0, col];
                    for (int line = 0; line < matrix.RowCount; line++)
                    {
                        if (matrixDevide[0, col] != 0)
                        {
                            result[line, col] = matrix[line, col] / matrixDevide[0, col];
                        }
                    }
                    //result.SetColumn(col, vector);
                }
                return true;
            }
            return false;
        }
    }
}
