using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace WebApi.Html5.Upload.Repositorio
{
    public class Repositorio<T> where T : class
    {
        private readonly MongoDatabase db;
        private readonly MongoServer server;

        public Repositorio()
        {
            var conneccaoString = new MongoConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Banco"].ConnectionString);
            server = MongoServer.Create(conneccaoString);
            db = server.GetDatabase(conneccaoString.DatabaseName);
            Collection = db.GetCollection<T>(typeof(T).Name.ToLower());

            //corrige a hora no servidor do banco
            DateTimeSerializationOptions.Defaults = new DateTimeSerializationOptions(DateTimeKind.Local, BsonType.Document);

            var w7Convensoes = new ConventionProfile();

            //Ajuda no migration, se tiver campo a mais no banco ele ignora
            w7Convensoes.SetIgnoreExtraElementsConvention(new AlwaysIgnoreExtraElementsConvention());
            BsonClassMap.RegisterConventions(w7Convensoes, (type) => true);
        }

        public MongoCollection<T> Collection { get; set; }

        public string InserirArquivo(Stream arquivo, string Nome, string contentType)
        {
            var gfs = new MongoGridFS(db);
            var fileInfo = gfs.Upload(arquivo, Nome);
            gfs.SetContentType(fileInfo, contentType);
            return fileInfo.Id.AsObjectId.ToString();
        }

    }
}