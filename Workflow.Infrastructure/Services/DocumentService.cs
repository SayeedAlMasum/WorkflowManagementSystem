using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Domain.Entities;
using Workflow.Infrastructure.Persistence;

namespace Workflow.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;
    private readonly IMapper _mapper;

    public DocumentService(AppDbContext db, IStorageService storage, IMapper mapper)
    {
        _db = db;
        _storage = storage;
        _mapper = mapper;
    }

    public async Task<DocumentDto> UploadAsync(CreateDocumentDto dto, string uploaderId)
    {
        // Save file to storage
        var url = await _storage.SaveAsync(dto.Content, dto.FileName, dto.MimeType);

        var doc = new Document
        {
            FileName = dto.FileName,
            Url = url,
            MimeType = dto.MimeType,
            UploaderId = uploaderId,
            CreatedDate = DateTime.UtcNow
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        return _mapper.Map<DocumentDto>(doc);
    }

    public async Task<List<DocumentDto>> ListAsync(string? uploaderId = null)
    {
        var query = _db.Documents.AsQueryable();

        if (!string.IsNullOrEmpty(uploaderId))
            query = query.Where(d => d.UploaderId == uploaderId);

        var items = await query
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        return _mapper.Map<List<DocumentDto>>(items);
    }

    public async Task<DocumentDto> GetByIdAsync(int id)
    {
        var doc = await _db.Documents.FindAsync(id);

        if (doc == null)
            throw new KeyNotFoundException($"Document with ID {id} not found");

        return _mapper.Map<DocumentDto>(doc);
    }

    public async Task DeleteAsync(int id)
    {
        var doc = await _db.Documents.FindAsync(id);

        if (doc == null)
            throw new KeyNotFoundException($"Document with ID {id} not found");

        // Delete physical file
        await _storage.DeleteAsync(doc.Url);

        // Delete database record
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
    }
}










