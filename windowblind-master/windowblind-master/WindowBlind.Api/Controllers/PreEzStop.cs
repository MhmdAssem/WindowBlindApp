using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowBlind.Api.Models;

using System.Xml;

namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PreEzStop : ControllerBase
    {
        public PreEzStop(IRepository repository)
        {
            _repository = repository;
        }
        private readonly IRepository _repository;

        [HttpGet("GenerateXMLFile")]
        public async Task<ResultModel> GenerateXMLFile([FromHeader] string LineNumber)
        {
            try
            {


                XmlDocument doc = new XmlDocument();

                //xml declaration is recommended, but not mandatory
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

                //create the root element
                XmlElement root = doc.DocumentElement;
                doc.InsertBefore(xmlDeclaration, root);

                //string.Empty makes cleaner code
                XmlElement element1 = doc.CreateElement(string.Empty, "OrderObject", string.Empty);
                doc.AppendChild(element1);

                XmlElement element2 = doc.CreateElement(string.Empty, "OrderNumber", string.Empty);



                XmlText text1 = doc.CreateTextNode(LineNumber);

                element1.AppendChild(element2);
                element2.AppendChild(text1);

                var XMLDirectory = await _repository.Settings.FindAsync(set => set.settingName == "XML Folder").Result.FirstOrDefaultAsync();

                if (!Directory.Exists(XMLDirectory.settingPath))
                    return new ResultModel { Message = "File Doesnt Exists",Data = null, Status = System.Net.HttpStatusCode.BadRequest,StackTrace = "" };

                doc.Save(Path.Combine(XMLDirectory.settingPath, RandomNameGenerator(LineNumber)));

                return Repository.ReturnSuccessfulRequest("");
            }
            catch (Exception e)
            {
                return Repository.ReturnBadRequest(e);
            }
        }


        private string RandomNameGenerator(string start)
        {
            string chars = "2346789ABCDEFGH____JKL__MNPQRTUVWXYZabcdefghjkmnpqrtuvwxyz";
            Random rnd = new Random();
            string name;

            
            name = start + "_";
            while (name.Length < 30)
            {
                name += chars.Substring(rnd.Next(chars.Length), 1);
            }
             
            name += ".xml";
            return name;
        }

    }
}
