using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystems.Model
{
    class ResultModel
    {
        public Matrix<double> raw_data;
        public Matrix<double> train_data;
        public Matrix<double> test_data;
        public Matrix<double> predic_data;
        public double rmse;
    }
}
