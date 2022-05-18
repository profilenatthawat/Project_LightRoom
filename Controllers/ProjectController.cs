using Microsoft.AspNetCore.Mvc;
using myproject.Models;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Client.Options;
using System.Net;
using System.Text;

namespace myproject
{
    public class ProjectController : Controller
    {
        private readonly myprojectDbContext _db;
        public ProjectController(myprojectDbContext db)
        {
            _db = db;
        }

         public IActionResult Index()
        {
            dynamic model = new ExpandoObject();

            var rawsql_light = "SELECT * FROM Data WHERE is_delete = false order by create_date ASC";
            var light = _db.data.FromSqlRaw(rawsql_light).ToList();
            model.light = light;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Addlight(Data data)
        {
            data.light = "1";
            data.status = "off";
            data.create_date = DateTime.Now;
            data.update_date = DateTime.Now;
            data.is_delete = false;
             _db.data.Add(data);
            _db.SaveChanges();
            return Redirect("~/Project/Index/" + data.id);
        }

        private void LineNotify(string lineToken, string message)
        {
            try
            {
                message = System.Web.HttpUtility.UrlEncode(message, Encoding.UTF8);
                var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", message);
                var data = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + lineToken);
                var stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [HttpPost]
        public IActionResult CallInLineNotify(int number, string status,int id,string time)
        {
            var data = _db.data.FirstOrDefault(d => d.id == id);
            if(status == "on"){
                status = "off";
                data.status = "off";
            }else if (status == "off"){
               status = "on";
               data.status = "on";
            }
            data.update_date = DateTime.Now;
            time = data.formatdatetime(data.update_date) ;
            data.is_delete = false;
            _db.data.Update(data);
            _db.SaveChangesAsync();
            
            string token = "RdWlBLG10QraE2NOqnZKLg0W3QEiFvPaohsGq8oVd22";
            string message = "หลอดไฟที่"+ number + " " + "สถานะ : " + status + "\n " + "เวลา : " + time + " น." ;
            LineNotify(token, message);
            Main(number+","+status+","+time);
            
            return Redirect("~/Project/Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _db.data.FirstOrDefaultAsync(d => d.id == id);
            if (data == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            data.is_delete = true;
            _db.data.Attach(data).Property(x => x.is_delete).IsModified = true;
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }

        public static async Task Main(string data)
        {
            String Data = data ;

            var mqttFactory = new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                            .WithClientId(Guid.NewGuid().ToString())
                            .WithTcpServer("broker.emqx.io",1883)
                            .WithCleanSession()
                            .Build();

            client.UseConnectedHandler( e => 
            {
                Console.WriteLine("Connected to broker successfully");
            });

            client.UseDisconnectedHandler( e=> 
            {
                Console.WriteLine("Disconnect from broker successfully");
            });
            await client.ConnectAsync(options);
            await PublishMessageAsync(client, Data) ;
            await client.DisconnectAsync();
        }

         private static async Task PublishMessageAsync(IMqttClient client,string message_from_web)
        {
            string messagepayload = message_from_web ;
            var message = new MqttApplicationMessageBuilder()
                            .WithTopic("light_room")
                            .WithPayload(messagepayload)
                            .WithAtLeastOnceQoS()
                            .Build();
            
            if(client.IsConnected)
            {
                await client.PublishAsync(message);
            }
        }
    }
}