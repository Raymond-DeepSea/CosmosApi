using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CosmosApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;

namespace CosmosApi.Controllers
{
    //[Route("[controller]")]
    [ApiController]
    public class DataQueryController : ControllerBase
    {
        private readonly IDataQueryServices _services;
        
        public DataQueryController(IDataQueryServices services)
        {
            _services = services;
        }
        
        [Route("")]
        public string Index()
        {
            return "Welcome to Web Crawler";
        }

        [HttpGet]
        [Route("[Action]")]
        public ActionResult<List<dynamic>> GetData()
        {
            var data = _services.QueryCosmosData();
            if (data?.Count == 0)
            {
                return NotFound();
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawDate">drawDate in format yyyyMMdd</param>
        /// <returns></returns>
        [HttpGet("[action]/{drawDate}")]
        //[Route("[Action]/{drawDate}")]
        public ActionResult<List<dynamic>> GetData(int drawDate)
        {
            //int drawDate = 20190207;
            var drawDateStr = drawDate.ToString();
            drawDateStr = $"{drawDateStr.Substring(0, 4)}-{drawDateStr.Substring(4, 2)}-{drawDateStr.Substring(6, 2)}";
            var data = _services.QueryCosmosData(drawDateStr);
            return data;
        }
    }
}