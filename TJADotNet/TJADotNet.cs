using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TJADotNet.Format;

namespace TJADotNet
{
    /// <summary>
    /// .tjaフォーマットの読み込みからパースまでを担当するメインクラス。
    /// </summary>
    public class TJADotNet
    {
        /// <summary>
        /// 譜面をパースします。
        /// </summary>
        /// <param name="str">.tjaフォーマットな文字列。</param>
        public TJADotNet(string str)
        {
            // COURSE: で分割 ついでにコメントも消す
            var splitedCourses = Regex.Replace(str, @" *//.*", "", RegexOptions.Multiline).Split(new string[] { "COURSE:" }, StringSplitOptions.None);
            // 各要素にCOURSE: をくっつける ただし、[0]は共通ヘッダなので、COURSE:をつけない。
            for (int i = 1; i < splitedCourses.Length; i++)
            {
                // COURSE:を再び付与してsplitedCoursesに入れる
                splitedCourses[i] = "COURSE:" + splitedCourses[i];
            }

            // 共通ヘッダを取り出して coursesに投げる。
            var commonHeader = splitedCourses[0];
            var courses = new string[splitedCourses.Length - 1];
            courses = splitedCourses.Skip(1).ToArray();

            // 共通ヘッダのパース
            Chart.CommonHeader = GetHeaderFromString(commonHeader);

            // 各コースのパース
            for (int i = 0; i < courses.Length; i++)
            {
                void setComposite(string course, bool first)
                {
                    var (header, text, remain, side) = SplitCourse(course);
                    var headerList = GetHeaderFromString(header);
                    if (first)
                    {
                        Chart.Courses.Add(new Course(headerList, text));
                    }
                    if (side == "")
                    {
                        // #START だった
                        Chart.Courses[i].Measure.Common = getMeasureThisPlayerSide(text);
                    }
                    else if (side == "P1")
                    {
                        // #START P1だった
                        Chart.Courses[i].Measure.Player1 = getMeasureThisPlayerSide(text);
                    }
                    else if (side == "P2")
                    {
                        // #START P2だった
                        Chart.Courses[i].Measure.Player2 = getMeasureThisPlayerSide(text);
                    }
                    if (!string.IsNullOrWhiteSpace(remain))
                    {
                        setComposite(remain, false);
                    }
                }

                List<string> getMeasureThisPlayerSide(string playerMeasure)
                {
                    var reader = new StringReader(playerMeasure);
                    var nowMeasure = "";
                    var measures = new List<string>();
                    while (reader.Peek() > -1)
                    {
                        var nowLine = reader.ReadLine();
                        if (nowLine.Trim().IndexOf(",") > -1)
                        {
                            // 行にカンマがある
                            if (nowLine.Trim().StartsWith("#"))
                            {
                                // カンマがあるけど、多分命令行なので処理を続行する
                                nowMeasure += nowLine + NewLine;
                                continue;
                            }
                            else
                            {
                                // 小節の終わり
                                // 0,0, という書き方もあるので、ちゃんと今の小節だけ抜き出してやる。
                                void addOneMeasure(string measureLine)
                                {
                                    // ,を除く譜面の抜き出し。
                                    var target = measureLine.Substring(0, measureLine.IndexOf(","));
                                    // 空だった場合、一小節としてカウントする
                                    if (target == "")
                                    {
                                        // つまり,で一小節
                                        // 0,と解釈させる
                                        target = "0";
                                    }
                                    nowMeasure += target;
                                    // それをListに追加
                                    measures.Add(nowMeasure);
                                    // クリア
                                    nowMeasure = "";
                                    // 残りに,が存在する(=その行にまだ小節がある)
                                    var remain = measureLine.Substring(measureLine.IndexOf(",") + 1);
                                    if (remain.Contains(","))
                                    {
                                        // 再帰処理
                                        addOneMeasure(remain);
                                    }
                                }
                                addOneMeasure(nowLine.Trim());
                                continue;
                            }
                        }
                        else
                        {
                            // もちろん続ける
                            nowMeasure += nowLine + NewLine;
                            continue;
                        }
                    }
                    reader.Dispose();
                    return measures;
                }
                setComposite(courses[i], true);
            }
            foreach (var common in Chart.CommonHeader)
            {
                bool header(string name)
                {
                    return name == common.Name.Trim();
                }
                string subtitler(string value, out SubTitleModes subtitleMode)
                {
                    if (value.StartsWith("--") || value.StartsWith("++"))
                    {
                        var trimedValue = value.Substring(2);
                        if (value.StartsWith("--"))
                        {
                            subtitleMode = SubTitleModes.Hide;
                        }
                        else
                        {
                            subtitleMode = SubTitleModes.Show;
                        }
                        return trimedValue;
                    }
                    else
                    {
                        subtitleMode = SubTitleModes.Hide;
                        return value;
                    }
                }
                if (header("TITLE"))
                {
                    Chart.Info.Title = common.Value;
                }
                else if (header("SUBTITLE"))
                {
                    Chart.Info.SubTitle = subtitler(common.Value, out var mode);
                    Chart.Info.SubTitleMode = mode;
                }
                else if (header("BPM"))
                {
                    if (double.TryParse(common.Value, out var result))
                    {
                        Chart.Info.BPM = result;
                    }
                }
                else if (header("WAVE"))
                {
                    Chart.Info.Wave = common.Value;
                }
                else if (header("OFFSET"))
                {
                    if (double.TryParse(common.Value, out var result))
                    {
                        Chart.Info.Offset = result;
                    }
                }
                else if (header("DEMOSTART"))
                {
                    if (double.TryParse(common.Value, out var result))
                    {
                        Chart.Info.DemoStart = result;
                    }
                }
                else if (header("GENRE"))
                {
                    Chart.Info.Genre = common.Value;
                }
                else if (header("SONGVOL"))
                {
                    if (int.TryParse(common.Value, out var result))
                    {
                        Chart.Info.SongVol = result;
                    }
                }
                else if (header("SEVOL"))
                {
                    if (int.TryParse(common.Value, out var result))
                    {
                        Chart.Info.SeVol = result;
                    }
                }
                else if (header("SCOREMODE"))
                {
                    if (int.TryParse(common.Value, out var result))
                    {
                        switch (result)
                        {
                            case 0:
                                Chart.Info.ScoreMode = ScoreModes.Gen1;
                                break;
                            case 1:
                                Chart.Info.ScoreMode = ScoreModes.Gen2;
                                break;
                            case 2:
                                Chart.Info.ScoreMode = ScoreModes.Gen3;
                                break;
                            default:
                                Chart.Info.ScoreMode = ScoreModes.Gen3;
                                break;
                        }
                    }
                }
                else if (header("SIDE"))
                {
                    if (int.TryParse(common.Value, out var result))
                    {
                        switch (result)
                        {
                            case 0:
                                Chart.Info.Side = Sides.Normal;
                                break;
                            case 1:
                                Chart.Info.Side = Sides.Extra;
                                break;
                            case 2:
                                Chart.Info.Side = Sides.Both;
                                break;
                            default:
                                Chart.Info.Side = Sides.Both;
                                break;
                        }
                    }
                    else
                    {
                        switch (common.Value)
                        {
                            case "NORMAL":
                                Chart.Info.Side = Sides.Normal;
                                break;
                            case "EX":
                                Chart.Info.Side = Sides.Extra;
                                break;
                            case "BOTH":
                                Chart.Info.Side = Sides.Both;
                                break;
                            default:
                                Chart.Info.Side = Sides.Both;
                                break;
                        }
                    }
                }
                else if (header("LIFE"))
                {
                    if (int.TryParse(common.Value, out var result))
                    {
                        Chart.Info.Life = result;
                    }
                }
                else if (header("BGIMAGE"))
                {
                    Chart.Info.BgImage = common.Value;
                }
                else if (header("BGMOVIE"))
                {
                    Chart.Info.BgMovie = common.Value;
                }
                else if (header("MOVIEOFFSET"))
                {
                    if (double.TryParse(common.Value, out var result))
                    {
                        Chart.Info.MovieOffset = result;
                    }
                }
            }

            foreach (var course in Chart.Courses)
            {
                foreach (var item in course.Headers)
                {
                    bool header(string name)
                    {
                        return name == item.Name.Trim();
                    }
                    if (header("COURSE"))
                    {
                        if (int.TryParse(item.Value, out var result))
                        {
                            course.Info.Course = CourseConverter.GetCoursesFromNumber(result);
                        }
                        else
                        {
                            course.Info.Course = CourseConverter.GetCoursesFromString(item.Value);
                        }
                    }
                    else if (header("LEVEL"))
                    {
                        if (int.TryParse(item.Value, out var result))
                        {
                            course.Info.Level = result;
                        }
                    }
                    else if (header("BALLOON"))
                    {
                        // 末尾の,対策
                        var split = item.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < split.Length; i++)
                        {
                            if (int.TryParse(split[i], out var result))
                            {
                                course.Info.Balloon.Add(result);
                            }
                        }
                    }
                    else if (header("SCOREINIT"))
                    {
                        // ,で区切って、真打配点をしている場合がある
                        var split = item.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length > 1)
                        {
                            // 真打あり
                            if (int.TryParse(split[0], out var result))
                            {
                                course.Info.ScoreInit = result;
                            }
                            if (int.TryParse(split[1], out var shin))
                            {
                                course.Info.ScoreInit_Shinuchi = shin;
                            }
                        }
                        else
                        {
                            if (int.TryParse(item.Value, out var result))
                            {
                                course.Info.ScoreInit = result;
                            }
                        }
                    }
                    else if (header("SCOREDIFF"))
                    {
                        if (int.TryParse(item.Value, out var result))
                        {
                            course.Info.ScoreDiff = result;
                        }
                    }
                    else if (header("STYLE"))
                    {
                        if (int.TryParse(item.Value, out var result))
                        {
                            switch (result)
                            {
                                case 1:
                                    course.Info.Style = Styles.Single;
                                    break;
                                case 2:
                                    course.Info.Style = Styles.Double;
                                    break;
                                default:
                                    course.Info.Style = Styles.Single;
                                    break;
                            }
                        }
                        else
                        {
                            switch (item.Value)
                            {
                                case "single":
                                    course.Info.Style = Styles.Single;
                                    break;
                                case "double":
                                case "couple":
                                    course.Info.Style = Styles.Double;
                                    break;
                                default:
                                    course.Info.Style = Styles.Single;
                                    break;
                            }
                        }
                    }
                    else if (header("EXAM1") || header("EXAM2") || header("EXAM3"))
                    {
                        var split = item.Value.Split(new string[] { "," }, StringSplitOptions.None);
                        var exam = new Exam();
                        // 条件
                        if (split[0] != null)
                        {
                            switch (split[0])
                            {
                                case "g":
                                    exam.Condition = Conditions.Gauge;
                                    break;
                                case "jp":
                                    exam.Condition = Conditions.JudgePerfect;
                                    break;
                                case "jg":
                                    exam.Condition = Conditions.JudgeGood;
                                    break;
                                case "jb":
                                    exam.Condition = Conditions.JudgeBad;
                                    break;
                                case "s":
                                    exam.Condition = Conditions.Score;
                                    break;
                                case "r":
                                    exam.Condition = Conditions.Roll;
                                    break;
                                case "h":
                                    exam.Condition = Conditions.Hit;
                                    break;
                                case "c":
                                    exam.Condition = Conditions.Combo;
                                    break;
                                default:
                                    exam.Condition = Conditions.Gauge;
                                    break;
                            }
                        }
                        else
                        {
                            exam.Condition = Conditions.Gauge;
                        }
                        // 範囲
                        if (split[3] != null)
                        {
                            switch (split[3])
                            {
                                case "m":
                                    exam.Scope = Scopes.More;
                                    break;
                                case "l":
                                    exam.Scope = Scopes.Less;
                                    break;
                                default:
                                    exam.Scope = Scopes.More;
                                    break;
                            }
                        }
                        else
                        {
                            exam.Scope = Scopes.More;
                        }
                        // 赤合格
                        if (split[1] != null)
                        {
                            if (int.TryParse(split[1], out var result))
                            {
                                exam.RedValue = result;
                            }
                        }
                        else
                        {
                            exam.RedValue = 0;
                        }

                        // 金合格
                        if (split[2] != null)
                        {
                            if (int.TryParse(split[2], out var result))
                            {
                                exam.GoldValue = result;
                            }
                        }
                        else
                        {
                            exam.GoldValue = 0;
                        }

                        // 最後にEXAM何かを決めて、それに代入。
                        switch (item.Name.Trim())
                        {
                            case "EXAM1":
                                course.Info.Exam1 = exam;
                                break;
                            case "EXAM2":
                                course.Info.Exam2 = exam;
                                break;
                            case "EXAM3":
                                course.Info.Exam3 = exam;
                                break;
                            default:
                                throw new InvalidDataException();
                        }
                    }
                    else if (header("GAUGEINCR"))
                    {
                        switch (item.Value)
                        {
                            case "NORMAL":
                                course.Info.GaugeIncrease = Gauges.Normal;
                                break;
                            case "FLOOR":
                                course.Info.GaugeIncrease = Gauges.Floor;
                                break;
                            case "ROUND":
                                course.Info.GaugeIncrease = Gauges.Round;
                                break;
                            case "NOTFIX":
                                course.Info.GaugeIncrease = Gauges.NotFix;
                                break;
                            case "CEILING":
                                course.Info.GaugeIncrease = Gauges.Ceiling;
                                break;
                            default:
                                course.Info.GaugeIncrease = Gauges.Normal;
                                break;
                        }
                    }
                    else if (header("TOTAL"))
                    {
                        if (double.TryParse(item.Value, out var result))
                        {
                            course.Info.Total = result;
                        }
                    }
                    else if (header("HIDDENBRANCH"))
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            course.Info.HiddenBranch = true;
                        }
                        else
                        {
                            course.Info.HiddenBranch = false;
                        }
                    }
                }
            }

            // 各譜面のパース処理。
            foreach (var course in Chart.Courses)
            {
                void parseTJA(List<Chip> list, IReadOnlyList<string> measures)
                {
                    var nowTime = (long)(Chart.Info.Offset * 1000 * 1000.0) * -1;
                    var nowScroll = 1.0D;
                    var nowBPM = Chart.Info.BPM;
                    var gogoTime = false;
                    var branching = false;
                    var nowBranch = Branches.Normal;
                    var nowMeasure = new Measure(4, 4);
                    var measureCount = 0;
                    var branchBeforeMeasureCount = 0;
                    var branchBeforeTime = 0L;
                    var branchCount = 0;
                    var branchAfterMeasure = 0;
                    var balloonIndex = 0;
                    Chip rollBegin = null;

                    var bgm = new Chip();
                    bgm.ChipType = Chips.BGMStart;
                    bgm.Time = 0;
                    list.Add(bgm);
                    foreach (var measure in measures)
                    {
                        var nowMeasureNotes = 0;
                        // まずはその小節にある音符数(空白含む)を調べる
                        foreach (var line in measure.Split(new string[] { NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!line.StartsWith("#"))
                            {
                                nowMeasureNotes += line.Length;
                            }
                        }


                        // 実際にListにブチ込んでいく
                        foreach (var line in measure.Split(new string[] { NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            // 小節開始時の一つの音符あたりの時間
                            // わざわざ文字の度再計算させてるけど仕方ないな！
                            var timePerNotes = (long)(nowMeasure.GetRate() / nowBPM / nowMeasureNotes * 1000 * 1000.0);

                            //小節線
                            var measureChip = new Chip();
                            measureChip.ChipType = Chips.Measure;
                            measureChip.IsHitted = false;
                            measureChip.IsGoGoTime = gogoTime;
                            measureChip.CanShow = true;
                            measureChip.Scroll = nowScroll;
                            measureChip.Branch = nowBranch;
                            measureChip.Branching = branching;
                            measureChip.Time = nowTime;
                            measureChip.Scroll = nowScroll;
                            measureChip.BPM = nowBPM;
                            measureChip.Measure = measureCount;
                            // Listへ
                            list.Add(measureChip);

                            if (!line.StartsWith("#"))
                            {
                                // 音符
                                foreach (var note in line)
                                {
                                    var chip = new Chip();
                                    chip.ChipType = Chips.Note;
                                    chip.NoteType = NotesConverter.GetNotesFromChar(note);
                                    chip.IsHitted = false;
                                    chip.IsGoGoTime = gogoTime;
                                    chip.CanShow = true;
                                    chip.Scroll = nowScroll;
                                    chip.Branch = nowBranch;
                                    chip.Branching = branching;
                                    chip.Time = nowTime;
                                    chip.Scroll = nowScroll;
                                    chip.BPM = nowBPM;
                                    chip.Measure = measureCount;

                                    if (chip.NoteType == Notes.Balloon)
                                    {
                                        // ふうせん連打のノルマ
                                        chip.RollCount = course.Info.Balloon[balloonIndex];
                                        balloonIndex++;
                                    }

                                    if (chip.NoteType == Notes.Balloon || chip.NoteType == Notes.RollStart || chip.NoteType == Notes.ROLLStart)
                                    {
                                        // 連打
                                        // 始点を記憶しておく
                                        rollBegin = chip;
                                    }

                                    if (chip.NoteType == Notes.RollEnd)
                                    {
                                        // 連打終端
                                        if (rollBegin != null)
                                        {
                                            rollBegin.RollEnd = chip;
                                        }
                                        rollBegin = null;
                                    }

                                    // ひとつ進める
                                    nowTime += timePerNotes;

                                    // Listへ
                                    list.Add(chip);
                                }
                            }
                            else
                            {
                                // 命令
                                bool command(string name)
                                {
                                    return line.Trim().StartsWith(name);
                                }

                                var chip = new Chip();
                                chip.IsHitted = false;
                                chip.CanShow = false;

                                var trimed = line.Trim();

                                if (command("#MEASURE"))
                                {
                                    var param = trimed.Substring("#MEASURE".Length).Trim();
                                    var split = param.Split(new string[] { "/" }, StringSplitOptions.None);
                                    // 再計算は自動的にされるから問題ない。
                                    if (!string.IsNullOrWhiteSpace(split[0]))
                                    {
                                        nowMeasure.Part = Convert.ToDouble(split[0]);
                                    }
                                    if (!string.IsNullOrWhiteSpace(split[1]))
                                    {
                                        nowMeasure.Beat = Convert.ToDouble(split[1]);
                                    }
                                    chip.ChipType = Chips.MeasureChange;
                                }
                                else if (command("#BPMCHANGE"))
                                {
                                    var param = trimed.Substring("#BPMCHANGE".Length).Trim();
                                    if (!string.IsNullOrWhiteSpace(param))
                                    {
                                        nowBPM = Convert.ToDouble(param);
                                    }
                                    chip.ChipType = Chips.BPMChange;
                                }
                                else if (command("#DELAY"))
                                {
                                    var param = trimed.Substring("#DELAY".Length).Trim();
                                    var delay = 0L;
                                    if (!string.IsNullOrWhiteSpace(param))
                                    {
                                        delay = Convert.ToInt64(Convert.ToDouble(param) * 1000 * 1000.0);
                                    }
                                    nowTime += delay;
                                }
                                else if (command("#SCROLL"))
                                {
                                    var param = trimed.Substring("#SCROLL".Length).Trim();
                                    if (!string.IsNullOrWhiteSpace(param))
                                    {
                                        nowScroll = Convert.ToDouble(param);
                                    }
                                    chip.ChipType = Chips.ScrollChange;
                                }
                                else if (command("#GOGOSTART"))
                                {
                                    gogoTime = true;
                                    chip.ChipType = Chips.GoGoStart;
                                }
                                else if (command("#GOGOEND"))
                                {
                                    gogoTime = false;
                                    chip.ChipType = Chips.GoGoEnd;
                                }
                                else if (command("#SECTION"))
                                {
                                    // シミュレータ側で実装するのでここでは特に何も無い。
                                    chip.ChipType = Chips.Section;
                                }
                                else if (command("#BRANCHSTART"))
                                {
                                    // #BRANCHSTART <type>,expert,master
                                    chip.ChipType = Chips.BranchStart;
                                    branching = true;

                                    branchBeforeMeasureCount = measureCount;
                                    branchBeforeTime = nowTime;
                                    branchCount = 0;

                                    // 1小節前に分岐するフックを入れる。
                                    var beforeMeasure = GetBeforeMeasureFromList(list, list.Count);
                                    var beforeMeasureChip = new Chip();
                                    beforeMeasureChip.ChipType = Chips.Branching;
                                    beforeMeasureChip.BPM = list[beforeMeasure].BPM;
                                    beforeMeasureChip.Scroll = list[beforeMeasure].Scroll;
                                    beforeMeasureChip.Time = list[beforeMeasure].Time;
                                    beforeMeasureChip.IsGoGoTime = list[beforeMeasure].IsGoGoTime;
                                    list.Insert(beforeMeasure, beforeMeasureChip);
                                }
                                else if (command("#BRANCHEND"))
                                {
                                    // 時間を……元に戻すッ！！！
                                    nowTime = branchBeforeTime;
                                    measureCount = branchAfterMeasure;

                                    chip.ChipType = Chips.BranchStart;
                                    branching = false;
                                }
                                else if (command("#N") || command("#E") || command("#M"))
                                {
                                    var type = trimed.Substring(0, 2);
                                    if (!string.IsNullOrWhiteSpace(type))
                                    {
                                        // 時間を……元に戻すッ！！！
                                        nowTime = branchBeforeTime;
                                        branchCount++;
                                        // 一番初めに書かれた譜面分岐の小節数を保持
                                        if (branchCount == 2)
                                        {
                                            branchAfterMeasure = measureCount;
                                        }
                                        measureCount = branchBeforeMeasureCount;
                                        switch (type)
                                        {
                                            case "#N":
                                                nowBranch = Branches.Normal;
                                                break;
                                            case "#E":
                                                nowBranch = Branches.Expert;
                                                break;
                                            case "#M":
                                                nowBranch = Branches.Master;
                                                break;
                                        }
                                    }
                                }
                                else if (command("#LEVELHOLD"))
                                {
                                    // シミュレータ側で実装するのでここでは特に何も無い。
                                    chip.ChipType = Chips.LevelHold;
                                }

                                // 共通
                                chip.IsGoGoTime = gogoTime;
                                chip.Scroll = nowScroll;
                                chip.BPM = nowBPM;
                                chip.Branch = nowBranch;
                                chip.Branching = branching;
                                chip.Time = nowTime;
                                chip.Measure = measureCount;

                                list.Add(chip);
                            }
                            measureCount++;
                        }
                    }
                }
                parseTJA(course.Chip.Common, course.Measure.Common);
                parseTJA(course.Chip.Player1, course.Measure.Player1);
                parseTJA(course.Chip.Player2, course.Measure.Player2);
            }

        }

        public List<Header> GetHeaderFromString(string str)
        {
            var headers = new List<Header>();
            foreach (var item in str.Split(new string[] { NewLine }, StringSplitOptions.None))
            {
                var line = item.Trim();
                var split = line.Split(new string[] { ":" }, StringSplitOptions.None);
                if (split.Length >= 2)
                {
                    // splitした結果、要素数が2以上ならヘッダーとして成立する。
                    // タイトルなどに:が付く場合もあるので、ちゃんとjoinしてあげる
                    headers.Add(new Header(split[0], string.Join(":", split.Skip(1))));
                }
            }
            return headers;
        }

        public (string header, string text, string remain, string side) SplitCourse(string str)
        {
            var split = str.Split(new string[] { NewLine }, StringSplitOptions.None);
            var splitHeader = "";
            var splitText = "";
            var remain = "";
            var side = "";
            for (int lines = 0; lines < split.Length; lines++)
            {
                if (split[lines].Trim().StartsWith("#START"))
                {
                    splitHeader = string.Join(NewLine, split.Take(lines));
                    // #ENDまでTakeしたい
                    for (int endPoint = lines; endPoint < split.Length; endPoint++)
                    {
                        if (split[endPoint].Trim().StartsWith("#END"))
                        {
                            // 抜き出したい部分は、linesからendPoint - linesまで。
                            splitText = string.Join(NewLine, split.Skip(lines).Take(endPoint - lines));
                            if (endPoint < split.Length)
                            {
                                // 残りは、endPointの次から最後まで。
                                remain = string.Join(NewLine, split.Skip(endPoint + 1));
                            }
                            side = split[lines].Trim().Substring("#START".Length).Trim();
                            break;
                        }
                    }
                    break;
                }
            }
            return (splitHeader, splitText, remain, side);
        }

        /// <summary>
        /// パース結果。
        /// </summary>
        public Chart Chart { get; private set; } = new Chart();

        /// <summary>
        /// TJADotNetで扱う難易度はいくつあるのか。
        /// </summary>
        public static int Total_Difficulty
        {
            get
            {
                return Enum.GetNames(typeof(Courses)).Length;
            }
        }

        /// <summary>
        /// TJADotNetで扱う譜面分岐数はいくつあるのか。
        /// </summary>
        public static int Total_Branches
        {
            get
            {
                return Enum.GetNames(typeof(Branches)).Length;
            }
        }

        /// <summary>
        /// TJADotNetのバージョンを返します。
        /// </summary>
        public static string Version
        {
            get
            {
                var ver = "Ver.";
                var asm = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // Ver.x.x
                return ver + string.Format("{0}.{1}", asm.Major, asm.Minor);                
            }
        }


        /// <summary>
        /// 改行。Unix環境でも常にCRLFとして扱う。
        /// </summary>
        private const string NewLine = "\r\n";

        /// <summary>
        /// チップリストからインデックスの前にある小節を返します。
        /// </summary>
        /// <param name="list">チップリスト。</param>
        /// <param name="index">インデックス。</param>
        /// <returns>小節番号。</returns>
        public int GetBeforeMeasureFromList(IReadOnlyList<Chip> list, int index)
        {
            for (var i = index; i > 0; i--)
            {
                if (list[i].ChipType == Chips.Measure)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}