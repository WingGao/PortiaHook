using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hooks
{
	using System.IO;
	using System.Text;
	using System.Collections.Generic;

		public class HttpContentParser
		{
			public HttpContentParser(string conten)
			{
				this.Parse(conten);
			}
		public HttpContentParser(Stream stream)
		{
			this.Parse(stream, Encoding.UTF8);
		}

		public HttpContentParser(Stream stream, Encoding encoding)
		{
			this.Parse(stream, encoding);
		}


		private void Parse(Stream stream, Encoding encoding)
		{
			// Read the stream into a byte array
			byte[] data = ToByteArray(stream);

			// Copy to a string for header parsing
			string content = encoding.GetString(data);
			this.Parse(content);
		}

		private void Parse(string content)
			{
				this.Success = false;


				string name = string.Empty;
				string value = string.Empty;
				bool lookForValue = false;
				int charCount = 0;

				foreach (var c in content)
				{
					if (c == '=')
					{
						lookForValue = true;
					}
					else if (c == '&')
					{
						lookForValue = false;
						AddParameter(name, value);
						name = string.Empty;
						value = string.Empty;
					}
					else if (!lookForValue)
					{
						name += c;
					}
					else
					{
						value += c;
					}

					if (++charCount == content.Length)
					{
						AddParameter(name, value);
						break;
					}
				}

				// Get the start & end indexes of the file contents
				//int startIndex = nameMatch.Index + nameMatch.Length + "\r\n\r\n".Length;
				//Parameters.Add(name, s.Substring(startIndex).TrimEnd(new char[] { '\r', '\n' }).Trim());

				// If some data has been successfully received, set success to true
				if (Parameters.Count != 0)
					this.Success = true;
			}

			private void AddParameter(string name, string value)
			{
				if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
					Parameters.Add(Uri.UnescapeDataString(name.Trim()), Uri.UnescapeDataString(value.Trim()));
			}

			public IDictionary<string, string> Parameters = new Dictionary<string, string>();

			public bool Success
			{
				get;
				private set;
			}

		public static byte[] ToByteArray(Stream stream)
		{
			byte[] buffer = new byte[32768];
			using (MemoryStream ms = new MemoryStream())
			{
				while (true)
				{
					int read = stream.Read(buffer, 0, buffer.Length);
					if (read <= 0)
						return ms.ToArray();
					ms.Write(buffer, 0, read);
				}
			}
		}
	}


}
