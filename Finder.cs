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
        IEnumerable<FindResult> GetBatchOfThreads();
    }
    public class Finder : IFinder
    {
        private readonly GoogleSecrets secrets;
        private readonly GmailClient gmail;
        private readonly IDictionary<string, Label> labels;
        private string[] labelsYouCanIgnore = { "TRASH", "CATEGORY_FORUMS", "CATEGORY_UPDATES", "CATEGORY_PERSONAL", "CATEGORY_PROMOTIONS", "CATEGORY_SOCIAL", "STARRED", "UNREAD", "IMPORTANT" };
        private string nextPageToken;

        public Finder(IOptions<GoogleSecrets> secrets, GmailClient gmail)
        {
            this.secrets = secrets.Value;
            this.gmail = gmail;

            labels = gmail.Users.Labels.List("me").Execute().Labels.ToDictionary(l => l.Id);
        }


        public IEnumerable<FindResult> GetBatchOfThreads()
        {
            var threadsRequest = gmail.Users.Threads.List("me");
            threadsRequest.MaxResults = 50;
            if (nextPageToken != null)
            {
                threadsRequest.PageToken = nextPageToken;
            }

            var threadsResponse = threadsRequest.Execute();
            nextPageToken = threadsResponse.NextPageToken;

            foreach (var thread in threadsResponse.Threads)
            {
                var request = gmail.Users.Threads.Get("me", thread.Id);
                request.Format = UsersResource.ThreadsResource.GetRequest.FormatEnum.Metadata;
                thread.Messages = request.Execute().Messages;

                var threadLabels = thread.Messages.SelectMany(m => m.LabelIds).Distinct().ToList();
                var uniqueLabels = threadLabels.Except(labelsYouCanIgnore).ToList();


                var fr = new FindResult
                {
                    // There are some labels one can ignore
                    UniqueLabels = uniqueLabels.Select(ul => labels[ul]).ToList(),
                    Thread = thread
                };

                yield return fr;
            }
        }
    }

    public class FindResult
    {
        public List<Label> UniqueLabels;
        public Thread Thread;

        public override string ToString()
        {
            return $"{Thread.Snippet}\n - {Thread.Messages.Count} messages\n - {UniqueLabels.Count} labels ({String.Join<string>(',', UniqueLabels.Select(l => l.Name))})";
        }

    }
}