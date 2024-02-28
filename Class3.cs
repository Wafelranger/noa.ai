public class ChatBot
{
    private readonly SentimentAnalysisModel sentimentAnalysisModel;

    public ChatBot(string modelPath)
    {
        sentimentAnalysisModel = new SentimentAnalysisModel(modelPath);
    }

    public string GetResponse(string userMessage)
    {
        // Use the sentiment analysis model to predict sentiment
        string sentiment = sentimentAnalysisModel.PredictSentiment(userMessage);

        // Generate a response based on sentiment
        if (sentiment == "Positive")
        {
            return "That sounds great! How can I assist you?";
        }
        else
        {
            return "I'm sorry to hear that. How can I help?";
        }
    }
}
