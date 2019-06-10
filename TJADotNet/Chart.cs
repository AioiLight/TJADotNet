using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJADotNet
{
    public class Chart
    {
        public List<Header> CommonHeader { get; set; } = new List<Header>();
        public List<Course> Courses { get; set; } = new List<Course>();
        public ChartInfo Info { get; set; } = new ChartInfo();
    }


    public class Course
    {
        public Course(List<Header> headers, string text)
        {
            Headers = headers;
            Text = text;
        }
        public List<Header> Headers { get; set; } = new List<Header>();
        public string Text { get; set; }
        public Composite_Measure Measure { get; set; } = new Composite_Measure();
        public Composite_Chip Chip { get; set; } = new Composite_Chip();
        public CourseInfo Info { get; set; } = new CourseInfo();
    }

    public class Composite_Measure
    {
        public List<string> Common { get; set; } = new List<string>();
        public List<string> Player1 { get; set; } = new List<string>();
        public List<string> Player2 { get; set; } = new List<string>();
    }

    public class Composite_Chip
    {
        public List<Chip> Common { get; set; } = new List<Chip>();
        public List<Chip> Player1 { get; set; } = new List<Chip>();
        public List<Chip> Player2 { get; set; } = new List<Chip>();
    }

    public class Header
    {
        public Header(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, Value);
        }
    }
}
