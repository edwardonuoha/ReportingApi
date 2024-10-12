using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telerik.Reporting;
using Newtonsoft.Json.Serialization;
namespace ReportingApi
{

        public static class ReportingFunc
        {
            [FunctionName("ReportingFunc")]
            public static async Task<FileStreamResult> Run(
                [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
                ILogger log)
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                string name = req.Query["name"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                name = name ?? data?.name;

                string responseMessage = string.IsNullOrEmpty(name)
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, {name}. This HTTP triggered function executed successfully.";


                // var uriReportSource = new Telerik.Reporting.UriReportSource();
                var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
                //  uriReportSource.Uri = "Report//Report2.trdp";
                var deviceInfo = new System.Collections.Hashtable();
                var reportPackager = new ReportPackager();
                Report report;
                InstanceReportSource instanceReportSource = new InstanceReportSource();

            bool isDev = Convert.ToBoolean(Environment.GetEnvironmentVariable("IsDevelopment"));
            string path = string.Empty;
            if (isDev)
            {
                path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\..\\Report\\Report2.trdp";
            }
            else
            {
                //linux hosted path
                log.LogInformation(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "..//Report//Report2.trdp";
                log.LogInformation(path);

                path = "/home/site/wwwroot/Report/Report2.trdp";
            }
 
                using (var sourceStream = System.IO.File.OpenRead(path))
                {
                    report = (Report)reportPackager.UnpackageDocument(sourceStream);
                }
                // mock data
                var dataModel = new MockData
                {
                    Name = "Terence",
                    Amount = 1000,
                    Password = "NitishTest"
                };
                var ds = new JsonDataSource
                {
                    DataSelector = "$",
                    Source = JsonConvert.SerializeObject(dataModel,
                   new JsonSerializerSettings
                   {
                       ContractResolver = new CamelCasePropertyNamesContractResolver()
                   })
                };

                report.DataSource = ds;

                instanceReportSource.ReportDocument = report;

                Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", instanceReportSource, deviceInfo);
                var output = result.DocumentBytes;

               
                //File.WriteAllBytes(pdfPath, output);
                //object p = File(output, "application/pdf");
                MemoryStream ms = new MemoryStream(output);
                return new FileStreamResult(ms, "application/pdf");
            }
        }
 }

