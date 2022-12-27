using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobStorage
{
    public static class Extension
    {
        public static async Task<BlobContainerClient> GetContainerAsync(string connectionString, string containerName)
        {
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
            return containerClient;
        }

        public static BlobClient CreateBlobClient(this BlobContainerClient containerClient, string blobName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            return blobClient;
        }

        public static async Task<List<string>> GetAllContainerAsync(this BlobServiceClient blobServiceClient)
        {
            Console.WriteLine("Listing containers...");
            List<string> blobContainerList = new List<string>();
            await foreach (BlobContainerItem blobContainer in blobServiceClient.GetBlobContainersAsync())
            {
                blobContainerList.Add(blobContainer.Name);
            }
            return blobContainerList;
        }

        public static async Task<List<string>> GetAllBlobAsync(this BlobContainerClient containerClient)
        {
            Console.WriteLine("Listing blobs...");
            List<string> blobList = new List<string>();
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                blobList.Add(blobItem.Name);
            }
            return blobList;
        }

        public static async Task CreateFileAsync(this BlobContainerClient containerClient, string fileName)
        {
            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "./data/";
            if (!Directory.Exists(Path.GetFullPath(localPath)))
            {
                Directory.CreateDirectory(Path.GetFullPath(localPath));
            }
            string localFilePath = Path.Combine(localPath, fileName) + ".txt";

            // Write text to the file
            await File.WriteAllTextAsync(localFilePath, $"Hello, World! {fileName}");

            // Get a reference to the blob
            BlobClient blobClient = containerClient.CreateBlobClient(fileName);
            await blobClient.UploadAsync(localFilePath);
            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);
        }

        public static async Task DownloadBlobAsync(this BlobContainerClient containerClient, string blobName)
        {
            string localPath = "./data/";
            string downloadFilePath = Path.Combine(localPath, blobName);

            Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

            BlobClient blobClient = containerClient.CreateBlobClient(blobName);

            // Download the blob's contents and save it to a file
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            await using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                await download.Content.CopyToAsync(downloadFileStream);
            }

            Console.WriteLine("\nLocate the local file in the data directory created earlier to verify it was downloaded.");
        }

        public static async Task DeleteBlobAsync(this BlobContainerClient containerClient, string blobName)
        {
            
        }

        public static async Task DeleteContainerAsync(this BlobContainerClient containerClient)
        {
            Console.WriteLine("\n\nDeleting blob container...");
            await containerClient.DeleteAsync();
            Console.WriteLine("Finished cleaning up.");
        }
    }
}
