using MongoDB.Driver;
using POS_Service.Extra;
using POS_Service.Model;
using System;
using System.IO.Ports;

namespace POS_Service
{
    public class ServiceBusiness
    {
        private readonly IMongoDatabase _mongoDb;
        private string Table = Reader.TableReader()[0];
        private string Port = Reader.TableReader()[2];
        private string Rate = Reader.TableReader()[3];
        private string Parity2 = Reader.TableReader()[4];

        private IMongoCollection<ComportModel> _collection;
        public ServiceBusiness()
        {
            var connectionString = Reader.TableReader()[1];
            var client = new MongoClient(connectionString);
            _mongoDb = client.GetDatabase("windowblind");
            _collection = _mongoDb.GetCollection<ComportModel>("comport");
        }

        //start point
        public void SendToComPort()
        {
            var data = _collection.FindSync(e => e.status == "Read" && e.tablename == Table).ToList();

            foreach (var msg in data)
            {
                try
                {
                    SerialPort R = new SerialPort();
                    R.BaudRate = Convert.ToInt32(Rate);
                    R.Parity = Parity2 == "None" ? Parity.None : Parity2 == "Even" ? Parity.Even : Parity2 == "Odd" ? Parity.Odd : Parity.None;
                    R.DataBits = 8;
                    R.ReadTimeout = 3000;
                    R.WriteBufferSize = 3000;
                    R.StopBits = StopBits.One;
                    R.ReadBufferSize = 128;
                    R.WriteBufferSize = 128;
                    R.PortName = Port;
                    R.DtrEnable = false;
                    R.RtsEnable = false;
                    R.Open();



                    if (R.IsOpen)
                    {
                        R.Write(msg.value);
                    }


                    msg.status = "Sent";
                    var filter = Builders<ComportModel>.Filter.Eq(s => s.id, msg.id);

                    _collection.ReplaceOneAsync(filter, msg);
                    R.Close();

                }
                catch (Exception e)
                {

                }
            }
        }
    }
}
