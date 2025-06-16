using QRCoder;

namespace LiveVibe.Server.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCodeSvg(string url);
    }

    public class QRCodeService : IQRCodeService
    {
        public string GenerateQRCodeSvg(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new SvgQRCode(qrData);
            return qrCode.GetGraphic(4);
        }
    }

}
