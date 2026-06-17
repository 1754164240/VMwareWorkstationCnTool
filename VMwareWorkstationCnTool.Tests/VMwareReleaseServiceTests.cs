using System.Net;
using System.Text;
using VMwareWorkstationCnTool.Core;

namespace VMwareWorkstationCnTool.Tests;

public sealed class VMwareReleaseServiceTests
{
    [Fact]
    public void 解析最新Release时会优先选择WindowsExe资源()
    {
        var json = """
        {
          "tag_name": "26H1",
          "html_url": "https://github.com/201853910/VMwareWorkstation/releases/tag/26H1",
          "assets": [
            {
              "name": "VMware-Workstation-Full-26H1-25388281.x86_64.bundle",
              "browser_download_url": "https://example.com/linux.bundle",
              "size": 340821664
            },
            {
              "name": "VMware-Workstation-Full-26H1-25388281.exe",
              "browser_download_url": "https://example.com/windows.exe",
              "size": 287670872
            }
          ]
        }
        """;

        var release = VMwareReleaseService.ParseLatestRelease(json);

        Assert.Equal("26H1", release.TagName);
        Assert.Equal("VMware-Workstation-Full-26H1-25388281.exe", release.WindowsAsset.Name);
        Assert.Equal("https://example.com/windows.exe", release.WindowsAsset.DownloadUrl);
        Assert.Equal(287670872, release.WindowsAsset.Size);
    }

    [Fact]
    public void 没有Exe资源时会给出明确错误()
    {
        var json = """
        {
          "tag_name": "26H1",
          "html_url": "https://github.com/201853910/VMwareWorkstation/releases/tag/26H1",
          "assets": [
            {
              "name": "VMware-Workstation-Full-26H1-25388281.x86_64.bundle",
              "browser_download_url": "https://example.com/linux.bundle",
              "size": 340821664
            }
          ]
        }
        """;

        var ex = Assert.Throws<InvalidOperationException>(() => VMwareReleaseService.ParseLatestRelease(json));

        Assert.Contains("exe", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task 下载最新版安装包时会写入目标文件()
    {
        using var directory = TestDirectory.Create();
        var handler = new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsoluteUri.EndsWith("/releases/latest", StringComparison.OrdinalIgnoreCase) == true)
            {
                var json = """
                {
                  "tag_name": "26H1",
                  "html_url": "https://github.com/201853910/VMwareWorkstation/releases/tag/26H1",
                  "assets": [
                    {
                      "name": "VMware-Workstation-Full-26H1-25388281.exe",
                      "browser_download_url": "https://example.com/windows.exe",
                      "size": 4
                    }
                  ]
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 })
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = new VMwareReleaseService(httpClient);

        var result = await service.DownloadLatestWindowsInstallerAsync(directory.Path, CancellationToken.None);

        Assert.True(File.Exists(result.FilePath));
        Assert.Equal("VMware-Workstation-Full-26H1-25388281.exe", Path.GetFileName(result.FilePath));
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(result.FilePath));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            this.respond = respond;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(respond(request));
        }
    }
}
