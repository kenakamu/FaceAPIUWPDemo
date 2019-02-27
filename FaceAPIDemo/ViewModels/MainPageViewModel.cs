using FaceAPIDemo.Services;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace FaceAPIDemo.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; NotifyPropertyChanged(); }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        public string Title = "Welcome to Edge AI";

        public CaptureElement CaptureElement;

        private MediaCapture mediaCapture = new MediaCapture();

        private FaceServiceClient faceClient = new FaceServiceClient(Settings.FaceKey, Settings.FaceApiRoot); // Azure Cognitive Face Service Client
        //private FaceServiceClient faceClient = new FaceServiceClient(Settings.FaceKey); // Azure Cognitive Face Service Client
        private FaceDetector faceDetector; // local camera face detector
        private IList<DetectedFace> detectedFaces; // Store faces detected locally.
        private bool newModel = false;

        public MainPageViewModel()
        {
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            Message = "Initializing..";
            faceDetector = await FaceDetector.CreateAsync();
            await mediaCapture.InitializeAsync();
            CaptureElement.Source = mediaCapture;
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            Message = "Loading Data..";

            // Check if face group alredy exists, otherwise create it.
            try
            {
                await faceClient.GetPersonGroupAsync(Settings.PersonGroupId);
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode == "PersonGroupNotFound")
                {
                    await faceClient.CreatePersonGroupAsync(Settings.PersonGroupId, Settings.PersonGroupId);
                    newModel = true;
                }
                else
                    throw;
            }
            catch(Exception ex)
            {

            }
       
            await mediaCapture.StartPreviewAsync();
            Identify();
        }

        /// <summary>
        /// Detect face by using local function.
        /// </summary>
        private async Task<InMemoryRandomAccessStream> DetectFaceAsync()
        {
            var imgFormat = ImageEncodingProperties.CreateJpeg();

            while (true)
            {
                try
                {
                    var stream = new InMemoryRandomAccessStream();
                    await mediaCapture.CapturePhotoToStreamAsync(imgFormat, stream);

                    var image = await ImageConverter.ConvertToSoftwareBitmapAsync(stream);
                    detectedFaces = await faceDetector.DetectFacesAsync(image);

                    if (detectedFaces.Count == 0)
                        continue;
                    else if (detectedFaces.Count != 1)
                        Message = "too many faces!";
                    else
                        return stream;
                }
                catch(Exception ex)
                {

                }
            }
        }       
        
        /// <summary>
        /// Identify the person by using Cognitive Face API
        /// </summary>
        private async void Identify()
        {
            while (true)
            {
                Message = "Seeing you...";
                using (var stream = await DetectFaceAsync())
                {
                    try
                    {
                        var faces = await faceClient.DetectAsync(ImageConverter.ConvertImage(stream));

                    if (!faces.Any())
                        continue;
                    
                        Person person;

                        if (newModel)
                        {
                            person = await RegisterAsync(stream);
                        }
                        else
                        {
                            var identifyResults = await faceClient.IdentifyAsync(Settings.PersonGroupId, faces.Select(x => x.FaceId).ToArray());
                            if (identifyResults.FirstOrDefault()?.Candidates?.Count() > 0)
                                person = await faceClient.GetPersonAsync(Settings.PersonGroupId, identifyResults.First().Candidates.First().PersonId);
                            else
                                person = await RegisterAsync(stream);
                        }            
                        Message = $"Hi {person.Name}!";
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Register face to Cognitive Face API
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task<Person> RegisterAsync(InMemoryRandomAccessStream stream)
        {
            while(string.IsNullOrEmpty(Name))
            {
                Message = "Enter your name";
                await Task.Delay(100);
            }
            var createPersonResult = await faceClient.CreatePersonAsync(Settings.PersonGroupId, Name);
            Guid personId = createPersonResult.PersonId;
            // Register face information and discard image.
            var addPersistedFaceResult = await faceClient.AddPersonFaceAsync(
                Settings.PersonGroupId, personId, ImageConverter.ConvertImage(stream));
            stream.Dispose();
            Name = null;
            newModel = false;
            await faceClient.TrainPersonGroupAsync(Settings.PersonGroupId);
            return await faceClient.GetPersonAsync(Settings.PersonGroupId, personId);
        }        
    }
}
