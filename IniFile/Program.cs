using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static Program;

internal class Program
{
    class IniFile
    {
        private readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name; 
        private readonly string AppNameWithPath;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        // Конструктор класса
        public IniFile(string? IniPath = null)
        {
            AppNameWithPath = new FileInfo(IniPath ?? AppName + ".ini").FullName;
        }

        // Запись ключа Key со значением Value в секцию Section ini-файла
        public void WriteKey(string? Key, string? Value, string? Section = null)
        {
            WritePrivateProfileString(Section ?? AppName, Key, Value, AppNameWithPath);
        }

        // Чтение ключа Key из секции Section ini-файла
        public string ReadKey(string Key, string? Section = null)
        {
            int Size = 1024;
            var RetVal = new StringBuilder(Size);
            GetPrivateProfileString(Section ?? AppName, Key, "", RetVal, Size, AppNameWithPath);
            return RetVal.ToString();
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IniInfoAttribute:Attribute
    {
        private string _path;
        private string _prop;

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string Prop { get => _prop; set => _prop = value; }

        public IniInfoAttribute() { }
        public IniInfoAttribute(string path, string prop)
        {
            Path = path;
            _prop = prop;
        }

    }
    [AttributeUsage(AttributeTargets.Class)]
    public class ExamAttribute:Attribute
    {
        public int _threshold;
        public ExamAttribute() { }
        public ExamAttribute(int treshold)
        {
            _threshold = treshold;
        }
    }

    [Exam(60)]
    class Student
    {
        [IniInfo("*.ini","Name")]          
        public string Name { get; set; }
        [IniInfo("*.ini","Age")]    
        public string Age { get; set;  }
        [IniInfo("*.ini", "Mark")]
        public int ExamMark { get; set; } = 0;

        public override string ToString()
        {
            return $"\tName: {Name}\n \tAge: {Age}\n \tMark = {ExamMark}\n\n";
        }
    } 

    private static void Main(string[] args)
    {
        GetAttribute(typeof(Student));

        IniFile file = new("inifile.ini");

        Student man = new();
        man.Name = file.ReadKey("name", "student1");
        man.Age = file.ReadKey("age", "student1");
        man.ExamMark = int.Parse(file.ReadKey("mark", "student1"));


        Student woman = new();
        woman.Name = file.ReadKey("name", "student2");
        woman.Age = file.ReadKey("age", "student2");
        woman.ExamMark = int.Parse(file.ReadKey("mark", "student2"));

        List<Student> students = new List<Student>
        {
            man,
            woman
        };
        foreach (var item in students)
        {
            if (EntryTreshold(item))
            {
                Console.WriteLine($"Студент:\n{item} экзамен сдал!");
            }
            else
                Console.WriteLine($"Студент:\n{item} экзамен провалил!");
        }
    }

    public static void GetAttribute(Type t)
    {

        MemberInfo[] MyMemberInfo = (MemberInfo[])t.GetRuntimeProperties();

        for (int i = 0; i < MyMemberInfo.Length; i++)
        {
            IniInfoAttribute att = (IniInfoAttribute)Attribute.GetCustomAttribute(MyMemberInfo[i], typeof(IniInfoAttribute));
            if (att == null)
                Console.WriteLine("Метод {0} не имеет атрибута.\n", MyMemberInfo[i].ToString());
            else
            {
                Console.WriteLine($"Значение свойства {MyMemberInfo[i]} класса {t} в файле {att.Path}, под ключом {att.Prop} " );
            }
        }

    }
    static bool EntryTreshold(Student student)
    {
        Type t = typeof(Student);
        foreach (object item in t.GetCustomAttributes(false))
        {
            ExamAttribute att = item as ExamAttribute;
            if (att == null)
                continue;
            if (student.ExamMark >= att._threshold)
                return true;
            else
                return false;
        }
        return true;
    }
}