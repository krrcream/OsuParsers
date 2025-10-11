using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OsuParsers.Beatmaps.Objects.Mania;

public class newPositionX
{
    private int[] _x;
    
    public int CircleSize { get; private set; }
    
    // 使用私有字段和公共属性，增加边界检查
    public int[] X 
    { 
        get => _x; 
        private set => _x = value; 
    }

    // 添加索引器用于安全访问数组元素
    public int this[int index]
    {
        get
        {
            if (index < 0 || index >= _x.Length)
                throw new ArgumentOutOfRangeException(nameof(index), 
                    $"Index {index} is out of range. Valid range is 0 to {_x.Length - 1} for CircleSize {CircleSize}");
            return _x[index];
        }
    }
    
    public newPositionX(int circleSize)
    {
        if (!KeyValueMap.ContainsKey(circleSize))
            throw new ArgumentOutOfRangeException(nameof(circleSize), $"circleSize must be between 1 and {KeyValueMap.Keys.Max()}");

        CircleSize = circleSize;
        X = KeyValueMap[circleSize];
    }

    public Vector2 Vector2(int column)
    {
        return new(X[column], 192);
    }    
    
    public int getColumn(int column)
    {
        var x = (int)Math.Floor((column + 0.5) * (512.0 / CircleSize));
        return x;
    }
    
    public int setPositionX(int columns)
    {
        if (columns < 0 || columns >= X.Length)
            throw new ArgumentOutOfRangeException(nameof(columns), $"columns must be between 0 and {X.Length - 1} for CircleSize={CircleSize}");

        return X[columns];
    }
    
    //保留原有方法使用
    public static int positionXToColumn(int CS, int X)
    {
        var column = (int)Math.Floor(X * (double)CS / 512);
        return column;
    }
    //保留原有方法使用
    public static int columnToPositionX(int CS, int column)
    {
        var x = (int)Math.Floor((column + 0.5) * (512.0 / CS));
        return x;
    }    

    private static readonly Dictionary<int, int[]> KeyValueMap = new()
    {
        [1] = [256],
        [2] = [128, 384],
        [3] = [85, 256, 426],
        [4] = [64, 192, 320, 448],
        [5] = [51, 153, 256, 358, 460],
        [6] = [42, 128, 213, 298, 384, 469],
        [7] = [36, 109, 182, 256, 329, 402, 475],
        [8] = [32, 96, 160, 224, 288, 352, 416, 480],
        [9] = [28, 85, 142, 199, 256, 312, 369, 426, 483],
        [10] = [25, 76, 128, 179, 230, 281, 332, 384, 435, 486],
        [11] = [23, 69, 116, 162, 209, 256, 302, 349, 395, 442, 488],
        [12] = [21, 64, 106, 149, 192, 234, 277, 320, 362, 405, 448, 490],
        [13] = [19, 59, 98, 137, 177, 256, 216, 295, 334, 374, 413, 452, 492],
        [14] = [18, 54, 91, 128, 164, 201, 237, 274, 310, 347, 384, 420, 457, 493],
        [15] = [17, 51, 85, 119, 153, 187, 221, 256, 290, 324, 358, 392, 426, 460, 494],
        [16] = [16, 48, 80, 112, 144, 176, 240, 208, 272, 304, 336, 368, 400, 432, 464, 496],
        [17] = [15, 45, 75, 135, 165, 105, 195, 225, 316, 286, 256, 376, 346, 406, 436, 466, 496],
        [18] = [16, 48, 80, 112, 128, 144, 176, 208, 240, 272, 304, 336, 368, 384, 400, 432, 464, 496],
        [19] = [13, 39, 66, 93, 120, 147, 174, 201, 228, 255, 282, 309, 336, 363, 390, 417, 444, 471, 498],
        [20] = [12, 37, 63, 88, 114, 140, 165, 191, 216, 242, 268, 293, 319, 344, 370, 396, 421, 447, 472, 498],
        [21] = [12, 36, 60, 85, 109, 133, 158, 182, 207, 231, 255, 280, 304, 328, 353, 377, 402, 426, 450, 475, 499],
        [22] = [11, 34, 57, 80, 104, 127, 150, 173, 197, 220, 243, 267, 290, 313, 336, 360, 383, 406, 429, 453, 476, 499],
        [23] = [11, 33, 55, 77, 100, 122, 144, 166, 189, 211, 233, 255, 278, 300, 322, 344, 367, 389, 411, 433, 456, 478, 500],
        [24] = [10, 31, 52, 74, 95, 116, 138, 159, 180, 202, 223, 244, 266, 287, 308, 330, 351, 372, 394, 415, 436, 458, 479, 500]
    };
}
