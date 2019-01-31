using System;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.IO;

namespace SayisalLotoSonuclari
{
    class GoogleDriveManager
    {
        StringBuilder stringBuilder = new StringBuilder();
        readonly string startupPath = AppDomain.CurrentDomain.BaseDirectory;
        static string fileName = "sonuclar.csv";
        string saveFile = "save.csv";
        string saveFilePath = "";
        static string credentialJsonPath = "credentials.json";
        public static string localFilePath;
        private string onlineFileId;
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };

        public GoogleDriveManager()
        {
            Console.WriteLine(startupPath);
            localFilePath =Path.Combine( startupPath , fileName);
            localFilePath = Directory.GetParent(localFilePath).FullName;
            localFilePath = Directory.GetParent(localFilePath).FullName;
            localFilePath = Directory.GetParent(localFilePath).FullName;
            saveFilePath = Path.Combine(localFilePath, saveFile);
            localFilePath = Path.Combine(localFilePath, fileName);          
            Console.WriteLine(localFilePath);
           

        }
        public void AddToCSV(string title)
        {
            stringBuilder.AppendLine(title);
        }
        public void SaveCSVLocal(bool newFile)
        {
            string currentContent = String.Empty;
            if (File.Exists(localFilePath))
            {
                currentContent = File.ReadAllText(localFilePath);
            }

            try
            {
                File.WriteAllText(localFilePath, stringBuilder.ToString() + currentContent);
            }
            catch (Exception)
            {
                throw;
            }

            Thread.Sleep(1000);
            Console.WriteLine("Local File Saved");
            /*
            if (newFile)
            {

                UploadNewFileAsync(localFilePath);
            }
            else
            {

                UpdateFile(localFilePath);
            }
            */
        }
        public async Task<DriveService> RequestServiceAsync()
        {
            UserCredential credential = await AuthorizeDrive();

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Sayisal Loto",
            });
            return service;
        }
        async Task<UserCredential> AuthorizeDrive()
        {
            UserCredential credential;
            if(File.Exists(credentialJsonPath))
            {
                Console.WriteLine("Login Json file exists !");
            }
            else
            {
                Console.WriteLine("No Login file!");
            }
            var stream = new FileStream(credentialJsonPath,FileMode.Open,FileAccess.Read);
            string credPath = "token.json";
            credential =await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None,new FileDataStore(credPath,true));
            Console.WriteLine(credential.UserId);
            return credential;
        }
        private async void UploadNewFileAsync(string localFilePath)
        {
            var service = await RequestServiceAsync();

            if (!File.Exists(localFilePath))
            {
                Console.WriteLine("NO FILE EXIST!");
                return;
            }
            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Description = "sayisal loto sonucları",
            };

            FilesResource.CreateMediaUpload request;
            var uploadStream = new FileStream(localFilePath, FileMode.Open);
            try
            {
                request = service.Files.Create(body, uploadStream, "text/csv");
                request.Fields = "id";
                request.Upload();
            }
            catch (Exception)
            {
                throw;
            }
            Google.Apis.Drive.v3.Data.File file = request.ResponseBody;
            SaveFileId(file.Id);
            Console.WriteLine("\"{0}\" was uploaded successfully, file id \"{1}\"", fileName,file.Id);

        }
        private async void UpdateFile(string localFilePath)
        {
            LoadFileIdAsync();
      
            var service = await RequestServiceAsync();
            FilesResource.UpdateMediaUpload request;
            Google.Apis.Drive.v3.Data.File file = new Google.Apis.Drive.v3.Data.File();
            file.Name = Path.GetFileName(fileName);
            file.Description = "File Updated " + DateTime.Now.ToLocalTime().ToString();
            var uploadStream = new FileStream(localFilePath, FileMode.Open);
            try
            {
                request = service.Files.Update(file,onlineFileId,uploadStream,"text/csv");
                request.Upload();
            }
            catch (Exception)
            {
                throw;
            }
            
            if(request.ResponseBody != null)
            Console.WriteLine("\"{0}\" was uploaded successfully, file id \"{1}\"", fileName, request.ResponseBody.Id);
            else
                Console.WriteLine("\"{0}\" not uploaded.", fileName);
        }
        public async void ListDriveFilesAsync()
        {
            string pageToken = null;
            DriveService driveService = await RequestServiceAsync();
            do
            {
                var request = driveService.Files.List();
                //request.Q = "mimeType='image/jpeg'";
                request.Spaces = "drive";
                request.Fields = "nextPageToken, files(id, name)";
                request.PageToken = pageToken;
                var result = request.Execute();
                foreach (var file in result.Files)
                {
                    if (String.Equals(file.Name, fileName))
                        Console.WriteLine(String.Format(
                                "Found file: {0} ({1})", file.Name, file.Id));
                }
                pageToken = result.NextPageToken;
            } while (pageToken != null);
        }
        async Task<string> GetOnlineFileId()
        {
            string pageToken = null;
            string id = string.Empty;
            DriveService driveService = await RequestServiceAsync();
            do
            {
                var request = driveService.Files.List();
                //request.Q = "mimeType='image/jpeg'";
                request.Spaces = "drive";
                request.Fields = "nextPageToken, files(id, name)";
                request.PageToken = pageToken;
                var result = request.Execute();
                foreach (var file in result.Files)
                {
                    if (String.Equals(file.Name, fileName))
                    {
                        id = file.Id;
                        Console.WriteLine(String.Format("Found file: {0} ({1})", file.Name, file.Id));
                        break;
                    }
                }
                pageToken = result.NextPageToken;
            } while (pageToken != null);
            return id;
        }
        void SaveFileId(string id)
        {
            File.WriteAllText(saveFilePath, id);
        }
        public void LoadFileId()
        {
            LoadFileIdAsync();
        }
        async void LoadFileIdAsync()
        {
            if(File.Exists(saveFilePath))
            {
                onlineFileId = File.ReadAllText(saveFilePath);
                if(string.IsNullOrEmpty(onlineFileId))
                {
                    onlineFileId = await GetOnlineFileId();
                    File.WriteAllText(saveFilePath, onlineFileId);
                }
            }
            else
            {
                onlineFileId = await GetOnlineFileId();
                File.WriteAllText(saveFilePath, onlineFileId);
            }
            Console.WriteLine("Online file id loaded, file id = "+onlineFileId);
        }
    }
}
