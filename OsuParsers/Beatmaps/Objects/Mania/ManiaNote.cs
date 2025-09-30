using OsuParsers.Enums.Beatmaps;
using System;
using System.Collections.Generic;
using System.Numerics;
#nullable disable
namespace OsuParsers.Beatmaps.Objects.Mania
{
    public class ManiaNote : HitCircle
    {
        // 存储原始数据的私有字段
        private int? _noteCircleSize;
        private int? _colIndex;
        private bool _isColIndexManuallySet = false;
        private bool _isFirstSetNoteCircleSize = true;
        
        //in this StartTime's BeatLength. default is 500ms
        public int BeatLengthOfThisNote { get; set; } = 500; 
        
        //use to generate matrix
        public int? ColIndex
        {
            get
            {
                if (_colIndex == null && _noteCircleSize.HasValue)
                {
                    _colIndex = GetColumn(_noteCircleSize.Value);
                }
                return _colIndex;
            }
            set
            {
                _colIndex = value;
                _isColIndexManuallySet = true;
                // 当 ColIndex 被手动设置且 NoteCircleSize 有值时，更新 Position.X
                if (value.HasValue && _noteCircleSize.HasValue)
                {
                    SetColumn(_noteCircleSize.Value, value.Value);
                }
            }
        }
        public int? RowIndex { get; set; } 
        public int? OriginalColIndex { get; private set; }
        // 懒加载属性
        public int? NoteCircleSize 
        { 
            get => _noteCircleSize;
            set 
            {
                int? oldColIndex = ColIndex; // 保存当前列索引
                _noteCircleSize = value;
                // 初次设置 NoteCircleSize 时更新 _colIndex
                if (_isFirstSetNoteCircleSize)
                {
                    if (value.HasValue)
                    {
                        _colIndex = GetColumn(value.Value);
                        OriginalColIndex = _colIndex;
                    }
                    _isFirstSetNoteCircleSize = false;
                }
                else
                {
                    // 非初次设置时，维持原来的列索引，更新 Position.X
                    int? oldNoteCircleSize = _noteCircleSize; 
                    
                    if (oldNoteCircleSize.HasValue)
                    {
                        // 列数减少时 保持 Position.X 不变，重新计算列索引
                        if (value.Value < oldNoteCircleSize.Value)
                        {
                            _colIndex = GetColumn(value.Value);
                            SetColumn(value.Value, _colIndex.Value);
                        }
                        else
                        {
                            // 列数增加时 列索引不变，重新计算 Position.X
                            SetColumn(value.Value, oldColIndex.Value);
                        }
                    }
                    // 如果不是手动设置 ColIndex，则清除缓存以便重新计算
                    if (!_isColIndexManuallySet)
                    {
                        _colIndex = null;
                    }
                }
            }
        }
        
        public virtual int HoldLength
        {
            get => 0;
            set {  }
        }

        public ManiaNote(Vector2 position, int startTime, int endTime, HitSoundType hitSound, Extras extras, bool isNewCombo, int comboOffset)
            : base(position, startTime, endTime, hitSound, extras, isNewCombo, comboOffset)
        {
            
        }

        public ManiaNote(HitObject HitObject)
            : base(HitObject.Position, HitObject.StartTime, HitObject.EndTime, HitObject.HitSound, HitObject.Extras, HitObject.IsNewCombo, HitObject.ComboOffset)
        {
            
        }

        /// <summary>
        /// Sets column of this object.
        /// </summary>
        /// <param name="count">Total number of columns of this beatmap. Usually it's CircleSize.</param>
        /// <param name="column">Index of the column you want to set. Should start from 0. e.g. index of first column is 0.</param>
        public void SetColumn(int count, int column)
        {
            // 检查 Map 中是否存在该 count 键
            if (!Map.ContainsKey(count))
                throw new ArgumentException($"No column mapping found for count {count}", nameof(count));
    
            // 检查 column 是否在有效范围内
            if (column < 0 || column >= Map[count].Length)
                throw new ArgumentOutOfRangeException(nameof(column), $"Column index {column} is out of range for count {count} (valid range: 0-{Map[count].Length - 1})");
            
            Position = new Vector2(Map[count][column], 0);
        }

