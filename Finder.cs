using System;
using Microsoft.Extensions.Options;

namespace FindMultiLabelledEmails
{
    public interface IFinder
    {
        public void Find();
    }
    public class Finder : IFinder
    {
        private readonly GoogleSecrets secrets;

        public Finder()
        {
        }

        public Finder(IOptions<GoogleSecrets> secrets)
        {
            this.secrets = secrets.Value;
        }

        public void Find()
        {
            Console.WriteLine("Secrets");
            Console.WriteLine(secrets.ClientSecret);
        }


    }
}