using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.BusinessLogic;

namespace Application
{
    class Program
    {
        enum CommandLineInfo
        {
            COMMAND_NAME_INDEX,
            COMMAND_PARAM_INDEX
        }

        static void Main(string[] args)
        {
            var dbConnData = new Tuple<string, int, string, string, string>("localhost", 1521, "orcl", "root", "root");
            Console.WriteLine("Используйте dbinfoset <host> <port> <sid> <user> <password> для установки сведений о соединении с Oracle DB");
            while (true)
            {
                Console.WriteLine("Используйте do <путь к папке/файлу>");
                string input = Console.ReadLine();

                string[] p = input.Split(' ');
                if(p.Length == 0)
                    continue;

                if (p[(int) CommandLineInfo.COMMAND_NAME_INDEX].CompareTo("do") == 0 && p.Length == 2)
                {
                    try
                    {
                        new MyTask(p[(int)CommandLineInfo.COMMAND_PARAM_INDEX], dbConnData).run();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if(p[(int)CommandLineInfo.COMMAND_NAME_INDEX].CompareTo("dbinfoset") == 0 && p.Length == 6)
                {
                    dbConnData = new Tuple<string, int, string, string, string>(p[(int)CommandLineInfo.COMMAND_PARAM_INDEX], Convert.ToInt32(p[(int)CommandLineInfo.COMMAND_PARAM_INDEX + 1]),
                        p[(int)CommandLineInfo.COMMAND_PARAM_INDEX + 2], p[(int)CommandLineInfo.COMMAND_PARAM_INDEX + 3], p[(int)CommandLineInfo.COMMAND_PARAM_INDEX + 4]);

                    Console.WriteLine("Параметры подключения БД успешно изменены");
                }
            }
        }
    }
}
