namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class PagedStudentDto
    {
        public List<StudentDto> Students { get; set; } = new();

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPrevious => CurrentPage > 1;

        public bool HasNext => CurrentPage < TotalPages;
    }
}
