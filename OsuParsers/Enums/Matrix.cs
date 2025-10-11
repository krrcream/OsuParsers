using System;
using System.Runtime.InteropServices;

namespace OsuParsers.Enums
{
    public class Matrix
    {
    /// <summary>
    /// 矩阵封装，适用于各种矩阵操作
    /// </summary>
    
        private readonly int[] _data;
        private readonly int _rows;
        private readonly int _cols;

        /// <summary>
        /// 空位置常量
        /// </summary>
        public const int Empty = -1;

        /// <summary>
        /// 长音符身体常量
        /// </summary>
        public const int HoldBody = -7;

        /// <summary>
        /// 行数
        /// </summary>
        public int Rows => _rows;

        /// <summary>
        /// 列数
        /// </summary>
        public int Cols => _cols;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Matrix(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
            _data = new int[rows * cols];
            Array.Fill(_data, Empty);
        }

        /// <summary>
        /// 从现有数组构造
        /// </summary>
        public Matrix(int[,] data)
        {
            _rows = data.GetLength(0);
            _cols = data.GetLength(1);
            _data = new int[_rows * _cols];
            
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    _data[i * _cols + j] = data[i, j];
                }
            }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        public int this[int row, int col]
        {
            get
            {
                ThrowIfInvalidPosition(row, col);
                return GetRowSpan(row)[col];
            }
            set
            {
                ThrowIfInvalidPosition(row, col);
                GetRowSpan(row)[col] = value;
            }
        }

        /// <summary>
        /// 获取内部数组（用于兼容现有代码）
        /// </summary>
        public int[,] GetData()
        {
            var result = new int[_rows, _cols];
            var resultFlat = MemoryMarshal.CreateSpan(ref result[0, 0], _data.Length);
            _data.AsSpan().CopyTo(resultFlat);
            return result;
        }

        /// <summary>
        /// 复制数据到另一个矩阵
        /// </summary>
        public void CopyTo(Matrix target)
        {
            if (Rows != target.Rows || Cols != target.Cols)
                throw new ArgumentException("Matrix dimensions must match");
            Array.Copy(_data, target._data, _data.Length);
        }

        /// <summary>
        /// 从另一个矩阵复制数据
        /// </summary>
        public void CopyFrom(Matrix source)
        {
            source.CopyTo(this);
        }

        /// <summary>
        /// 获取行数据作为 Span（用于 MemoryMarshal）
        /// </summary>
        public Span<int> GetRowSpan(int row)
        {
            ThrowIfInvalidRow(row);
            return MemoryMarshal.CreateSpan(ref _data[row * _cols], _cols);
        }

        /// <summary>
        /// 克隆矩阵
        /// </summary>
        public Matrix Clone()
        {
            var clone = new Matrix(Rows, Cols);
            CopyTo(clone);
            return clone;
        }
        
        /// <summary>
        /// 批量操作
        /// </summary>
        /// <returns></returns>
        public Span<int> AsSpan() => _data.AsSpan();
        
        private bool IsValidPosition(int row, int col) => 
            row >= 0 && row < _rows && col >= 0 && col < _cols;

        private void ThrowIfInvalidPosition(int row, int col)
        {
            if (!IsValidPosition(row, col))
                throw new IndexOutOfRangeException($"Index out of range: row={row}, col={col}");
        }
        
        private void ThrowIfInvalidColumn(int col)
        {
            if (col < 0 || col >= _cols)
                throw new IndexOutOfRangeException($"Column index out of range: {col}");
        }

        private void ThrowIfInvalidRow(int row)
        {
            if (row < 0 || row >= _rows)
                throw new IndexOutOfRangeException($"Row index out of range: {row}");
        }
        
        /// <summary>
        /// 交换两列的数据
        /// </summary>
        /// <param name="colA">第一列索引</param>
        /// <param name="colB">第二列索引</param>
        public void SwapColumns(int colA, int colB)
        {
            // 检查列索引有效性
            ThrowIfInvalidColumn(colA);
            ThrowIfInvalidColumn(colB);
            
            // 如果是同一列，无需交换
            if (colA == colB)
                return;

            // 逐行交换两列的数据
            for (int row = 0; row < Rows; row++)
            {
                int indexA = row * _cols + colA;
                int indexB = row * _cols + colB;
                (_data[indexA], _data[indexB]) = (_data[indexB], _data[indexA]);
            }
        }
        
        /// <summary>
        /// 交换两行的数据
        /// </summary>
        /// <param name="rowA">第一行索引</param>
        /// <param name="rowB">第二行索引</param>
        public void SwapRows(int rowA, int rowB)
        {
            // 检查行索引有效性
            if (rowA < 0 || rowA >= Rows)
                throw new IndexOutOfRangeException($"Row A index out of range: {rowA}");
            if (rowB < 0 || rowB >= Rows)
                throw new IndexOutOfRangeException($"Row B index out of range: {rowB}");

            // 如果是同一行，无需交换
            if (rowA == rowB)
                return;

            // 使用Span进行高效行交换
            var spanA = GetRowSpan(rowA);
            var spanB = GetRowSpan(rowB);
            spanA.CopyTo(spanB);
            spanB.CopyTo(spanA);
        }
        
