namespace UserAuth.Api.Results
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int? Id { get; set; }

        // Convenience helpers
        public static Result Success(int? id = null) =>
            new Result { IsSuccess = true, Id = id };

        public static Result Failure(string errorMessage) =>
            new Result { IsSuccess = false, ErrorMessage = errorMessage };


    }
}
