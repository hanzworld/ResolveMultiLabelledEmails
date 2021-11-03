using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Gmail.v1.Data;

namespace GmailToIMAPMigration.ResolveMultiLabelledEmails
{
    public interface IResolver
    {
        void Resolve();
    }
    public class Resolver : IResolver
    {
        private readonly IFinder finder;
        private readonly GmailClient gmail;
        private int autoResolved;
        private int manuallyResolved;
        private int unresolved;
        private int count = 0;

        public Resolver(IFinder finder, GmailClient gmail)
        {
            this.finder = finder;
            this.gmail = gmail;
        }
        public void Resolve()
        {
            var batch = finder.GetBatchOfThreads();
            do
            {
                ProcessBatch(batch);
                batch = finder.GetBatchOfThreads();
            } while (batch.Any() && manuallyResolved < 2);

            Console.WriteLine(this);
        }

        private void ProcessBatch(IEnumerable<FindResult> batch)
        {
            foreach (var result in batch)
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
                RemoveLabel(result, "SENT");
                return true;
            }
            return false;

        }

        private void RemoveLabel(FindResult result, string id)
        {

            var keep = result.UniqueLabels.Select(k => k.Id).Where(l => l != id).ToArray();
            var modifyRequest = new ModifyThreadRequest()
            {
                AddLabelIds = keep,
                RemoveLabelIds = new[] { id }
            };

            var output = new StringBuilder("AUTORESOLVE SUMMARY");
            output.AppendLine();
            output.AppendLine(result.Thread.Snippet);

            output.AppendLine($"Adding: {String.Join(',', modifyRequest.AddLabelIds)}");
            output.AppendLine($"Removing: {String.Join(',', modifyRequest.RemoveLabelIds)}");
            Console.WriteLine(output.ToString());
        }
        //gmail.Users.Threads.Modify(null, "me", result.Thread.Id)


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