using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WebApi.Html5.Upload.Infrastructure;
using WebApi.Html5.Upload.Models;

namespace WebApi.Html5.Upload.Controllers
{
    public class UploadMongoController : ApiController
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
                        //---------- SALVA NO MONGO
                        var repositorio = new Repositorio.Repositorio<String>();
                        var caminho = path + "/" + info.Name;
                        var fileee = new FileStream(caminho, FileMode.Open);

                        repositorio.InserirArquivo(fileee, info.Name, "image/jpeg");
                        //--------- FIM
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
