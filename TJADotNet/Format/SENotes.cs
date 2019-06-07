using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJADotNet.Format
{
    /// <summary>
    /// 音符の擬音列挙型。
    /// </summary>
    public enum SENotes
    {
        /// <summary>
        /// ドン
        /// </summary>
        Don,
        /// <summary>
        /// ド
        /// </summary>
        Do,
        /// <summary>
        /// コ
        /// </summary>
        Ko,
        /// <summary>
        /// カッ
        /// </summary>
        Katsu,
        /// <summary>
        /// カ
        /// </summary>
        Ka,
        /// <summary>
        /// ドン(大)
        /// </summary>
        DON,
        /// <summary>
        /// カッ(大)
        /// </summary>
        KA,
        /// <summary>
        /// 連打
        /// </summary>
        RollStart,
        /// <summary>
        /// 連打中
        /// </summary>
        Rolling,
        /// <summary>
        /// 連打終了
        /// </summary>
        RollEnd,
        /// <summary>
        /// ふうせん
        /// </summary>
        Balloon,
        /// <summary>
        /// くすだま
        /// </summary>
        Kusudama
    }
}
