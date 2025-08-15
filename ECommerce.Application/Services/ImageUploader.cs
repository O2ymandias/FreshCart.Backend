using ECommerce.Core.Common;
using ECommerce.Core.Common.Constants;
using ECommerce.Core.Common.Options;
using ECommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Services;
public class ImageUploader : IImageUploader
{
	private readonly IWebHostEnvironment _env;
	private readonly IStringLocalizer<ImageUploader> _localizer;
	private readonly string[] _allowedExtensions;
	private readonly long _maxImageSize;


	public ImageUploader(
		IWebHostEnvironment env,
		IOptions<ImageUploaderOptions> imageUploaderOptions,
		IStringLocalizer<ImageUploader> localizer
		)
	{
		_env = env;
		_localizer = localizer;
		_allowedExtensions = imageUploaderOptions.Value.AllowedExtensions;
		_maxImageSize = imageUploaderOptions.Value.MaxFileSizeMB * 1024 * 1024;
	}
	public async Task<UploadImageResult> UploadImageAsync(IFormFile file, string folderName)
	{
		if (file is null)
			return new() { ErrorMessage = _localizer[L.ImageUploadMessages.noImageProvided] };

		if (file.Length == 0)
			return new() { ErrorMessage = _localizer[L.ImageUploadMessages.emptyImage] };

		if (file.Length > _maxImageSize)
		{
			var maxSizeMB = _maxImageSize / 1024 / 1024;
			return new() { ErrorMessage = _localizer[L.ImageUploadMessages.imageTooLarge, maxSizeMB] };
		}

		var extension = Path.GetExtension(file.FileName)?.Trim().ToLower();
		if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
		{
			var allowed = string.Join(", ", _allowedExtensions);
			return new() { ErrorMessage = _localizer[L.ImageUploadMessages.unsupportedFileType, allowed] };
		}

		try
		{
			var folderPath = Path.Combine(_env.WebRootPath, folderName);
			Directory.CreateDirectory(folderPath);

			var fileName = $"{Guid.NewGuid()}{extension}";
			var filePath = Path.Combine(folderPath, fileName);

			using var stream = new FileStream(filePath, FileMode.Create);
			await file.CopyToAsync(stream);

			return new()
			{
				Uploaded = true,
				FilePath = Path.Combine(folderName, fileName).Replace("\\", "/")
			};
		}
		catch (Exception ex)
		{
			return new() { ErrorMessage = _localizer[L.ImageUploadMessages.uploadError, ex.Message] };
		}
	}

	public void DeleteFile(string? filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath)) return;

		var path = Path.Combine(_env.WebRootPath, filePath);
		if (File.Exists(path))
			File.Delete(path);
	}
}
