using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace WebApi.Html5.Upload.tools
{
    public class FileUpload<T>
    {
        private readonly string _RawValue;

        public T Value { get; set; }
        public string FileName { get; set; }
        public string MediaType { get; set; }
        public byte[] Buffer { get; set; }

        public FileUpload(byte[] buffer, string mediaType, string fileName, string value)
        {
            Buffer = buffer;
            MediaType = mediaType;
            FileName = fileName.Replace("\"", "");
            _RawValue = value;

            Value = JsonConvert.DeserializeObject<T>(_RawValue);
        }

        public void Save(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var NewPath = Path.Combine(path, FileName);
            if (File.Exists(NewPath))
            {
                File.Delete(NewPath);
            }

            File.WriteAllBytes(NewPath, Buffer);

            var Property = Value.GetType().GetProperty("FileName");
            Property.SetValue(Value, FileName, null);
        }
    }

    public class FileMediaFormatter<T> : MediaTypeFormatter
    {

        public FileMediaFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
            
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(FileUpload<T>);
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public async override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {

            if (!content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var Parts = await content.ReadAsMultipartAsync();
            var FileContent = Parts.Contents.First(x =>
                SupportedMediaTypes.Contains(x.Headers.ContentType));

            var DataString = "";
            foreach (var Part in Parts.Contents.Where(x => x.Headers.ContentDisposition.DispositionType == "form-data"
                                                        && x.Headers.ContentDisposition.Name == "\"data\""))
            {
                var Data = await Part.ReadAsStringAsync();
                DataString = Data;
            }

            string FileName = FileContent.Headers.ContentDisposition.FileName;
            string MediaType = FileContent.Headers.ContentType.MediaType;

            using (var Imgstream = await FileContent.ReadAsStreamAsync())
            {
                byte[] Imagebuffer = ReadFully(Imgstream);
                return new FileUpload<T>(Imagebuffer, MediaType, FileName, DataString);
            }
        }

        private byte[] ReadFully(Stream input)
        {
            var Buffer = new byte[16 * 1024];
            using (var Ms = new MemoryStream())
            {
                int Read;
                while ((Read = input.Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    Ms.Write(Buffer, 0, Read);
                }
                return Ms.ToArray();
            }
        }


    }
}