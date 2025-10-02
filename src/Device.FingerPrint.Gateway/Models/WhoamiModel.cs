namespace Device.FingerPrint.Gateway.Models
{
    public class WhoamiModel
    {
        public required string Environment { get; set; }

        public required string ServerName { get; set; }

        public required string LocalIP { get; set; }

        public DateTime ServerTime { get; set; }

        public required string RealIP { get; set; }

        public required string FakeIP { get; set; }

        public required string XFW { get; set; }

        public required string ClientIP { get; set; }

        public required string ActualIP { get; set; }
    }
}
