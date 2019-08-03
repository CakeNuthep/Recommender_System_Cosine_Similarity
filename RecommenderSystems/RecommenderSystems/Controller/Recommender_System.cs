using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystems.Controller
{
    public class Recommender_System
    {

        /// <summary>
        /// this method for calculate cosine simularity.
        /// </summary>
        /// <param name="data">Metrix data</param>
        /// <returns></returns>
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

       

        public static Matrix<double> predict(Matrix<double> ratings, Matrix<double> similarity, string type = "user")
        {
            Matrix<double> pred = DenseMatrix.Create(ratings.RowCount, ratings.ColumnCount, 0);

            if (type.ToLower() == "user")
            {
                Matrix<double> mean_user_rating = mean(ratings);
                Matrix<double> ratings_diff;
                if (subMatrix(ratings, mean_user_rating, out ratings_diff))//ratings.Subtract(mean_user_rating);
                {
                    Matrix<double> sim_mul_ratingDiff = similarity.Multiply(ratings_diff);
                    Vector<double> vector_sum = RowAbsoluteSums(similarity);
                    Matrix<double> sum = DenseMatrix.Create(1, vector_sum.Count, 0);
                    sum.SetRow(0, vector_sum);
                    Matrix<double> devide_matrix;
                    if (devideMatrix(sim_mul_ratingDiff, sum.Transpose(), out devide_matrix))
                    {
                        addMatrix(devide_matrix, mean_user_rating, out pred);//mean_user_rating.Add(devide_matrix);
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
                if (devideMatrix(rat_mul_sim, sum, out devide_matrix))
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

        public static bool addMatrix(Matrix<double> matrix, Matrix<double> addmatrix, out Matrix<double> result)
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

        public static bool subMatrix(Matrix<double> matrix, Matrix<double> subMatrix, out Matrix<double> result)
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

        public static bool devideMatrix(Matrix<double> matrix, Matrix<double> matrixDevide, out Matrix<double> result)
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
