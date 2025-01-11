using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi
{
    public static class URL
    {
        public static string ProductionBaseURL = "https://cpc.getswish.net/";
        public static string ProductionPaymentRequest = ProductionBaseURL + "swish-cpcapi/api/v2/paymentrequests/";
        public static string ProductionGetQRCodeByToken = "https://mpc.getswish.net/qrg-swish/api/v1/commerce";
        public static string ProductionRefundRequest = ProductionBaseURL + "swish-cpcapi/api/v2/refunds/";
        public static string ProductionPayoutRequest = ProductionBaseURL + "swish-cpcapi/api/v1/payouts";


        public static string SandboxBaseURL = "https://staging.getswish.pub.tds.tieto.com/";
        public static string SandboxPaymentRequest = SandboxBaseURL + "swish-cpcapi/api/v2/paymentrequests/";
        public static string SandboxGetQRCodeByToken = SandboxBaseURL + "qrg-swish/api/v1/commerce";
        public static string SandboxRefundRequest = SandboxBaseURL + "swish-cpcapi/api/v2/refunds/";
        public static string SandboxPayoutRequest = SandboxBaseURL + "swish-cpcapi/api/v1/payouts";

        public static string EmulatorBaseURL = "https://mss.cpc.getswish.net/";
        public static string EmulatorPaymentRequest = EmulatorBaseURL + "swish-cpcapi/api/v2/paymentrequests/";
        public static string EmulatorGetQRCodeByToken = "https://mpc.getswish.net/qrg-swish/api/v1/commerce";
        public static string EmulatorRefundRequest = EmulatorBaseURL + "swish-cpcapi/api/v2/refunds/";
        public static string EmulatorPayoutRequest = EmulatorBaseURL + "swish-cpcapi/api/v1/payouts";
    }
}
