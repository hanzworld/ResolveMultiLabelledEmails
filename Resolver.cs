using System;
using System.Linq;
using System.Text;

namespace GmailToIMAPMigration.ResolveMultiLabelledEmails
{
    public interface IResolver
    {
        void Resolve();
    }
    public class Resolver : IResolver
    {
        private readonly IFinder finder;
        private int autoResolved;
        private int manuallyResolved;
        private int unresolved;
        private int count = 0;

        public Resolver(IFinder finder)
        {
            this.finder = finder;
        }
        public void Resolve()
        {

            foreach (var result in finder.GetBatchOfThreads())
            {
                if (result.UniqueLabels.Count > 1)
                {
                    if (TryAutoResolve(result))
                    {
                        autoResolved++;
                        continue;

                    }

                    if (TryManualResolve(result))
                    {
                        manuallyResolved++;
                        continue;
                    }
                    unresolved++;
                }
                else
                {
                    count++;
                }
            }

            Console.WriteLine(this);
        }

        private bool TryManualResolve(FindResult result)
        {
            result.UniqueLabels.ForEach(Console.WriteLine);
            Console.WriteLine(result);
            return true;
        }
        private bool TryAutoResolve(FindResult result)
        {
            if (result.UniqueLabels.Count == 2 && result.UniqueLabels.Any(l => l.Id == "SENT"))
            {
                //we can logically assume we "move" anything from sent into the other "folder" instead so they live together
                return true;
            }
            return false;

        }

        public override string ToString()
        {
            var state = new StringBuilder();
            state.AppendLine($"Automatically resolved : {autoResolved}");
            state.AppendLine($"Manually resolved : {manuallyResolved}");
            state.AppendLine($"Unresolved : {unresolved}");
            state.AppendLine($"Ignored : {count}");

            return state.ToString();

        }
    }
}