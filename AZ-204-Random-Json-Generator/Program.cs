using Azure.Storage.Blobs;
using System.Text.Json;

namespace AZ_204_Random_Json_Generator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = string.Empty;

            Console.Write("Random string generator console application. ");
            //connectionString = Console.ReadLine();
            string outputDirectory = "GeneratedJsonFiles";
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            Console.Write($"{System.Environment.NewLine}All generated file will be stored in local directory at. {outputDirectory} ");

            Console.Write($"{System.Environment.NewLine}Enter the azure storage connection string. For simulator use  : UseDevelopmentStorage=true  :  ");
           

            connectionString = Console.ReadLine();// "UseDevelopmentStorage=true"; // Replace with your Azure Storage connection string
            
            string containerName = "genabc"; // Replace with your Azure Blob Storage container name

            Console.Write($"{System.Environment.NewLine}All json file will be uploaded in {containerName}  container in storage account");

            try
            {



                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Create container if it doesn't exist
                await containerClient.CreateIfNotExistsAsync();

                Random random = new Random();

                // Generate 100 random JSON files
                for (int i = 1; i <= 100; i++)
                {
                    // Create a random JSON structure with at least 6 properties
                    var randomJson = GenerateJsonWithMinimumProperties(random);

                    // Serialize the JSON object to a file
                    string jsonString = JsonSerializer.Serialize(randomJson, new JsonSerializerOptions { WriteIndented = true });
                    string fileName = $"random_json_{i}.json";
                    string filePath = Path.Combine(outputDirectory, fileName);

                    File.WriteAllText(filePath, jsonString);
                    Console.WriteLine($"Generated: {filePath}");

                    // Upload the generated JSON file to Azure Blob Storage
                    await UploadFileToBlobStorage(containerClient, filePath, fileName);
                }

                Console.WriteLine("{System.Environment.NewLine}100 random JSON files generated and uploaded to Azure Blob Storage successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{System.Environment.NewLine}Exception : {ex.Message.ToString()}");
            }

        }

        // Function to generate a JSON object with at least 6 properties
        static Dictionary<string, object> GenerateJsonWithMinimumProperties(Random random)
        {
            var jsonObject = new Dictionary<string, object>();

            // Add 6 predefined properties
            jsonObject["name"] = $"random_name_{random.Next(1000)}";
            jsonObject["age"] = random.Next(18, 100);
            jsonObject["isEmployed"] = random.Next(2) == 0;
            jsonObject["salary"] = random.Next(30000, 150000);
            jsonObject["hobbies"] = new List<string> { "reading", "traveling", "sports" };
            jsonObject["address"] = new Dictionary<string, string>
            {
                { "street", $"random_street_{random.Next(100)}" },
                { "city", $"random_city_{random.Next(100)}" },
                { "zipcode", $"{random.Next(10000, 99999)}" }
            };

            // Add randomly generated additional properties
            int additionalProperties = random.Next(2, 5); // Randomly determine the number of extra properties
            for (int i = 0; i < additionalProperties; i++)
            {
                string key = $"random_property_{random.Next(1000)}";
                object value = GenerateRandomValue(random, random.Next(1, 4)); // Random depth for nested objects
                jsonObject[key] = value;
            }

            return jsonObject;
        }

        // Function to generate a random value based on type
        static object GenerateRandomValue(Random random, int depth)
        {
            int valueType = random.Next(5); // 0: string, 1: int, 2: bool, 3: array, 4: object

            switch (valueType)
            {
                case 0:
                    return $"random_string_{random.Next(1000)}";
                case 1:
                    return random.Next(1000);
                case 2:
                    return random.Next(2) == 0;
                case 3:
                    return GenerateRandomArray(random);
                case 4:
                    return depth > 1 ? GenerateRandomJson(random, depth - 1) : $"nested_object_limit";
                default:
                    return null;
            }
        }

        // Function to generate a random array of values
        static List<object> GenerateRandomArray(Random random)
        {
            int arrayLength = random.Next(2, 6); // Random array length
            var array = new List<object>();

            for (int i = 0; i < arrayLength; i++)
            {
                array.Add(GenerateRandomValue(random, 1));
            }

            return array;
        }

        // Function to generate a random JSON object for nested properties
        static Dictionary<string, object> GenerateRandomJson(Random random, int depth)
        {
            var jsonObject = new Dictionary<string, object>();
            int numberOfProperties = random.Next(2, 6); // Randomly determine the number of properties

            for (int i = 0; i < numberOfProperties; i++)
            {
                string key = $"nested_property_{random.Next(1000)}";
                object value = GenerateRandomValue(random, depth);
                jsonObject[key] = value;
            }

            return jsonObject;
        }

        // Function to upload a file to Azure Blob Storage
        static async Task UploadFileToBlobStorage(BlobContainerClient containerClient, string filePath, string fileName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Upload the file
            using FileStream uploadFileStream = File.OpenRead(filePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            Console.WriteLine($"Uploaded {fileName} to Azure Blob Storage.");
        }
    }
}