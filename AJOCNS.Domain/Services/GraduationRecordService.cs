using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.GraduationRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Services
{
    public class GraduationRecordService : IGraduationRecordService
    {
        private readonly IGraduationRecordRepository _graduationRecordRepo;
        private readonly IStudentRepository _studentRepo;

        public GraduationRecordService(IGraduationRecordRepository graduationRecordRepo, IStudentRepository studentRepo)
        {
            _graduationRecordRepo = graduationRecordRepo;
            _studentRepo = studentRepo;
        }

        public async Task<Result<PagedGraduationRecordDto>> GetGraduationRecordsPagedAsync(int page, int pageSize, string? degreeCode = null, short? graduationYear = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _graduationRecordRepo.GetGraduationRecordsPagedAsync(page, pageSize, degreeCode, graduationYear);

            if (items is null || !items.Any())
            {
                return Result<PagedGraduationRecordDto>.Success(new PagedGraduationRecordDto
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                });
            }

            var paged = new PagedGraduationRecordDto
            {
                Records = items.Select(gr => new GraduationRecordDto
                {
                    Id = gr.GrecordId,
                    StudentId = gr.StudentId,
                    Srn = gr.Student?.Srn,
                    OfficialName = gr.OfficialName,
                    Grn = gr.Grn,
                    GraduationYear = gr.GraduationYear,
                    DegreeName = gr.Degree?.DegreeName ?? "-",
                    AccStatus = gr.AccStatus
                }).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Result<PagedGraduationRecordDto>.Success(paged);
        }

        public async Task<Result<List<short>>> GetGraduationYearsAsync()
        {
            var years = await _graduationRecordRepo.GetDistinctGraduationYearsAsync();
            if (years == null || !years.Any())
            {
                return Result<List<short>>.Failure("No graduation years found");
            }
            return Result<List<short>>.Success(years);
        }

        public async Task<Result<bool>> DeleteGraduationRecordAsync(int grecordId)
        {
            bool deleted = await _graduationRecordRepo.DeleteGraduationRecordAsync(grecordId);
            if (!deleted)
                return Result<bool>.Failure("Failed to delete graduation record.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<EditGraduationRecordDto>> GetGraduationRecordByIdAsync(int grecordId)
        {
            var record = await _graduationRecordRepo.GetGraduationRecordByIdAsync(grecordId);
            if (record is null)
                return Result<EditGraduationRecordDto>.Failure("Graduation record not found.");

            var dto = new EditGraduationRecordDto
            {
                Id = record.GrecordId,
                StudentId = record.StudentId,
                Srn = record.Student?.Srn,
                OfficialName = record.OfficialName,
                Grn = record.Grn,
                GraduationYear = record.GraduationYear,
                DegreeId = record.DegreeId,
                AccStatus = record.AccStatus
            };

            return Result<EditGraduationRecordDto>.Success(dto);
        }

        public async Task<Result<bool>> UpdateGraduationRecordAsync(EditGraduationRecordDto dto)
        {
            var record = await _graduationRecordRepo.GetGraduationRecordByIdAsync(dto.Id);
            if (record is null)
                return Result<bool>.Failure("Graduation record not found.");

            bool updated = await _graduationRecordRepo.UpdateGraduationRecordAsync(new Database.Entities.GraduationRecord
            {
                GrecordId = dto.Id,
                OfficialName = dto.OfficialName,
                Grn = dto.Grn,
                GraduationYear = dto.GraduationYear,
                DegreeId = dto.DegreeId,
                AccStatus = dto.AccStatus
            });

            if (!updated)
                return Result<bool>.Failure("Failed to update graduation record.");

            if (record.StudentId.HasValue)
            {
                var student = await _studentRepo.GetStudentByIdAsync(record.StudentId.Value);
                if (student != null)
                {
                    student.GraduationStatus = "Graduated";
                    await _studentRepo.UpdateStudentAsync(student);
                }
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<DegreeOptionDto>>> GetDegreesAsync()
        {
            var degrees = await _graduationRecordRepo.GetDegreesAsync();
            if (degrees == null || !degrees.Any())
            {
                return Result<List<DegreeOptionDto>>.Failure("No degrees found");
            }
            var degreeDtos = degrees.Select(d => new DegreeOptionDto
            {
                Id = d.DegreeId,
                DegreeName = d.DegreeName,
                DegreeCode = d.DegreeCode
            }).ToList();
            return Result<List<DegreeOptionDto>>.Success(degreeDtos);
        }
    }
}
