using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuessMyPhoto.Models
{
    public class CountryCode
    {
        public string ContryName { get; set; }
        public string ContryCode { get; set; }
        public string PhoneCode { get; set; }

        public string PhoneCodeDigitOnly => Regex.Replace(PhoneCode, "[^.0-9]", "");
        public string CountryWithCode => $"{ContryName} ({PhoneCode})";
    }
}