        /// <summary>
        /// Returns column of this object.
        /// </summary>
        /// <param name="count">Total number of columns of this beatmap. Usually it's CircleSize.</param>
        public int GetColumn(int count)
        {
            double width = 512.0 / count;
            return (int)(Position.X / width);
        }

        public new Vector2 Position
        {
            set 
            { 
                base.Position = value;
                // 当 Position.X 改变时，清除 ColIndex 缓存以重新计算
                if (_noteCircleSize.HasValue)
                {
                    _colIndex = GetColumn(_noteCircleSize.Value);
                }
            }
            get => new Vector2(base.Position.X, 0);
        }
        
        private static readonly Dictionary<int, int[]> Map = new Dictionary<int, int[]>
        {
            {1, new int[] {256}},
            {2, new int[] {128, 384}},
            {3, new int[] {85, 256, 426}},
            {4, new int[] {64, 192, 320, 448}},
            {5, new int[] {51, 153, 256, 358, 460}},
            {6, new int[] {42, 128, 213, 298, 384, 469}},
            {7, new int[] {36, 109, 182, 256, 329, 402, 475}},
            {8, new int[] {32, 96, 160, 224, 288, 352, 416, 480}},
            {9, new int[] {28, 85, 142, 199, 256, 312, 369, 426, 483}},
            {10, new int[] {25, 76, 128, 179, 230, 281, 332, 384, 435, 486}},
            {11, new int[] {23, 69, 116, 162, 209, 256, 302, 349, 395, 442, 488}},
            {12, new int[] {21, 64, 106, 149, 192, 234, 277, 320, 362, 405, 448, 490}},
            {13, new int[] {19, 59, 98, 137, 177, 256, 216, 295, 334, 374, 413, 452, 492}},
            {14, new int[] {18, 54, 91, 128, 164, 201, 237, 274, 310, 347, 384, 420, 457, 493}},
            {15, new int[] {17, 51, 85, 119, 153, 187, 221, 256, 290, 324, 358, 392, 426, 460, 494}},
            {16, new int[] {16, 48, 80, 112, 144, 176, 240, 208, 272, 304, 336, 368, 400, 432, 464, 496}},
            {17, new int[] {15, 45, 75, 135, 165, 105, 195, 225, 316, 286, 256, 376, 346, 406, 436, 466, 496}},
            {18, new int[] {16, 48, 80, 112, 128, 144, 176, 208, 240, 272, 304, 336, 368, 384, 400, 432, 464, 496}},
            {19, new int[] {13, 39, 66, 93, 120, 147, 174, 201, 228, 255, 282, 309, 336, 363, 390, 417, 444, 471, 498}},
            {20, new int[] {12, 37, 63, 88, 114, 140, 165, 191, 216, 242, 268, 293, 319, 344, 370, 396, 421, 447, 472, 498}},
            {21, new int[] {12, 36, 60, 85, 109, 133, 158, 182, 207, 231, 255, 280, 304, 328, 353, 377, 402, 426, 450, 475, 499}},
            {22, new int[] {11, 34, 57, 80, 104, 127, 150, 173, 197, 220, 243, 267, 290, 313, 336, 360, 383, 406, 429, 453, 476, 499}},
            {23, new int[] {11, 33, 55, 77, 100, 122, 144, 166, 189, 211, 233, 255, 278, 300, 322, 344, 367, 389, 411, 433, 456, 478, 500}},
            {24, new int[] {10, 31, 52, 74, 95, 116, 138, 159, 180, 202, 223, 244, 266, 287, 308, 330, 351, 372, 394, 415, 436, 458, 479, 500}}
        };
        
        public void InitializeRowData(int circleSize)
        {
            NoteCircleSize = circleSize;
            ColIndex = GetColumn(circleSize);
            OriginalColIndex = ColIndex; 
        }
        public void ResetColIndexManualFlag()
        {
            _isColIndexManuallySet = false;
            _colIndex = null;
        }
        public void ResetFirstSetNoteCircleSize()
        {
            _isFirstSetNoteCircleSize = true;
            ResetColIndexManualFlag();
        }
    }
}
