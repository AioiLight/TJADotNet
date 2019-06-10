using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJADotNet
{
    /// <summary>
    /// 譜面情報クラス。
    /// </summary>
    public class ChartInfo
    {
        /// <summary>
        /// 譜面のタイトル。
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 譜面のサブタイトル。
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// サブタイトルの表示方式。
        /// </summary>
        public SubTitleModes SubTitleMode { get; set; }
        /// <summary>
        /// 基本BPM。
        /// </summary>
        public double BPM { get; set; }
        /// <summary>
        /// 音源ファイル。
        /// </summary>
        public string Wave { get; set; }
        /// <summary>
        /// オフセット。
        /// </summary>
        public double Offset { get; set; }
        /// <summary>
        /// デモ音源再生開始時間。
        /// </summary>
        public double DemoStart { get; set; }
        /// <summary>
        /// ジャンル。
        /// </summary>
        public string Genre { get; set; }
        /// <summary>
        /// 曲音量。
        /// </summary>
        public int SongVol { get; set; }
        /// <summary>
        /// 効果音量。
        /// </summary>
        public int SeVol { get; set; }
        /// <summary>
        /// 配点方式。
        /// </summary>
        public ScoreModes ScoreMode { get; set; }
        /// <summary>
        /// 表譜面・裏譜面
        /// </summary>
        public Sides Side { get; set; }
        /// <summary>
        /// 残機。
        /// </summary>
        public int Life { get; set; }
        /// <summary>
        /// 太鼓譜面・指譜面。
        /// </summary>
        public Games Game { get; set; }
        /// <summary>
        /// 背景画像。
        /// </summary>
        public string BgImage { get; set; }
        /// <summary>
        /// 背景動画。
        /// </summary>
        public string BgMovie { get; set; }
        /// <summary>
        /// 背景動画のオフセット。
        /// </summary>
        public double MovieOffset { get; set; }
    }

    public enum ScoreModes
    {
        /// <summary>
        /// 旧作配点
        /// </summary>
        Gen1,
        /// <summary>
        /// 旧筐体配点
        /// </summary>
        Gen2,
        /// <summary>
        /// 新筐体配点
        /// </summary>
        Gen3
    }

    /// <summary>
    /// 表譜面・裏譜面
    /// </summary>
    public enum Sides
    {
        Normal,
        Extra,
        Both
    }

    /// <summary>
    /// 太鼓譜面か指譜面か
    /// </summary>
    public enum Games
    {
        Taiko,
        Jube
    }

    /// <summary>
    /// サブタイトルの表示方式。
    /// </summary>
    public enum SubTitleModes
    {
        Hide,
        Show
    }
}
