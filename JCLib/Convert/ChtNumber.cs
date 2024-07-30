using System;
using System.Configuration;
using System.Text;

namespace JCLib
{
    public class ChtNumber
    {
        // 阿拉伯數字轉中文大寫
        public static string Transform(int number)
        {
            string[] chineseNumber = { "零", "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖" };
            string[] unit = { "", "拾", "佰", "仟", "萬", "拾萬", "佰萬", "仟萬", "億", "拾億", "佰億", "仟億", "兆", "拾兆", "佰兆", "仟兆" };
            StringBuilder ret = new StringBuilder();
            string inputNumber = number.ToString();
            int idx = inputNumber.Length;
            bool needAppendZero = false;
            foreach (char c in inputNumber)
            {
                idx--;
                if (c > '0')
                {
                    if (needAppendZero)
                    {
                        ret.Append(chineseNumber[0]);
                        needAppendZero = false;
                    }
                    ret.Append(chineseNumber[(int)(c - '0')] + unit[idx]);
                }
                else
                    needAppendZero = true;
            }
            string chtString = ret.Length == 0 ? chineseNumber[0] : ret.ToString();

            chtString = removeChar(chtString, '兆');
            chtString = removeChar(chtString, '億');
            chtString = removeChar(chtString, '萬');

            return chtString;
        }

        private static string removeChar(string inputString, char varChar)
        {
            while (inputString.Split(varChar).Length >= 3) // 字元出現兩次以上
                inputString = inputString.Remove(inputString.IndexOf(varChar), 1);
            return inputString;
        }
    }

}
