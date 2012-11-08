using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using WebApi.Html5.Upload.Models;
using WebApi.Html5.Upload.Infrastructure;

namespace WebApi.Html5.Upload.Controllers
{
    public class UploadController : ApiController
    {
        public Task<IEnumerable<FileDesc>> Post()
        {
            const string nomeDaPasta = "uploads";
            var path = HttpContext.Current.Server.MapPath("~/" + nomeDaPasta);
            var rootUrl = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.AbsolutePath, String.Empty);

            if (Request.Content.IsMimeMultipartContent())
            {
                var streamProvider = new CustomMultipartFormDataStreamProvider(path);
                var task = Request.Content.ReadAsMultipartAsync(streamProvider).ContinueWith(t =>
                {

                    if (t.IsFaulted || t.IsCanceled)
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);

                    var fileInfo = streamProvider.FileData.Select(i =>
                    {
                        var info = new FileInfo(i.LocalFileName);
                        return new FileDesc(info.Name, rootUrl + "/" + nomeDaPasta + "/" + info.Name, info.Length / 1024);
                    });
                    return fileInfo;
                });

                return task;
            }

            throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable, "A requisição não esta bem formatada"));
        }
    }
}
