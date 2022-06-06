using System.Collections.Generic;
using System;
namespace ExtendedWinConsole
{
    internal class Logger
    {
        List<string> log = new();
        private const int tagDistance = -7, messageDistance = -30, timeStampDistance = -20;
        public int Count
        {
            get
            {
                return log.Count;
            }

        }
        public void addError(string message)
        {
            log.Add($"{"Error:",tagDistance} {message,messageDistance} {DateTime.Now,timeStampDistance}");
        }
        public void addInfo(string message)
        {
            log.Add($"{"Info:",tagDistance} {message,messageDistance} {DateTime.Now,timeStampDistance}");
        }
        public string getLatest()
        {
            return log[log.Count-1];
        }
        public string getIndex(int index)
        {
            if (index < 0 || index > log.Count)
                return string.Empty;
            else
                return log[index];
        }
        public string[] getAll()
        {
            return log.ToArray();
        }
    }
}
