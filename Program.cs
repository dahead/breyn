using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.TrainCatalogBase;

namespace breyn
{
    class Program
    {
        // <SnippetDeclareGlobalVariables>
        private static string _appPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string _trainDataPath => Path.Combine(_appPath, "..", "..", "..", "Data", "items_train.tsv");
        private static string _testDataPath => Path.Combine(_appPath, "..", "..", "..", "Data", "items_test.tsv");
        private static string _modelPath => Path.Combine(_appPath, "..", "..", "..", "Models", "model.zip");

        private static MLContext _mlContext;
        private static PredictionEngine<Item, ItemPrediction> _predEngine;
        private static ITransformer _trainedModel;
        static IDataView _trainingDataView;
        // </SnippetDeclareGlobalVariables>
        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            // <SnippetCreateMLContext>
            _mlContext = new MLContext(seed: 0);
            // </SnippetCreateMLContext>

            // STEP 1: Common data loading configuration
            // CreateTextReader<GitHubIssue>(hasHeader: true) - Creates a TextLoader by inferencing the dataset schema from the Item data model type.
            // .Read(_trainDataPath) - Loads the training text file into an IDataView (_trainingDataView) and maps from input columns to IDataView columns.
            Console.WriteLine($"=============== Loading Dataset  ===============");
            _trainingDataView = _mlContext.Data.LoadFromTextFile<Item>(_trainDataPath, hasHeader: true);

            Console.WriteLine($"=============== Finished Loading Dataset  ===============");
            var pipeline = ProcessData();
            var trainingPipeline = BuildAndTrainModel(_trainingDataView, pipeline);

            Evaluate(_trainingDataView.Schema);
            PredictTag();
        }

        public static IEstimator<ITransformer> ProcessData()
        {
            Console.WriteLine($"=============== Processing Data ===============");
            // STEP 2: Common data process configuration with pipeline data transformations
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Tag", outputColumnName: "Label")
                            .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Value", outputColumnName: "ValueFeaturized"))
                            // .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Description", outputColumnName: "DescriptionFeaturized"))
                            // .Append(_mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"))
                            .AppendCacheCheckpoint(_mlContext);

            Console.WriteLine($"=============== Finished Processing Data ===============");

            return pipeline;
        }

        public static IEstimator<ITransformer> BuildAndTrainModel(IDataView trainingDataView, IEstimator<ITransformer> pipeline)
        {
            // STEP 3: Create the training algorithm/trainer
            // Use the multi-class SDCA algorithm to predict the label using features.
            //Set the trainer/algorithm and map label to value (original readable state)

            var trainingPipeline = pipeline.Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine($"=============== Training the model  ===============");
            _trainedModel = trainingPipeline.Fit(trainingDataView);
            Console.WriteLine($"=============== Finished Training the model Ending time: {DateTime.Now.ToString()} ===============");

            // (OPTIONAL) Try/test a single prediction with the "just-trained model" (Before saving the model)
            Console.WriteLine($"=============== Single Prediction just-trained-model ===============");

            // Create prediction engine related to the loaded trained model
            _predEngine = _mlContext.Model.CreatePredictionEngine<Item, ItemPrediction>(_trainedModel);
            Item itm = new Item() { Value = "WebSockets communication is slow in my machine" };

            var prediction = _predEngine.Predict(itm);
            Console.WriteLine($"=============== Single Prediction just-trained-model - Result: {prediction.Tag} ===============");
            return trainingPipeline;
        }

        public static void Evaluate(DataViewSchema trainingDataViewSchema)
        {
            // STEP 5:  Evaluate the model in order to get the model's accuracy metrics
            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Starting time: {DateTime.Now.ToString()} ===============");

            //Load the test dataset into the IDataView
            var testDataView = _mlContext.Data.LoadFromTextFile<Item>(_testDataPath, hasHeader: true);

            //Evaluate the model on a test dataset and calculate metrics of the model on the test data.
            var testMetrics = _mlContext.MulticlassClassification.Evaluate(_trainedModel.Transform(testDataView));

            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Ending time: {DateTime.Now.ToString()} ===============");
            // <SnippetDisplayMetrics>
            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:0.###}");
            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:0.###}");
            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:#.###}");
            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:#.###}");
            Console.WriteLine($"*************************************************************************************************************");

            // Save the new model to .ZIP file
            SaveModelAsFile(_mlContext, trainingDataViewSchema, _trainedModel);
        }

        public static void PredictTag()
        {
            ITransformer loadedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
            Item singleItm = new Item() { Value = "Entity Framework crashes" };
            _predEngine = _mlContext.Model.CreatePredictionEngine<Item, ItemPrediction>(loadedModel);
            var prediction = _predEngine.Predict(singleItm);
            Console.WriteLine($"=============== Single Prediction - Result: {prediction.Tag} ===============");
        }

        private static void SaveModelAsFile(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            mlContext.Model.Save(model, trainingDataViewSchema, _modelPath);
            Console.WriteLine("The model is saved to {0}", _modelPath);
        }
    }
}
