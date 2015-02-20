using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2_announce
{
    public class ServerInfo
    {
        public int GetChronikiCategory()
        {
            switch (Chroniki.ToLower())
            {
                case "interlude": 
				case "interlude+":
				case "interlude +": return 1;
				case "final":
                case "gracia final": return 2;
                case "epilogue": return 3;
                case "freya": return 4;
                case "high five": return 5;
                case "goddest of destruction":
                case "god": return 6;
                case "TOP 100": return 7;
                case "c4": return 8;
				case "lindvior" : return 9;
            }

            return 1;
        }
        public string Chroniki;
        public string Rates;
        public string Name;
        public string Description;
        public string Url;
        public string Date;
        public ServerInfo()
        {
            Chroniki = "";
            Rates = "";
            Name = "";
            Description = "";
            Url = "";
            Date = "";
        }
    }
}
