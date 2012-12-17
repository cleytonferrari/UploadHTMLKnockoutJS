using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApi.Html5.Upload.Models;

namespace WebApi.Html5.Upload.Controllers
{
    public class UploadMongoController : ApiController
    {
        public async Task<IEnumerable<FileDesc>> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                var sb = new StringBuilder();

                await Request.Content.ReadAsMultipartAsync(provider);

                //usado para pegar os outros dados do formulario, alem do arquivo.
                foreach (var key in provider.FormData.AllKeys)
                {
                    foreach (var val in provider.FormData.GetValues(key))
                        sb.Append(string.Format("{0}: {1}\n", key, val));
                }

                var retorno = new List<FileDesc>();
                //pega os arquivos
                foreach (var file in provider.FileData)
                {
                    var fileInfo = new FileInfo(file.LocalFileName);
                    sb.Append(string.Format("Uploaded file: {0} ({1} bytes)\n", fileInfo.Name, fileInfo.Length));

                    var repositorio = new Repositorio.Repositorio<String>();
                    var caminho = root + "/" + fileInfo.Name;

                    var streamDoArquivo = new FileStream(caminho, FileMode.Open);
                    var nomeDoArquivo = file.Headers.ContentDisposition.FileName; //fileInfo.Name;
                    var tipoDoArquivo = file.Headers.ContentType.ToString();
                    var id = repositorio.InserirArquivo(streamDoArquivo, nomeDoArquivo, tipoDoArquivo);

                    retorno.Add(new FileDesc(nomeDoArquivo, id, fileInfo.Length / 1024));
                }
                return retorno;

            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, e));
            }
        }
    }
}
