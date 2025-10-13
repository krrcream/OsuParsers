using System;
using System.Linq;
using OsuParsers.Enums;

namespace OsuParsers.Helpers;

public class MatrixHelper
{
    public static void PrintMatrices(int i, params object[] matrices)
    {
        if (matrices == null || matrices.Length == 0)
            return;

        // 确定实际要打印的行数
        int maxRows = 0;
        foreach (var matrix in matrices)
        {
            int rows = matrix switch
            {
                Matrix m => m.Rows,
                DoubleMatrix dm => dm.Rows,
                BoolMatrix bm => bm.Rows,
                _ => 0
            };
            maxRows = Math.Max(maxRows, rows);
        }

        // 只打印前i行（倒序）
        int rowsToPrint = Math.Min(i, maxRows);

        // 倒序打印前i行
        for (int row = rowsToPrint - 1; row >= 0; row--)
        {
            for (int m = 0; m < matrices.Length; m++)
            {
                var matrix = matrices[m];
                string cellValue = matrix switch
                {
                    Matrix mtx => (row < mtx.Rows && row >= 0) ? string.Join(" ", Enumerable.Range(0, mtx.Cols).Select(col => mtx[row, col])) : "",
                    DoubleMatrix dmtx => (row < dmtx.Rows && row >= 0) ? string.Join(" ", Enumerable.Range(0, dmtx.Cols).Select(col => dmtx[row, col].ToString("F2"))) : "",
                    BoolMatrix bmtx => (row < bmtx.Rows && row >= 0) ? string.Join(" ", Enumerable.Range(0, bmtx.Cols).Select(col => bmtx[row, col] ? "T" : "F")) : "",
                    _ => ""
                };

                Console.Write(cellValue);
        
                // 如果不是最后一个矩阵，添加分隔符
                if (m < matrices.Length - 1)
                    Console.Write(" | ");
            }
            Console.WriteLine(); // 换行
        }
    }
}