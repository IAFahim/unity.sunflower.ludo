// Define your DTOs here (Mirrors your Backend DTOs)
namespace Shared.DTOs
{
    [System.Serializable]
    public record PlayerUpdatedMessage
    {
        public string UserId;
        public long NewCoins;
        public string Username;
        public string Email;
    };

    [System.Serializable]
    public record TransactionResult
    {
        public bool Success;
        public long NewBalance;
        public string ErrorMessage;
    };
    
    [System.Serializable]
    public struct Vector3Data { public float x, y, z; }
}