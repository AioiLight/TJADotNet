using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJADotNet.Format
{
    /// <summary>
    /// 音符の列挙型。
    /// </summary>
    public enum Notes
    {
        /// <summary>
        /// 空白
        /// </summary>
        Space,
        /// <summary>
        /// ドン
        /// </summary>
        Don,
        /// <summary>
        /// カッ
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
        /// 連打開始
        /// </summary>
        RollStart,
        /// <summary>
        /// 連打開始(大)
        /// </summary>
        ROLLStart,
        /// <summary>
        /// ふうせん連打
        /// </summary>
        Balloon,
        /// <summary>
        /// 連打終了
        /// </summary>
        RollEnd,
        /// <summary>
        /// くすだま
        /// </summary>
        Kusudama
    }

    public static class NotesConverter
    {
        public static Notes GetNotesFromChar(char ch)
        {
            switch (ch)
            {
                case '0': return Notes.Space;
                case '1': return Notes.Don;
                case '2': return Notes.Ka;
                case '3': return Notes.DON;
                case '4': return Notes.KA;
                case '5': return Notes.RollStart;
                case '6': return Notes.ROLLStart;
                case '7': return Notes.Balloon;
                case '8': return Notes.RollEnd;
                case '9': return Notes.Kusudama;
                default: return Notes.Space;
            }
        }
    }
}
