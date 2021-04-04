// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.12.2

using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmotionallyIntelligentBot.Bots
{
    public class OpinionMiningBot : ActivityHandler
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("ADD_YOUR_TEXT_ANALYTICS_KEY_HERE");
        private static readonly Uri endpoint = new Uri("ADD_YOUR_TEXT_ANALYTICS_ENDPOINT_HERE");
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);

            //Uncomment to run Classic Sentiment Analysis
            //DocumentSentiment documentSentiment = ClassicSentimentAnalysis(client, turnContext.Activity.Text);
            //foreach (var sentence in documentSentiment.Sentences)
            //{
            //    var replyText = $"Sentiment: \"{sentence.Sentiment}\"";
            //    replyText += $"\nPositive score: \"{sentence.ConfidenceScores.Positive:0.00}\"";
            //    replyText += $"\nNegative score: \"{sentence.ConfidenceScores.Negative:0.00}\"";
            //    replyText += $"\nNeutral score: \"{sentence.ConfidenceScores.Neutral:0.00}\"";
            //    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            //}

            //SentimentAnalysisWithOpinionMining
            AnalyzeSentimentResultCollection reviews = SentimentAnalysisWithOpinionMining(client, new List<string> { turnContext.Activity.Text });

            foreach (AnalyzeSentimentResult review in reviews)
            {
                foreach (SentenceSentiment sentence in review.DocumentSentiment.Sentences)
                {
                    var sentenceText = $"Sentence Sentiment: \"{sentence.Sentiment}\"";
                    sentenceText += $"\n\nPositive score: \"{sentence.ConfidenceScores.Positive:0.00}\"";
                    sentenceText += $"\n\nNegative score: \"{sentence.ConfidenceScores.Negative:0.00}\"";
                    sentenceText += $"\n\nNeutral score: \"{sentence.ConfidenceScores.Neutral:0.00}\"";
                    await turnContext.SendActivityAsync(MessageFactory.Text(sentenceText, sentenceText), cancellationToken);

                    foreach (SentenceOpinion sentenceOpinion in sentence.Opinions)
                    {
                        var sentenceOpinionText = $"\n\nTarget: {sentenceOpinion.Target.Text}";
                        foreach (AssessmentSentiment assessment in sentenceOpinion.Assessments)
                        {
                            sentenceOpinionText += $"\n\nRelated Assessment: {assessment.Text}, Value: {assessment.Sentiment}";
                            sentenceOpinionText += $"\n\nPositive score: {assessment.ConfidenceScores.Positive:0.00}";
                            sentenceOpinionText += $"\n\nNegative score: {assessment.ConfidenceScores.Negative:0.00}";
                            await turnContext.SendActivityAsync(MessageFactory.Text(sentenceOpinionText, sentenceOpinionText), cancellationToken);

                        }
                    }
                }
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
        static DocumentSentiment ClassicSentimentAnalysis(TextAnalyticsClient client, string inputText)
        {
            DocumentSentiment documentSentiment = client.AnalyzeSentiment(inputText);
            return documentSentiment;
        }

        static AnalyzeSentimentResultCollection SentimentAnalysisWithOpinionMining(TextAnalyticsClient client, List<string> documents)
        {
            AnalyzeSentimentResultCollection reviews = client.AnalyzeSentimentBatch(documents, options: new AnalyzeSentimentOptions()
            {
                IncludeOpinionMining = true
            });
            return reviews;
        }

    }
}
