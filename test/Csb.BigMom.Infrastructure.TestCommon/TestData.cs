using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Csb.BigMom.Infrastructure.TestCommon
{
    public class TestData
    {
        public List<Mcc> Mccs { get; }

        public List<Commerce> Commerces { get; }

        public List<Contrat> Contrats { get; }

        public List<Tpe> Tpes { get; }

        public List<Application> Applications { get; }

        public List<Tlc> Tlcs { get; }

        public TestData()
        {
            var serializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new TimeSpanJsonConverter()
                }
            };
            Mccs = JsonSerializer.Deserialize<List<Mcc>>(File.ReadAllText("Data/mccs.json"), serializerOptions);
            Commerces = JsonSerializer.Deserialize<List<Commerce>>(File.ReadAllText("Data/commerces.json"));
            Commerces.ForEach(c =>
            {
                c.Mcc = Mccs.Single(m => m.Code == c.Mcc.Code);
                c.Tpes = new List<Tpe>();
                c.Contrats = new List<Contrat>();
            });
            Contrats = JsonSerializer.Deserialize<List<Contrat>>(File.ReadAllText("Data/contrats.json"));
            Contrats.ForEach(c =>
            {
                c.Commerce = Commerces.Single(m => m.Identifiant == c.Commerce.Identifiant);
                c.Commerce.Contrats.Add(c);
                c.Applications = new List<Application>();
            });
            Tpes = JsonSerializer.Deserialize<List<Tpe>>(File.ReadAllText("Data/tpes.json"));
            Tpes.ForEach(t =>
            {
                t.Commerce = Commerces.Single(m => m.Identifiant == t.Commerce.Identifiant);
                t.Commerce.Tpes.Add(t);
                t.Applications = new List<Application>();
            });
            Applications = JsonSerializer.Deserialize<List<Application>>(File.ReadAllText("Data/applications.json"), serializerOptions);
            Applications.ForEach(a =>
            {
                a.Tpe = Tpes.Single(t => t.NoSerie == a.Tpe.NoSerie);
                a.Tpe.Applications.Add(a);
                a.Contrat = Contrats.Single(c => c.NoContrat == a.Contrat.NoContrat);
                a.Contrat.Applications.Add(a);
            });
            Tlcs = JsonSerializer.Deserialize<List<Tlc>>(File.ReadAllText("Data/tlcs.json"), serializerOptions);
            Tlcs.ForEach(t =>
            {
                t.App = Applications.Single(a => a.Tpe.NoSerie == t.App.Tpe.NoSerie && a.Contrat.NoContrat == t.App.Contrat.NoContrat);
                t.App.Tlcs.Add(t);
            });
            var hashComputer = new CommerceHashComputer();
            Commerces.ForEach(c => c.Hash = hashComputer.ComputeHash(c));
        }
    }
}
