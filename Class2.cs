using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;

public class SentimentData
{
    [LoadColumn(0)]
    public string SentimentText;

    [LoadColumn(1), ColumnName("Label")]
    public bool Sentiment;
}

public class SentimentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction;
}

public class SentimentAnalysisModel
{
    private readonly PredictionEngine<SentimentData, SentimentPrediction> predictionEngine;

    public SentimentAnalysisModel(string modelPath)
    {
        var context = new MLContext();
        var model = context.Model.Load(modelPath, out var modelSchema);
        predictionEngine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    }

    public string PredictSentiment(string text)
    {
        var input = new SentimentData { SentimentText = text };
        var prediction = predictionEngine.Predict(input);
        return prediction.Prediction ? "Positive" : "Negative";
    }
}
public class SentimentAnalysisModel
{
    private readonly PredictionEngine<SentimentData, SentimentPrediction> predictionEngine;

    public SentimentAnalysisModel(string modelPath)
    {
        var context = new MLContext();

        var model = context.Model.Load(modelPath, out var modelSchema);

        predictionEngine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    }

    public string PredictSentiment(string text)
    {
        var input = new SentimentData { SentimentText = text };

        var prediction = predictionEngine.Predict(input);

        return prediction.Prediction == "1" ? "Positive" : "Negative";
    }

    private class SentimentData
    {
        public string SentimentText { get; set; }
    }

    private class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }
    }
}
