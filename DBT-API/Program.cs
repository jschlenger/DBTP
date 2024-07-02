using DBT_API.Repositories;
using Microsoft.OpenApi.Models;
using Minio;
using DBT_API.Settings;
using Minio.AspNetCore;
using VDS.RDF;
using VDS.RDF.Storage;
using DBT_API.Entities;
using DBT_API.Util;
using DBT_API.Enums;
using InfluxDB.Client;

namespace DBT_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<ISetupRepository, MinioSetupRepository>();
            builder.Services.AddSingleton<IValidationRepository, InMemValidationRepository>();
            builder.Services.AddSingleton<ITimeseriesRepository, InfluxTimeseriesRepository>();

            // setup for MinIO blob repo
            builder.Services.AddSingleton(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return configuration.GetSection(nameof(MinioDbSettings)).Get<MinioDbSettings>();
            });

            builder.Services.AddMinio(options =>
            {
                var minioDbSettings = builder.Services.BuildServiceProvider().GetService<MinioDbSettings>();
                options.Endpoint = minioDbSettings.Host + ":" + minioDbSettings.Port;
                options.AccessKey = minioDbSettings.User;
                options.SecretKey = minioDbSettings.Password;
                options.ConfigureClient(client =>
                {
                    client.WithSSL(false);
                });
            });
            builder.Services.AddSingleton<IBlobRepository, MinioBlobRepository>();

            // setup for InfluxDB time series repo
            builder.Services.AddSingleton(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return configuration.GetSection(nameof(InfluxDbSettings)).Get<InfluxDbSettings>();
            });

            builder.Services.AddSingleton<InfluxDBClient>(provider =>
            {
                var influxDbSettings = builder.Services.BuildServiceProvider().GetService<InfluxDbSettings>();
                return new InfluxDBClient(influxDbSettings.Host, token: influxDbSettings.Token);
            });
            builder.Services.AddSingleton<ITimeseriesRepository, InfluxTimeseriesRepository>();

            // setup of GraphDB graph repo
            builder.Services.AddSingleton(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return configuration.GetSection(nameof(GraphDbSettings)).Get<GraphDbSettings>();
            });
            builder.Services.AddSingleton<SparqlUpdater>(provider =>
            {
                var graphDbSettings = provider.GetRequiredService<GraphDbSettings>();
                return new SparqlUpdater(graphDbSettings.BaseRepo);
            });
            builder.Services.AddSingleton<IGraphRepository, GraphDBRepository>();

            builder.Services.AddSingleton<GraphNetwork>(provider =>
            {
                var graphDbSettings = provider.GetRequiredService<GraphDbSettings>();

                // get data graph
                SparqlConnector dataReadConnector = provider.GetRequiredService<SparqlUpdater>().ReturnReadConnector(KID.Data);
                Graph dataGraph = new();
                dataGraph = (Graph)dataReadConnector.Query("CONSTRUCT { ?a ?b ?c } WHERE { ?a ?b ?c } LIMIT 100000");

                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#Property");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2002/07/owl#TransitiveProperty");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2002/07/owl#SymmetricProperty");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#List");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#Class");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#Datatype");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2002/07/owl#equivalentClass");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2002/07/owl#equivalentProperty");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2002/07/owl#inverseOf");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2002/07/owl#differentFrom");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#Container");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#seeAlso");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#ContainerMembershipProperty");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#subPropertyOf");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#subClassOf");
                GraphHelper.RemoveTriples(dataGraph, "http://www.w3.org/2000/01/rdf-schema#Literal");
                GraphHelper.RemoveSubjectObjectDoubles(dataGraph);

                List<Node> dataNodes = new();
                GraphHelper.Convert2Csharp(dataGraph, dataNodes);

                // get information graph
                SparqlConnector infoReadConnector = provider.GetRequiredService<SparqlUpdater>().ReturnReadConnector(KID.Information);
                Graph infoGraph = new();
                infoGraph = (Graph)infoReadConnector.Query("CONSTRUCT { ?a ?b ?c } WHERE { ?a ?b ?c } LIMIT 100000");

                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#Property");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2002/07/owl#TransitiveProperty");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2002/07/owl#SymmetricProperty");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#List");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#Class");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#Datatype");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2002/07/owl#equivalentClass");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2002/07/owl#equivalentProperty");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2002/07/owl#inverseOf");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2002/07/owl#differentFrom");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#Container");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#seeAlso");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#ContainerMembershipProperty");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#subPropertyOf");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#subClassOf");
                GraphHelper.RemoveTriples(infoGraph, "http://www.w3.org/2000/01/rdf-schema#Literal");
                GraphHelper.RemoveSubjectObjectDoubles(infoGraph);

                List<Node> infoNodes = new();
                GraphHelper.Convert2Csharp(infoGraph, infoNodes);

                // get knowlege graph
                SparqlConnector knowReadConnector = provider.GetRequiredService<SparqlUpdater>().ReturnReadConnector(KID.Knowledge);
                Graph knowGraph = new();
                knowGraph = (Graph)knowReadConnector.Query("CONSTRUCT { ?a ?b ?c } WHERE { ?a ?b ?c } LIMIT 100000");

                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#Property");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2002/07/owl#TransitiveProperty");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2002/07/owl#SymmetricProperty");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#List");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#Class");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#Datatype");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2002/07/owl#equivalentClass");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2002/07/owl#equivalentProperty");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2002/07/owl#inverseOf");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2002/07/owl#differentFrom");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#Container");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#seeAlso");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#ContainerMembershipProperty");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#subPropertyOf");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#subClassOf");
                GraphHelper.RemoveTriples(knowGraph, "http://www.w3.org/2000/01/rdf-schema#Literal");
                GraphHelper.RemoveSubjectObjectDoubles(knowGraph);

                List<Node> knowNodes = new();
                GraphHelper.Convert2Csharp(knowGraph, knowNodes);

                return new GraphNetwork(dataNodes, infoNodes, knowNodes);
            });

            // Register custom converter
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new NodeConverter());
            });

            builder.Services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DBT-Platform", Version = "v1" });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
