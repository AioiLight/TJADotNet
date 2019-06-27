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
        /// 連打(大)
        /// </summary>
        ROLLStart,
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

    public static class SENoteGenerator
    {
        /// <summary>
        /// Chip型Listから前後の譜面を考慮してSENotesを返します。
        /// </summary>
        /// <param name="chips">Chip型List</param>
        /// <returns></returns>
        public static void GenerateSENotes(List<Chip> chips)
        {
            for (int i = 0; i < chips.Count; i++)
            {
                if (chips[i].NoteType == Notes.Space)
                {
                    // 空白だったら次へ進む。
                    continue;
                }

                // とりあえず必要なものたち。
                var nowChip = chips[i];
                var beforeChip = GetBeforeChip(i, chips);
                var afterChip = GetAfterChip(i, chips);

                // 実際にSENoteをつける。
                // 確定分はそのまま処理。

                switch(nowChip.NoteType)
                {
                    case Notes.DON:
                        nowChip.SENote = SENotes.DON;
                        break;
                    case Notes.KA:
                        nowChip.SENote = SENotes.KA;
                        break;
                    case Notes.RollStart:
                        nowChip.SENote = SENotes.RollStart;
                        break;
                    case Notes.ROLLStart:
                        nowChip.SENote = SENotes.ROLLStart;
                        break;
                    case Notes.Balloon:
                        nowChip.SENote = SENotes.Balloon;
                        break;
                    case Notes.Kusudama:
                        nowChip.SENote = SENotes.Kusudama;
                        break;
                    default:
                        nowChip.SENote = GetSENoteFromDuration(i, chips);
                        break;
                }
            }
        }

        private static Chip GetBeforeChip(int i, IReadOnlyList<Chip> chips)
        {
            // 範囲を超えない範囲(?)で、前後のチップを取得する。
            if (i > 0)
            {
                // 1以上なので必ず前のチップがある
                for (int index = i; index > 0; index--)
                {
                    if (chips[index].NoteType != Notes.Space)
                    {
                        // 空白以外の音符があるなら、それを記憶。
                        return chips[index];
                    }
                }
            }
            return null;
        }

        private static Chip GetAfterChip(int i, IReadOnlyList<Chip> chips)
        {
            if (i < chips.Count - 1)
            {
                // Listの最大値の-1以下なので必ず後のチップがある
                for (int index = i; index < chips.Count; index++)
                {
                    if (chips[index].NoteType != Notes.Space)
                    {
                        // 空白以外の音符があるなら、それを記憶。
                        return chips[index];
                    }
                }
            }
            return null;
        }

        private static SENotes GetSENoteFromDuration(int i, IReadOnlyList<Chip> chips)
        {
            // とりあえず必要なものたち。
            var nowChip = chips[i];
            var beforeChip = GetBeforeChip(i, chips);
            var afterChip = GetAfterChip(i, chips);

            var nowTime = nowChip.Time;
            var diffBefore = nowTime - beforeChip.Time;
            var diffAfter = afterChip.Time - nowTime;


            var time16 = (long)(nowChip.Measure.GetRate() / nowChip.BPM / 16 * 1000 * 1000.0);
            var time12 = (long)(nowChip.Measure.GetRate() / nowChip.BPM / 12 * 1000 * 1000.0);

            if (diffBefore > time12 && diffAfter > time12)
            {
                // 3連符の間隔より大きく離れてる …… ドン・カッ
                if (nowChip.NoteType == Notes.Don) return SENotes.Don;
                if (nowChip.NoteType == Notes.Ka) return SENotes.Katsu;
            }

            if (diffBefore <= time12 && diffAfter <= time12)
            {
                // 3連符の間隔未満離れてる …… ド・カ
                if (nowChip.NoteType == Notes.Don) return SENotes.Do;
                if (nowChip.NoteType == Notes.Ka) return SENotes.Ka;
            }
            return nowChip.SENote;
        }
    }
}
