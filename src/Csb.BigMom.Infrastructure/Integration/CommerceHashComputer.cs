using Csb.BigMom.Infrastructure.Entities;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Csb.BigMom.Infrastructure.Integration
{
    /// <summary>
    /// Provides an utility to compute <see cref="Commerce"/> hash fingerprint.
    /// </summary>
    public sealed class CommerceHashComputer : IDisposable
    {
        private readonly SHA1Managed _sha1 = new();

        /// <summary>
        /// Computes the hash fingerprint of the provided <see cref="Commerce"/>.
        /// </summary>
        /// <param name="commerce">The commerce.</param>
        /// <returns>The hash.</returns>
        public string ComputeHash(Commerce commerce)
        {
            if (commerce is null)
            {
                throw new ArgumentNullException(nameof(commerce));
            }

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            writer.Write(commerce.Identifiant);
            writer.Write(commerce.Mcc.Code);
            writer.Write(commerce.Nom);
            writer.Write(commerce.Email);
            writer.Write(commerce.Ef);
            writer.Write(commerce.DateAffiliation.Ticks);
            writer.Write(commerce.DateResiliation?.Ticks);
            foreach (var tpe in commerce.Tpes)
            {
                foreach (var contrat in tpe.Contrats)
                {
                    writer.Write(contrat.NoContrat);
                    writer.Write(contrat.Code);
                    writer.Write(contrat.DateDebut.Ticks);
                    writer.Write(contrat.DateFin?.Ticks);
                }
            }
            foreach (var tpe in commerce.Tpes)
            {
                writer.Write(tpe.NoSerie);
                writer.Write(tpe.NoSite);
                writer.Write(tpe.Modele);
                writer.Write(tpe.Statut);
            }
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            var hashBytes = _sha1.ComputeHash(ms);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        public void Dispose() => _sha1.Dispose();
    }
}
