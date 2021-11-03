using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Microsoft.Extensions.Options;

namespace GmailToIMAPMigration.ResolveMultiLabelledEmails
{
    public interface IFinder
    {
        public void Find();
    }
    public class Finder : IFinder
    {
        private readonly GoogleSecrets secrets;
        private readonly GmailClient gmail;
        private readonly IDictionary<string, Label> labels;
        private string[] labelsYouCanIgnore = { "TRASH", "CATEGORY_FORUMS", "CATEGORY_UPDATES", "CATEGORY_PERSONAL", "CATEGORY_PROMOTIONS", "CATEGORY_SOCIAL", "STARRED", "UNREAD", "IMPORTANT" };

        public Finder(IOptions<GoogleSecrets> secrets, GmailClient gmail)
        {
            this.secrets = secrets.Value;
            this.gmail = gmail;

            labels = gmail.Users.Labels.List("me").Execute().Labels.ToDictionary(l => l.Id);
        }

        public void Find()
        {
            int count = 0;
            foreach (var result in GetBatchOfThreads())
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

        private IEnumerable<FindResult> GetBatchOfThreads()
        {
            var threadsRequest = gmail.Users.Threads.List("me");
            threadsRequest.MaxResults = 10;
            var threadsResponse = threadsRequest.Execute();

            foreach (var thread in threadsResponse.Threads)
            {
                var request = gmail.Users.Threads.Get("me", thread.Id);
                request.Format = UsersResource.ThreadsResource.GetRequest.FormatEnum.Metadata;
                thread.Messages = request.Execute().Messages;

                var threadLabels = thread.Messages.SelectMany(m => m.LabelIds).Distinct().ToList();


                var fr = new FindResult
                {
                    // There are some labels one can ignore
                    UniqueLabels = threadLabels.Except(labelsYouCanIgnore).ToList(),
                    Thread = thread
                };

                yield return fr;
            }
        }
    }

    public class FindResult
    {
        public List<string> UniqueLabels;
        public Thread Thread;

        public override string ToString()
        {
            return $"{Thread.Snippet}\n - {Thread.Messages.Count} messages\n - {UniqueLabels.Count} labels ({String.Join<string>(',', UniqueLabels)})";
        }

    }
}