        /// <summary>
        /// 应用索引映射矩阵，将当前矩阵作为索引应用于目标矩阵
        /// </summary>
        /// <param name="target">目标矩阵</param>
        /// <returns>新矩阵，包含根据当前矩阵索引从目标矩阵获取的值</returns>
        public Matrix ApplyIndexMapping(Matrix target)
        {
            // 检查行数是否匹配
            if (this.Rows != target.Rows)
                throw new ArgumentException("Matrix row counts must match");

            // 创建与目标矩阵相同尺寸的新矩阵
            var result = new Matrix(target.Rows, target.Cols);

            // 获取所有矩阵的Span以提高性能
            var targetSpan = target.AsSpan();
            var resultSpan = result.AsSpan();
            var thisSpan = this.AsSpan();

            // 对于每个元素target[i,j]，设置result[i,j] = this[i, target[i,j]]
            for (int i = 0; i < target.Rows; i++)
            {
                // 计算每行在Span中的起始位置
                int thisRowOffset = i * _cols;
                int targetRowOffset = i * target._cols;
                int resultRowOffset = i * result._cols;

                for (int j = 0; j < target.Cols; j++)
                {
                    int targetIndex = targetSpan[targetRowOffset + j];

                    // 检查索引是否有效
                    if (targetIndex < 0 || targetIndex >= _cols)
                    {
                        resultSpan[resultRowOffset + j] = Empty;
                    }
                    else
                    {
                        resultSpan[resultRowOffset + j] = thisSpan[thisRowOffset + targetIndex];
                    }
                }
            }

            return result;
        }
        
        /// <summary>
        /// 根据目标矩阵或布尔掩码标记当前位置为空
        /// </summary>
        /// <param name="target">目标矩阵，用于根据值判断是否标记为空</param>
        /// <param name="mask">可选的布尔掩码，true的位置将被标记为空</param>
        /// <returns>新的矩阵，根据条件标记为空位置</returns>
        public Matrix MaskEmpty(Matrix target, bool[,] mask = null)
        {
            // 检查矩阵维度是否匹配
            if (this.Rows != target.Rows || this.Cols != target.Cols)
                throw new ArgumentException("Target matrix dimensions must match current matrix");
                
            if (mask != null && (this.Rows != mask.GetLength(0) || this.Cols != mask.GetLength(1)))
                throw new ArgumentException("Mask dimensions must match current matrix");

            // 创建结果矩阵
            var result = new Matrix(this.Rows, this.Cols);
            this.CopyTo(result);

            // 获取Span以提高性能
            var thisSpan = this.AsSpan();
            var targetSpan = target.AsSpan();
            var resultSpan = result.AsSpan();

            // 应用标记逻辑
            for (int i = 0; i < this.Rows; i++)
            {
                int rowOffset = i * this.Cols;
                
                for (int j = 0; j < this.Cols; j++)
                {
                    int index = rowOffset + j;
                    int currentValue = thisSpan[index];
                    
                    // 如果当前位置值小于0，或目标矩阵对应位置值小于0，或掩码为true，则标记为空
                    if (currentValue < 0 || targetSpan[index] < 0 || (mask != null && mask[i, j]))
                    {
                        resultSpan[index] = Empty;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据布尔掩码标记当前位置为空（针对锯齿数组）
        /// </summary>
        /// <param name="mask">布尔掩码，true的位置将被标记为空</param>
        /// <returns>新的矩阵，根据条件标记为空位置</returns>
        public Matrix MaskEmpty(bool[][] mask)
        {
            // 检查矩阵维度是否匹配
            if (this.Rows != mask.Length)
                throw new ArgumentException("Mask row count must match current matrix");
                
            for (int i = 0; i < this.Rows; i++)
            {
                if (this.Cols != mask[i].Length)
                    throw new ArgumentException($"Mask column count at row {i} must match current matrix");
            }

            // 创建结果矩阵
            var result = new Matrix(this.Rows, this.Cols);
            this.CopyTo(result);

            // 获取Span以提高性能
            var thisSpan = this.AsSpan();
            var resultSpan = result.AsSpan();

            // 应用标记逻辑
            for (int i = 0; i < this.Rows; i++)
            {
                int rowOffset = i * this.Cols;
                
                for (int j = 0; j < this.Cols; j++)
                {
                    int index = rowOffset + j;
                    int currentValue = thisSpan[index];
                    
                    // 如果当前位置值小于0，或掩码为true，则标记为空
                    if (currentValue < 0 || mask[i][j])
                    {
                        resultSpan[index] = Empty;
                    }
                }
            }

            return result;
        }
        
  
        
    }
    
}