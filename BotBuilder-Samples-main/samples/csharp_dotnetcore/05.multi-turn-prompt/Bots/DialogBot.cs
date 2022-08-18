// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler where T : Dialog 
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        protected readonly int ExpireAfterSeconds;
        protected readonly IStatePropertyAccessor<DateTime> LastAccessedTimeProperty;
        protected readonly IStatePropertyAccessor<DialogState> DialogStateProperty;
        public DialogBot(IConfiguration configuration, ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
            ExpireAfterSeconds = configuration.GetValue<int>("ExpireAfterSeconds");
            DialogStateProperty = ConversationState.CreateProperty<DialogState>(nameof(DialogState));
            LastAccessedTimeProperty = ConversationState.CreateProperty<DateTime>(nameof(LastAccessedTimeProperty));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var lastAccess = await LastAccessedTimeProperty.GetAsync(turnContext, () => DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
            if ((DateTime.UtcNow - lastAccess) >= TimeSpan.FromSeconds(ExpireAfterSeconds))
            {
                // Notify the user that the conversation is being restarted.
                await turnContext.SendActivityAsync("Welcome back!  Let's start over from the beginning.").ConfigureAwait(false);

                // Clear state.
                await ConversationState.ClearStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }

            await base.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);

            // Set LastAccessedTime to the current time.
            await LastAccessedTimeProperty.SetAsync(turnContext, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);

        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
