using Microsoft.AspNetCore.Mvc;
using System.IO;

[Route("api/ImageController")]
[ApiController]
public class ImageController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly string _frontendImagePath;

    public ImageController(IWebHostEnvironment env)
    {
        _env = env;
        // Path to frontend images (absolute path)
        var currentDir = Directory.GetCurrentDirectory();
        var projectRoot = Directory.GetParent(currentDir)?.FullName ?? currentDir;
        _frontendImagePath = Path.Combine(projectRoot, "PTUD 2", "image");
        
        // Fallback: try absolute path if relative doesn't work
        if (!Directory.Exists(_frontendImagePath))
        {
            _frontendImagePath = @"E:\HappyLunch\PTUD 2\image";
        }
    }

    // Serve product images with intelligent fallback
    [HttpGet("products/{filename}")]
    public IActionResult GetProductImage(string filename)
    {
        // 1. Try to find the image in uploads/products (backend)
        var backendImagePath = Path.Combine(
            _env.WebRootPath,
            "uploads",
            "products",
            filename
        );

        if (System.IO.File.Exists(backendImagePath))
        {
            var contentType = GetContentType(filename);
            return PhysicalFile(backendImagePath, contentType);
        }

        // 2. Try to find a mapped image from frontend
        var mappedImage = GetMappedFrontendImage(filename);
        if (mappedImage != null && System.IO.File.Exists(mappedImage))
        {
            var contentType = GetContentType(mappedImage);
            return PhysicalFile(mappedImage, contentType);
        }

        // 3. Try to find any product image from frontend (fallback to product1-7)
        var fallbackImage = GetFallbackProductImage();
        if (fallbackImage != null && System.IO.File.Exists(fallbackImage))
        {
            var contentType = GetContentType(fallbackImage);
            return PhysicalFile(fallbackImage, contentType);
        }

        // 4. Return 404 - frontend will use placeholder
        return NotFound();
    }

    // Map backend filename to frontend image
    private string? GetMappedFrontendImage(string filename)
    {
        // Mapping database filenames to frontend product images
        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "com-trua-dac-biet-1.jpg", "product1.png" },
            { "com-trua-dac-biet-2.jpg", "product1.png" },
            { "com-ga-nuong-1.jpg", "product2.png" },
            { "com-ga-nuong-2.jpg", "product2.png" },
            { "pho-bo-1.jpg", "product3.jpg" },
            { "pho-bo-2.jpg", "product3.jpg" },
            { "bun-cha-1.jpg", "product4.png" },
            { "bun-cha-2.jpg", "product4.png" },
            { "banh-mi-1.jpg", "product5.png" },
            { "banh-mi-2.jpg", "product5.png" },
            { "bun-bo-hue-1.jpg", "product6.jpg" },
            { "com-chay-nam-1.jpg", "product7.png" },
            { "trai-cay-mix-1.jpg", "product1.png" },
            { "ca-phe-da-1.jpg", "product2.png" },
            { "combo-van-phong-1.jpg", "product3.jpg" }
        };

        if (mappings.TryGetValue(filename, out var mappedFile))
        {
            return Path.Combine(_frontendImagePath, mappedFile);
        }

        return null;
    }

    // Get a fallback product image (product1-7)
    private string? GetFallbackProductImage()
    {
        var productImages = new[] { "product1.png", "product2.png", "product3.jpg", "product4.png", "product5.png", "product6.jpg", "product7.png" };
        
        foreach (var img in productImages)
        {
            var path = Path.Combine(_frontendImagePath, img);
            if (System.IO.File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private string GetContentType(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
