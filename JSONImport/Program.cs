using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONImport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Program().GetAll();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public async void GetAll()
        {
            //呼叫遠端WebAPI，以取得JSON字串
            HttpClient client = new HttpClient();
            HttpResponseMessage response =
                await client.GetAsync("http://localhost:35705/api/MassData/Get");
            response.EnsureSuccessStatusCode();

            //把接到的HttpResponseMessage.Content，讀到字串中
            string responseBody = await response.Content.ReadAsStringAsync();

            //如果JSON字串裡面有包含反斜線，則恢復字串格式
            //ex: \"DataTable1\" → "DataTable1"
            responseBody = JToken.Parse(responseBody).ToString();
            
            //把JSON字串轉換回DataSet
            DataSet ds = JsonConvert.DeserializeObject<DataSet>(responseBody);

            Console.WriteLine(ds.Tables.Count);
        }
    }
}
