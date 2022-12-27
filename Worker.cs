using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Azure;

namespace AzureBlobStorage;

internal class Worker
{
    private readonly IConfiguration _configuration;

    public Worker(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public void DoWork()
    {
        ProcessAsync().GetAwaiter().GetResult();
    }

    public async Task ProcessAsync()
    {
        // Copy the connection string from the portal in the variable below.
        string storageConnectionString = _configuration["ConnectionString:azBlobConnection"];

        // Create a client that can authenticate with a connection string
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

        RefreshUi();
        Console.WriteLine("Please Select the Option: ");
        Console.WriteLine("1. Show list of container.");
        Console.WriteLine("2. Create/view container.");
        var input = Console.ReadLine();
        switch (input)
        {
            case "1":
                await DisplayListOfContainer(blobServiceClient);
                await GoBack();
                break;
            case "2":
                await CreateViewContainer(storageConnectionString);
                break;
            default:
                Console.WriteLine("Invalid choice please select again");
                await ProcessAsync();
                break;
        }
    }

    public async Task GoBack(BlobContainerClient storageContainer = null, string connectionString = null)
    {
        Console.WriteLine("Please Select the Option: ");
        Console.WriteLine("1. Go to main menu.");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("2. Go to blob operation");
        }
        Console.WriteLine("0. Exit");
        var input = Console.ReadLine();
        switch (input)
        {
            case "1":
                await ProcessAsync();
                break;
            case "2":
                await BlobOperation(storageContainer, connectionString);
                break;
            case "0":
                Exit();
                break;
            default:
                Console.WriteLine("Invalid input, please try again.");
                await GoBack();
                break;
        }
    }

    public async Task CreateViewContainer(string storageConnectionString)
    {
        var storageContainer = await CreateContainer(storageConnectionString);
        await BlobOperation(storageContainer, storageConnectionString);
    }

    public async Task BlobOperation(BlobContainerClient storageContainer, string storageConnectionString)
    {
        RefreshUi();
        Console.WriteLine("Please Select the Option: ");
        Console.WriteLine("1. Show list of blob.");
        Console.WriteLine("2. Create and upload blob.");
        Console.WriteLine("3. download blob.");
        Console.WriteLine("4. delete blob.");
        Console.WriteLine("5, Show container property.");
        Console.WriteLine("6, Set container metadata.");
        var blobInput = Console.ReadLine();
        switch (blobInput)
        {
            case "1":
                RefreshUi();
                await DisplayListOfBlob(storageContainer);
                await GoBack(storageContainer, storageConnectionString);
                break;
            case "2":
                RefreshUi();
                Console.WriteLine("Please enter the blob name: ");
                var blobNameForCreate = Console.ReadLine();
                await storageContainer.CreateFileAsync(blobNameForCreate);
                await GoBack(storageContainer, storageConnectionString);
                break;
            case "3":
                RefreshUi();
                await DownloadBlob(storageContainer);
                await GoBack(storageContainer, storageConnectionString);
                break;
            case "4":
                RefreshUi();
                await DeleteBlobAsync(storageContainer);
                await GoBack(storageContainer, storageConnectionString);
                break;
            case "5":
                RefreshUi();
                await ReadContainerPropertiesAsync(storageContainer);
                await GoBack(storageContainer, storageConnectionString);
                break;
            case "6":
                RefreshUi();
                await AddContainerMetadataAsync(storageContainer);
                await GoBack(storageContainer, storageConnectionString);
                break;
            default:
                Console.WriteLine("Invalid choice please select again");
                await BlobOperation(storageContainer, storageConnectionString);
                break;
        }
    }

    static async Task DeleteBlobAsync(BlobContainerClient storageContainer)
    {
        var blobList = await storageContainer.GetAllBlobAsync();
        Console.WriteLine("Please enter the blob name: ");
        var blobName = Console.ReadLine();
        if (blobList.Contains(blobName))
        {
            Console.WriteLine("\n\nDeleting blob file...");
            await storageContainer.DeleteBlobAsync(blobName);
            Console.WriteLine("Finished deleting.");
        }
        else
        {
            Console.WriteLine("Invalid blob name, Please try again.");
        }
    }

    static async Task DownloadBlob(BlobContainerClient storageContainer)
    {
        var blobList = await storageContainer.GetAllBlobAsync();
        Console.WriteLine("Please enter the blob name: ");
        var blobNameForDownload = Console.ReadLine();
        if (blobList.Contains(blobNameForDownload))
        {
            await storageContainer.DownloadBlobAsync(blobNameForDownload);
        }
        else
        {
            Console.WriteLine("Invalid blob name, Please try again.");
        }
    }

    static async Task DisplayListOfBlob(BlobContainerClient storageContainer)
    {
        Console.WriteLine("Blob Name List: ");
        var blobList = await storageContainer.GetAllBlobAsync();
        foreach (var blob in blobList)
        {
            Console.WriteLine("\t" + blob);
        }
    }

    static void RefreshUi()
    {
        Console.Clear();
        Console.WriteLine("--------------------Azure Blob Storage Example---------------------");
    }

    static async Task<BlobContainerClient> CreateContainer(string connectionString)
    {
        Console.WriteLine("Please enter container Name: ");
        var containerName = Console.ReadLine();
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
        return containerClient;
    }

    static async Task DisplayListOfContainer(BlobServiceClient blobServiceClient)
    {
        Console.WriteLine("Container Name List: ");
        var listOfContainer = await blobServiceClient.GetAllContainerAsync();
        foreach (var container in listOfContainer)
        {
            Console.WriteLine("\t" + container);
        }
    }

    private static async Task ReadContainerPropertiesAsync(BlobContainerClient container)
    {
        try
        {
            // Fetch some container properties and write out their values.
            var properties = await container.GetPropertiesAsync();
            Console.WriteLine($"Properties for container {container.Uri}");
            Console.WriteLine($"Public access level: {properties.Value.PublicAccess}");
            Console.WriteLine($"Last modified time in UTC: {properties.Value.LastModified}");
            if (properties.Value.Metadata.Any())
            {
                foreach (var metadata in properties.Value.Metadata)
                {
                    Console.WriteLine($"{metadata.Key}: {metadata.Value}");
                }
            }
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
            Console.WriteLine(e.Message);
        }
    }

    public static async Task AddContainerMetadataAsync(BlobContainerClient container)
    {
        try
        {
            IDictionary<string, string> metadata =
                new Dictionary<string, string>();

            // Add some metadata to the container.
            metadata.Add("docType", "textDocuments");
            metadata.Add("category", "guidance");

            // Set the container's metadata.
            await container.SetMetadataAsync(metadata);
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
            Console.WriteLine(e.Message);
        }
    }

    static void Exit()
    {
        Console.WriteLine("Press enter to exit the sample application.");
        Console.ReadLine();
    }
}