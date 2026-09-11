using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Mentor;

namespace AJOCNS.Domain.Services
{
    public class MentorService : IMentorService
    {
        private readonly IMentorRepository _mentorRepo;

        public MentorService(IMentorRepository mentorRepo)
        {
            _mentorRepo = mentorRepo;
        }

        public async Task<Result<MentorProfileDto>> GetMentorProfileAsync(int userId)
        {
            var mentor = await _mentorRepo.GetMentorByUserIdAsync(userId);
            if (mentor is null)
                return Result<MentorProfileDto>.Failure("Mentor profile not found");

            return Result<MentorProfileDto>.Success(MapToProfileDto(mentor));
        }

        public async Task<Result<List<MentorProfileDto>>> GetAllMentorsAsync()
        {
            var mentors = await _mentorRepo.GetAllMentorsAsync();
            var dtos = mentors.Select(MapToProfileDto).ToList();
            return Result<List<MentorProfileDto>>.Success(dtos);
        }

        public async Task<Result<MentorProfileDto>> UpdateMentorProfileAsync(int userId, MentorProfileEditDto dto)
        {
            var mentor = await _mentorRepo.GetMentorByUserIdAsync(userId);
            if (mentor is null)
                return Result<MentorProfileDto>.Failure("Mentor profile not found");

            mentor.Name = dto.Name.Trim();
            mentor.Expertise = dto.Expertise;
            var updated = await _mentorRepo.UpdateMentorAsync(mentor);
            if (!updated)
                return Result<MentorProfileDto>.Failure("Failed to update mentor profile");

            return Result<MentorProfileDto>.Success(MapToProfileDto(mentor));
        }

        public async Task<Result<List<EmploymentRecordDto>>> GetEmploymentRecordsAsync(int mentorId)
        {
            var records = await _mentorRepo.GetEmploymentRecordsByMentorIdAsync(mentorId);
            var dtos = records.Select(MapToEmploymentRecordDto).ToList();
            return Result<List<EmploymentRecordDto>>.Success(dtos);
        }

        public async Task<Result<EmploymentRecordDto>> CreateEmploymentRecordAsync(int mentorId, CreateEmploymentRecordDto dto)
        {
            var mentor = await _mentorRepo.GetMentorByIdAsync(mentorId);
            if (mentor is null)
                return Result<EmploymentRecordDto>.Failure("Mentor not found");

            if (dto.IsCurrentPosition)
                dto.EndDate = null;

            var company = await _mentorRepo.GetOrCreateCompanyAsync(dto.CompanyName);
            if (company is null)
                return Result<EmploymentRecordDto>.Failure("Failed to resolve company");

            var position = await _mentorRepo.GetOrCreatePositionAsync(dto.PositionName);
            if (position is null)
                return Result<EmploymentRecordDto>.Failure("Failed to resolve position");

            var record = new EmploymentRecord
            {
                MentorId = mentorId,
                CompanyId = company.CompanyId,
                PositionId = position.PositionId,
                StartDate = DateOnly.FromDateTime(dto.StartDate),
                EndDate = dto.EndDate.HasValue ? DateOnly.FromDateTime(dto.EndDate.Value) : null
            };

            var created = await _mentorRepo.CreateEmploymentRecordAsync(record);
            if (!created)
                return Result<EmploymentRecordDto>.Failure("Failed to create employment record");

            var savedRecord = await _mentorRepo.GetEmploymentRecordByIdAsync(record.EmploymentRId);
            return Result<EmploymentRecordDto>.Success(MapToEmploymentRecordDto(savedRecord!));
        }

        public async Task<Result<EmploymentRecordDto>> UpdateEmploymentRecordAsync(int mentorId, UpdateEmploymentRecordDto dto)
        {
            var record = await _mentorRepo.GetEmploymentRecordByIdAsync(dto.Id);
            if (record is null || record.MentorId != mentorId)
                return Result<EmploymentRecordDto>.Failure("Employment record not found");

            if (dto.IsCurrentPosition)
                dto.EndDate = null;

            var company = await _mentorRepo.GetOrCreateCompanyAsync(dto.CompanyName);
            if (company is null)
                return Result<EmploymentRecordDto>.Failure("Failed to resolve company");

            var position = await _mentorRepo.GetOrCreatePositionAsync(dto.PositionName);
            if (position is null)
                return Result<EmploymentRecordDto>.Failure("Failed to resolve position");

            record.CompanyId = company.CompanyId;
            record.PositionId = position.PositionId;
            record.StartDate = DateOnly.FromDateTime(dto.StartDate);
            record.EndDate = dto.EndDate.HasValue ? DateOnly.FromDateTime(dto.EndDate.Value) : null;

            var updated = await _mentorRepo.UpdateEmploymentRecordAsync(record);
            if (!updated)
                return Result<EmploymentRecordDto>.Failure("Failed to update employment record");

            var savedRecord = await _mentorRepo.GetEmploymentRecordByIdAsync(record.EmploymentRId);
            return Result<EmploymentRecordDto>.Success(MapToEmploymentRecordDto(savedRecord!));
        }

        public async Task<Result<bool>> DeleteEmploymentRecordAsync(int mentorId, int recordId)
        {
            var record = await _mentorRepo.GetEmploymentRecordByIdAsync(recordId);
            if (record is null || record.MentorId != mentorId)
                return Result<bool>.Failure("Employment record not found");

            var deleted = await _mentorRepo.DeleteEmploymentRecordAsync(recordId);
            if (!deleted)
                return Result<bool>.Failure("Failed to delete employment record");

            return Result<bool>.Success(true);
        }

        private static MentorProfileDto MapToProfileDto(Mentor mentor)
        {
            return new MentorProfileDto
            {
                MentorId = mentor.MentorId,
                UserId = mentor.UserId,
                Name = mentor.Name,
                Email = mentor.User?.Email ?? "",
                Expertise = mentor.Expertise,
                AlumniGy = mentor.AlumniGy,
                AlumniGrn = mentor.AlumniGrn,
                Status = mentor.User?.Status ?? "",
                EmploymentRecords = mentor.EmploymentRecords?
                    .Select(MapToEmploymentRecordDto)
                    .OrderByDescending(e => e.StartDate)
                    .ToList() ?? new List<EmploymentRecordDto>()
            };
        }

        private static EmploymentRecordDto MapToEmploymentRecordDto(EmploymentRecord record)
        {
            return new EmploymentRecordDto
            {
                Id = record.EmploymentRId,
                CompanyId = record.CompanyId,
                CompanyName = record.Company?.CompanyName ?? "",
                PositionId = record.PositionId,
                PositionName = record.Position?.Position1 ?? "",
                StartDate = record.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = record.EndDate?.ToDateTime(TimeOnly.MinValue)
            };
        }
    }
}