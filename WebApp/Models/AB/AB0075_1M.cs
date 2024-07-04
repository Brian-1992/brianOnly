using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AB0075_1M : JCLib.Mvc.BaseModel
    {
        public string WORKDATE { get; set; } //日期
        public int SUMORDERNO { get; set; } //UD總筆數
        public int SUMBEDNO { get; set; } //UD總床數
        public int A0000 { get; set; } //00:01-08:00醫令筆數
        public int B0000 { get; set; } //00:01-08:00檢驗筆數
        public int A0801 { get; set; } //08:01-09:00醫令筆數
        public int B0801 { get; set; } //08:01-09:00檢驗筆數
        public int A0901 { get; set; } //09:01-10:00醫令筆數
        public int B0901 { get; set; } //09:01-10:00檢驗筆數
        public int A1001 { get; set; } //10:01-11:00醫令筆數
        public int B1001 { get; set; } //10:01-11:00檢驗筆數
        public int A1101 { get; set; } //11:01-12:00醫令筆數
        public int B1101 { get; set; } //11:01-12:00檢驗筆數
        public int A1201 { get; set; } //12:01-13:00醫令筆數
        public int B1201 { get; set; } //12:01-13:00檢驗筆數
        public int A1301 { get; set; } //13:01-14:00醫令筆數
        public int B1301 { get; set; } //13:01-14:00檢驗筆數
        public int A1401 { get; set; } //14:01-15:00醫令筆數
        public int B1401 { get; set; } //14:01-15:00檢驗筆數
        public int A1501 { get; set; } //15:01-16:00醫令筆數
        public int B1501 { get; set; } //15:01-16:00檢驗筆數
        public int A1601 { get; set; } //16:01-17:00醫令筆數
        public int B1601 { get; set; } //16:01-17:00檢驗筆數
        public int A1701 { get; set; } //17:01-18:00醫令筆數
        public int B1701 { get; set; } //17:01-18:00檢驗筆數
        public int A1801 { get; set; } //18:01-19:00醫令筆數
        public int B1801 { get; set; } //18:01-19:00檢驗筆數
        public int A1901 { get; set; } //19:01-20:00醫令筆數
        public int B1901 { get; set; } //19:01-20:00檢驗筆數
        public int A2001 { get; set; } //20:01-21:00醫令筆數
        public int B2001 { get; set; } //20:01-21:00檢驗筆數
        public int A2101 { get; set; } //21:01-22:00醫令筆數
        public int B2101 { get; set; } //21:01-22:00檢驗筆數
        public int A2201 { get; set; } //22:01-23:00醫令筆數
        public int B2201 { get; set; } //22:01-23:00檢驗筆數
        public int A2301 { get; set; } //23:01-00:00醫令筆數
        public int B2301 { get; set; } //23:01-00:00檢驗筆數
        public int A_TOTAL { get; set; } //TOTAL醫令筆數
        public int B_TOTAL { get; set; } //TOTAL檢驗筆數
    }
}