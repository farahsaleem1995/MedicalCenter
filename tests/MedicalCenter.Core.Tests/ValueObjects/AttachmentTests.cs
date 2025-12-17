using FluentAssertions;
using MedicalCenter.Core.ValueObjects;
using Xunit;

namespace MedicalCenter.Core.Tests.ValueObjects;

public class AttachmentTests
{
    [Fact]
    public void Create_WithValidData_CreatesAttachment()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var fileName = "test.pdf";
        var fileSize = 1024L;
        var contentType = "application/pdf";
        var uploadedAt = DateTime.UtcNow;

        // Act
        var attachment = Attachment.Create(fileId, fileName, fileSize, contentType, uploadedAt);

        // Assert
        attachment.Should().NotBeNull();
        attachment.FileId.Should().Be(fileId);
        attachment.FileName.Should().Be(fileName);
        attachment.FileSize.Should().Be(fileSize);
        attachment.ContentType.Should().Be(contentType);
        attachment.UploadedAt.Should().Be(uploadedAt);
    }

    [Fact]
    public void Create_WithEmptyFileId_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.Empty,
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyFileName_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.NewGuid(),
            string.Empty,
            1024,
            "application/pdf",
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroFileSize_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            0,
            "application/pdf",
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeFileSize_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            -1,
            "application/pdf",
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithLargeFileSize_CreatesAttachment()
    {
        // Arrange
        // Note: File size limit validation is done at service/endpoint level
        // This value object only ensures positive file size
        const long largeFileSize = 100 * 1024 * 1024; // 100MB

        // Act
        var attachment = Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            largeFileSize,
            "application/pdf",
            DateTime.UtcNow);

        // Assert
        attachment.Should().NotBeNull();
        attachment.FileSize.Should().Be(largeFileSize);
    }

    [Fact]
    public void Create_WithEmptyContentType_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            1024,
            string.Empty,
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceFileName_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.NewGuid(),
            "   ",
            1024,
            "application/pdf",
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceContentType_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            1024,
            "   ",
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithFutureUploadDate_ThrowsException()
    {
        // Act & Assert
        var act = () => Attachment.Create(
            Guid.NewGuid(),
            "test.pdf",
            1024,
            "application/pdf",
            DateTime.UtcNow.AddDays(1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;
        var attachment1 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", uploadedAt);
        var attachment2 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", uploadedAt);

        // Act & Assert
        attachment1.Should().Be(attachment2);
        (attachment1 == attachment2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentFileId_ReturnsFalse()
    {
        // Arrange
        var uploadedAt = DateTime.UtcNow;
        var attachment1 = Attachment.Create(Guid.NewGuid(), "test.pdf", 1024, "application/pdf", uploadedAt);
        var attachment2 = Attachment.Create(Guid.NewGuid(), "test.pdf", 1024, "application/pdf", uploadedAt);

        // Act & Assert
        attachment1.Should().NotBe(attachment2);
        (attachment1 != attachment2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentFileName_ReturnsFalse()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;
        var attachment1 = Attachment.Create(fileId, "test1.pdf", 1024, "application/pdf", uploadedAt);
        var attachment2 = Attachment.Create(fileId, "test2.pdf", 1024, "application/pdf", uploadedAt);

        // Act & Assert
        attachment1.Should().NotBe(attachment2);
    }

    [Fact]
    public void Equals_WithDifferentFileSize_ReturnsFalse()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;
        var attachment1 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", uploadedAt);
        var attachment2 = Attachment.Create(fileId, "test.pdf", 2048, "application/pdf", uploadedAt);

        // Act & Assert
        attachment1.Should().NotBe(attachment2);
    }

    [Fact]
    public void Equals_WithDifferentContentType_ReturnsFalse()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;
        var attachment1 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", uploadedAt);
        var attachment2 = Attachment.Create(fileId, "test.pdf", 1024, "image/jpeg", uploadedAt);

        // Act & Assert
        attachment1.Should().NotBe(attachment2);
    }

    [Fact]
    public void Equals_WithDifferentUploadedAt_ReturnsFalse()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var attachment1 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", DateTime.UtcNow.AddHours(-1));
        var attachment2 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", DateTime.UtcNow);

        // Act & Assert
        attachment1.Should().NotBe(attachment2);
    }

    [Fact]
    public void GetHashCode_ReturnsSameHashCode_ForEqualAttachments()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;
        var attachment1 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", uploadedAt);
        var attachment2 = Attachment.Create(fileId, "test.pdf", 1024, "application/pdf", uploadedAt);

        // Act & Assert
        attachment1.GetHashCode().Should().Be(attachment2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ReturnsDifferentHashCode_ForDifferentAttachments()
    {
        // Arrange
        var attachment1 = Attachment.Create(Guid.NewGuid(), "test1.pdf", 1024, "application/pdf", DateTime.UtcNow);
        var attachment2 = Attachment.Create(Guid.NewGuid(), "test2.pdf", 2048, "image/jpeg", DateTime.UtcNow);

        // Act & Assert
        attachment1.GetHashCode().Should().NotBe(attachment2.GetHashCode());
    }
}
