// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
// Import the library 
using AdaptiveCards.Templating;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";

            IActivity activity = turnContext.Activity;
            var reply = ((Microsoft.Bot.Schema.Activity)activity).CreateReply();
            reply.Type = "typing";

            await Task.Delay(5000);
            await turnContext.SendActivityAsync(reply, cancellationToken);


            var templateJson = @"
         {
    ""type"": ""AdaptiveCard"",
    ""version"": ""1.2"",
    ""body"": [
        {
            ""type"": ""TextBlock"",
            ""text"": ""Hello ${name}!""
        }
    ]
           }";
            // You can use any serializable object as your data
            var myData = new
            {
                Name = "Matt Hidinger"
            };
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(templateJson);
            string cardJson = template.Expand(myData);




            var templateJson1 = @"
{
    ""type"": ""AdaptiveCard"",
    ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
    ""version"": ""1.3"",
    ""body"": [
        {
            ""type"": ""Container"",
            ""items"": [
                {
                    ""type"": ""Input.ChoiceSet"",
                    ""choices"": [
                        {
                            ""$data"": ""${peers}"",
                            ""title"": ""${name}"",
                            ""value"": ""${title}""
                        }
                    ],
                    ""placeholder"": ""Placeholder text"",
                    ""style"": ""expanded"",
                    ""id"": ""1""
                }
            ]
        },
        {
            ""type"": ""ActionSet"",
            ""actions"": [
                {
                    ""type"": ""Action.Submit"",
                    ""title"": ""Action.Submit""
                }
            ]
        }
    ]
}
        ";



           

            

            string jsonData = @"

            {
                ""name"": ""Matt"",
    
    ""peers"": [
        {
                ""name"": ""Lei"",
            ""title"": ""Sr Program Manager""
        },
        {
                ""name"": ""Andrew"",
            ""title"": ""Program Manager II""
        },
        {
                ""name"": ""Mary Anne"",
            ""title"": ""Program Manager""
        }
    ]
}";

            AdaptiveCardTemplate template1 = new AdaptiveCardTemplate(templateJson1);
            string cardJson1 = template1.Expand(jsonData);







            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
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
    }
}
