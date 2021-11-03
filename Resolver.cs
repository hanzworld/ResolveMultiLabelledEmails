using System;

namespace GmailToIMAPMigration.ResolveMultiLabelledEmails
{
    public interface IResolver
    {
        void Resolve();
    }
    public class Resolver : IResolver
    {
        private readonly IFinder finder;

        public Resolver(IFinder finder)
        {
            this.finder = finder;
        }
        public void Resolve()
        {
            int count = 0;
            foreach (var result in finder.GetBatchOfThreads())
            {
                if (result.UniqueLabels.Count > 1)
                {
                    Console.WriteLine(result);
                }
                else
                {
                    count++;
                }
            }

            Console.WriteLine($"{count} threads ignored");
        }
    }
}