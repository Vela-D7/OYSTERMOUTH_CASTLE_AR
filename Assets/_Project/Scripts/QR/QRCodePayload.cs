using System;
using OystermouthCastle.Core;
using UnityEngine;

namespace OystermouthCastle.QR
{
    [Serializable]
    public struct QRCodePayload
    {
        public string ExperienceId;
        public AppLanguage? Language;
        public string RawPayload;

        public static bool TryParse(string rawPayload, out QRCodePayload payload)
        {
            payload = new QRCodePayload { RawPayload = rawPayload };

            if (string.IsNullOrWhiteSpace(rawPayload))
            {
                return false;
            }

            var trimmed = rawPayload.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                return TryParseUri(uri, ref payload);
            }

            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                return TryParseJson(trimmed, ref payload);
            }

            payload.ExperienceId = trimmed;
            return true;
        }

        private static bool TryParseUri(Uri uri, ref QRCodePayload payload)
        {
            var query = uri.Query.TrimStart('?').Split('&');
            for (var i = 0; i < query.Length; i++)
            {
                var parts = query[i].Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = Uri.UnescapeDataString(parts[0]).ToLowerInvariant();
                var value = Uri.UnescapeDataString(parts[1]);
                if (key == "experience" || key == "id")
                {
                    payload.ExperienceId = value;
                }
                else if (key == "lang" || key == "language")
                {
                    payload.Language = ParseLanguage(value);
                }
            }

            if (string.IsNullOrWhiteSpace(payload.ExperienceId))
            {
                var segments = uri.AbsolutePath.Trim('/').Split('/');
                if (segments.Length > 0)
                {
                    payload.ExperienceId = segments[segments.Length - 1];
                }
            }

            return !string.IsNullOrWhiteSpace(payload.ExperienceId);
        }

        private static bool TryParseJson(string json, ref QRCodePayload payload)
        {
            try
            {
                var data = JsonUtility.FromJson<JsonPayload>(json);
                payload.ExperienceId = !string.IsNullOrWhiteSpace(data.experience) ? data.experience : data.id;
                payload.Language = ParseLanguage(data.lang);
                return !string.IsNullOrWhiteSpace(payload.ExperienceId);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static AppLanguage? ParseLanguage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim().ToLowerInvariant();
            if (normalized == "cy" || normalized == "welsh")
            {
                return AppLanguage.Welsh;
            }

            if (normalized == "en" || normalized == "english")
            {
                return AppLanguage.English;
            }

            return null;
        }

        [Serializable]
        private struct JsonPayload
        {
            public string id;
            public string experience;
            public string lang;
        }
    }
}
