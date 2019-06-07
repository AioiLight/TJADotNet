using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJADotNet.Format
{
    /// <summary>
    /// チップの列挙型。
    /// </summary>
    public enum Chips
    {
        /// <summary>
        /// 音符
        /// </summary>
        Note,
        /// <summary>
        /// 小節線
        /// </summary>
        Measure,
        /// <summary>
        /// 拍子変更
        /// </summary>
        MeasureChange,
        /// <summary>
        /// BPM変更
        /// </summary>
        BPMChange,
        /// <summary>
        /// スクロール速度変更
        /// </summary>
        ScrollChange,
        /// <summary>
        /// ゴーゴータイム開始
        /// </summary>
        GoGoStart,
        /// <summary>
        /// ゴーゴータイム終了
        /// </summary>
        GoGoEnd,
        /// <summary>
        /// 小節線OFF
        /// </summary>
        BarLineOff,
        /// <summary>
        /// 小節線ON
        /// </summary>
        BarLineOn,
        /// <summary>
        /// 実際に分岐する
        /// </summary>
        Branching,
        /// <summary>
        /// 譜面分岐開始
        /// </summary>
        BranchStart,
        /// <summary>
        /// 譜面分岐終了
        /// </summary>
        BranchEnd,
        /// <summary>
        /// 譜面分岐リセット
        /// </summary>
        Section,
        /// <summary>
        /// 譜面分岐維持
        /// </summary>
        LevelHold,
        /// <summary>
        /// 段位認定モード 曲切り替え
        /// </summary>
        NextSong,
        /// <summary>
        /// 歌詞変更
        /// </summary>
        LyricChange,
        /// <summary>
        /// BGMの開始
        /// </summary>
        BGMStart,
        /// <summary>
        /// 背景動画の開始
        /// </summary>
        MovieStart,
        /// <summary>
        /// 背景画像の変更
        /// </summary>
        BGChange
    }
}
