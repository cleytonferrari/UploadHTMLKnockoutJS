namespace WebApi.Html5.Upload.Models
{
    public class FileDesc
    {
        public string Nome { get; set; }
        public string Path { get; set; }
        public long Tamanho { get; set; }

        public FileDesc(string n, string p, long s)
        {
            Nome = n;
            Path = p;
            Tamanho = s;
        }
    }
}