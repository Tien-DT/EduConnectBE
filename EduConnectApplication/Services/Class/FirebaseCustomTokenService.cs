using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduConnect.Application.Services.Class
{
    public class FirebaseCustomTokenService : IFirebaseCustomTokenService
    {
        private readonly ILogger<FirebaseCustomTokenService> _logger;
        private readonly FirebaseOption _firebaseOptions;
        private FirebaseApp? _firebaseApp;

        public FirebaseCustomTokenService(
            ILogger<FirebaseCustomTokenService> logger,
            IOptions<FirebaseOption> firebaseOptions)
        {
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
            InitializeFirebaseApp();
        }

        private void InitializeFirebaseApp()
        {
            try
            {
                if (_firebaseApp == null && !string.IsNullOrEmpty(_firebaseOptions.ServiceAccountKeyPath))
                {
                    var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    var serviceAccountPath = Path.IsPathRooted(_firebaseOptions.ServiceAccountKeyPath) 
                        ? _firebaseOptions.ServiceAccountKeyPath 
                        : Path.Combine(basePath, _firebaseOptions.ServiceAccountKeyPath);

                    if (File.Exists(serviceAccountPath))
                    {
                        _firebaseApp = FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(serviceAccountPath),
                            ProjectId = _firebaseOptions.ProjectId
                        });
                        _logger.LogInformation($"Firebase app initialized with service account key from: {serviceAccountPath}");
                    }
                    else
                    {
                        _logger.LogError($"Service account key file not found at: {serviceAccountPath}");
                    }
                }
                else if (_firebaseApp == null && !string.IsNullOrEmpty(_firebaseOptions.ServiceAccountJson))
                {
                    var credential = GoogleCredential.FromJson(_firebaseOptions.ServiceAccountJson);
                    _firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential,
                        ProjectId = _firebaseOptions.ProjectId
                    });
                    _logger.LogInformation("Firebase app initialized with service account JSON from configuration");
                }
                else
                {
                    _logger.LogWarning("No Firebase service account configuration found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase app");
            }
        }

        public async Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, object>? claims = null)
        {
            try
            {
                if (_firebaseApp == null)
                {
                    throw new InvalidOperationException("Firebase app is not initialized");
                }

                var auth = FirebaseAuth.GetAuth(_firebaseApp);
                
                if (claims != null && claims.Any())
                {
                    return await auth.CreateCustomTokenAsync(userId, claims);
                }
                else
                {
                    return await auth.CreateCustomTokenAsync(userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate custom token for user {userId}");
                throw;
            }
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            try
            {
                if (_firebaseApp == null)
                {
                    throw new InvalidOperationException("Firebase app is not initialized");
                }

                var auth = FirebaseAuth.GetAuth(_firebaseApp);
                var decodedToken = await auth.VerifyIdTokenAsync(token);
                return decodedToken != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify Firebase token");
                return false;
            }
        }
    }
}
