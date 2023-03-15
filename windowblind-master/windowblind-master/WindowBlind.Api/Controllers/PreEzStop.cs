﻿using Microsoft.AspNetCore.Mvc;
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
            Repository = repository;
        }
        private IRepository Repository;

        [HttpGet("GenerateXMLFile")]
        public async Task<IActionResult> GenerateXMLFile([FromHeader] string LineNumber)
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

                var XMLDirectory = await Repository.Settings.FindAsync(set => set.settingName == "XML Folder").Result.FirstOrDefaultAsync();

                if (!Directory.Exists(XMLDirectory.settingPath))
                    return BadRequest("File Doesnt Exists");

                doc.Save(Path.Combine(XMLDirectory.settingPath, RandomNameGenerator(LineNumber)));

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
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
